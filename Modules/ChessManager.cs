using System;

using Chess_Server.Templates.Internal;

namespace Chess_Server.Modules
{
    public static class ChessManager
    {
        #region Utils
        
        public static Block[] BoardToArray(Block[][] board)
        {
            Block[] result = new Block[64];
            for (int row = 0; row < 8; row++)
            {
                for (int col = 0; col < 8; col++)
                {
                    result[row * 8 + col] = board[row][col];
                }
            }
            return result;
        }

        public static Block[][] ArrayToBoard(Block[] array)
        {
            if (array.Length != 64) throw new ArgumentException("Array size must be 64.");

            Block[][] board = new Block[8][];
            for (int row = 0; row < 8; row++)
            {
                board[row] = new Block[8];
                for (int col = 0; col < 8; col++)
                {
                    board[row][col] = array[row * 8 + col];
                }
            }
            return board;
        }
        
        /// <summary>
        /// 0~63은 a1(0, 0)~h8(7, 7)에 대응합니다.
        /// </summary>
        private static (int row, int column) PositionToCoordinates(int position)
        {
            if (!InBoard(position)) throw new ArgumentException("Position must be between 0 and 63");
            return (position / 8, position % 8);
        }

        /// <summary>
        /// a1(0, 0)~h8(7, 7)은 0~63에 대응합니다.
        /// </summary>
        private static int CoordinatesToPosition(int row, int column)
        {
            if (!InBoard(row, column)) throw new ArgumentException("Invalid coordinates");
            return row * 8 + column;
        }
        
        private static bool InBoard(int position) => position >= 0 && position < 64;
        private static bool InBoard(int row, int column) => row >= 0 && row < 8 && column >= 0 && column < 8;

        private static bool IsEnemy(Block[][] board, int row1, int column1, int row2, int column2) => board[row2][column2].team != DefineTeam.None && board[row1][column1].team != board[row2][column2].team;

        private static Block[][] CloneBoard(Block[][] source)
        {
            Block[][] clone = new Block[8][];
            for (int row = 0; row < 8; row++)
            {
                clone[row] = new Block[8];
                Array.Copy(source[row], clone[row], 8);
            }
            return clone;
        }

        public static Block[][] InitializeBoard()
        {
            Block[][] board = new Block[8][];
            for (int i = 0; i < 8; i++)
            {
                board[i] = new Block[8];
                for (int j = 0; j < 8; j++)
                {
                    board[i][j] = new Block();
                }
            }

            // White
            board[0][0] = new Block(DefinePieces.Rook, DefineTeam.White);
            board[0][1] = new Block(DefinePieces.Knight, DefineTeam.White);
            board[0][2] = new Block(DefinePieces.Bishop, DefineTeam.White);
            board[0][3] = new Block(DefinePieces.Queen, DefineTeam.White);
            board[0][4] = new Block(DefinePieces.King, DefineTeam.White);
            board[0][5] = new Block(DefinePieces.Bishop, DefineTeam.White);
            board[0][6] = new Block(DefinePieces.Knight, DefineTeam.White);
            board[0][7] = new Block(DefinePieces.Rook, DefineTeam.White);
            for (int i = 0; i < 8; i++)
            {
                board[1][i] = new Block(DefinePieces.Pawn, DefineTeam.White);
            }

            // Black
            board[7][0] = new Block(DefinePieces.Rook, DefineTeam.Black);
            board[7][1] = new Block(DefinePieces.Knight, DefineTeam.Black);
            board[7][2] = new Block(DefinePieces.Bishop, DefineTeam.Black);
            board[7][3] = new Block(DefinePieces.Queen, DefineTeam.Black);
            board[7][4] = new Block(DefinePieces.King, DefineTeam.Black);
            board[7][5] = new Block(DefinePieces.Bishop, DefineTeam.Black);
            board[7][6] = new Block(DefinePieces.Knight, DefineTeam.Black);
            board[7][7] = new Block(DefinePieces.Rook, DefineTeam.Black);
            for (int i = 0; i < 8; i++)
            {
                board[6][i] = new Block(DefinePieces.Pawn, DefineTeam.Black);
            }

            return board;
        }

        #endregion

        #region Moves

        public static List<int> GetAvailableMoves(Block[][] board, int position)
        {
            (int row, int column) = PositionToCoordinates(position);
            List<(int row, int column)> rawResult = GetAvailableMoves(board, row, column);
            List<int> result = new List<int>();
            foreach ((int row, int column) element in rawResult)
            {
                result.Add(CoordinatesToPosition(row,column));
            }
            return result;
        }
        public static List<(int row, int column)> GetAvailableMoves(Block[][] board, int row, int column)
        {
            List<(int, int)> result = new List<(int, int)>();
            Block block = board[row][column];
            if (block.IsEmpty) return result;

            switch (block.piece)
            {
                case DefinePieces.Pawn:
                    AddPawnMoves(board, row, column, block.team, result);
                    break;
                case DefinePieces.Rook:
                    AddLineMoves(board, row, column, block.team, result, new (int, int)[]
                                                                         {
                                                                             (1, 0), (-1, 0), (0, 1), (0, -1)
                                                                         });
                    break;
                case DefinePieces.Bishop:
                    AddLineMoves(board, row, column, block.team, result, new (int, int)[]
                                                                         {
                                                                             (1, 1), (1, -1), (-1, 1), (-1, -1)
                                                                         });
                    break;
                case DefinePieces.Queen:
                    AddLineMoves(board, row, column, block.team, result, new (int, int)[]
                                                                         {
                                                                             (1, 0), (-1, 0), (0, 1), (0, -1),
                                                                             (1, 1), (1, -1), (-1, 1), (-1, -1)
                                                                         });
                    break;
                case DefinePieces.Knight:
                    AddKnightMoves(board, row, column, block.team, result);
                    break;
                case DefinePieces.King:
                    AddKingMoves(board, row, column, block.team, result);
                    break;
            }
            return result;
        }

        private static void AddPawnMoves(Block[][] board, int row, int column, DefineTeam team, List<(int, int)> result)
        {
            int direction = team == DefineTeam.White ? 1 : -1;
            int startRow = team == DefineTeam.White ? 1 : 6;

            int newRow = row + direction;
            if (InBoard(newRow, column) && board[newRow][column].IsEmpty)
            {
                result.Add((newRow, column));
                int newRow2 = row + direction * 2;
                if (row == startRow && board[newRow2][column].IsEmpty)
                {
                    result.Add((newRow2, column));
                }
            }
            foreach (int deltaColumn in new[] { -1, 1 })
            {
                int currentRow = row + direction;
                int currentColumn = column + deltaColumn;
                if (InBoard(currentRow, currentColumn) && IsEnemy(board, row, column, currentRow, currentColumn))
                {
                    result.Add((currentRow, currentColumn));
                }
            }
        }

        private static void AddLineMoves(Block[][] board, int row, int column, DefineTeam team, List<(int, int)> result, (int deltaRow, int deltaColumn)[] directions)
        {
            foreach ((int deltaRow, int deltaColumn) in directions)
            {
                int newRow = row + deltaRow, newColumn = column + deltaColumn;
                while (InBoard(newRow, newColumn))
                {
                    if (board[newRow][newColumn].IsEmpty)
                        result.Add((newRow, newColumn));
                    else
                    {
                        if (IsEnemy(board, row, column, newRow, newColumn)) result.Add((newRow, newColumn));
                        break;
                    }
                    newRow += deltaRow; newColumn += deltaColumn;
                }
            }
        }

        private static void AddKnightMoves(Block[][] board, int row, int column, DefineTeam team, List<(int, int)> result)
        {
            (int deltaRow, int deltaColumn)[] offsets = { (2, 1), (2, -1), (-2, 1), (-2, -1), (1, 2), (1, -2), (-1, 2), (-1, -2) };
            foreach ((int deltaRow, int deltaColumn) in offsets)
            {
                int newRow = row + deltaRow, newColumn = column + deltaColumn;
                if (!InBoard(newRow, newColumn) || board[newRow][newColumn].team == team) continue;
                result.Add((newRow, newColumn));
            }
        }

        private static void AddKingMoves(Block[][] board, int row, int column, DefineTeam team, List<(int, int)> result)
        {
            for (int deltaRow = -1; deltaRow <= 1; deltaRow++)
                for (int deltaColumn = -1; deltaColumn <= 1; deltaColumn++)
                {
                    if (deltaRow == 0 && deltaColumn == 0) continue;
                    int newRow = row + deltaRow, newColumn = column + deltaColumn;
                    if (!InBoard(newRow, newColumn) || board[newRow][newColumn].team == team) continue;
                    result.Add((newRow, newColumn));
                }
        }
        
        #endregion

        #region Move Validations

        public static List<(int row, int column)> GetPossibleMoves(Block[][] board, int fromPosition)
        {
            (int fromRow, int fromColumn) = PositionToCoordinates(fromPosition);
            return GetPossibleMoves(board, fromRow, fromColumn);
        }
        public static List<(int row, int column)> GetPossibleMoves(Block[][] board, int fromRow, int fromColumn)
        {
            List<(int row, int column)> pseudoMoves = GetAvailableMoves(board, fromRow, fromColumn);
            List<(int, int)> legalMoves = new List<(int, int)>();
            foreach ((int row, int column) in pseudoMoves)
            {
                if (IsValidMove(board, fromRow, fromColumn, row, column))
                {
                    legalMoves.Add((row, column));
                }
            }
            return legalMoves;
        }
        
        public static bool IsValidMove(Block[][] board, int fromPosition, int toPosition)
        {
            (int fromRow, int fromColumn) = PositionToCoordinates(fromPosition);
            (int toRow, int toColumn) = PositionToCoordinates(toPosition);
            return IsValidMove(board, fromRow, fromColumn, toRow, toColumn);
        }
        public static bool IsValidMove(Block[][] board, int fromRow, int fromColumn, int toRow, int toColumn)
        {
            Block block = board[fromRow][fromColumn];
            if (block.IsEmpty) return false;
            List<(int row, int column)> possibleMoves = GetAvailableMoves(board, fromRow, fromColumn);
            if (!possibleMoves.Contains((toRow, toColumn))) return false;
            Block[][] newBoard = SimulateMove(board, fromRow, fromColumn, toRow, toColumn);
            return !IsInCheck(block.team, newBoard);
        }

        public static Block[][] SimulateMove(Block[][] board, int fromPosition, int toPosition)
        {
            (int fromRow, int fromColumn) = PositionToCoordinates(fromPosition);
            (int toRow, int toColumn) = PositionToCoordinates(toPosition);
            return SimulateMove(board, fromRow, fromColumn, toRow, toColumn);
        }
        private static Block[][] SimulateMove(Block[][] board, int fromRow, int fromColumn, int toRow, int toColumn)
        {
            Block[][] newBoard = CloneBoard(board);
            newBoard[toRow][toColumn] = newBoard[fromRow][fromColumn];
            newBoard[fromRow][fromColumn] = new Block();
            return newBoard;
        }
        
        #endregion

        #region Promotion
        
        public static bool HandlePromotion(Block[][] board, int position)
        {
            (int row, int column) = PositionToCoordinates(position);
            return HandlePromotion(board, row, column);
        }
        public static bool HandlePromotion(Block[][] board, int row, int column)
        {
            Block block = board[row][column];
            if (block.piece != DefinePieces.Pawn) return false;
            if ((block.team == DefineTeam.White && row == 7) || (block.team == DefineTeam.Black && row == 0))
            {
                board[row][column] = new Block(DefinePieces.Queen, block.team);
                return true;
            }
            return false;
        }
        
        #endregion

        #region Check / Checkmate
        
        public static bool IsInCheck(DefineTeam team, Block[][] board)
        {
            int kingRow = -1, kingColumn = -1;
            for (int row = 0; row < 8; row++)
            {
                for (int column = 0; column < 8; column++)
                {
                    if (board[row][column].piece == DefinePieces.King && board[row][column].team == team)
                    {
                        kingRow = row;
                        kingColumn = column;
                        break;
                    }
                }
            }

            if (kingRow == -1) return true; // 킹 없음 => 체크

            DefineTeam enemyTeam = team == DefineTeam.White ? DefineTeam.Black : DefineTeam.White;
            for (int row = 0; row < 8; row++)
            {
                for (int column = 0; column < 8; column++)
                {
                    if (board[row][column].team == enemyTeam)
                    {
                        if (GetAvailableMoves(board, row, column).Contains((kingRow, kingColumn)))
                        {
                            return true;
                        }
                    }
                }
            }
            
            return false;
        }

        public static bool IsCheckmate(DefineTeam team, Block[][] board)
        {
            if (!IsInCheck(team, board)) return false;
            for (int row = 0; row < 8; row++)
            {
                for (int column = 0; column < 8; column++)
                {
                    if (board[row][column].team == team)
                    {
                        foreach ((int newRow, int newColumn) in GetPossibleMoves(board, row, column))
                        {
                            return false; // 탈출 가능 => 메이트 아님
                        }
                    }
                }
            }
            return true;
        }

        public static bool IsGameOver(Block[][] board, out DefineTeam winner)
        {
            if (IsCheckmate(DefineTeam.White, board))
            {
                winner = DefineTeam.Black;
                return true;
            }
            if (IsCheckmate(DefineTeam.Black, board))
            {
                winner = DefineTeam.White;
                return true;
            }

            winner = DefineTeam.None;
            return false;
        }
        
        #endregion
    }
}

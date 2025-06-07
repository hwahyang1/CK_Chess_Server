using System;

using Chess_Server.Templates;

namespace Chess_Server.Modules
{
    public static class ChessManager
    {
        #region Utils
        private static bool InBoard(int row, int column) => row >= 0 && row < 8 && column >= 0 && column < 8;

        private static bool IsEnemy(Block[][] board, int row1, int column1, int row2, int column2) => board[row2][column2].Team != DefineTeam.None && board[row1][column1].Team != board[row2][column2].Team;

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

        #endregion

        #region Moves
        private static List<(int row, int column)> GetAvailableMoves(Block[][] board, int row, int column)
        {
            List<(int, int)> result = new List<(int, int)>();
            Block block = board[row][column];
            if (block.IsEmpty) return result;

            switch (block.Piece)
            {
                case DefinePieces.Pawn:
                    AddPawnMoves(board, row, column, block.Team, result);
                    break;
                case DefinePieces.Rook:
                    AddLineMoves(board, row, column, block.Team, result, new (int, int)[]
                                                                         {
                                                                             (1, 0), (-1, 0), (0, 1), (0, -1)
                                                                         });
                    break;
                case DefinePieces.Bishop:
                    AddLineMoves(board, row, column, block.Team, result, new (int, int)[]
                                                                         {
                                                                             (1, 1), (1, -1), (-1, 1), (-1, -1)
                                                                         });
                    break;
                case DefinePieces.Queen:
                    AddLineMoves(board, row, column, block.Team, result, new (int, int)[]
                                                                         {
                                                                             (1, 0), (-1, 0), (0, 1), (0, -1),
                                                                             (1, 1), (1, -1), (-1, 1), (-1, -1)
                                                                         });
                    break;
                case DefinePieces.Knight:
                    AddKnightMoves(board, row, column, block.Team, result);
                    break;
                case DefinePieces.King:
                    AddKingMoves(board, row, column, block.Team, result);
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
                if (!InBoard(newRow, newColumn) || board[newRow][newColumn].Team == team) continue;
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
                    if (!InBoard(newRow, newColumn) || board[newRow][newColumn].Team == team) continue;
                    result.Add((newRow, newColumn));
                }
        }
        #endregion

        #region Move Validations
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

        public static bool IsValidMove(Block[][] board, int fromRow, int fromColumn, int toRow, int toColumn)
        {
            Block block = board[fromRow][fromColumn];
            if (block.IsEmpty) return false;
            List<(int row, int column)> possibleMoves = GetAvailableMoves(board, fromRow, fromColumn);
            if (!possibleMoves.Contains((toRow, toColumn))) return false;
            Block[][] newBoard = SimulateMove(board, fromRow, fromColumn, toRow, toColumn);
            return !IsInCheck(block.Team, newBoard);
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
        public static bool HandlePromotion(Block[][] board, int row, int column)
        {
            Block block = board[row][column];
            if (block.Piece != DefinePieces.Pawn) return false;
            if ((block.Team == DefineTeam.White && row == 7) || (block.Team == DefineTeam.Black && row == 0))
            {
                board[row][column] = new Block(DefinePieces.Queen, block.Team);
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
                    if (board[row][column].Piece == DefinePieces.King && board[row][column].Team == team)
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
                    if (board[row][column].Team == enemyTeam)
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
                    if (board[row][column].Team == team)
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

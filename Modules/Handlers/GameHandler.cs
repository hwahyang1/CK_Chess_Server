using System;
using System.Text.Json;

using Chess_Server.Templates.Request;
using Chess_Server.Templates.Response;
using Chess_Server.Templates.Internal;

namespace Chess_Server.Modules.Handlers
{
	public static class GameHandler
	{
		private static readonly string ROOM_UID_PLACEHOLDER = "<ROOM_UID>";
		private static readonly string BOARD_KEY = ROOM_UID_PLACEHOLDER + "_BoardInfo";

		private static readonly JsonSerializerOptions SERIALIZER_OPTIONS = new JsonSerializerOptions
		                                                                    {
			                                                                    IncludeFields = true,
			                                                                    PropertyNameCaseInsensitive = true
		                                                                    };

		private static Block[][] GetBoard(string roomId)
		{
			string key = BOARD_KEY.Replace(ROOM_UID_PLACEHOLDER, roomId);
			string? boardJson = RedisManager.Instance.Get(key);
			Block[][] board;
			Block[] databaseBoard;

			if (string.IsNullOrEmpty(boardJson))
			{
				board = ChessManager.InitializeBoard();
				databaseBoard = ChessManager.BoardToArray(board);
				
				string serializedBoard = JsonSerializer.Serialize<Block[]>(databaseBoard);
				RedisManager.Instance.Set(key, serializedBoard);
			}
			else
			{
				databaseBoard = JsonSerializer.Deserialize<Block[]>(boardJson);
				board = ChessManager.ArrayToBoard(databaseBoard);
			}

			return board;
		}
		
		public static GameBoardInfoResponse GetBoard(GameBoardInfoRequest request, string command = "")
		{
			if (command == "") command = request.command;
			RoomData? room = RoomManager.GetRoomByRoomId(request.roomId);
			if (room == null) return new GameBoardInfoResponse(request.clientUid, command, 404, "Not Found", []);
			
			Block[] board = ChessManager.BoardToArray(GetBoard(request.roomId));

			GameBoardInfoResponse response = new GameBoardInfoResponse(request.clientUid, command, 200, "OK", board);
			return response;
		}

		public static GamePieceAvailableMovementsResponse GetAvailableMovements(GamePieceAvailableMovementsRequest request, string command = "")
		{
			if (command == "") command = request.command;
			RoomData? room = RoomManager.GetRoomByRoomId(request.roomId);
			if (room == null) return new GamePieceAvailableMovementsResponse(request.clientUid, command, 404, "Not Found", []);
			
			Block[][] board = GetBoard(request.roomId);

			List<int> positions = ChessManager.GetAvailableMoves(board, request.position);

			GamePieceAvailableMovementsResponse response = new GamePieceAvailableMovementsResponse(request.clientUid, command, 200, "OK", positions.ToArray());
			return response;
		}

		public static GamePieceMoveResponse GamePieceMove(GamePieceMoveRequest request, string command = "")
		{
			if (command == "") command = request.command;
			RoomData? room = RoomManager.GetRoomByRoomId(request.roomId);
			if (room == null) return new GamePieceMoveResponse(request.clientUid, command, 404, "Not Found", DefineGameStatus.Unknown, DefineTeam.None, []);
			
			Block[][] board = GetBoard(request.roomId);

			// Validation
			if (!ChessManager.IsValidMove(board, request.fromPosition, request.toPosition))
			{
				return new GamePieceMoveResponse(request.clientUid, command, 400, "Invalid Move", DefineGameStatus.Running, DefineTeam.None, []);
			}

			// Move
			board = ChessManager.SimulateMove(board, request.fromPosition, request.toPosition);

			// Promotion (if met)
			ChessManager.HandlePromotion(board, request.toPosition);

			// Events
			DefineGameStatus gameStatus = DefineGameStatus.Running;
			DefineTeam dominantTeam = DefineTeam.None;
			if (ChessManager.IsGameOver(board, out DefineTeam winner))
			{
				gameStatus = DefineGameStatus.End;
				dominantTeam = winner;
			}
			else if (ChessManager.IsCheckmate(DefineTeam.White, board) || ChessManager.IsCheckmate(DefineTeam.Black, board))
			{
				gameStatus = DefineGameStatus.Checkmate;
				dominantTeam = ChessManager.IsCheckmate(DefineTeam.White, board) ? DefineTeam.Black : DefineTeam.White;
			}
			else if (ChessManager.IsInCheck(DefineTeam.White, board) || ChessManager.IsInCheck(DefineTeam.Black, board))
			{
				gameStatus = DefineGameStatus.Check;
				dominantTeam = ChessManager.IsInCheck(DefineTeam.White, board) ? DefineTeam.Black : DefineTeam.White;
			}

			// Save
			string key = BOARD_KEY.Replace(ROOM_UID_PLACEHOLDER, request.roomId);
			Block[] databaseBoard = ChessManager.BoardToArray(board);
			RedisManager.Instance.Set(key, JsonSerializer.Serialize<Block[]>(databaseBoard, SERIALIZER_OPTIONS));

			GamePieceMoveResponse response = new GamePieceMoveResponse(request.clientUid, command, 200, "OK", gameStatus, dominantTeam, databaseBoard);
			return response;
		}
	}
}

using System;
using System.Text.Json;

using Chess_Server.Templates.Request;
using Chess_Server.Templates.Response;
using Chess_Server.Templates.Internal;

namespace Chess_Server.Modules.Handlers
{
	public class GameHandler
	{
		private static readonly string ROOM_UID_PLACEHOLDER = "<ROOM_UID>";
		private static readonly string BOARD_KEY = ROOM_UID_PLACEHOLDER + "_BoardInfo";

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
				
				string serializedBoard = JsonSerializer.Serialize(databaseBoard);
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
	}
}

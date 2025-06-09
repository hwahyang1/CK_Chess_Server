using System;
using System.Text.Json;

using Chess_Server.Templates;
using Chess_Server.Templates.Request;
using Chess_Server.Templates.Response;
using Chess_Server.Templates.Internal;

namespace Chess_Server.Modules.Handlers
{
	public class GameHandler
	{
		private static readonly string ROOM_UID_PLACEHOLDER = "<ROOM_UID>";
		private static readonly string BOARD_KEY = ROOM_UID_PLACEHOLDER + "_BoardInfo";

		public static BoardInfoResponse GetBoard(BoardInfoRequest request, string command = "")
		{
			if (command == "") command = request.command;
			RoomData? room = RoomManager.GetRoomByRoomId(request.roomId);
			if (room == null) return new BoardInfoResponse(request.clientUid, command, 404, "Not Found", []);
			
			string key = BOARD_KEY.Replace(ROOM_UID_PLACEHOLDER, request.roomId);
			string? boardJson = RedisManager.Instance.Get(key);
			Block[] board;

			if (string.IsNullOrEmpty(boardJson))
			{
				Block[][] newBoard = ChessManager.InitializeBoard();
				board = ChessManager.BoardToArray(newBoard);
				
				string serializedBoard = JsonSerializer.Serialize(board);
				RedisManager.Instance.Set(key, serializedBoard);
			}
			else
			{
				board = JsonSerializer.Deserialize<Block[]>(boardJson)!;
			}

			BoardInfoResponse response = new BoardInfoResponse(request.clientUid, command, 200, "OK", board);
			return response;
		}
	}
}

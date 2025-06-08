using System;

using Chess_Server.Templates.Request;
using Chess_Server.Templates.Response;
using Chess_Server.Templates.Internal;

namespace Chess_Server.Modules.Handlers
{
	public class RoomHandler
	{
		public static RoomListsResponse GetRoomLists(RoomListsRequest request, string command = "")
		{
			if (command == "") command = request.command;
			RoomData[] rooms = RoomManager.GetRooms();
			RoomListsResponse response = new RoomListsResponse(request.clientUid, command, 200, "OK", rooms);
			return response;
		}
	}
}

using System;

using Chess_Server.Templates;
using Chess_Server.Templates.Request;
using Chess_Server.Templates.Response;
using Chess_Server.Templates.Internal;

namespace Chess_Server.Modules.Handlers
{
	public static class RoomHandler
	{
		public static RoomListsResponse GetRoomLists(RoomListsRequest request, string command = "")
		{
			if (command == "") command = request.command;
			RoomData[] rooms = RoomManager.GetRooms();
			RoomListsResponse response = new RoomListsResponse(request.clientUid, command, 200, "OK", rooms);
			return response;
		}

		public static (RoomInfoResponse response, RoomData[] previousRoom) RoomCreate(RoomCreateRequest request, string command = "")
		{
			if (command == "") command = request.command;
			string? userUid = UserHandler.GetUserUidByClientUid(request.clientUid);
			if (userUid == null) return (new RoomInfoResponse(request.clientUid, command, 403, "Forbidden", null, DefineTeam.None), []);
			(string roomId, RoomData[] previousRoom) = RoomManager.CreateRoom(request.roomName, userUid);
			RoomData room = RoomManager.GetRoomByRoomId(roomId);
			RoomInfoResponse response = new RoomInfoResponse(request.clientUid, command, 200, "OK", room, DefineTeam.None);
			return (response, previousRoom);
		}
		
		public static (RoomInfoResponse response, RoomData[] previousRoom) RoomJoin(RoomJoinRequest request, string command = "")
		{
			if (command == "") command = request.command;
			string? userUid = UserHandler.GetUserUidByClientUid(request.clientUid);
			if (userUid == null) return (new RoomInfoResponse(request.clientUid, command, 403, "Forbidden", null, DefineTeam.None), []);
			RoomData[] previousRoom = RoomManager.JoinRoom(request.roomId, userUid);
			// TODO: Game Start
			RoomData room = RoomManager.GetRoomByRoomId(request.roomId);
			RoomInfoResponse response = new RoomInfoResponse(request.clientUid, command, 200, "OK", room, DefineTeam.None);
			return (response, previousRoom);
		}

		public static RoomLeaveOrDeleteResponse LeaveOrDeleteRoom(RoomLeaveOrDeleteRequest request, string command = "")
		{
			if (command == "") command = request.command;
			string? userUid = UserHandler.GetUserUidByClientUid(request.clientUid);
			if (userUid == null) return new RoomLeaveOrDeleteResponse(request.clientUid, command, 403, "Forbidden", null);
			RoomData? room = RoomManager.LeaveRoom(request.roomId, userUid);
			RoomLeaveOrDeleteResponse roomLeaveOrDeleteResponse = new RoomLeaveOrDeleteResponse(request.clientUid, command, 200, "OK", room);
			return roomLeaveOrDeleteResponse;
		}
	}
}

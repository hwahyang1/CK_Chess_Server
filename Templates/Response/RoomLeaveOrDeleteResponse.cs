using System;
using System.Text.Json.Serialization;

using Chess_Server.Templates.Internal;

namespace Chess_Server.Templates.Response
{
	[Serializable]
	public class RoomLeaveOrDeleteResponse : BaseResponse
	{
		[JsonInclude]
		public RoomData? Room;

		public RoomLeaveOrDeleteResponse(string clientUid, string command, int statusCode, string reason, RoomData? room) : base(clientUid, command, statusCode, reason)
		{
			Room = room;
		}
	}
}

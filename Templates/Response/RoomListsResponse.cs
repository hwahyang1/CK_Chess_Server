using System;
using System.Text.Json.Serialization;

using Chess_Server.Templates.Internal;

namespace Chess_Server.Templates.Response
{
	[Serializable]
	public class RoomListsResponse : BaseResponse
	{
		[JsonInclude]
		public RoomData[] rooms;

		public RoomListsResponse(string clientUid, string command, int statusCode, string reason, RoomData[] rooms) : base(clientUid, command, statusCode, reason)
		{
			this.rooms = rooms;
		}
	}
}

using System;
using System.Text.Json.Serialization;

namespace Chess_Server.Templates.Request
{
	[Serializable]
	public class RoomLeaveOrDeleteRequest : BaseRequest
	{
		[JsonInclude]
		public string roomId;

		[JsonConstructor]
		public RoomLeaveOrDeleteRequest(string clientUid, string command, string roomId) : base(clientUid, command)
		{
			this.roomId = roomId;
		}
	}
}

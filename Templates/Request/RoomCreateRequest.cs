using System;
using System.Text.Json.Serialization;

namespace Chess_Server.Templates.Request
{
	[Serializable]
	public class RoomCreateRequest : BaseRequest
	{
		[JsonInclude]
		public string roomName;

		[JsonConstructor]
		public RoomCreateRequest(string clientUid, string command, string roomName) : base(clientUid, command)
		{
			this.roomName = roomName;
		}
	}
}

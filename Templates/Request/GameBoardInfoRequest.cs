using System;
using System.Text.Json.Serialization;

namespace Chess_Server.Templates.Request
{
	[Serializable]
	public class GameBoardInfoRequest : BaseRequest
	{
		[JsonInclude]
		public string roomId;

		[JsonConstructor]
		public GameBoardInfoRequest(string clientUid, string command, string roomId) : base(clientUid, command)
		{
			this.roomId = roomId;
		}
	}
}

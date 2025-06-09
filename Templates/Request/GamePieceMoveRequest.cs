using System;
using System.Text.Json.Serialization;

namespace Chess_Server.Templates.Request
{
	[Serializable]
	public class GamePieceMoveRequest : BaseRequest
	{
		[JsonInclude]
		public string roomId;

		[JsonInclude]
		public int fromPosition;
		
		[JsonInclude]
		public int toPosition;

		[JsonConstructor]
		public GamePieceMoveRequest(string clientUid, string command, string roomId, int fromPosition, int toPosition) : base(clientUid, command)
		{
			this.roomId = roomId;
			this.fromPosition = fromPosition;
			this.toPosition = toPosition;
		}
	}
}

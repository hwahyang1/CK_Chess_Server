using System;
using System.Text.Json.Serialization;

namespace Chess_Server.Templates.Request
{
	[Serializable]
	public class GamePieceAvailableMovementsRequest : BaseRequest
	{
		[JsonInclude]
		public string roomId;
		
		[JsonInclude]
		public int position;

		[JsonConstructor]
		public GamePieceAvailableMovementsRequest(string clientUid, string command, string roomId, int position) : base(clientUid, command)
		{
			this.roomId = roomId;
			this.position = position;
		}
	}
}

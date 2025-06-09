using System;
using System.Text.Json.Serialization;

using Chess_Server.Templates.Internal;

namespace Chess_Server.Templates.Response
{
	[Serializable]
	public class GamePieceAvailableMovementsResponse : BaseResponse
	{
		[JsonInclude]
		public int[] positions;

		public GamePieceAvailableMovementsResponse(string clientUid, string command, int statusCode, string reason, int[] positions) : base(clientUid, command, statusCode, reason)
		{
			this.positions = positions;
		}
	}
}

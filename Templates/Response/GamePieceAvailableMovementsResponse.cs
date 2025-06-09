using System;
using System.Text.Json.Serialization;

using Chess_Server.Templates.Internal;

namespace Chess_Server.Templates.Response
{
	[Serializable]
	public class GamePieceAvailableMovementsResponse : BaseResponse
	{
		[JsonInclude]
		public RoomData[] Rooms;

		public GamePieceAvailableMovementsResponse(string clientUid, string command, int statusCode, string reason, RoomData[] rooms) : base(clientUid, command, statusCode, reason)
		{
			Rooms = rooms;
		}
	}
}

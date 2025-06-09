using System;
using System.Text.Json.Serialization;

using Chess_Server.Templates.Internal;

namespace Chess_Server.Templates.Response
{
	[Serializable]
	public class RoomInfoResponse : BaseResponse
	{
		[JsonInclude]
		public RoomData room;

		[JsonInclude]
		public DefineTeam yourTeam;

		public RoomInfoResponse(string clientUid, string command, int statusCode, string reason, RoomData room, DefineTeam yourTeam) : base(clientUid, command, statusCode, reason)
		{
			this.room = room;
			this.yourTeam = yourTeam;
		}
	}
}

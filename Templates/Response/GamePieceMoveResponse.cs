using System;
using System.Text.Json.Serialization;

using Chess_Server.Templates.Internal;

namespace Chess_Server.Templates.Response
{
	[Serializable]
	public class GamePieceMoveResponse : BaseResponse
	{
		[JsonInclude]
		public DefineGameStatus gameStatus;
		
		[JsonInclude]
		public DefineTeam dominantTeam;
		
		[JsonInclude]
		public Block[] board;

		public GamePieceMoveResponse(string clientUid, string command, int statusCode, string reason, DefineGameStatus gameStatus, DefineTeam dominantTeam, Block[] board) : base(clientUid, command, statusCode, reason)
		{
			this.gameStatus = gameStatus;
			this.dominantTeam = dominantTeam;
			this.board = board;
		}
	}
}

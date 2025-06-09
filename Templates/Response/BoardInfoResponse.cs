using System;
using System.Text.Json.Serialization;

using Chess_Server.Templates.Internal;

namespace Chess_Server.Templates.Response
{
	[Serializable]
	public class BoardInfoResponse : BaseResponse
	{
		[JsonInclude]
		public Block[] board;

		public BoardInfoResponse(string clientUid, string command, int statusCode, string reason, Block[] board) : base(clientUid, command, statusCode, reason)
		{
			this.board = board;
		}
	}
}

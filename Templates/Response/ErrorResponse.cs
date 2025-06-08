using System;
using System.Text.Json.Serialization;

namespace Chess_Server.Templates.Response
{
	[Serializable]
	public class ErrorResponse : BaseResponse
	{
		//

		public ErrorResponse(string clientUid, int statusCode, string reason) : base(clientUid, "ERROR", statusCode, reason)
		{
			//
		}
	}
}

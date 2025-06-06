using System;
using System.Text.Json.Serialization;

namespace Chess_Server.Templates.Response
{
	[Serializable]
	public class ErrorResponse : BaseResponse
	{
		//

		public ErrorResponse(int statusCode, string reason) : base("ERROR", statusCode, reason)
		{
			//
		}
	}
}

using System;
using System.Text.Json.Serialization;

namespace Chess_Server.Templates.Response
{
	[Serializable]
	public class BaseResponse
	{
		[JsonInclude]
		public string command;
		[JsonInclude]
		public int statusCode;
		[JsonInclude]
		public string reason;

		[JsonConstructor]
		public BaseResponse(string command, int statusCode, string reason)
		{
			this.command = command;
			this.statusCode = statusCode;
			this.reason = reason;
		}
	}
}

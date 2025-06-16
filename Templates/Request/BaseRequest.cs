using System;
using System.Text.Json.Serialization;

namespace Chess_Server.Templates.Request
{
	[Serializable]
	public class BaseRequest
	{
		[JsonInclude]
		public string clientUid;
		
		[JsonInclude]
		public string command;

		[JsonConstructor]
		public BaseRequest(string clientUid, string command)
		{
			this.clientUid = clientUid;
			this.command = command;
		}
	}
}

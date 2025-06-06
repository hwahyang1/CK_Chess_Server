using System;
using System.Text.Json.Serialization;

namespace Chess_Server.Templates.Request
{
	[Serializable]
	public class BaseRequest
	{
		[JsonInclude]
		public string command;

		[JsonConstructor]
		public BaseRequest(string command)
		{
			this.command = command;
		}
	}
}
using System;
using System.Text.Json.Serialization;

namespace Chess_Server.Templates.Request
{
	[Serializable]
	public class UserNameRequest : BaseRequest
	{
		[JsonInclude]
		public string uid;

		[JsonConstructor]
		public UserNameRequest(string clientUid, string command, string uid) : base(clientUid, command)
		{
			this.uid = uid;
		}
	}
}

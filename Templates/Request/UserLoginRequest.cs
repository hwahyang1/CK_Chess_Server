using System;
using System.Text.Json.Serialization;

namespace Chess_Server.Templates.Request
{
	[Serializable]
	public class UserLoginRequest : BaseRequest
	{
		[JsonInclude]
		public string id;
		
		[JsonInclude]
		public string password;

		[JsonConstructor]
		public UserLoginRequest(string clientUid, string command, string id, string password) : base(clientUid, command)
		{
			this.id = id;
			this.password = password;
		}
	}
}

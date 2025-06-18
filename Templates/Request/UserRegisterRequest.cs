using System;
using System.Text.Json.Serialization;

namespace Chess_Server.Templates.Request
{
	[Serializable]
	public class UserRegisterRequest : BaseRequest
	{
		[JsonInclude]
		public string id;
		
		[JsonInclude]
		public string password;
		
		[JsonInclude]
		public string username;

		[JsonConstructor]
		public UserRegisterRequest(string clientUid, string command, string id, string password, string username) : base(clientUid, command)
		{
			this.id = id;
			this.password = password;
			this.username = username;
		}
	}
}

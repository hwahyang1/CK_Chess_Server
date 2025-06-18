using System;
using System.Text.Json.Serialization;

using Chess_Server.Templates.Internal;

namespace Chess_Server.Templates.Response
{
	[Serializable]
	public class UserLoginResponse : BaseResponse
	{
		[JsonInclude]
		public string uid;
		
		[JsonInclude]
		public string displayName;

		public UserLoginResponse(string clientUid, string command, int statusCode, string reason, string uid, string displayName) : base(clientUid, command, statusCode, reason)
		{
			this.uid = uid;
			this.displayName = displayName;
		}
	}
}

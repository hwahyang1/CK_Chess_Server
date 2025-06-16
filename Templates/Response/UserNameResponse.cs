using System;
using System.Text.Json.Serialization;

using Chess_Server.Templates.Internal;

namespace Chess_Server.Templates.Response
{
	[Serializable]
	public class UserNameResponse : BaseResponse
	{
		[JsonInclude]
		public string userName;

		public UserNameResponse(string clientUid, string command, int statusCode, string reason, string userName) : base(clientUid, command, statusCode, reason)
		{
			this.userName = userName;
		}
	}
}

using System;

using Chess_Server.Templates.Request;
using Chess_Server.Templates.Response;

namespace Chess_Server.Modules.Handlers
{
	public static class UserHandler
	{
		public static UserLoginResponse Login(UserLoginRequest request, string sourceIpv4, string command = "")
		{
			if (command == "") command = request.command;
			
			(bool isSuccess, string userUid) = UserManager.Login(request.id, request.password, sourceIpv4);
			string displayName = UserManager.GetUserName(userUid);

			if (!isSuccess) return new UserLoginResponse(request.clientUid, command, 401, "Unauthorized", "", "");
			
			return new UserLoginResponse(request.clientUid, command, 200, "OK", userUid, displayName);
		}
		
		public static UserRegisterResponse Register(UserRegisterRequest request, string sourceIpv4, string command = "")
		{
			if (command == "") command = request.command;
			
			(bool isSuccess, string userUid) = UserManager.Register(request.id, request.username, request.password, sourceIpv4);
			string displayName = UserManager.GetUserName(userUid);

			if (!isSuccess) return new UserRegisterResponse(request.clientUid, command, 401, "Unauthorized", "", "");
			
			return new UserRegisterResponse(request.clientUid, command, 200, "OK", userUid, displayName);
		}

		public static UserNameResponse GetUserName(UserNameRequest request, string command = "")
		{
			if (command == "") command = request.command;

			string userName = UserManager.GetUserName(request.uid);
			
			if (userName == "") return new UserNameResponse(request.clientUid, command, 404, "Not Found", "");
			
			return new UserNameResponse(request.clientUid, command, 200, "OK", userName);
		}
	}
}

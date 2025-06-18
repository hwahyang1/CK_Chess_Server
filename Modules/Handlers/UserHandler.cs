using System;

using Chess_Server.Templates.Request;
using Chess_Server.Templates.Response;

namespace Chess_Server.Modules.Handlers
{
	public static class UserHandler
	{
		private static readonly string CLIENT_UID_PLACEHOLDER = "<CLIENT_UID>";
		private static readonly string USER_KEY = CLIENT_UID_PLACEHOLDER + "_UserUid";
		
		private static readonly string USER_UID_PLACEHOLDER = "<USER_UID>";
		private static readonly string CLIENT_KEY = USER_UID_PLACEHOLDER + "_ClientUid";
		
		public static UserLoginResponse Login(UserLoginRequest request, string clientUid, string sourceIpv4, string command = "")
		{
			if (command == "") command = request.command;
			
			(bool isSuccess, string userUid) = UserManager.Login(request.id, request.password, sourceIpv4);
			string displayName = UserManager.GetUserName(userUid);

			if (!isSuccess) return new UserLoginResponse(request.clientUid, command, 401, "Unauthorized", "", "");
			
			string userKey = USER_KEY.Replace(CLIENT_UID_PLACEHOLDER, clientUid);
			string? existUserData = RedisManager.Instance.Get(userKey);
			string clientKey = CLIENT_KEY.Replace(USER_UID_PLACEHOLDER, userUid);
			string? existClientData = RedisManager.Instance.Get(clientKey);

			if (string.IsNullOrEmpty(existUserData) || string.IsNullOrEmpty(existClientData))
			{
				RedisManager.Instance.Set(userKey, userUid);
				RedisManager.Instance.Set(clientKey, clientUid);
			}
			else
			{
				return new UserLoginResponse(request.clientUid, command, 409, "Conflict", "", "");
			}
			
			return new UserLoginResponse(request.clientUid, command, 200, "OK", userUid, displayName);
		}
		
		public static UserRegisterResponse Register(UserRegisterRequest request, string clientUid, string sourceIpv4, string command = "")
		{
			if (command == "") command = request.command;
			
			(bool isSuccess, string userUid) = UserManager.Register(request.id, request.username, request.password, sourceIpv4);
			string displayName = UserManager.GetUserName(userUid);

			if (!isSuccess) return new UserRegisterResponse(request.clientUid, command, 401, "Unauthorized", "", "");
			
			string userKey = USER_KEY.Replace(CLIENT_UID_PLACEHOLDER, clientUid);
			string? existUserData = RedisManager.Instance.Get(userKey);
			string clientKey = CLIENT_KEY.Replace(USER_UID_PLACEHOLDER, userUid);
			string? existClientData = RedisManager.Instance.Get(clientKey);

			if (string.IsNullOrEmpty(existUserData) || string.IsNullOrEmpty(existClientData))
			{
				RedisManager.Instance.Set(userKey, userUid);
				RedisManager.Instance.Set(clientKey, clientUid);
			}
			else
			{
				return new UserRegisterResponse(request.clientUid, command, 409, "Conflict", "", "");
			}
			
			return new UserRegisterResponse(request.clientUid, command, 200, "OK", userUid, displayName);
		}

		public static void Logout(string clientUid)
		{
			string userKey = USER_KEY.Replace(CLIENT_UID_PLACEHOLDER, clientUid);
			string? userUid = RedisManager.Instance.Delete(userKey);
			
			string clientKey = CLIENT_KEY.Replace(USER_UID_PLACEHOLDER, userUid);
			RedisManager.Instance.Delete(clientKey);
		}

		public static UserNameResponse GetUserName(UserNameRequest request, string command = "")
		{
			if (command == "") command = request.command;

			string userName = UserManager.GetUserName(request.uid);
			
			if (userName == "") return new UserNameResponse(request.clientUid, command, 404, "Not Found", "");
			
			return new UserNameResponse(request.clientUid, command, 200, "OK", userName);
		}

		public static string? GetUserUidByClientUid(string clientUid)
		{
			string userKey = USER_KEY.Replace(CLIENT_UID_PLACEHOLDER, clientUid);
			return RedisManager.Instance.Get(userKey);
		}

		public static string? GetClientUidByUserUid(string userUid)
		{
			string clientKey = CLIENT_KEY.Replace(USER_UID_PLACEHOLDER, userUid);
			return RedisManager.Instance.Get(clientKey);
		}
	}
}

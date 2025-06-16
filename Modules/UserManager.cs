using System;
using System.Text;
using System.Data;
using System.Security.Cryptography;

namespace Chess_Server.Modules
{
	public static class UserManager
	{
		public static (bool isSuccess, string userUid) Login(string id, string password, string sourceIpv4)
		{
			DataTable existData =  MySqlManager.Instance.QueryDataTable("SELECT `uid`, `id`, `hash`, `salt` FROM `chess_users` WHERE `id` = @USERID AND `isActive` = TRUE",
			                                                            new Dictionary<string, object>() {
				                                                            {
					                                                            "@USERID", id
				                                                            }
			                                                            });

			if (existData.Rows.Count < 0) return (false, "");
			
			string hash = HashPassword(password + existData.Rows[0]["salt"].ToString());
			
			if (hash != existData.Rows[0]["hash"].ToString()) return (false, "");

			MySqlManager.Instance.Query("UPDATE `chess_users` SET `lastLoggedInAt`=NOW(),`lastLoggedInIpv4`=@LASTLOGGEDINIPV4 WHERE `uid` = @USERUID",
					                   new Dictionary<string, object>()
					                   {
						                   { "@LASTLOGGEDINIPV4", sourceIpv4 }
					                   });
			
			return (true, existData.Rows[0]["uid"].ToString());
		}

		public static (bool isSuccess, string userUid) Register(string id, string userName, string password, string sourceIpv4)
		{
			DataTable existData = MySqlManager.Instance.QueryDataTable("SELECT `uid` FROM `chess_users` WHERE `id` = @USERID",
								                                     new Dictionary<string, object>()
								                                     {
									                                     { "@USERID", id }
								                                     });

			if (existData.Rows.Count > 0) return (false, existData.Rows[0]["uid"].ToString());

			string userUid = GenerateUid("u_");
			string salt = GenerateRandomChars(Config.SALT_LENGTH);
			string hash = HashPassword(password + salt);

			MySqlManager.Instance.Query("INSERT INTO `chess_users`(`uid`, `id`, `displayName`, `hash`, `salt`, `isActive`, `lastLoggedInIpv4`) VALUES (@USERUID, @USERID, @DISPLAYNAME, @HASH, @SALT, TRUE, @LASTLOGGEDINIPV4)",
					                   new Dictionary<string, object>()
					                   {
						                   { "@USERUID", userUid },
						                   { "@USERID", id },
						                   { "@DISPLAYNAME", userName },
						                   { "@HASH", hash },
						                   { "@SALT", salt },
						                   { "@LASTLOGGEDINIPV4", sourceIpv4 },
					                   });
			
			return (true, userUid);
		}

		public static string GetUserName(string uid)
		{
			DataTable existData = MySqlManager.Instance.QueryDataTable("SELECT `uid` FROM `chess_users` WHERE `uid` = @USERUID",
			                                                            new Dictionary<string, object>() {
				                                                            {
					                                                            "@USERUID", uid
				                                                            }
			                                                            });

			if (existData.Rows.Count < 0) return "";
			
			return existData.Rows[0]["uid"].ToString();
		}

		#region Utils

		private static string HashPassword(string password)
		{
			if (password == "") return "";

			using (SHA512 sha = SHA512.Create())
			{
				byte[] bytes = Encoding.UTF8.GetBytes(password);
				byte[] hashBytes = sha.ComputeHash(bytes);

				StringBuilder sb = new StringBuilder(hashBytes.Length * 2);
				foreach (byte b in hashBytes)
				{
					sb.Append(b.ToString("x2"));
				}

				return sb.ToString();
			}
		}
		
		private static string GenerateUid(string prefix = "")
		{
			return prefix + Guid.NewGuid();
		}

		private static string GenerateRandomChars(int length = 2)
		{
			if (length <= 0) return string.Empty;

			const string pool = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
			StringBuilder result = new StringBuilder(length);

			using (RandomNumberGenerator rng = RandomNumberGenerator.Create())
			{
				Span<byte> bytes = stackalloc byte[length];
				rng.GetBytes(bytes);

				for (int i = 0; i < bytes.Length; i++)
				{
					result.Append(pool[i % pool.Length]);
				}
			}

			return result.ToString();
		}

		#endregion
	}
}

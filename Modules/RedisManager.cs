using System;

using StackExchange.Redis;

namespace Chess_Server.Modules
{
	public class RedisManager : Singleton<RedisManager>
	{
		private ConnectionMultiplexer? redis = null;

		RedisManager()
		{
			
		}
		
		public void Connect()
		{
			redis = ConnectionMultiplexer.Connect(Config.REDIS_CONFIG);
		}

		public void Dispose()
		{
			redis?.Dispose();
			redis = null;
		}

		public IDatabase? GetDatabase()
		{
			if (redis == null) return null;
			
			IDatabase db = redis.GetDatabase();
			return db;
		}

		public string? Get(string key)
		{
			string? value = GetDatabase()?.StringGet(key);
			return value;
		}

		public void Set(string key, string value)
		{
			GetDatabase()?.StringSet(key, value);
		}

		~RedisManager()
		{
			Dispose();
		}
	}
}

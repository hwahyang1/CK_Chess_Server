using System;
using System.Data;

using MySql.Data.MySqlClient;

namespace Chess_Server.Modules
{
	public class MySqlManager : Singleton<MySqlManager>
	{
		private static MySqlConnection? connection = null;

		MySqlManager()
		{
			//
		}

		/// <summary>
		/// Config 데이터를 기반으로 MySQL 서버에 접속합니다.
		/// </summary>
		public void Connect()
		{
			string connectionString = $"Server={Config.MYSQL_IP};Database={Config.MYSQL_DB};Uid={Config.MYSQL_ID};Pwd={Config.MYSQL_PW};";

			connection = new MySqlConnection(connectionString);

			try
			{
				connection.Open();
			}
			catch (Exception e)
			{
				throw new Exception($"Failed to connect to the server: {e.Message}");
			}
		}

		public void Disconnect()
		{
			connection?.Close();
			connection = null;
		}
		
		/// <summary>
		/// 값을 반환받지 않는 Query문(INSERT, UPDATE, ...)을 실행합니다.
		/// </summary>
		/// <param name="query">실행할 Query문을 입력합니다.</param>
		/// <param name="parameters">SQL Injection Attack을 방지하기 위해 Replace 할 목록을 지정합니다.</param>
		/// <returns>영향을 받은 Row의 수가 반환됩니다. (일반적으로, 0 = 작업 실패)</returns>
		/// <remarks>parameters와 관련한 내용은 다음 링크를 확인하세요: https://dev.mysql.com/doc/connectors/en/connector-net-programming-prepared.html</remarks>
		public int Query(string query, Dictionary<string, object>? parameters = null)
		{
			if (connection == null) return 0;
			
			MySqlCommand sqlCommand = new MySqlCommand(query, connection);
			if (parameters != null)
			{
				foreach (KeyValuePair<string, object> parameter in parameters)
				{
					sqlCommand.Parameters.AddWithValue(parameter.Key, parameter.Value);
				}
			}

			try
			{
				int result = sqlCommand.ExecuteNonQuery();
				return result;
			}
			catch (Exception e)
			{
				string parametersString = "";
				if (parameters != null)
				{
					foreach (KeyValuePair<string, object> parameter in parameters)
					{
						parametersString += $"{parameter.Key} - {parameter.Value}, ";
					}
				}

				throw new Exception($"Failed to execute command {query}({parametersString}): {e.Message}");
			}
		}

		/// <summary>
		/// 값을 반환받는 Query문(SELECT)을 실행합니다.
		/// </summary>
		/// <param name="query">실행할 Query문을 입력합니다.</param>
		/// <param name="parameters">SQL Injection Attack을 방지하기 위해 Replace 할 목록을 지정합니다.</param>
		/// <returns>서버로부터 반환된 데이터가 반환됩니다.</returns>
		/// <remarks>parameters와 관련한 내용은 다음 링크를 확인하세요: https://dev.mysql.com/doc/connectors/en/connector-net-programming-prepared.html</remarks>
		public DataTable? QueryDataTable(string query, Dictionary<string, object>? parameters = null)
		{
			if (connection == null) return null;
			
			MySqlCommand sqlCommand = new MySqlCommand(query, connection);
			if (parameters != null)
			{
				foreach (KeyValuePair<string, object> parameter in parameters)
				{
					sqlCommand.Parameters.AddWithValue(parameter.Key, parameter.Value);
				}
			}

			DataSet dataSet = new DataSet();
			MySqlDataAdapter adapter = new MySqlDataAdapter();

			adapter.SelectCommand = sqlCommand;
			try
			{
				adapter.Fill(dataSet);
			}
			catch (Exception e)
			{
				string parametersString = "";
				if (parameters != null)
				{
					foreach (KeyValuePair<string, object> parameter in parameters)
					{
						parametersString += $"{parameter.Key} - {parameter.Value}, ";
					}
				}

				throw new Exception($"Failed to execute command {query}({parametersString}): {e.Message}");
			}

			return dataSet.Tables[0];
		}

		~MySqlManager()
		{
			Disconnect();
		}
	}
}

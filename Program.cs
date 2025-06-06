using System;
using System.Net;
using System.Text;
using System.Text.Json;
using System.Net.Sockets;
using System.Collections.Concurrent;

using Chess_Server.Modules;

namespace Chess_Server
{
	public class Program
	{
		private static JsonSerializerOptions serializerOptions = new JsonSerializerOptions
		                                                         {
			                                                         IncludeFields = true,
			                                                         PropertyNameCaseInsensitive = true
		                                                         };
		
		private static TcpListener listener;
		private static ConcurrentDictionary<string, TcpClient> clientStreams = new ConcurrentDictionary<string, TcpClient>();
		
		private static readonly object uidLock = new object();
		private static int uidCounter = 0;
		
		static void Main(string[] args)
		{
			try
			{
				MySqlManager.Instance.Connect();
				Console.WriteLine("[Server] Server connected to MySQL.");
				
				RedisManager.Instance.Connect();
				Console.WriteLine("[Server] Server connected to Redis.");

				listener = new TcpListener(IPAddress.Any, Config.SERVER_PORT);
				listener.Start();
				Console.WriteLine("[Server] Server is listening on port {0}!", Config.SERVER_PORT);
				Console.WriteLine("[Server] Press Ctrl + C to exit.");

				while (true)
				{
					TcpClient client = listener.AcceptTcpClient();
					Console.WriteLine("[{0}] Client connected.", client.Client.RemoteEndPoint);

					Thread clientThread = new Thread(HandleClient);
					clientThread.Start(client);
				}
			}
			catch (Exception e)
			{
				Console.WriteLine(e.Message);
				Console.WriteLine(e.StackTrace);
			}
			finally
			{
				listener.Stop();
				MySqlManager.Instance.Disconnect();
				RedisManager.Instance.Dispose();
				Console.WriteLine("[Server] Server stopped.");
			}
		}
		
		private static void HandleClient(object obj)
		{
			TcpClient client = (TcpClient)obj;
			NetworkStream stream = client.GetStream();

			byte[] buffer = new byte[Config.BUFFER_SIZE];
			int byteCount;

			string uid = GenerateUid();
			clientStreams.TryAdd(uid, client);

			try
			{
				while ((byteCount = stream.Read(buffer, 0, buffer.Length)) != 0)
				{
					string data = Encoding.UTF8.GetString(buffer, 0, byteCount);
					Console.WriteLine("[{0}] Received: {1}", client.Client.RemoteEndPoint, data);

					string response = "DATA";
					byte[] responseBytes = Encoding.UTF8.GetBytes(response);
					stream.Write(responseBytes, 0, responseBytes.Length);
					Console.WriteLine("[{0}] Sent: {1}", client.Client.RemoteEndPoint, response);
				}
			}
			catch (Exception e)
			{
				Console.WriteLine("[{0}] Client Error: {1}", client.Client.RemoteEndPoint, e.Message);
			}
			finally
			{
				Console.WriteLine("[{0}] Client connection lost.", client.Client.RemoteEndPoint);
				
				stream.Close();
				client.Close();

				clientStreams.TryRemove(uid, out TcpClient _);
			}
		}

		/// <summary>
		/// 특정 클라이언트에 데이터를 전송합니다.
		/// </summary>
		/// <param name="uid">전송할 클라이언트의 uid를 지정합니다.</param>
		/// <param name="data">전송할 데이터를 지정합니다.</param>
		private static void SendData(string uid, string data)
		{
			TcpClient? client = null;
			NetworkStream? stream = null;
			try
			{
				if (!clientStreams.ContainsKey(uid)) return;
				client = clientStreams[uid];
				if (!IsClientConnected(client)) return;
				stream = client.GetStream();
				
				byte[] responseBytes = Encoding.UTF8.GetBytes(data);
				stream.Write(responseBytes, 0, responseBytes.Length);
				Console.WriteLine("[{0}] Sent: {1}", client.Client.RemoteEndPoint, data);
			}
			catch (Exception e)
			{
				Console.WriteLine("[{0}] Client Error: {1}", client?.Client.RemoteEndPoint?.ToString() ?? uid, e.Message);
			}
			finally
			{
				Console.WriteLine("[{0}] Client connection lost.", client?.Client.RemoteEndPoint?.ToString() ?? uid);
				
				stream?.Close();
				client?.Close();
			}
		}
		
		private static string GenerateUid()
		{
			lock (uidLock)
			{
				return "u_" + Interlocked.Increment(ref uidCounter).ToString("D8");
			}
		}
		
		private static bool IsClientConnected(TcpClient client)
		{
			try
			{
				if (client.Client.Poll(0, SelectMode.SelectRead) && client.Client.Available == 0)
				{
					return false;
				}
				return true;
			}
			catch
			{
				return false;
			}
		}
	}
}

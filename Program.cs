using System;
using System.Net;
using System.Text;
using System.Text.Json;
using System.Net.Sockets;

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
			}
		}
	}
}

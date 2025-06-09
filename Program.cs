using System;
using System.Net;
using System.Text;
using System.Text.Json;
using System.Net.Sockets;
using System.Collections.Concurrent;

using Chess_Server.Modules;
using Chess_Server.Modules.Handlers;
using Chess_Server.Templates.Request;
using Chess_Server.Templates.Response;
using Chess_Server.Templates.Internal;

namespace Chess_Server
{
	public class Program
	{
		private static readonly JsonSerializerOptions? SERIALIZER_OPTIONS = new JsonSerializerOptions
		                                                                    {
			                                                                    IncludeFields = true,
			                                                                    PropertyNameCaseInsensitive = true
		                                                                    };
		
		private static TcpListener? listener;
		private static readonly ConcurrentDictionary<string, TcpClient> clientStreams = new ConcurrentDictionary<string, TcpClient>();
		
		public static void Main(string[] args)
		{
			try
			{
				MySqlManager.Instance.Connect();
				RoomManager.DeleteAllRooms();
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
				listener?.Stop();
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

			string clientUid = GenerateUid("c_");
			clientStreams.TryAdd(clientUid, client);
			Console.WriteLine("[{0}] Client is assigned uid {1}.", client.Client.RemoteEndPoint, clientUid);
			
			BaseResponse uidResponse = new BaseResponse(clientUid, "ClientUid", 200, "OK");
			string uidResponseString = JsonSerializer.Serialize<BaseResponse>(uidResponse);
			byte[] uidResponseBytes = Encoding.UTF8.GetBytes(uidResponseString);
			stream.Write(uidResponseBytes, 0, uidResponseBytes.Length);
			Console.WriteLine("[{0}] Sent: {1}", client.Client.RemoteEndPoint, uidResponseString);

			try
			{
				while ((byteCount = stream.Read(buffer, 0, buffer.Length)) != 0)
				{
					string data = Encoding.UTF8.GetString(buffer, 0, byteCount);
					Console.WriteLine("[{0}] Received: {1}", client.Client.RemoteEndPoint, data);

					string response = HandleMessage(client, data, clientUid);
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
				
				// TODO: clientUid to userUid
				RoomData? room = RoomManager.GetRoomByPlayerId(clientUid);
				if (room != null) RoomManager.LeaveRoom(room.id, clientUid);
				
				stream.Close();
				client.Close();

				clientStreams.TryRemove(clientUid, out TcpClient _);
			}
		}
		
		private static string HandleMessage(TcpClient client, string rawMessage, string clientUid)
		{
			string response;
			
			try
			{
				BaseRequest? commandMessage = JsonSerializer.Deserialize<BaseRequest>(rawMessage, SERIALIZER_OPTIONS);
				if (commandMessage?.clientUid != clientUid)
				{
					response = JsonSerializer.Serialize<ErrorResponse>(new ErrorResponse(clientUid, 404, "Not Found"));
				}
				else
				{
					switch (commandMessage?.command.ToLower() ?? "-")
					{
						case "RoomLists":
							RoomListsRequest roomListsRequest = JsonSerializer.Deserialize<RoomListsRequest>(rawMessage, SERIALIZER_OPTIONS);
							response = JsonSerializer.Serialize<RoomListsResponse>(RoomHandler.GetRoomLists(roomListsRequest));
							break;
						case "RoomCreate":
							RoomCreateRequest roomCreateRequest = JsonSerializer.Deserialize<RoomCreateRequest>(rawMessage, SERIALIZER_OPTIONS);
							(RoomInfoResponse roomCreateResponse, RoomData[] roomCreateUserPreviousRoom) = RoomHandler.RoomCreate(roomCreateRequest, "RoomJoined");
							// TODO: Broadcast to previous Room
							response = JsonSerializer.Serialize<RoomInfoResponse>(roomCreateResponse);
							break;
						case "RoomJoin":
							RoomJoinRequest roomJoinRequest = JsonSerializer.Deserialize<RoomJoinRequest>(rawMessage, SERIALIZER_OPTIONS);
							(RoomInfoResponse roomJoinResponse, RoomData[] roomJoinUserPreviousRoom) = RoomHandler.RoomJoin(roomJoinRequest, "RoomJoined");
							// TODO: Broadcast to previous Room
							response = JsonSerializer.Serialize<RoomInfoResponse>(roomJoinResponse);
							break;
						case "RoomLeaveOrDelete":
							RoomLeaveOrDeleteRequest roomLeaveOrDeleteRequest = JsonSerializer.Deserialize<RoomLeaveOrDeleteRequest>(rawMessage, SERIALIZER_OPTIONS);
							response = JsonSerializer.Serialize<RoomLeaveOrDeleteResponse>(RoomHandler.LeaveOrDeleteRoom(roomLeaveOrDeleteRequest, "RoomLeave"));
							// TODO: Broadcast to Room
							break;
						case "GameReady":
							// TODO
							response = JsonSerializer.Serialize<ErrorResponse>(new ErrorResponse(clientUid, 404, "Not Found"));
							break;
						case "GameBoardInfo":
							GameBoardInfoRequest gameBoardInfoRequest = JsonSerializer.Deserialize<GameBoardInfoRequest>(rawMessage, SERIALIZER_OPTIONS);
							response = JsonSerializer.Serialize<GameBoardInfoResponse>(GameHandler.GetBoard(gameBoardInfoRequest));
							break;
						case "GamePieceAvailableMovements":
							GamePieceAvailableMovementsRequest gamePieceAvailableMovementsRequest = JsonSerializer.Deserialize<GamePieceAvailableMovementsRequest>(rawMessage, SERIALIZER_OPTIONS);
							response = JsonSerializer.Serialize<GamePieceAvailableMovementsResponse>(GameHandler.GetAvailableMovements(gamePieceAvailableMovementsRequest));
							break;
						case "GamePieceMove":
							GamePieceMoveRequest gamePieceMoveRequest = JsonSerializer.Deserialize<GamePieceMoveRequest>(rawMessage, SERIALIZER_OPTIONS);
							response = JsonSerializer.Serialize<GamePieceMoveResponse>(GameHandler.GamePieceMove(gamePieceMoveRequest));
							// TODO: Broadcast to Room
							break;
						default:
							response = JsonSerializer.Serialize<ErrorResponse>(new ErrorResponse(clientUid, 404, "Not Found"));
							break;
					}
				}
			}
			catch (Exception e)
			{
				Console.WriteLine("[{0}] {1}", client.Client.RemoteEndPoint, e.Message);
				response = JsonSerializer.Serialize<ErrorResponse>(new ErrorResponse(clientUid, 500, "Internal Server Error"));
			}
			
			return response;
		}

		#region Utils

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
				if (!IsClientConnected(client)) clientStreams.TryRemove(uid, out TcpClient _);
				stream = client.GetStream();
				
				byte[] responseBytes = Encoding.UTF8.GetBytes(data);
				stream.Write(responseBytes, 0, responseBytes.Length);
				Console.WriteLine("[{0}] Sent: {1}", client.Client.RemoteEndPoint, data);
			}
			catch (Exception e)
			{
				Console.WriteLine("[{0}] Client Error: {1}", client?.Client.RemoteEndPoint?.ToString() ?? uid, e.Message);
			}
		}
		
		private static string GenerateUid(string prefix = "")
		{
			return prefix + Guid.NewGuid();
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

		#endregion
	}
}

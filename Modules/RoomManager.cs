using System;
using System.Data;
using System.Text.Json;

using Chess_Server.Templates.Internal;

namespace Chess_Server.Modules
{
	public static class RoomManager
	{
		private static readonly JsonSerializerOptions? SERIALIZER_OPTIONS = new JsonSerializerOptions
		                                                                    {
			                                                                    IncludeFields = true,
			                                                                    PropertyNameCaseInsensitive = true
		                                                                    };
		
		/// <summary>
		/// 새로운 방을 생성합니다.
		/// </summary>
		/// <param name="roomName">방의 이름을 지정합니다.</param>
		/// <param name="roomOwner">방 소유자의 Uid를 지정합니다.</param>
		/// <returns>생성된 방의 Uid가 반환됩니다. 기존에 roomOwner가 소유중인 방이 있을 경우, 해당 방의 데이터 또한 반환합니다</returns>
		/// <remarks>기존에 roomOwner가 참여/소유중인 방이 있다면, 기존 방에서 탈퇴하거나 방을 제거합니다.</remarks>
		public static (string roomId, RoomData[] previousRoom) CreateRoom(string roomName, string roomOwner)
		{
			DataTable existData = MySqlManager.Instance.QueryDataTable("SELECT `id`, `owner`, `participants` FROM `chess_rooms` WHERE `owner` = @OWNERID OR JSON_CONTAINS(participants, JSON_QUOTE(@OWNERID));",
									                                     new Dictionary<string, object>()
									                                     {
										                                     { "@OWNERID", roomOwner }
									                                     });

			List<RoomData> previousRooms = new List<RoomData>();
			
			if (existData.Rows.Count > 0)
			{
				foreach (DataRow row in existData.Rows)
				{
					RoomData room = LeaveRoom(row["id"].ToString(), roomOwner);
					if (room == null) continue;
					previousRooms.Add(room);
				}
			}

			string roomId = GenerateUid("r_");
			MySqlManager.Instance.Query("INSERT INTO `chess_rooms`(`id`, `displayName`, `owner`, `participants`) VALUES (@ROOMID, @DISPLAYNAME, @OWNERID, '[]')",
			                            new Dictionary<string, object>()
			                            {
				                            { "@ROOMID", roomId },
				                            { "@DISPLAYNAME", roomName },
				                            { "@OWNERID", roomOwner }
			                            });

			return (roomId, previousRooms.ToArray());
		}

		/// <summary>
		/// 모든 방의 정보를 가져옵니다.
		/// </summary>
		public static RoomData[] GetRooms()
		{
			// TODO: Filter & Sorting
			DataTable rawData = MySqlManager.Instance.QueryDataTable("SELECT `id`, `displayName`, `owner`, `participants`, `isStarted` FROM `chess_rooms`");
			
			if (rawData.Rows.Count == 0) return [];
			
			List<RoomData> rooms = new List<RoomData>();
			foreach (DataRow row in rawData.Rows)
			{
				RoomData data = ConvertDataRowToRoomData(row);
				rooms.Add(data);
			}
			return rooms.ToArray();
		}

		/// <summary>
		/// 특정 방의 정보를 가져옵니다.
		/// </summary>
		/// <param name="roomId">찾을 방의 Uid를 지정합니다.</param>
		public static RoomData? GetRoomByRoomId(string roomId)
		{
			DataTable rawData = MySqlManager.Instance.QueryDataTable("SELECT `id`, `displayName`, `owner`, `participants`, `isStarted` FROM `chess_rooms` WHERE `id` = @ROOMID",
			                                                         new Dictionary<string, object>()
			                                                         {
				                                                         { "@ROOMID", roomId }
			                                                         });
			
			if (rawData.Rows.Count == 0) return null;

			return ConvertDataRowToRoomData(rawData.Rows[0]);
		}

		/// <summary>
		/// 특정 플레이어가 참여중인/소유한 방의 정보를 가져옵니다.
		/// </summary>
		/// <param name="playerId">찾을 플레이어의 Uid를 지정합니다.</param>
		public static RoomData? GetRoomByPlayerId(string playerId)
		{
			DataTable rawData = MySqlManager.Instance.QueryDataTable("SELECT `id`, `displayName`, `owner`, `participants`, `isStarted` FROM `chess_rooms` WHERE `owner` = @OWNERID OR JSON_CONTAINS(participants, JSON_QUOTE(@OWNERID));",
			                                                           new Dictionary<string, object>()
			                                                           {
				                                                           { "@OWNERID", playerId }
			                                                           });
			
			if (rawData.Rows.Count == 0) return null;
			
			return ConvertDataRowToRoomData(rawData.Rows[0]);
		}

		/// <summary>
		/// 기존 방에 참여합니다.
		/// </summary>
		/// <param name="roomId">방의 Uid를 지정합니다.</param>
		/// <param name="playerId">방 참여자의 Uid를 지정합니다.</param>
		/// <remarks>기존에 player가 참여/소유중인 방이 있다면, 기존 방에서 탈퇴하거나 방을 제거합니다.</remarks>
		public static RoomData[] JoinRoom(string roomId, string playerId)
		{
			DataTable existData = MySqlManager.Instance.QueryDataTable("SELECT `id`, `owner`, `participants` FROM `chess_rooms` WHERE `owner` = @OWNERID OR JSON_CONTAINS(participants, JSON_QUOTE(@OWNERID));",
			                                                           new Dictionary<string, object>()
			                                                           {
				                                                           { "@OWNERID", playerId }
			                                                           });

			List<RoomData> previousRooms = new List<RoomData>();
			
			if (existData.Rows.Count > 0)
			{
				foreach (DataRow row in existData.Rows)
				{
					RoomData room = LeaveRoom(row["id"].ToString(), playerId);
					if (room == null) continue;
					previousRooms.Add(room);
				}
			}
			
			RoomData targetRoom = GetRoomByRoomId(roomId);
			if (targetRoom == null) return [];
			
			List<string> participants = new List<string>(targetRoom.Participants);
			if (participants.Contains(playerId)) return [];

			participants.Add(playerId);
			string participantsString = JsonSerializer.Serialize(participants.ToArray());
					
			MySqlManager.Instance.Query("UPDATE `chess_rooms` SET `participants`=@DATA WHERE `id` = @ROOMID",
			                            new Dictionary<string, object>()
			                            {
				                            { "@DATA", participantsString },
				                            { "@ROOMID", roomId }
			                            });
			
			return previousRooms.ToArray();
		}

		/// <summary>
		/// 참여 중인 방에서 나갑니다.
		/// </summary>
		/// <param name="roomId">방의 Uid를 지정합니다.</param>
		/// <param name="playerId">방 참여자의 Uid를 지정합니다.</param>
		/// <remarks>해당 방의 데이터를 반환합니다.</remarks>
		/// <remarks>player가 방의 소유자라면 방을 제거합니다.</remarks>
		public static RoomData? LeaveRoom(string roomId, string playerId)
		{
			RoomData room = GetRoomByRoomId(roomId);
			if (room == null) return null;

			List<string> participants = new List<string>(room.Participants);
			if (!participants.Contains(playerId)) return null;
			
			if (room.OwnerId == playerId)
			{
				MySqlManager.Instance.Query("DELETE FROM `chess_rooms` WHERE `id` = @ROOMID",
				                            new Dictionary<string, object>()
				                            {
					                            { "@ROOMID", roomId }
				                            });
			}
			else
			{
				participants.Remove(playerId);
				string participantsString = JsonSerializer.Serialize(participants.ToArray());
						
				MySqlManager.Instance.Query("UPDATE `chess_rooms` SET `participants`=@DATA WHERE `id` = @ROOMID",
				                            new Dictionary<string, object>()
				                            {
					                            { "@DATA", participantsString },
					                            { "@ROOMID", roomId }
				                            });
			}
			return room;
		}

		public static void DeleteAllRooms()
		{
			MySqlManager.Instance.Query("DELETE FROM `chess_rooms`");
		}

		#region Utils

		private static RoomData ConvertDataRowToRoomData(DataRow row)
		{
			string[] participants = JsonSerializer.Deserialize<string[]>(row["participants"].ToString(), SERIALIZER_OPTIONS);
			RoomData data = new RoomData(row["id"].ToString(), row["displayName"].ToString(), row["owner"].ToString(), participants, Convert.ToBoolean(row["isStarted"].ToString()));
			return data;
		}
		
		private static string GenerateUid(string prefix = "")
		{
			return prefix + Guid.NewGuid();
		}

		#endregion
	}
}

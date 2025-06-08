using System;
using System.Text.Json.Serialization;

namespace Chess_Server.Templates.Internal
{
	[Serializable]
	public class RoomData
	{
		[JsonInclude]
		public string Id;
		
		[JsonInclude]
		public string DisplayName;
		
		[JsonInclude]
		public string OwnerId;
		
		[JsonInclude]
		public string[] Participants;
		
		[JsonInclude]
		public bool IsStarted;

		public RoomData(string id, string displayName, string ownerId, string[] participants, bool isStarted)
		{
			Id = id;
			DisplayName = displayName;
			OwnerId = ownerId;
			Participants = participants;
			IsStarted = isStarted;
		}
	}
}

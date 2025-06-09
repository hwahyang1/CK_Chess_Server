using System;
using System.Text.Json.Serialization;

namespace Chess_Server.Templates.Internal
{
	[Serializable]
	public class RoomData
	{
		[JsonInclude]
		public string id;
		
		[JsonInclude]
		public string displayName;
		
		[JsonInclude]
		public string ownerId;
		
		[JsonInclude]
		public string[] participants;
		
		[JsonInclude]
		public bool isStarted;

		public RoomData(string id, string displayName, string ownerId, string[] participants, bool isStarted)
		{
			this.id = id;
			this.displayName = displayName;
			this.ownerId = ownerId;
			this.participants = participants;
			this.isStarted = isStarted;
		}
	}
}

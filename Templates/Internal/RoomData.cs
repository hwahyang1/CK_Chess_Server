using System;

namespace Chess_Server.Templates.Internal
{
	public class RoomData
	{
		private readonly string id;
		public string Id => id;
		
		private readonly string displayName;
		public string DisplayName => displayName;
		
		private readonly string ownerId;
		public string OwnerId => ownerId;
		
		private readonly string[] participants;
		public string[] Participants => participants;
		
		private readonly bool isStarted;
		public bool IsStarted => isStarted;

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

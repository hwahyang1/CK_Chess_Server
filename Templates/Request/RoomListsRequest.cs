using System;
using System.Text.Json.Serialization;

namespace Chess_Server.Templates.Request
{
	[Serializable]
	public class RoomListsRequest : BaseRequest
	{
		// TODO: Filter & Sorting

		[JsonConstructor]
		public RoomListsRequest(string clientUid, string command) : base(clientUid, command)
		{
			//
		}
	}
}

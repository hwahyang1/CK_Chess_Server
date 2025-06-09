using System;
using System.Text.Json.Serialization;

namespace Chess_Server.Templates.Internal
{
	[Serializable]
	public struct Block
	{
		[JsonInclude]
		public DefinePieces piece;
		
		[JsonInclude]
		public DefineTeam team;

		public Block(DefinePieces piece = DefinePieces.None, DefineTeam team = DefineTeam.None)
		{
			this.piece = piece;
			this.team = team;
		}
		
		[JsonIgnore]
		public bool IsEmpty => piece == DefinePieces.None;
	}
}

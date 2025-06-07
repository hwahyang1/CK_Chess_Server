using System;

namespace Chess_Server.Templates
{
	public struct Block
	{
		private DefinePieces piece;
		public DefinePieces Piece => piece;
		
		private DefineTeam team;
		public DefineTeam Team => team;

		public Block(DefinePieces piece = DefinePieces.None, DefineTeam team = DefineTeam.None)
		{
			this.piece = piece;
			this.team = team;
		}
		
		public bool IsEmpty => piece == DefinePieces.None;
	}
}

using System;
namespace GenRPG
{
	public class SpecialTile
	{
		public Coordinate Posistion;
		public ConsoleColor TileColor;
		public char RepChar;
		public bool Mov;
		public int ID;
		public SpecialTile(Coordinate EPOS, int EID)
		{
			Posistion = EPOS;
		InvalidID:
			switch (EID)
			{
				//Rock
				case 1:
					TileColor = ConsoleColor.Gray;
					RepChar = '^';
					Mov = false;
					break;
				//Bush
				case 2:
					TileColor = ConsoleColor.Green;
					RepChar = '*';
					Mov = false;
					break;
				case 3:
					TileColor = ConsoleColor.Red;
					RepChar = '+';
					Mov = true;
					break;
				default:
					goto InvalidID;
			}
			ID = EID;
		}
	}
}

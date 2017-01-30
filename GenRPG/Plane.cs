using System;
using System.Collections.Generic;
namespace GenRPG
{
	public class Plane
	{
		public List<SpecialTile> SpecialTiles = new List<SpecialTile>();
		public bool isTown;
		public Coordinate Size;
		public int HealthPacksUsed;
		public Plane(List<SpecialTile> ESpecialTiles, bool EIsTown)
		{
			SpecialTiles = ESpecialTiles;
			isTown = EIsTown;
			Size = new Coordinate(Console.BufferWidth, Console.BufferHeight);
			HealthPacksUsed = 0;
		}
		public Plane(List<SpecialTile> ESpecialTiles, bool EIsTown, int EHPU, bool Loaded)
		{
			SpecialTiles = ESpecialTiles;
			isTown = EIsTown;
			Size = new Coordinate(Console.BufferWidth, Console.BufferHeight);
			HealthPacksUsed = EHPU;
		}
	}
}

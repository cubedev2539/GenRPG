using System;
namespace GenRPG
{
	class EntityInfo
	{
		public Coordinate EntityPosition;
		public Coordinate PreviousEntityPosition;
		public PlayerStats EntStats;
		bool isMono;
		Coordinate FetchCoordinate(int ID)
		{
			Coordinate Out;
			Type t = Type.GetType("Mono.Runtime");
			if (t != null)
				isMono = true;
			else
				isMono = false;
			switch (ID)
			{
				case 0:
					Out = new Coordinate(0, 0);
					break;
				case 1:
					if (isMono)
					{
						Out = new Coordinate(Console.BufferWidth - 1, Console.BufferHeight - 1);
					}
					else
					{
						Out = new Coordinate(Console.WindowWidth - 1, Console.WindowHeight - 1);
					}
					break;
				default:
					Out = new Coordinate(0, 0);
					break;
			}
			return Out;
		}
		public EntityInfo(PlayerStats CPlStats)
		{
			Random random = new Random(Guid.NewGuid().GetHashCode());
			EntityPosition = new Coordinate(random.Next(0, (FetchCoordinate(1).x)), random.Next(2, (FetchCoordinate(1).y - 2)));
			EntStats = new PlayerStats(random.Next((CPlStats.Stamina - 50), (CPlStats.Stamina + 10)), random.Next((CPlStats.Health - 50), (CPlStats.Health + 10)), random.Next((CPlStats.Agility - 50), (CPlStats.Agility + 10)), random.Next((CPlStats.Damage - 50), (CPlStats.Damage + 10)), random.Next((CPlStats.Accuracy - 50), (CPlStats.Accuracy + 10)), random.Next((CPlStats.PDefense - 50), (CPlStats.PDefense + 10)), random.Next((CPlStats.RDefense - 50), (CPlStats.RDefense + 10)), random.Next((CPlStats.MDefense - 50), (CPlStats.MDefense + 10)), random.Next((CPlStats.Charisma - 50), (CPlStats.Charisma + 10)), random.Next((CPlStats.Intelligence - 50), (CPlStats.Intelligence + 10)));
			PreviousEntityPosition = new Coordinate(0, 2);
		}
	}
}

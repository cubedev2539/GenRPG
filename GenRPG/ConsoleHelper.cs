using System;
namespace GenRPG
{
	public class ConsoleHelper
	{
		/// <summary>
		/// The coordinate value of the bottom-right corner.
		/// </summary>
		public static Coordinate ScreenCoordinate
		{
			get
			{
				bool isMono;
				Type t = Type.GetType("Mono.Runtime");
				if (t != null)
					isMono = true;
				else
					isMono = false;
				if (isMono)
				{
					return new Coordinate(Console.BufferWidth - 1, Console.BufferHeight - 1);
				}
				else
				{
					return new Coordinate(Console.WindowWidth - 1, Console.WindowHeight - 1);
				}
			}
		}
		public static void SetCursorPositionToCoordinate(Coordinate Pos)
		{
			Console.SetCursorPosition(Pos.x, Pos.y);
		}
	}
}

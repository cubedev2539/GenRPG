using System;
using System.Collections.Generic;
namespace GenRPG
{
	public class ConsoleWriter
	{
		public static void Write(string outText)
		{
			string ManipulatedText = outText;
			int ConsoleWidth;
			List<string> Lines = new List<string>();
			bool isSpace = false;
			Type t = Type.GetType("Mono.Runtime");
			if (t != null)
				ConsoleWidth = Console.BufferWidth - 1;
			else
				ConsoleWidth = Console.WindowWidth - 1;
			while (ManipulatedText.ToCharArray().Length > ConsoleWidth)
			{
				isSpace = false;
				for (int c = ConsoleWidth; isSpace == false; c--)
				{
					if (c <= 0)
					{
						Lines.Add(ManipulatedText);
						goto PrintString;
					}
					if (ManipulatedText.ToCharArray()[c] == ' ')
					{
						isSpace = true;
						Lines.Add(ManipulatedText.Remove(c));
						ManipulatedText = ManipulatedText.Remove(0, c);
					}
				}
			}
			if (ManipulatedText != string.Empty)
				Lines.Add(ManipulatedText);
			PrintString:
			foreach (string line in Lines)
				Console.WriteLine(line);
		}
	}
}

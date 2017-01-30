/*
The MIT License (MIT)

Copyright (c) 2016 GenRPG Developers

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
SOFTWARE.
*/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.IO;
using System.Reflection;

namespace GenRPG
{
	/// <summary>
	/// A variable to hold class names (used in saving, etc.)
	/// </summary>
	enum Class
	{
		Mage = 0, Gunner, Fighter, Swordsman, Marksman, Paladin, Healer, Assassin, Robot
	}

	/// <summary>
	/// A variable used to define character gender (used in saving, etc.)
	/// </summary>
	enum Gender
	{
		Male, Female
	}
	/// <summary>
	/// A small variable to hold any kind of coordinates used in the game
	/// </summary>
	public class Coordinate
	{
		public int x, y;

		public Coordinate(int PointX, int PointY)
		{
			x = PointX;
			y = PointY;
		}
	}
	/// <summary>
	/// A class used to hold stats for any player (or entity)
	/// </summary>
	class PlayerStats
	{
		public int Accuracy;
		public int Agility;
		public int Charisma;
		public int Damage;
		public int Health;
		public int Intelligence;
		public int MDefense;
		public int PDefense;
		public int RDefense;
		public int Stamina;
		public PlayerStats(int EStamina, int EHealth, int EAgility, int EDamage, int EAccuracy, int EPDefense, int ERDefense, int EMDefense, int ECharisma, int EIntelligence)
		{
			Stamina = EStamina;
			Health = EHealth;
			Agility = EAgility;
			Damage = EDamage;
			Accuracy = EAccuracy;
			PDefense = EPDefense;
			RDefense = ERDefense;
			MDefense = EMDefense;
			Charisma = ECharisma;
			Intelligence = EIntelligence;
		}
	}
	class WeaponStats
	{
		public string WeaponName;
		public double PDamage;
		public double MDamage;
		public double RDamage;
		public double Accuracy;
		public double Durability;
		public double CritChance;
		public double AtkSpeed;
		public double Cooldown;
		public double BlockedDamage;
		public int ID;
		public WeaponStats(string EWeaponName, double EPDamage, double EMDamage, double ERDamage, double EAccuracy, double EDurability, double ECritChance, double EAtkSpeed, double ECooldown, double EBlockedDamage, int EID)
		{
			WeaponName = EWeaponName;
			PDamage = EPDamage;
			MDamage = EMDamage;
			RDamage = ERDamage;
			Accuracy = EAccuracy;
			Durability = EDurability;
			CritChance = ECritChance;
			AtkSpeed = EAtkSpeed;
			Cooldown = ECooldown;
			BlockedDamage = EBlockedDamage;
			ID = EID;

		}
	}

	class Program
	{
		#if DEBUG
		static bool Stable = false;
		#else
		static bool Stable = true;
		#endif
		//Thread used to capture key press on title sequence
		static Thread TitleThreadObj;
		static ThreadStart TitleThreadStart;
		//Used in the title sequence
		static bool TitleKP = false;
		static Random TitleRand = new Random();
		//Thread used for logging
		static Thread LogThreadObj;
		static ThreadStart LogThreadStart;
		//Stores time used in log file
		static string LogTime;
		//Used to store line that is to be logged through log thread
		static string LogBuffer = string.Empty;
		static string PrevLog = string.Empty;
		//Used to store the data path, this cannot be changed during execution
		static readonly string DataDirPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "GenRPG");
		//Used to store the working directory, this holds (or should hold) every pre-execution or user modifiable config file. If installed (WIP) it should be the install directory
		static readonly string WorkingDir = Directory.GetCurrentDirectory();
		//File that holds all weapon stats (pre-exec)
		static readonly string WeaponsConf = Path.Combine(DataDirPath, "WeaponsList.conf");
		//The directory that holds logs
		static readonly string LogDir = Path.Combine(DataDirPath, "Log");
		//A variable used whenever data is enter from the console and needs to be stored temporarily
		static string StrIn;
		//A variable used to store all default weapon stats
		static List<WeaponStats> WeaponsDir = new List<WeaponStats>();
		//The name chosen by the player
		static string CharacterName;
		//The gender chosen by the player
		static Gender CharacterGender;
		//The class chosen by the character
		static Class CharacterClass;
		//The players current stats
		static PlayerStats CharacterStats;
		//A variable containing data for every map (plane) that has been loaded
		static Dictionary<Coordinate, Plane> PlaneList = new Dictionary<Coordinate, Plane>();
		//The current plane coord that the player is on
		static Coordinate PMCoordinate = new Coordinate(0, 0);
		//A generic random number generator
		static Random GenRand
		{
			get
			{
				return new Random(Guid.NewGuid().GetHashCode());
			}
		}
		//A switch that, if pulled, exits the game
		static bool ExitGame = false;
		//The players coordinates inside a plane (can be from 0,0 to 80,35)
		static Coordinate PlayerCoordinates = new Coordinate((int) ConsoleHelper.ScreenCoordinate.x/2, (int)ConsoleHelper.ScreenCoordinate.y/2);
		//Switched if the map has just been changed and its graphical elements need to be loaded onto the screen
		static bool LoadingMap = false;
		//The character's current HP level
		static int CharacterHP;
		//The character's current stamina level
		static int CharacterStamina;
		//The character's previous location (used to write over players previous location when moving)
		static Coordinate OldPlayerCoordinates = new Coordinate((int) (ConsoleHelper.ScreenCoordinate.x/2)+1, (int)(ConsoleHelper.ScreenCoordinate.y/2)+1);
		//The character's level
		static int CharacterLevel = 1;
		//The charcter's earned XP points
		static int XP = 0;
		//The list of entities on the map
		static List<EntityInfo> EntityList = new List<EntityInfo>();
		//A switch tripped if the player tries to walk on a special tile that has Mov set to false
		static bool NoMove = false;
		//A switch tripped if a battle has started
		static bool BATTLE = false;
		//The stats of your enemy in a battle
		static PlayerStats EnemyStats;
		//The HP of your enemy in battle
		static int EnemyHP;
		//The stamina of your enemy
		static int EnemyStamina;
		//The inventory of the player
		static List<WeaponStats> Inventory = new List<WeaponStats>();
		//Tripped if running under Mono
		static bool isMono;
		//Tripped if the user acceses the hidden file flush system
		static bool FlushData = false;
		//Tripped if entity generation is finished
		//Number of towns generated
		static int Towns = 1;
		//Temporary entity list (used by realtime entity generation)
		static List<EntityInfo> TempEntityList = new List<EntityInfo>();
		//Tripped if windows size change
		static bool SizeChange = false;
		/// <summary>
		/// Return the PlayerStats value corresponding to the default stats for that class 
		/// </summary>
		/// <param name="SelClass">The class to get stats for</param>
		/// <returns></returns>
		static PlayerStats GetClassStats(Class SelClass)
		{
			switch (SelClass)
			{
			case Class.Mage:
				return new PlayerStats(100, 150, 105, 110, 85, 105, 95, 85, 110, 125);
			case Class.Gunner:
				return new PlayerStats(100, 145, 80, 125, 90, 85, 100, 90, 105, 100);
			case Class.Fighter:
				return new PlayerStats(100, 135, 120, 120, 80, 90, 80, 95, 95, 95);
			case Class.Swordsman:
				return new PlayerStats(100, 175, 85, 135, 75, 100, 90, 110, 115, 105);
			case Class.Marksman:
				return new PlayerStats(100, 150, 100, 110, 85, 85, 95, 90, 105, 115);
			case Class.Paladin:
				return new PlayerStats(100, 200, 75, 85, 80, 150, 125, 175, 100, 105);
			case Class.Healer:
				return new PlayerStats(100, 145, 105, 80, 85, 105, 95, 90, 125, 120);
			case Class.Assassin:
				return new PlayerStats(100, 120, 130, 130, 80, 85, 80, 95, 85, 110);
			case Class.Robot:
				return new PlayerStats(100, 180, 115, 120, 85, 125, 120, 110, 80, 130);

			}
			return new PlayerStats(0, 0, 0, 0, 0, 0, 0, 0, 0, 0);
		}
		static void InitializeWeaponsDirectory()
		{
			string[] WeaponFileStore = File.ReadAllLines(WeaponsConf);
			foreach (string weaponstat in WeaponFileStore)
			{
				int id = Convert.ToInt32(weaponstat.Split('=')[0]);
				string[] Strstats = weaponstat.Split('=')[1].Split('-');
				string name = Strstats[0];
				List<double> stats = new List<double>();
				foreach (string onestat in Strstats)
				{
					if (onestat != name)
					{
						double tmp = Convert.ToDouble(onestat);
						stats.Add(tmp);
					}
				}
				WeaponsDir.Add(new WeaponStats(name, stats[0], stats[1], stats[2], stats[3], stats[4], stats[5], stats[6], stats[7], stats[8], id));
			}
		}
		/// <summary>
		/// Log the entered log string (LogStr)
		/// </summary>
		/// <param name="LogStr">Log string</param>
		static void Log(string LogStr)
		{
			LogBuffer = LogStr;
		}
		/// <summary>
		/// Background thread used to log events to a file
		/// </summary>
		static void LogThread()
		{
			Console.WriteLine("Logging Thread started");
			using (StreamWriter logwriter = new StreamWriter(Path.Combine(LogDir, LogTime)))
			{
				logwriter.AutoFlush = true;
				while (!ExitGame)
				{
					if (LogBuffer != PrevLog)
					{
						logwriter.Write("[{0}] {1}\n", DateTime.Now.ToString("h:mm:ss"), LogBuffer);
						PrevLog = LogBuffer;
					}
				}
                logwriter.Dispose();
			}
		}
		/// <summary>
		/// The thread used to detect a keypress on the title sequence and end the animation
		/// </summary>
		static void TitleThread()
		{
			ConsoleKey InKey;
		TBegin:
			InKey = Console.ReadKey(true).Key;
			if (InKey == ConsoleKey.Enter)
			{
				//Trips switch to end title sequence
				TitleKP = true;
				//Stops the thread
				TitleThreadObj.Abort();
			}
			else if (InKey == ConsoleKey.X)
			{
				//Stops log thread
				ExitGame = true;
				//Trips the file flush bool so the program can exit
				FlushData = true;
				//Trips switch to end title sequence
				TitleKP = true;
				//Stops the thread
				TitleThreadObj.Abort();
			}
			else 
			{
				goto TBegin;
			}
		}
		#region Game
		static void NewMap()
		{
			int RA = GenRand.Next(0, 39+Towns);
			int RB = GenRand.Next(0, 39+Towns);
			if(RA != RB)
			{
				List<SpecialTile> TempTileList = new List<SpecialTile>();
				int SpecialTileNumber = GenRand.Next (7+((int)(ConsoleHelper.ScreenCoordinate.x*ConsoleHelper.ScreenCoordinate.y)/24), 20+((int)(ConsoleHelper.ScreenCoordinate.x*ConsoleHelper.ScreenCoordinate.y)/20));
				while (SpecialTileNumber != 0)
				{
					if (GenRand.Next (0, 1000) == 10) {
						TempTileList.Add (new SpecialTile (new Coordinate (GenRand.Next (0, (ConsoleHelper.ScreenCoordinate.x)), GenRand.Next (2, (ConsoleHelper.ScreenCoordinate.y - 2))), 3));
					} else {
						TempTileList.Add(new SpecialTile(new Coordinate(GenRand.Next(0, (ConsoleHelper.ScreenCoordinate.x)), GenRand.Next(2, (ConsoleHelper.ScreenCoordinate.y-2))), GenRand.Next(1, 3)));
					}
					SpecialTileNumber--;
				}
				PlaneList.Add(PMCoordinate, new Plane(TempTileList, false));
			}
			else
			{
				PlaneList.Add(PMCoordinate, new Plane(new List<SpecialTile>(),true));
				Towns++;
			}
			LoadingMap = true;
		}
		static void RWMap(int HealthPacksUsedPrev, bool ExistTown) 
		{
			PlaneList.Remove (PMCoordinate);
			List<SpecialTile> TempTileList = new List<SpecialTile> ();
			int SpecialTileNumber = GenRand.Next (7+((int)(ConsoleHelper.ScreenCoordinate.x*ConsoleHelper.ScreenCoordinate.y)/24), 20+((int)(ConsoleHelper.ScreenCoordinate.x*ConsoleHelper.ScreenCoordinate.y)/20));
			while (SpecialTileNumber != 0) {
				if (GenRand.Next (0, 1000 + (100 * HealthPacksUsedPrev)) == 10) {
					TempTileList.Add (new SpecialTile (new Coordinate (GenRand.Next (0, (ConsoleHelper.ScreenCoordinate.x)), GenRand.Next (2, (ConsoleHelper.ScreenCoordinate.y - 2))), 3));
				} else {
					TempTileList.Add (new SpecialTile (new Coordinate (GenRand.Next (0, (ConsoleHelper.ScreenCoordinate.x)), GenRand.Next (2, (ConsoleHelper.ScreenCoordinate.y - 2))), GenRand.Next (1, 3)));
				}
				SpecialTileNumber--;
			}
			PlaneList.Add (PMCoordinate, new Plane (TempTileList, ExistTown));
			LoadingMap = true;
		}
		static void Main(string[] args)
		{
			Type t = Type.GetType ("Mono.Runtime");
			if (t != null)
				isMono = true;
			else
				isMono = false;
			if (!isMono) {
				Console.WriteLine ("You are not using Mono, assuming you are using Microsoft.NET...");
			} else {
				Console.WriteLine ("You are using Mono.");
			}
			if (!Directory.Exists (Environment.GetFolderPath (Environment.SpecialFolder.ApplicationData))) {
				Directory.CreateDirectory (Environment.GetFolderPath (Environment.SpecialFolder.ApplicationData));
			}
			//Pre-emptivley grab time for the writing of log file
			LogTime = string.Format (@"Log {0}.log", DateTime.Now.ToString ("MM-dd-yyyy h.mm"));
			//Assign values to thread objects
			LogThreadStart = new ThreadStart (LogThread);
			LogThreadObj = new Thread (LogThreadStart);
			TitleThreadStart = new ThreadStart (TitleThread);
			TitleThreadObj = new Thread (TitleThreadStart);
			//Checks for logging directory
			if (!Directory.Exists (LogDir)) {
				Directory.CreateDirectory (LogDir);
			}
			//Starts logging thread to ensure that data exists for debugging
			LogThreadObj.Start ();
			Log ("Logging thread started!");
			if (!isMono) {
				Log ("Not running on Mono, assuming settings used for Microsoft.NET");
			} else {
				Log ("Running on Mono, disabling Windows-only methods");
			}
			//Checks for the data storage directory, if it does not exist, it is created
			Log ("Checking for data directory");
			if (!Directory.Exists (DataDirPath)) {
				Log ("Data directory not found, creating it now...");
				Directory.CreateDirectory (DataDirPath);
			}
			//Checks for weapons config
			Log ("Checking for pre-configured weapons file");
			if (!File.Exists (WeaponsConf)) {
				Log ("Weapons file not found, creating default one...");
				using (StreamWriter weapconwriter = new StreamWriter (WeaponsConf)) {
					weapconwriter.WriteLine ("1=Staff-0-120-0-80-0-6.5-100-0-0");
					weapconwriter.WriteLine ("2=Pistol-0-0-125-85-0-7.5-150-0-0");
					weapconwriter.WriteLine ("3=Steel Gloves-120-0-0-80-0-8-130-0-0");
					weapconwriter.WriteLine ("4=Rusty Sword-130-0-0-75-0-7.5-90-0-0");
					weapconwriter.WriteLine ("5=Bow-0-0-130-80-0-7-95-0-0");
					weapconwriter.WriteLine ("6=Shield-0-0-0-100-0-0-0-3-5");
					weapconwriter.WriteLine ("7=Infected Needle-90-15-0-90-0-6-105-0-0");
					weapconwriter.WriteLine ("8=Old Medkit-0-0-0-100-0-0-0-2-0");
					weapconwriter.WriteLine ("9=Shiv-115-0-0-80-0-10-145-0-0");
					weapconwriter.WriteLine ("10=Laser Extension I-0-130-0-85-0-7-110-0-0");
				}
			}
			InitializeWeaponsDirectory ();
			//Title sequence
			Log ("Running title sequence");
			TitleThreadObj.Start ();
			Random TitleColorGenerator = new Random(Guid.NewGuid().GetHashCode());
			while (!TitleKP) {
				Console.ForegroundColor = (ConsoleColor)TitleColorGenerator.Next(1, 15);
				Console.SetCursorPosition (TitleRand.Next (0, ConsoleHelper.ScreenCoordinate.x), TitleRand.Next (0, ConsoleHelper.ScreenCoordinate.y));
				Console.WriteLine ("GenRPG");
				Thread.Sleep(1);
			}
			if (FlushData) {
				Console.Clear ();
                //Deletes the temporary data directory
                Directory.Delete(DataDirPath, true);
                Console.WriteLine ("Data files flushed!");
				Console.ReadKey ();
				goto Exit;
			}
			Console.Clear ();
			Console.ForegroundColor = ConsoleColor.White;
			Console.WriteLine ("GENRPG");
			Console.WriteLine ("VERSION {0}", typeof(Program).Assembly.GetName().Version);
			Console.WriteLine ("Press any key to continue...");
			Console.ReadKey (true);
			Console.Clear ();
			Console.WriteLine("1. Start Game");
			Console.WriteLine("2. Exit");
		InvalidKey_mainmenu:
			switch (Console.ReadKey(true).KeyChar)
			{
				case '1':
					break;
				case '2':
					return;
				default:
					goto InvalidKey_mainmenu;
			}
			Console.Clear();
			//Ask for the name of the charater being created
			Console.WriteLine ("Enter your character's name:");
			DeclineName:
			StrIn = Console.ReadLine ();
			//Confirms the name, the player can chage their mind and re-enter the name
			Console.WriteLine ("Your character's name will be: {0}? (y,n)", StrIn);
        InvalidKey_nameentry:
            switch (Console.ReadKey(true).KeyChar)
            {
                case 'y':
                    {
                        Log("Character name confirmed");
                        Console.WriteLine("Character name is {0}", StrIn);
                        CharacterName = StrIn;
                        break;
                    }
                case 'n':
                    {
                        Log("Character name declined, asking again");
                        Console.WriteLine("Name declined by user, enter a new name:");
                        goto DeclineName;
                    }
                default:
                    {
                        goto InvalidKey_nameentry;
                    }
            }
            Thread.Sleep(1200);
            Console.Clear();
			//Asks the player to enter the desired gender for the in-game character
			Console.WriteLine ("What is your character's gender? (m (Male), f (Female))");
            InvalidKey_genderentry:
            switch (Console.ReadKey (true).KeyChar) {
			case 'm':
				{
					CharacterGender = Gender.Male;
					Log ("Character gender set to male");
					Console.WriteLine ("Character gender set to male!");
					Thread.Sleep (1000);
					break;
				}
			case 'f':
				{
					CharacterGender = Gender.Female;
					Log ("Character gender set to female!");
					Console.WriteLine ("Character gender set to female!");
					Thread.Sleep (1000);
					break;
				}
			default:
				{
					goto InvalidKey_genderentry;
				}
			}
			//Asks the player what they they would like to play
			RechooseClass:
			Console.Clear ();
			Console.WriteLine ("Assign your character a class (press the key correspoding to an option below):");
			Console.WriteLine ("1. Mage");
			Console.WriteLine ("2. Gunner");
			Console.WriteLine ("3. Fighter");
			Console.WriteLine ("4. Swordsman");
			Console.WriteLine ("5. Marksman");
			Console.WriteLine ("6. Paladin");
			Console.WriteLine ("7. Healer");
			Console.WriteLine ("8. Assassin");
			Console.WriteLine ("9. Robot");
			WrongKeyClass:
			switch (Console.ReadKey (true).KeyChar) {
			case '1':
				{
					Console.WriteLine ("An intelligent wizard that uses powerful magic as a weapon. \nYour weapon is a crystal staff laced with powerful magic.");
					Console.WriteLine ("Do you want to choose this class? (y,n)");
					WrongKeyConf:
					switch (Console.ReadKey (true).KeyChar) {
					case 'y':
						CharacterClass = Class.Mage;
						CharacterStats = GetClassStats (Class.Mage);
						Inventory.Add (WeaponsDir [0]);
						break;
					case 'n':
						goto RechooseClass;
					default:
						goto WrongKeyConf;
					}
					break;
				}
			case '2':
				{
					Console.WriteLine ("A strong-willed hero that uses guns to obliterate enemies. \nYour weapon is a 20 mm caliber pistol with the power of infinte ammo.");
					Console.WriteLine ("Do you want to choose this class? (y,n)");
					WrongKeyConf:
					switch (Console.ReadKey (true).KeyChar) {
					case 'y':
						CharacterClass = Class.Gunner;
						CharacterStats = GetClassStats (Class.Gunner);
						Inventory.Add (WeaponsDir [1]);
						break;
					case 'n':
						goto RechooseClass;
					default:
						goto WrongKeyConf;
					}

					break;
				}
			case '3':
				{
					Console.WriteLine ("An uncontrollable boxer that uses his fists as a powerful source of damage. \nYour weapons are steel gloves, the strongest you've ever had.");
					Console.WriteLine ("Do you want to choose this class? (y,n)");
					WrongKeyConf:
					switch (Console.ReadKey (true).KeyChar) {
					case 'y':
						CharacterClass = Class.Fighter;
						CharacterStats = GetClassStats (Class.Fighter);
						Inventory.Add (WeaponsDir [2]);
						break;
					case 'n':
						goto RechooseClass;
					default:
						goto WrongKeyConf;
					}

					break;
				}
			case '4':
				{
					Console.WriteLine ("A muscular person that wields a sharp sword to slash at enemies. \nYour weapon is a strong metal sword crafted with the thoughts of murder in mind.");
					Console.WriteLine ("Do you want to choose this class? (y,n)");
					WrongKeyConf:
					switch (Console.ReadKey (true).KeyChar) {
					case 'y':
						CharacterClass = Class.Swordsman;
						CharacterStats = GetClassStats (Class.Swordsman);
						Inventory.Add (WeaponsDir [3]);
						break;
					case 'n':
						goto RechooseClass;
					default:
						goto WrongKeyConf;
					}

					break;
				}
			case '5':
				{
					Console.WriteLine ("A skilled marksman that has exact aim (kind of) and a powerful weapon. \nYour weapon is a hand-crafted birchwood bow with sharp, metal arrows.");
					Console.WriteLine ("Do you want to choose this class? (y,n)");
					WrongKeyConf:
					switch (Console.ReadKey (true).KeyChar) {
					case 'y':
						CharacterClass = Class.Marksman;
						CharacterStats = GetClassStats (Class.Marksman);
						Inventory.Add (WeaponsDir [4]);
						break;
					case 'n':
						goto RechooseClass;
					default:
						goto WrongKeyConf;
					}

					break;
				}
			case '6':
				{
					Console.WriteLine ("A defensive tanky class that uses a rusty sword and mighty shield. \nYour weapon is a dull sword, but your shield is the best part of your kit. It blocks most incoming damage with its powerful metal base.");
					Console.WriteLine ("Do you want to choose this class? (y,n)");
					WrongKeyConf:
					switch (Console.ReadKey (true).KeyChar) {
					case 'y':
						CharacterClass = Class.Paladin;
						CharacterStats = GetClassStats (Class.Paladin);
						Inventory.Add (WeaponsDir [3]);
						Inventory.Add (WeaponsDir [5]);
						break;
					case 'n':
						goto RechooseClass;
					default:
						goto WrongKeyConf;
					}

					break;
				}
			case '7':
				{
					Console.WriteLine ("A doctor that has a Ph. D in heal. \nYour weapon is a infected needle. You also have a Medkit to heal yourself with.");
					Console.WriteLine ("Do you want to choose this class? (y,n)");
					WrongKeyConf:
					switch (Console.ReadKey (true).KeyChar) {
					case 'y':
						CharacterClass = Class.Healer;
						CharacterStats = GetClassStats (Class.Healer);
						Inventory.Add (WeaponsDir [6]);
						Inventory.Add (WeaponsDir [7]);
						break;
					case 'n':
						goto RechooseClass;
					default:
						goto WrongKeyConf;
					}

					break;
				}
			case '8':
				{
					Console.WriteLine ("A stealthy class that uses shivs and knives to surprise-attack people. \nYour weapon is a shiv that is crafted to have a high critical strike chance.");
					Console.WriteLine ("Do you want to choose this class? (y,n)");
					WrongKeyConf:
					switch (Console.ReadKey (true).KeyChar) {
					case 'y':
						CharacterClass = Class.Assassin;
						CharacterStats = GetClassStats (Class.Assassin);
						Inventory.Add (WeaponsDir [8]);
						break;
					case 'n':
						goto RechooseClass;
					default:
						goto WrongKeyConf;
					}

					break;
				}
			case '9':
				{
					Console.WriteLine ("An 'intelligent' (artificial, of course) man-made device that blasts enemies \nwith powerful lasers. \nYour weapon is an extention containing information about how to use a powerful \nplasma laser.");
					Console.WriteLine ("Do you want to choose this class? (y,n)");
					WrongKeyConf:
					switch (Console.ReadKey (true).KeyChar) {
					case 'y':
						CharacterClass = Class.Robot;
						CharacterStats = GetClassStats (Class.Robot);
						Inventory.Add (WeaponsDir [9]);
						break;
					case 'n':
						goto RechooseClass;
					default:
						goto WrongKeyConf;
					}

					break;
				}
			default:
				goto WrongKeyClass;
			}
			CharacterHP = CharacterStats.Health;
			CharacterStamina = CharacterStats.Stamina;
			//Preface
			Console.Clear ();
			Console.ForegroundColor = ConsoleColor.Green;
			Console.WriteLine ("Use WASD to move!");
			Console.WriteLine ("Press any key to start!");
			Console.ReadKey (true);
			Console.BackgroundColor = ConsoleColor.Black;
			Console.Clear ();
			Console.WriteLine ("CHAPTER I: IN A LAND OF GREEN");
			Console.WriteLine ("Preface:");
			ConsoleWriter.Write("This chapter is located in the land of Ironcrest, a land that you have lived in all your life. Being an orphan (your parents were killed by the shadows of Zerion, a soul-eating demon who possesses an army of shadows. His army of shadows controls 90% of the known planet, and these shadows are occupying your homeland. You have built a plucidium wall around your house, the only known material that can repel shadows), you must go on a quest to get revenge on Zerion for killing the only two people on the planet that you have ever loved. You've been trained by Master Milo to counter the arts of the shadow race. Master Milo was brutally murdered by Zerion, and it is up to you to defeat him, since you are the last known being in the universe that can counter shadows.");
			Console.Write("\n\n"); 
			ConsoleWriter.Write($"You, {CharacterName}, must journey to the depths of Gargon, a fiery wasteland that's full of shadows of all sizes, shapes, and brutality, where Zerion is wating for you...");
			Console.ReadKey (true);
			Console.ForegroundColor = ConsoleColor.DarkGreen;
			Console.Clear ();
			Console.ForegroundColor = ConsoleColor.White;
			Console.SetCursorPosition (0, 2);
			int WriteCounter;
			WriteCounter = Console.BufferWidth;
			while (WriteCounter >= 0) {
				Console.Write ('-');
				WriteCounter--;
			}
			Console.SetCursorPosition (0, (ConsoleHelper.ScreenCoordinate.y - 1));
			WriteCounter = Console.BufferWidth;
			while (WriteCounter >= 0) {
				Console.Write ('-');
				WriteCounter--;
			}
			Console.SetCursorPosition (0, 1);
			Console.Write ("{0}  LVL:{1}", CharacterName, CharacterLevel);
			Console.SetCursorPosition (0, (ConsoleHelper.ScreenCoordinate.y));
			Console.Write ("HP:{0}/{1}  Stamina:{2}/{3}", CharacterHP, CharacterStats.Health, CharacterStamina, CharacterStats.Stamina);
			while (!ExitGame) {
				SizeChange:
				if (!PlaneList.ContainsKey (PMCoordinate)) {
					NewMap();
				} else {
					if(PlaneList[PMCoordinate].Size.x != Console.BufferWidth || PlaneList[PMCoordinate].Size.y != Console.BufferHeight)
						RWMap(PlaneList[PMCoordinate].HealthPacksUsed, PlaneList[PMCoordinate].isTown);
				}
				if (SizeChange) {
					PlayerCoordinates = new Coordinate ((int)ConsoleHelper.ScreenCoordinate.x / 2, (int)ConsoleHelper.ScreenCoordinate.y / 2);
					OldPlayerCoordinates = new Coordinate ((int)(ConsoleHelper.ScreenCoordinate.x / 2) + 1, (int)(ConsoleHelper.ScreenCoordinate.y / 2) + 1);
				}
				if (PlaneList [PMCoordinate].isTown || (PMCoordinate.x == 0) & (PMCoordinate.y == 0)) {
					EndActionTown:
					Console.Clear();
					Console.BackgroundColor = ConsoleColor.Black;
					Console.SetCursorPosition(0, 1);
					WriteCounter = Console.BufferWidth-1;
					while (WriteCounter >= 0) {
						Console.Write ('-');
						WriteCounter--;
					}
					Console.SetCursorPosition(0, (ConsoleHelper.ScreenCoordinate.y-1));
					WriteCounter = Console.BufferWidth-1;
					while (WriteCounter >= 0) {
						Console.Write ('-');
						WriteCounter--;
					}
					Console.SetCursorPosition(0, 0);
					Console.Write("{0}  LVL:{1}",CharacterName, CharacterLevel);
					Console.SetCursorPosition(0, (ConsoleHelper.ScreenCoordinate.y));
					Console.Write("HP:{0}/{1}  Stamina:{2}/{3}", CharacterHP, CharacterStats.Health, CharacterStamina, CharacterStats.Stamina);
					Console.SetCursorPosition (0, 2);
					Console.WriteLine("You have reached a place of civilization, not yet touched by darkness...");
					Console.WriteLine("What would you like to do here?");
					Console.WriteLine("1. Rest in an inn...");
					Console.WriteLine("2. Leave the town");
					WrongKeyTown:
					switch(Console.ReadKey(true).Key)
					{
					case ConsoleKey.D1:
						CharacterHP = CharacterStats.Health;
						CharacterStamina = CharacterStats.Stamina;
						Console.WriteLine("You feel well rested, your health and stamina have been restored to full");
						Console.ReadKey(true);
						goto EndActionTown;
					case ConsoleKey.D2:
						Console.Clear();
						Console.WriteLine("Press the arrow key corresponding to the driection you want to go...");
						switch(Console.ReadKey(true).Key)
						{
						case ConsoleKey.UpArrow:
							PMCoordinate.y++;
							break;
						case ConsoleKey.DownArrow:
							PMCoordinate.y--;
							break;
						case ConsoleKey.LeftArrow:
							PMCoordinate.x++;
							break;
						case ConsoleKey.RightArrow:
							PMCoordinate.x--;
							break;
                        case ConsoleKey.W:
                            PMCoordinate.y++;
                            break;
                        case ConsoleKey.S:
                            PMCoordinate.y--;
                            break;
                        case ConsoleKey.D:
                            PMCoordinate.x++;
                            break;
                        case ConsoleKey.A:
                            PMCoordinate.x--;
                            break;
                        }
						Console.Clear();
						break;
                        case ConsoleKey.Escape:
                            ExitGame = true;
                            goto Exit;
					default:
						goto WrongKeyTown;
					}
					LoadingMap = true;
				} else {
					do {
					StartMov:
						Console.Clear();
						if (LoadingMap)
						{
							Console.SetCursorPosition(0, 2);
							Console.Write("Loading Map...");
						}
						if (PlaneList [PMCoordinate].Size.x != Console.BufferWidth || PlaneList [PMCoordinate].Size.y != Console.BufferHeight) {
								PlaneList.Remove (PMCoordinate);
								Console.SetCursorPosition (0, 0);
								Console.WriteLine ("Window size changed, generating new map");
								Log ("Window size changed, generating new map");
								Thread.Sleep (1000);
								SizeChange = true;
								goto SizeChange;
						}
						Console.ForegroundColor = ConsoleColor.White;
						Console.SetCursorPosition (0, 1);
						WriteCounter = Console.BufferWidth-1;
						while (WriteCounter >= 0) {
							Console.Write ('-');
							WriteCounter--;
						}
						Console.SetCursorPosition (0, (ConsoleHelper.ScreenCoordinate.y - 1));
						WriteCounter = Console.BufferWidth-1;
						while (WriteCounter >= 0) {
							Console.Write ('-');
							WriteCounter--;
						}
						Console.SetCursorPosition (0, 0);
						Console.Write ("{0}  LVL:{1}       LOCATION:{2},{3}", CharacterName, CharacterLevel, PMCoordinate.x, PMCoordinate.y);
						Console.SetCursorPosition (0, (ConsoleHelper.ScreenCoordinate.y));
						Console.Write ("HP:{0}/{1}  Stamina:{2}/{3}", CharacterHP, CharacterStats.Health, CharacterStamina, CharacterStats.Stamina);
						if (BATTLE == true) {
							Log("The player has entered a battle");
							bool Special = false;
							bool ND = false;
							bool Flee = false;
							int Cooldown = 0;
							while (((CharacterHP > 0) && (EnemyHP > 0)) && (!Flee)) {
								SpecUsed:
								Console.Clear ();
								Console.SetCursorPosition (0, 1);
								WriteCounter = Console.BufferWidth;
								while (WriteCounter >= 0) {
									Console.Write ('-');
									WriteCounter--;
								}
								Console.SetCursorPosition (0, ConsoleHelper.ScreenCoordinate.y - 1);
								WriteCounter = Console.BufferWidth;
								while (WriteCounter >= 0) {
									Console.Write ('-');
									WriteCounter--;
								}
								Console.SetCursorPosition (0, 0);
								Console.Write ("{0}    HP:{1}/{2}  Stamina:{3}/{4}", CharacterName, CharacterHP, CharacterStats.Health, CharacterStamina, CharacterStats.Stamina);
								Console.SetCursorPosition (0, ConsoleHelper.ScreenCoordinate.y);
								Console.Write ("ENEMY    HP:{0}/{1}  Stamina{2}/{3}", EnemyHP, EnemyStats.Health, EnemyStamina, EnemyStats.Stamina);
								Console.SetCursorPosition (0, 2);
								Console.WriteLine ("You are met with a fearsome foe...");
								Console.WriteLine ("What will you do?");
								Console.WriteLine ("1. ATTACK!");
								foreach (WeaponStats Weapon in Inventory) {
									if ((Weapon.ID == 6) | (Weapon.ID == 8)) {
										Special = true;
									}
								}
								Console.SetCursorPosition (0, 5);
								if (Special == true) {
									Console.Write ("2. Use Special Abitity({0})", Cooldown);
									Console.SetCursorPosition (0, 6);
									Console.Write ("3. Flee");
								} else {
									Console.Write ("2. Flee");
								}
								WrongKeyBattle:
								switch (Console.ReadKey (true).Key) {
								case ConsoleKey.D1:
									try {
										if(CharacterStamina<=0)
										{
											if (GenRand.Next (99) <= Inventory [0].CritChance)
												EnemyHP -= GenRand.Next ((int)((Inventory [0].MDamage + Inventory [0].PDamage + Inventory [0].RDamage) * 1.10), (int)(CharacterStats.Damage * 1.10)) / 10/4;
											EnemyHP -= GenRand.Next ((int)(Inventory [0].MDamage + Inventory [0].PDamage + Inventory [0].RDamage), CharacterStats.Damage) / 10/4;
										}
										else{
											if (GenRand.Next (99) <= Inventory [0].CritChance)
												EnemyHP -= GenRand.Next ((int)((Inventory [0].MDamage + Inventory [0].PDamage + Inventory [0].RDamage) * 1.10), (int)(CharacterStats.Damage * 1.10)) / 10;
											EnemyHP -= GenRand.Next ((int)(Inventory [0].MDamage + Inventory [0].PDamage + Inventory [0].RDamage), CharacterStats.Damage) / 10;
										}
										if (GenRand.Next (99) <= Inventory [0].CritChance)
											EnemyHP -= GenRand.Next ((int)((Inventory [0].MDamage + Inventory [0].PDamage + Inventory [0].RDamage) * 1.10), (int)(CharacterStats.Damage * 1.10)) / 10;
										EnemyHP -= GenRand.Next ((int)(Inventory [0].MDamage + Inventory [0].PDamage + Inventory [0].RDamage), CharacterStats.Damage) / 10;
									} catch {
										if(CharacterStamina<=0)
										{
											if (GenRand.Next (99) <= Inventory [0].CritChance)
												EnemyHP -= GenRand.Next ((int)(CharacterStats.Damage * 1.10), (int)((Inventory [0].MDamage + Inventory [0].PDamage + Inventory [0].RDamage) * 1.10)) / 10 / 4;
											EnemyHP -= GenRand.Next (CharacterStats.Damage, (int)(Inventory [0].MDamage + Inventory [0].PDamage + Inventory [0].RDamage)) / 10 /4;
										}
										else{
											if (GenRand.Next (99) <= Inventory [0].CritChance)
												EnemyHP -= GenRand.Next ((int)(CharacterStats.Damage * 1.10), (int)((Inventory [0].MDamage + Inventory [0].PDamage + Inventory [0].RDamage) * 1.10)) / 10;
											EnemyHP -= GenRand.Next (CharacterStats.Damage, (int)(Inventory [0].MDamage + Inventory [0].PDamage + Inventory [0].RDamage)) / 10;
										}
									}
									int StamLoss = GenRand.Next(0,10);
									if(CharacterStamina-StamLoss<=0)
									{
										CharacterStamina = 0;
									}else{
										CharacterStamina-=StamLoss;
									}
									break;
								case ConsoleKey.D2:
									if (Special && (Cooldown == 0)) {
										if (CharacterClass == Class.Paladin) {
											ND = true;
											Cooldown += 3;
										} else if (CharacterClass == Class.Healer) {
											CharacterHP += (int)(CharacterStats.Health * 0.10);
											if (CharacterHP > CharacterStats.Health) {
												CharacterHP = CharacterStats.Health;
											} else if (CharacterHP == CharacterStats.Health) {
												Console.WriteLine ("You already have full health");
												Console.ReadKey (true);
												goto WrongKeyBattle;
											}
											Cooldown += 5;
										}
										goto SpecUsed;
									} else if (Special && (Cooldown != 0)) {
										goto WrongKeyBattle;
									} else {
										Flee = true;
										continue;
									}
								case ConsoleKey.D3:
									Flee = true;
									continue;
								default:
									goto WrongKeyBattle;
								}
								if (!ND) {
									CharacterHP -= GenRand.Next (EnemyStats.Damage - 25, EnemyStats.Damage + 25) / 10;
								} else {
									ND = false;
								}
								if (Cooldown > 0) {
									Cooldown--;
								}
							}
							if (CharacterHP <= 0) {
								Console.Clear ();
								Console.WriteLine ("YOU HAVE DIED...");
								Console.WriteLine (":(");
								Console.ReadKey (true);
								BATTLE = false;
								goto Exit;
							}
							if (Flee) {
								Console.Clear ();
								Console.WriteLine ("You have fled the battle");
								Console.WriteLine ("Press any key to continue...");
								Console.ReadKey (true);
								BATTLE = false;
								goto StartMov;
							} else if (EnemyHP <= 0) {
								Console.Clear ();
								Console.WriteLine ("You have won the battle");
								XP += GenRand.Next (30, 175);
								if (XP >= CharacterLevel * 100) {
									Console.WriteLine ("You have leveled up!");
									XP = 0;
									CharacterLevel++;
									CharacterStats.Accuracy = (int)(CharacterStats.Accuracy * 1.10);
									CharacterStats.Agility = (int)(CharacterStats.Agility * 1.10);
									CharacterStats.Charisma = (int)(CharacterStats.Charisma * 1.10);
									CharacterStats.Damage = (int)(CharacterStats.Damage * 1.10);
									CharacterStats.Health = (int)(CharacterStats.Health * 1.10);
									CharacterStats.Intelligence = (int)(CharacterStats.Intelligence * 1.10);
									CharacterStats.MDefense = (int)(CharacterStats.MDefense * 1.10);
									CharacterStats.PDefense = (int)(CharacterStats.PDefense * 1.10);
									CharacterStats.RDefense = (int)(CharacterStats.RDefense * 1.10);
									CharacterStats.Stamina = (int)(CharacterStats.Stamina * 1.10);
								}
								Console.WriteLine ("Press any key to continue...");
								Console.ReadKey (true);
								BATTLE = false;
								goto StartMov;
							}
						}
						if (LoadingMap == true) {
							int TempCount;
							TempEntityList.Clear();
							TempCount = GenRand.Next(6, 17 + (int) ConsoleHelper.ScreenCoordinate.y / 4);
							while (TempCount != 0)
							{
								TempEntityList.Add(new EntityInfo(CharacterStats));
								TempCount--;
								Thread.Sleep(231);
							}
							EntityList = TempEntityList;
							LoadingMap=false;
							Console.SetCursorPosition(0, 2);
							Console.Write("              ");
						}
						foreach (SpecialTile DrTile in PlaneList[PMCoordinate].SpecialTiles) {
							Console.ForegroundColor = DrTile.TileColor;
							Console.SetCursorPosition (DrTile.Posistion.x, DrTile.Posistion.y);
							Console.Write (DrTile.RepChar);
						}
						Console.ForegroundColor = ConsoleColor.Red;
						for (var i = 0; i < EntityList.Count; i++) {
							try
							{
								ConsoleHelper.SetCursorPositionToCoordinate (EntityList[i].PreviousEntityPosition);
								Console.Write (' ');
								ConsoleHelper.SetCursorPositionToCoordinate (EntityList[i].EntityPosition);
								Console.Write ('#');
								EntityList[i].PreviousEntityPosition = EntityList[i].EntityPosition;
							}
							catch
							{
								i--;
							}
						}
						Console.ForegroundColor = ConsoleColor.White;
						ConsoleHelper.SetCursorPositionToCoordinate (OldPlayerCoordinates);
						Console.Write (' ');
						ConsoleHelper.SetCursorPositionToCoordinate (PlayerCoordinates);
						Console.Write ('@');
						OldPlayerCoordinates = PlayerCoordinates;
						WrongKey:
						switch (Console.ReadKey (true).Key) {
						case ConsoleKey.W:
							if (PlayerCoordinates.y != 2) {
								PlayerCoordinates.y--;
							} else {
								PMCoordinate.y++;
								PlayerCoordinates = new Coordinate (PlayerCoordinates.x, ConsoleHelper.ScreenCoordinate.y - 2);
								LoadingMap = true;
							}
							break;
						case ConsoleKey.S:
							if (PlayerCoordinates.y != ConsoleHelper.ScreenCoordinate.y - 2) {
								PlayerCoordinates.y++;
							} else {
								PMCoordinate.y--;
								PlayerCoordinates = new Coordinate (PlayerCoordinates.x, 2);
								LoadingMap = true;
							}
							break;
						case ConsoleKey.D:
							if (PlayerCoordinates.x != ConsoleHelper.ScreenCoordinate.x) {
								PlayerCoordinates.x++;
							} else {
								PMCoordinate.x++;
								PlayerCoordinates = new Coordinate (0, PlayerCoordinates.y);
								LoadingMap = true;
							}
							break;
						case ConsoleKey.A:
							if (PlayerCoordinates.x != 0) {
								PlayerCoordinates.x--;
							} else {
								PMCoordinate.x--;
								PlayerCoordinates = new Coordinate (ConsoleHelper.ScreenCoordinate.x, PlayerCoordinates.y);
								LoadingMap = true;
							}
							break;
						case ConsoleKey.Escape:
                            ExitGame = true;
							goto Exit;
						default:
							goto WrongKey;
						}
						for (var i = 0; i < EntityList.Count; i++) {
							if ((EntityList[i].EntityPosition.x == PlayerCoordinates.x) && (EntityList[i].EntityPosition.y == PlayerCoordinates.y) && BATTLE != true) {
								BATTLE = true;
								EnemyStats = EntityList[i].EntStats;
								EnemyHP = EnemyStats.Health;
								EnemyStamina = EnemyStats.Stamina;
								EntityList[i].EntityPosition = new Coordinate (GenRand.Next (0, ConsoleHelper.ScreenCoordinate.x), GenRand.Next (2, ConsoleHelper.ScreenCoordinate.y - 2));
							}
						}
						foreach (EntityInfo Ent in EntityList) {
							IVMoveEnt:
							switch (GenRand.Next (0, 4)) {
							case 0:
								if (Ent.EntityPosition.y != 2)
									Ent.EntityPosition.y--;
								else
									goto IVMoveEnt;
								break;
							case 1:
								if (Ent.EntityPosition.y != ConsoleHelper.ScreenCoordinate.y-2)
									Ent.EntityPosition.y++;
								else
									goto IVMoveEnt;
								break;
							case 2:
								if (Ent.EntityPosition.x != 0)
									Ent.EntityPosition.x--;
								else
									goto IVMoveEnt;
								break;
							case 3:
								if (Ent.EntityPosition.x != ConsoleHelper.ScreenCoordinate.x)
									Ent.EntityPosition.x++;
								else
									goto IVMoveEnt;
								break;
							default:
								goto IVMoveEnt;

							}
						}
						for (var i = 0; i < EntityList.Count; i++) {
							if ((EntityList[i].EntityPosition.x == PlayerCoordinates.x) && (EntityList[i].EntityPosition.y == PlayerCoordinates.y) && BATTLE != true) {
								BATTLE = true;
								EnemyStats = EntityList[i].EntStats;
								EnemyHP = EnemyStats.Health;
								EnemyStamina = EnemyStats.Stamina;
								EntityList[i].EntityPosition = new Coordinate (GenRand.Next (0, ConsoleHelper.ScreenCoordinate.x), GenRand.Next (2, ConsoleHelper.ScreenCoordinate.y - 2));
							}
						}
						for (var i = 0; i < PlaneList [PMCoordinate].SpecialTiles.Count; i++) {
							if ((PlaneList [PMCoordinate].SpecialTiles [i].Posistion.x == PlayerCoordinates.x && PlaneList [PMCoordinate].SpecialTiles [i].Posistion.y == PlayerCoordinates.y) & (PlaneList [PMCoordinate].SpecialTiles [i].ID == 3)) {
								if (((int)CharacterStats.Health / 3) + CharacterHP < CharacterStats.Health) {
									CharacterHP += (int)CharacterStats.Health / 3;
								} else {
									CharacterHP = CharacterStats.Health;
								}
								if (((int)CharacterStats.Stamina / 3) + CharacterStamina < CharacterStats.Stamina) {
									CharacterStamina += (int)CharacterStats.Stamina / 3;
								} else {
									CharacterStamina = CharacterStats.Stamina;
								}
								PlaneList [PMCoordinate].SpecialTiles.Remove (PlaneList [PMCoordinate].SpecialTiles [i]);
								PlaneList[PMCoordinate].HealthPacksUsed++;
							}
						}
					} while (LoadingMap == false);
				}
			}
			//End of program
			Exit:
			LogThreadObj.Abort();
			TitleThreadObj.Abort ();
			return;
		}
	}
		#endregion
}

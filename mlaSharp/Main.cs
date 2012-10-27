using System;

namespace mlaSharp
{
	class MainClass
	{
		public static void Main (string[] args)
		{
			GameEngine env = new GameEngine();
			
			Console.WriteLine("mlaSharp : Magic the Gathering Learning Agent");
			for(int i = 1; i <= 2; i++)
			{
				
#if INPUT_DECKLISTS
				Console.WriteLine("Player " + i + " name: ");
				Console.Write("mla> ");
				string name = Console.In.ReadLine();
				Player player = new ConsolePlayer(env,name);
				Console.Write("Decklist path:\nmla> ");
				string decklistPath = Console.In.ReadLine ();
#else
				Player player = new ConsolePlayer(env,"p" + i);
				string decklistPath = null;
#endif				
				Library lib = null;
				if(String.IsNullOrEmpty(decklistPath))
				{
					lib = Library.ParseDecklist("40 Goblin Piker\n20 Mountain",player);
				}
				else
				{
					throw new NotImplementedException("Custom decklists not currently implemented");	
				}
				env.AddPlayer(player,lib);
				Console.WriteLine("Player " + player.Name + " created!");
			}
			
			env.StartGame();
		}
	}
}

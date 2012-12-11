using System;

namespace mlaSharp
{
	class MainClass
	{
		public static void Main (string[] args)
		{
			GameEngine env = new GameEngine();
			string[] names = {"joe", "bob"};
			Console.WriteLine("mlaSharp : Magic the Gathering Learning Agent");
			for(int i = 0; i < 2; i++)
			{
				
#if INPUT_DECKLISTS
				Console.WriteLine("Player " + i + " name: ");
				Console.Write("mla> ");
				string name = Console.In.ReadLine();
				Player player = new ConsolePlayer(env,name);
				Console.Write("Decklist path:\nmla> ");
				string decklistPath = Console.In.ReadLine ();
#else
				Player player = new ConsolePlayer(env,names[i]);
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
				
				// sanity checking
				for(int a = 0; a < lib.Count; a++)
				{
					Card ca, cb;
					ca = lib[a];
					for( int b = a+1; b < lib.Count; b++)
					{
						cb = lib[b];
						if(ca == cb)
							throw new Exception("One card is a reference to another");
					}
				}
				
				env.AddPlayer(player,lib);
				Console.WriteLine("Player " + player.Name + " created!");
			}
			
			env.StartGame();
		}
	}
}

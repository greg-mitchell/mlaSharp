using System;
using System.Collections.Generic;

namespace mlaSharp
{
	class MainClass
	{
		public static void Main (string[] args)
		{
			string[] names = {"joe", "bob"};
			string[] decklists = {"10 Hill Giant\n30 Goblin Piker\n20 Mountain",
				"35 Hill Giant\n25 Mountain"
			};
			Dictionary<string,int> wins = new Dictionary<string, int>();
			foreach(var s in names)
				wins[s] = 0;
			
			Console.WriteLine("mlaSharp : Magic the Gathering Learning Agent");
			for(int iterations = 0; iterations< 50; iterations++)
			{
				GameEngine env = new GameEngine();
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
					Player player = new RandomPlayer(env,env.rng,names[i]);
					string decklistPath = null;
	#endif				
					Library lib = null;
					if(String.IsNullOrEmpty(decklistPath))
					{
						lib = Library.ParseDecklist(decklists[i],player);
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
				
				Player winner = env.StartGame();
				if(winner != null)
					wins[winner.Name]++;
			}
			
			foreach(var s in wins.Keys)
			{
				Console.WriteLine(String.Format("Player {0} won {1} times.",s,wins[s]));
			}
		}
	}
}

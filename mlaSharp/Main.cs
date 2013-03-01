using System;
using System.Collections.Generic;

namespace mlaSharp
{
	class MainClass
	{
		public static void Main (string[] args)
		{
			const int GAMES_TO_PLAY = 1000;
			
			string[] names = {"joe", "bob"};
			string[] decklists = {"35 Hill Giant\n25 Mountain",
				"35 Hill Giant\n25 Mountain"
			};
			Dictionary<string,int> wins = new Dictionary<string, int>();
			Dictionary<string,int> winsOnPlay = new Dictionary<string, int>();
			foreach(var s in names)
			{
				wins[s] = 0;
				winsOnPlay[s] = 0;
			}
			
			Console.WriteLine("mlaSharp : Magic the Gathering Learning Agent");
			for(int iterations = 0; iterations< GAMES_TO_PLAY; iterations++)
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
//					
//					// sanity checking
//					for(int a = 0; a < lib.Count; a++)
//					{
//						Card ca, cb;
//						ca = lib[a];
//						for( int b = a+1; b < lib.Count; b++)
//						{
//							cb = lib[b];
//							if(ca == cb)
//								throw new Exception("One card is a reference to another");
//						}
//					}
					
					env.AddPlayer(player,lib);
					Console.WriteLine("Player " + player.Name + " created!");
				}
				Player winner = env.StartGame();
				if(winner != null)
				{
					wins[winner.Name]++;
					if(winner == env.StartingPlayer)
					{
						winsOnPlay[winner.Name]++;	
					}
				}
			}
			
			foreach(var s in wins.Keys)
			{
				int winsCount, winsOnPlayCount;
				double winPercent, winOnPlayPercent;
				winsCount = wins[s];
				winsOnPlayCount = winsOnPlay[s];
				winPercent = (double)winsCount / GAMES_TO_PLAY;
				winOnPlayPercent = (double)winsOnPlayCount / winsCount;
				Console.WriteLine(String.Format("Player {0} won {1} times ({2}%), {3} times on the play ({4}%).",s,winsCount,winPercent,winsOnPlayCount,winOnPlayPercent));
			}
		}
	}
}

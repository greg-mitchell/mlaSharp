using System;
using System.Collections.Generic;
using log4net;
using log4net.Config;
using System.Reflection;

namespace mlaSharp
{
	
	class MainClass
	{
		private static readonly ILog logger = LogManager.GetLogger( MethodBase.GetCurrentMethod().DeclaringType);
		
		public static void Main (string[] args)
		{
			
			const int GAMES_TO_PLAY = 1000;
			
			string[] names = {"joe", "bob"};
			string[] decklists = {"20 Goblin Piker\n20 Grey Ogre\n20 Mountain",
				"20 Grey Ogre\n15 Hill Giant\n25 Mountain"
			};
			Dictionary<string,int> wins = new Dictionary<string, int>();
			Dictionary<string,int> winsOnPlay = new Dictionary<string, int>();
			foreach(var s in names)
			{
				wins[s] = 0;
				winsOnPlay[s] = 0;
			}
			
			logger.Info("mlaSharp : Magic the Gathering Learning Agent");
			for(int iterations = 0; iterations< GAMES_TO_PLAY; iterations++)
			{
				GameEngine env = new GameEngine();
				for(int i = 0; i < 2; i++)
				{
#if RANDOM_PLAYERS					
					Player player = new RandomPlayer(env,env.rng,names[i]);
#elif MCTS_PLAYERS
					Player player = new MctsPlayer(env,names[i]);
#else
//					Console.WriteLine("Player " + i + " name: ");
//					Console.Write("mla> ");
//					string name = Console.In.ReadLine();
					Player player = new ConsolePlayer(env,names[i]);
#endif
					
	#if INPUT_DECKLISTS
					Console.Write("Decklist path:\nmla> ");
					string decklistPath = Console.In.ReadLine ();
	#else
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
					
					env.AddPlayer(player,lib);
					logger.Debug("Player " + player.Name + " created!");
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
				logger.Info(String.Format("Player {0} won {1} times ({2:f2}%), {3} times on the play ({4:f2}%).",s,winsCount,winPercent,winsOnPlayCount,winOnPlayPercent));
			}
		}
	}
}

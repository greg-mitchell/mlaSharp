using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Linq;

namespace mlaSharp
{
	public class GameEngine
	{
		public const int STARTING_HAND_SIZE = 7;
		
		private State CurrState {get; set;}
		
		private List<Player> players;
		private Dictionary<Player,Library> libraries;
		private Dictionary<Player,int> initialHandSize;
		private RandomNumberGenerator rng;
		
		public GameEngine ()
		{
			CurrState = new State();
			players = new List<Player>();
			libraries = new Dictionary<Player, Library>();
			rng = new RNGCryptoServiceProvider();
		}
		
		public List<GameActionDelegate> EnumActions(Player p)
		{
			throw new NotImplementedException();
		}
		
		public State GetCurrState()
		{
			return CurrState;
		}
		
		public void PerformAction(GameActionDelegate action)
		{
			
		}
		
		/// <summary>
		/// Adds the player to the game.
		/// </summary>
		/// <param name='p'>
		/// The player to add.
		/// </param>
		/// <param name='deck'>
		/// The initialized deck to add.  The deck will be shuffled before starting.
		/// </param>
		public void AddPlayer(Player p, Library deck)
		{
			players.Add(p);
			libraries[p] = deck;			
		}
		
		public void StartGame()
		{
			if(players.Count != 2)
				throw new NotImplementedException("The game engine currently only supports exactly 2 players");
			
			// randomize starting player
			byte[] buf = new byte[1];
			int n = players.Count;
			do rng.GetBytes(buf);
			while((buf[0] < n * (Byte.MaxValue / n)));
			int k = (buf[0] % n);
			
			System.Console.WriteLine(String.Format("Game started.  Player {0} ({1}) plays first.",k+1,players[k].Name));
			CurrState.PlayerWithPriority = players[k];
			
			// shuffle libraries
			foreach(Player p in players)
				libraries[p].Shuffle(rng);
			
			// draw opening hands and resolve mulligans
			// create an ordered list of players who have not kept
			List<Player> notKept = new List<Player>(n);
			// create a dictionary of player -> hand size to keep track of how many cards to mulligan to
			initialHandSize = new Dictionary<Player, int>(n);
			// initialize both structures
			for(int i = 0; i < n; i++)
			{
				Player pcurr = players[(i+k)%n];
				notKept.Add(pcurr);
				initialHandSize[pcurr] = STARTING_HAND_SIZE;
			}
			
			int j = 0;
			while(notKept.Count > 0)
			{
				Player p = notKept[j];
				libraries[p].AddRange(p.Hand);
				p.Hand.Clear();
				var hand = libraries[p].Draw (initialHandSize[p]);
				p.Hand.AddRange(hand);
				
				j = (j+1) % n;	
				
				if(!p.MulliganHand())
				{
					notKept.Remove (p);
				}
				else
				{
					initialHandSize[p]--;
				}				
			}
			
			CurrState.CurrStep = Steps.main1;
			
			MainGameLoop();
		}
		
		/// <summary>
		/// The main game loop.
		/// On every iteration, checks state-based actions, then polls the player with priority for an action.
		/// </summary>
		private void MainGameLoop()
		{
			while(true)
			{
				IEnumerable<Player> losingPlayers;
				if((losingPlayers = CheckPlayerLost()) != null)
				{
					foreach(Player p in losingPlayers)
						Console.WriteLine(String.Format("Player {0} lost!",p.Name));
					
					break;
				}
				
				DoSBA();
				
				var action = CurrState.PlayerWithPriority.GetAction();
				action(CurrState.PlayerWithPriority,CurrState);
			}
		}
		
		/// <summary>
		/// Performs State-based actions (except for a player losing).
		/// </summary>
		private void DoSBA()
		{
			
		}
		
		/// <summary>
		/// Returns a list of players that have lost.
		/// </summary>
		/// <returns>
		/// The players who have lost.
		/// </returns>
		private IEnumerable<Player> CheckPlayerLost()
		{
			return null;
		}
		
		/// <summary>
		/// Shuffles player p's library.
		/// </summary>
		/// <param name='p'>
		/// The player whose library to shuffle.
		/// </param>
		public void ShuffleLibrary(Player p)
		{
			libraries[p].Shuffle(rng);
		}
	}
	
	
	/// <summary>
	/// A function that performs a game action
	/// </summary>
	public delegate void GameActionDelegate(Player p, State s);
	
	/// <summary>
	/// Sequential steps in the game
	/// </summary>
	public enum Steps
	{
		untap,
		upkeep,
		draw,
		main1,
		beginCombat,
		declareAtk,
		declareBlk,
		damage,
		endCombat,
		main2,
		end,
		cleanup
	}
}


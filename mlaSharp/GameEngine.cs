using System;
using System.Collections.Generic;
using System.Linq;

namespace mlaSharp
{
	public class GameEngine
	{
		public const int STARTING_HAND_SIZE = 7;
		
		private State CurrState {get; set;}
		
		public List<Player> Players { get; private set; }
		private Dictionary<Player,Library> libraries;
		private Dictionary<Player,int> initialHandSize;
		private Random rng;
		
		public GameEngine ()
		{
			CurrState = new State(this);
			Players = new List<Player>();
			libraries = new Dictionary<Player, Library>();
			rng = new CryptoRandom();
		}
		
		public List<GameActionDelegate> EnumActions(Player p)
		{
			var actions = new List<GameActionDelegate>();
			// if it's a main phase of the current player's turn, the stack is empty, and that player has priority, 
			// he can play sorcery speed effects
			if(p == CurrState.PlayersTurn 
			   && p == CurrState.PlayerWithPriority 
			   && (CurrState.CurrStep == Steps.main1 || CurrState.CurrStep == Steps.main2) 
			   && CurrState.Stack.Count == 0)
			{
				// if a land has not been played this turn, can play a land
				if(CurrState.LandsLeftToPlayThisTurn > 0)
				{
					var lands = from c in CurrState.Hands[CurrState.PlayerWithPriority]
								where c.Type.Contains("Land") 
								select c;
					foreach(Card land in lands)
					{
						actions.Add((Player player, State s) => {
							s.Battlefield.Add(land);
							s.Hands[player].Remove(land);
						});
					}
				}
			}
			
			return actions;
		}
		
		public State GetCurrState()
		{
			return CurrState;
		}
		
		public void PerformAction(GameActionDelegate action)
		{
			action(CurrState.PlayerWithPriority,CurrState);
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
			Players.Add(p);
			libraries[p] = deck;			
		}
		
		public void StartGame()
		{
			if(Players.Count != 2)
				throw new NotImplementedException("The game engine currently only supports exactly 2 players");
			
			int n = Players.Count;
			var playOrder = Enumerable.Range(0,n).ToList();
			playOrder.Shuffle(rng);
			int k = playOrder[0];
			
			System.Console.WriteLine(String.Format("Game started.  Player {0} plays first.",k+1,Players[k].Name));
			CurrState.PlayerWithPriority = Players[k];
			
			// shuffle libraries
			foreach(Player p in Players)
				libraries[p].Shuffle(rng);
			
			// draw opening hands and resolve mulligans
			// create an ordered list of players who have not kept
			List<Player> notKept = new List<Player>(n);
			// create a dictionary of player -> hand size to keep track of how many cards to mulligan to
			initialHandSize = new Dictionary<Player, int>(n);
			// initialize both structures
			for(int i = 0; i < n; i++)
			{
				Player pcurr = Players[(i+k)%n];
				notKept.Add(pcurr);
				initialHandSize[pcurr] = STARTING_HAND_SIZE;
			}
			
			int j = 0;
			while(notKept.Count > 0)
			{
				Player p = notKept[j];
				libraries[p].AddRange(CurrState.Hands[p]);
				CurrState.Hands[p].Clear();
				var hand = libraries[p].Draw (initialHandSize[p]);
				CurrState.Hands[p].AddRange(hand);
								
				if(!p.MulliganHand())
				{
					notKept.Remove (p);
				}
				else
				{
					initialHandSize[p]--;
				}				
				
				
				j = (j+1) %  ((notKept.Count > 0) ? notKept.Count : 1);	
			}
			
			CurrState.PlayersTurn = Players[k];
			CurrState.PlayerWithPriority = Players[k];
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
			/// TODO: check for SBAs
		}
		
		/// <summary>
		/// Returns a list of players that have lost.
		/// </summary>
		/// <returns>
		/// The players who have lost.
		/// </returns>
		private IEnumerable<Player> CheckPlayerLost()
		{
			// TODO: check for players losing
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


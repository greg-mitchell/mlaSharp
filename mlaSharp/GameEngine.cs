using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Linq;

namespace mlaSharp
{
	public class GameEngine
	{
		#region Game Constants
		public static readonly int STARTING_HAND_SIZE = 7;
		#endregion
		private State CurrState {get; set;}
		
		private List<Player> players;
		private Dictionary<Player,List<Card>> libraries;
		private RandomNumberGenerator rng;
		
		public GameEngine ()
		{
			CurrState = new State();
			players = new List<Player>();
			libraries = new Dictionary<Player, List<Card>>();
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
		public void AddPlayer(Player p, List<Card> deck)
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
			while((box[0] < n * (Byte.MaxValue / n)));
			int k = (box[0] % n);
			
			System.Console.WriteLine(String.Format("Game started.  Player {0} ({1}) plays first.",k+1,players[k].Name));
			
			// shuffle libraries
			foreach(Player p in players)
				libraries[p].Shuffle(rng);
			
			// draw opening hands and resolve mulligans
			// create an ordered list of players who have not kept
			List<Player> notKept = new List<Player>(n);
			// create a dictionary of player -> hand size to keep track of how many cards to mulligan to
			Dictionary<Player,int> handSize = new Dictionary<Player, int>(n);
			// initialize both structures
			for(int i = 0; i < n; i++)
			{
				Player pcurr = players[(i+k)%n];
				notKept.Add(pcurr);
				handSize[pcurr] = STARTING_HAND_SIZE;
			}
			
			int i = 0;
			while(notKept.Count > 0)
			{
				notKept[i].Hand.Clear();
				var hand = libraries[notKept[i]].Take(handSize[notKept[i]]);
				notKept[i].Hand.AddRange(hand);
			}
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


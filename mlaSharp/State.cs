using System;
using System.Collections.Generic;
using mlaSharp;

namespace mlaSharp
{
	public class State
	{
		public Steps CurrStep { get; set;}
		public Player PlayersTurn { get; set; }
		public Player PlayerWithPriority { get; set;}
		public List<Card> Battlefield { get; private set;}
		public List<StackObject> Stack { get; private set;}
		public Dictionary<Player,ManaPool> ManaPools { get; private set; }
		public Dictionary<Player,List<Card>> Hands;
		public Dictionary<Player,List<Card>> Graveyards;
		
		public int LandsLeftToPlayThisTurn { get; set; }
		
		private GameEngine env;
		
		public State (GameEngine env)
		{			
			Battlefield = new List<Card>();
			Stack = new List<StackObject>();
			ManaPools = new Dictionary<Player, ManaPool>();
			Hands = new Dictionary<Player, List<Card>>();
			Graveyards = new Dictionary<Player, List<Card>>();
			
			this.env = env;
		}
		
		/// <summary>
		/// Copy constructor for the <see cref="mlaSharp.State"/> class.
		/// </summary>
		/// <param name='s'>
		/// State s to copy.
		/// </param>
		public State (State s)
		{
			CurrStep = s.CurrStep;
			PlayersTurn = s.PlayersTurn;
			PlayerWithPriority = s.PlayerWithPriority;
			Battlefield = new List<Card>(s.Battlefield);
			Stack = new List<StackObject>(s.Stack);
			ManaPools = new Dictionary<Player, ManaPool>();
			Hands = new Dictionary<Player, List<Card>>();			
			Graveyards = new Dictionary<Player, List<Card>>();
			
			env = s.env;
			
			// deep copy dictionary entries
			foreach(Player p in s.env.Players)
			{
				// because ManaPool is a struct, each value in ManaPools<Player,ManaPool> is unique on copy
				ManaPools[p] = new ManaPool(s.ManaPools[p]);	
				Hands[p] = new List<Card>(s.Hands[p]);
				Graveyards[p] = new List<Card>(s.Graveyards[p]);
			}
		}
		
	}
}


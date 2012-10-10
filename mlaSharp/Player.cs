using System;
using System.Collections.Generic;

namespace mlaSharp
{
	/// <summary>
	/// A generic player in the game.  Exposes methods to enumerate and perform game actions
	/// </summary>
	public abstract class Player
	{
		public string Name { get; private set;}
		public GameEngine Env { get; private set;}
		public ManaPool ManaPool {get; set;}
		public List<Card> Hand { get; private set;}
		public List<Card> Graveyard { get; private set;}
		
		public Player (GameEngine env, string name = "")
		{
			this.Name = name;
			this.Env = env;
			this.Hand = new List<Card>();
			this.Graveyard = new List<Card>();
			this.ManaPool = new ManaPool();
		}
		
		public abstract bool MulliganHand();
		
		public abstract GameActionDelegate GetAction();
	}
	
	public class ManaPool
	{
		public int W;
		public int U;
		public int B;
		public int R;
		public int G;
		public int Generic;
	}
}


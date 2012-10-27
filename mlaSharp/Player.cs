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
		
		public Player (GameEngine env, string name = "")
		{
			this.Name = name;
			this.Env = env;
		}
		
		public abstract bool MulliganHand();
		
		public abstract GameActionDelegate GetAction();
		
		public override string ToString ()
		{
			return Name;
		}
	}
	
}


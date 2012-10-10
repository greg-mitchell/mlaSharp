using System;

namespace mlaSharp
{
	/// <summary>
	/// A triggered, activated, or static ability
	/// </summary>
	public class Ability
	{
		/// <summary>
		/// A fuction that should return true if the ability's prereqs are fulfilled
		/// </summary>
		public delegate bool AbilityAvailablePredicate(Player p, State s);
		
		public AbilityAvailablePredicate AbilityAvailable { get; private set;}
		public GameActionDelegate AbilityAction { get; private set;}
		
		public Ability (AbilityAvailablePredicate pred, GameActionDelegate a)
		{
			this.AbilityAvailable = pred;
			this.AbilityAction = a;
		}
	}
	
}


using System;

namespace mlaSharp
{
	/// <summary>
	/// A triggered, activated, or static ability
	/// </summary>
	public class Ability
	{
		
		public AbilityAvailablePredicate AbilityAvailable { get; private set;}
		public GameActionDelegate AbilityAction { get; private set;}
		
		public Ability (AbilityAvailablePredicate pred, GameActionDelegate a)
		{
			this.AbilityAvailable = pred;
			this.AbilityAction = a;
		}
	}
	
}


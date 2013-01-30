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
		
		/// <summary>
		/// Ask the player whether to mulligan the hand.
		/// </summary>
		/// <returns>
		/// True if mulliganing, false if keeping.
		/// </returns>
		public abstract bool MulliganHand();
		
		/// <summary>
		/// Asks the player what action to take.
		/// </summary>
		/// <returns>
		/// The action.
		/// </returns>
		public abstract GameActionDelegate GetAction();
		
		/// <summary>
		/// Ask the player to choose the attackers.
		/// </summary>
		/// <returns>
		/// The attackers.
		/// </returns>
		/// <param name='possibleAttackers'>
		/// Set of possible attackers.
		/// </param>
		public abstract ISet<CreatureCard> ChooseAttackers(IList<CreatureCard> possibleAttackers);
		
		/// <summary>
		/// Chooses the blockers.
		/// </summary>
		/// <param name='attackersToBlockersDictionary'>
		/// Attackers to blockers dictionary.
		/// </param>
		/// <param name='possibleBlockers'>
		/// Possible blockers.
		/// </param>
		public abstract void ChooseBlockers(IDictionary<CreatureCard,IList<CreatureCard>> attackersToBlockersDictionary, IList<CreatureCard> possibleBlockers);
		
		/// <summary>
		/// Orders the blockers in the passed dictionary.
		/// </summary>
		/// <param name='attackersToBlockersDictionary'>
		/// Attackers to blockers dictionary.
		/// </param>
		public abstract void OrderBlockers(IDictionary<CreatureCard,IList<CreatureCard>> attackersToBlockersDictionary);
		
		public override string ToString ()
		{
			return Name;
		}
	}
	
}


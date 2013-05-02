using System;
using System.Collections.Generic;

namespace mlaSharp
{
	/// <summary>
	/// Mcts player.
	/// Uses Monte Carlo Tree Search to choose optimal actions.
	/// </summary>
	public class MctsPlayer : Player
	{
		
		public MctsPlayer (GameEngine env, string name = "")
			:base(env,name)
		{
			
		}
		
		public override GameActionDelegate GetAction ()
		{
			var actions = Env.EnumActions(this);
			
			throw new NotImplementedException ();
		}
		
		public override bool MulliganHand ()
		{
			throw new NotImplementedException ();
		}
		
		public override ISet<CreatureCard> ChooseAttackers (IList<CreatureCard> possibleAttackers)
		{
			throw new NotImplementedException ();
		}
		
		public override void ChooseBlockers (IDictionary<CreatureCard, IList<CreatureCard>> attackersToBlockersDictionary, IList<CreatureCard> possibleBlockers)
		{
			throw new NotImplementedException ();
		}
		
		public override void OrderBlockers (IDictionary<CreatureCard, IList<CreatureCard>> attackersToBlockersDictionary)
		{
			throw new NotImplementedException ();
		}
	}
}


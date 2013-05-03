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
		private MctsEvaluator mctsEval;
		private int budgetPerMoveMs;
		
		public MctsPlayer (GameEngine env, string name = "")
			:base(env,name)
		{
			mctsEval = new MctsEvaluator(env);
		}
		
		public MctsPlayer (GameEngine env, string name, int budgetPerMoveInMs)
			:this(env,name)
		{
			this.budgetPerMoveMs = budgetPerMoveInMs;
		}
		
		public override GameActionDelegate GetAction ()
		{
			// TODO: somehow need to have it select the best action even though the actions are opaque delegates
			// TODO: should bound the number of moves in a game
			var actions = Env.EnumActions(this);
			mctsEval.FindBestChild(Env.CurrState,budgetPerMoveMs);
			
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


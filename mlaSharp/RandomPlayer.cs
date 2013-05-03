using System;
using System.Collections.Generic;

namespace mlaSharp
{
	/// <summary>
	/// Random player.
	/// Chooses randomly from among possible actions.
	/// </summary>
	public class RandomPlayer : Player
	{
		private double pMulligan, pLand, pSpell, pAttack, pBlock;
		private Random rng;
		private bool castingSpell;
		
		public RandomPlayer(GameEngine env, string name)
			: base (env, name)
		{
			this.rng = env.rng;
		}
		
		public RandomPlayer(GameEngine env, Random rng, string name = "", 
		                    double pMulligan = 0.8, double pLand = 1.0, double pSpell = 0.75, 
		                    double pAttack = 0.75, double pBlock = 0.75)
			: base (env, name)
		{
			this.rng = rng;
			this.pMulligan = pMulligan;
			this.pLand = pLand;
			this.pSpell = pSpell;
			this.pAttack = pAttack;
			this.pBlock = pBlock;
		}
		
		public override GameActionDelegate GetAction ()
		{
			var actions = Env.EnumActions(this);
			
			// determine whether to cast a spell or go to the next step
			double castingSpellSample = rng.NextDouble();
			if(castingSpell || castingSpellSample <= pSpell)
			{
				castingSpell = true;
				// tap all lands with p=1
				// for now, assume lands are the only ones with mana abilities
				foreach(var a in actions)
				{
					if(a.ActionDescription.Contains("activated ability"))
						return a.GameAction;
				}
				
				// play all lands with p=1
				foreach(var a in actions)
				{
					if(a.ActionDescription.Contains("Play") && a.ActionDescription.Contains("land"))
					{
						//Console.WriteLine(String.Format("Player {0} plays a land for turn",this.Name));
						return a.GameAction;
					}
				}
				
				foreach(var a in actions)
				{
					if(a.ActionDescription.Contains("creature") && a.ActionDescription.Contains("Cast"))
					{
						// strip "Cast " from beginning of action description and "(creature)" from end
						//int sublength = a.ActionDescription.LastIndexOf('(') - 5;
						//Console.WriteLine(String.Format("Player {0} plays a {1}",this.Name, a.ActionDescription.Substring(5,sublength)));
						castingSpell = false;
						return a.GameAction;
					}
				}
				
				castingSpell = false;
			}
			
			// go to next step
			foreach(var a in actions)
			{
				if(a.ActionDescription.Contains("step"))
					return a.GameAction;
			}
			
			// some unforseen circumstance.  Randomly choose an action.
			return actions[rng.Next(actions.Count)].GameAction;
		}
		
		public override bool MulliganHand ()
		{
			return rng.NextDouble() <= pMulligan;
		}
		
		public override System.Collections.Generic.ISet<CreatureCard> ChooseAttackers (System.Collections.Generic.IList<CreatureCard> possibleAttackers)
		{
			ISet<CreatureCard> attackers = new HashSet<CreatureCard>();
			foreach(CreatureCard c in possibleAttackers)
			{
				if(rng.NextDouble() <= pAttack)
					attackers.Add(c);
			}
			return attackers;
		}
		
		public override void ChooseBlockers (IDictionary<CreatureCard, IList<CreatureCard>> attackersToBlockersDictionary, IList<CreatureCard> possibleBlockers)
		{
			
			// for each attacker, randomly determine if blocking using pBlock, then determine how many blockers
			foreach(var attacker in attackersToBlockersDictionary.Keys)
			{
				double blockSample = rng.NextDouble();
				if(blockSample <= pBlock)
				{
					// +1 because upper bound is exclusive
					int numBlockers = rng.Next(possibleBlockers.Count+1);
					for(int i = 0; i < numBlockers; i++)
					{
						int nextIndex = rng.Next(possibleBlockers.Count);
						CreatureCard blocker = possibleBlockers[nextIndex];
						bool result = possibleBlockers.Remove(blocker);
						
						if(!result)
							throw new Exception("WTF MAN");
						
						attackersToBlockersDictionary[attacker].Add(blocker);
					}
				}
			} 
		}
		
		public override void OrderBlockers (System.Collections.Generic.IDictionary<CreatureCard, System.Collections.Generic.IList<CreatureCard>> attackersToBlockersDictionary)
		{
			return;
		}
	}
}


using System;
using System.Linq;
using System.Collections.Generic;
using mlaSharp;

namespace mlaSharp
{
	public class State
	{
		public Steps CurrStep;
		public Player PlayersTurn;
		public Player PlayerWithPriority;
		public bool AttackersDeclared;
		public bool BlockersDeclared;
		public int LandsLeftToPlayThisTurn;
		public int TurnNumber;
		public List<Card> Battlefield { get; private set;}
		public List<StackObject> Stack { get; private set;}
		public Dictionary<Player,ManaPool> ManaPools { get; private set; }
		public Dictionary<Player,int> LifeTotals { get; private set; }
		public Dictionary<Player,List<Card>> Hands { get; private set; }
		public Dictionary<Player,List<Card>> Graveyards { get; private set; }
		
		
		private GameEngine env;
		
		public State (GameEngine env)
		{			
			TurnNumber = 1;
			Battlefield = new List<Card>();
			Stack = new List<StackObject>();
			ManaPools = new Dictionary<Player, ManaPool>();
			LifeTotals = new Dictionary<Player, int>();
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
			LifeTotals = new Dictionary<Player, int>();
			Hands = new Dictionary<Player, List<Card>>();			
			Graveyards = new Dictionary<Player, List<Card>>();
			
			env = s.env;
			
			// deep copy dictionary entries
			foreach(Player p in s.env.Players)
			{
				ManaPools[p] = new ManaPool(s.ManaPools[p]);	
				LifeTotals[p] = s.LifeTotals[p];
				Hands[p] = new List<Card>(s.Hands[p]);
				Graveyards[p] = new List<Card>(s.Graveyards[p]);
			}
		}
		
		/// <summary>
		/// Moves to next step.
		/// </summary>
		/// <returns>
		/// The to next step.
		/// </returns>
		public void MoveToNextStep()
		{
			// a little hackish in that it's implementation-dependent
			// but assuming that the Steps enum is sequential, this trick works
			int next = ((int)CurrStep+1) % Conversions.NUMBER_OF_STEPS;
			CurrStep = (Steps)next;
			foreach(var mp in ManaPools.Values)
			{
				mp.Clear();
			}
			switch(CurrStep)
			{
			case Steps.cleanup:
				env.currentPlayerIndex = ++env.currentPlayerIndex % env.Players.Count;
				PlayersTurn = env.Players[env.currentPlayerIndex];
				PlayerWithPriority = PlayersTurn;		
				LandsLeftToPlayThisTurn = 1;
				TurnNumber++;
				// TODO implement max hand size
				
				// no priority during cleanup so move to next step
				MoveToNextStep();
				break;
			case Steps.untap:				
				Untap();
				
				// no priority during untap so move to next step
				MoveToNextStep();
				break;
			case Steps.draw:
				Hands[PlayersTurn].Add(env.Libraries[PlayersTurn].Draw());
				
				// while dealing with simplified game, skip priority pass
				MoveToNextStep();
				break;
			case Steps.damage:
				DealDamage();
				
				// while dealing with simplified game, skip priority pass
				MoveToNextStep();
				break;
				
			case Steps.endCombat:
				if(env.AttackersToBlockersDictionary != null)
					env.AttackersToBlockersDictionary.Clear();
								
				// while dealing with simplified game, skip priority pass
				MoveToNextStep();
				break;
				
				// while dealing with simplified game, skip most priority passes
			case Steps.upkeep:
			case Steps.beginCombat: 
			case Steps.end:
				MoveToNextStep();
				break;
			}
		}
		
		/// <summary>
		/// Untaps each card controlled by the active player.
		/// </summary>
		public void Untap()
		{
			var cardsToUntap = from c in Battlefield
								where c.Controller == PlayersTurn
								select c;
			foreach(Card c in cardsToUntap)
			{
				c.Status &= ~Card.StatusEnum.Tapped;
			}
		}
		
		/// <summary>
		/// Returns a formatted string representation of this state suitable for printing to console.
		/// </summary>
		/// <returns>
		/// The state string.
		/// </returns>
		/// <param name='verbosity'>
		/// Verbosity level, 0 being the least verbose, 2 being the most verbose.
		/// </param>
		public string PrintState(int verbosity = 0)
		{
			if(verbosity < 0)
				verbosity = 0;
			if(verbosity > 2)
				verbosity = 2;
			
			if(verbosity != 0)
				throw new NotImplementedException("Have not implemented more verbose state printing");
			
			var sb = new System.Text.StringBuilder();
			sb.Append("Step: "); sb.Append(CurrStep.ToString());
			sb.Append(", AP: "); sb.Append(PlayersTurn.Name);
			sb.Append(", floating: "); sb.AppendLine(ManaPools[PlayerWithPriority].ToString());
			return sb.ToString();
		}
		
		private void DealDamage()
		{
			if(env.AttackersToBlockersDictionary == null
			   || env.AttackersToBlockersDictionary.Count == 0)
				return;
			
			foreach(CreatureCard attacker in env.AttackersToBlockersDictionary.Keys)
			{
				// unblocked
				if(env.AttackersToBlockersDictionary[attacker] == null
				   || env.AttackersToBlockersDictionary[attacker].Count() == 0)
				{
					this.LifeTotals[env.DefendingPlayer] -= attacker.P;
					Console.WriteLine(env.DefendingPlayer.Name + " takes " + attacker.P + " putting him at " + LifeTotals[env.DefendingPlayer]);
					continue;
				}
				
				// deal damage to each blocker sequentially and from each blocker to the attacker
				int damageToDeal = attacker.P;
				foreach(CreatureCard blocker in env.AttackersToBlockersDictionary[attacker])
				{
					if (blocker.T > damageToDeal)
					{
						blocker.DamageMarked += damageToDeal;
					}
					else
					{					
						blocker.DamageMarked += blocker.T;
						damageToDeal -= blocker.T;
					}
					
					attacker.DamageMarked += blocker.P;
				}
			}
		}
	}
}


using System;
using System.Linq;
using System.Collections.Generic;
using mlaSharp;
using log4net;
using System.Reflection;

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
		public Dictionary<Player,Library> Libraries { get; set; }
		
		private GameEngine env;		
		private readonly ILog logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
		
		public State (GameEngine env)
		{			
			TurnNumber = 0;
			Battlefield = new List<Card>();
			Stack = new List<StackObject>();
			ManaPools = new Dictionary<Player, ManaPool>();
			LifeTotals = new Dictionary<Player, int>();
			Hands = new Dictionary<Player, List<Card>>();
			Graveyards = new Dictionary<Player, List<Card>>();
			Libraries = new Dictionary<Player, Library>();
			
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
				Hands[PlayersTurn].Add(Libraries[PlayersTurn].Draw());
				if(PlayersTurn is ConsolePlayer)
				{
					Console.WriteLine(PlayersTurn.Name + " drew " + Hands[PlayersTurn].Last().Name + " for turn.");
				}
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
				AttackersDeclared = BlockersDeclared = false;
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
			
			int damageDealtToPlayer = 0;
			int creaturesUnblocked = 0;
			foreach(CreatureCard attacker in env.AttackersToBlockersDictionary.Keys)
			{
				// unblocked
				if(env.AttackersToBlockersDictionary[attacker] == null
				   || env.AttackersToBlockersDictionary[attacker].Count() == 0)
				{
					this.LifeTotals[env.DefendingPlayer] -= attacker.P;
					damageDealtToPlayer += attacker.P;
					creaturesUnblocked++;
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
			
			if(damageDealtToPlayer > 0)
				logger.Debug(String.Format("Player {0} takes {1} damage from {2} creatures, putting him to {3}",env.DefendingPlayer.Name, damageDealtToPlayer, creaturesUnblocked,LifeTotals[env.DefendingPlayer]));
					
		}
		
		/// <summary>
		/// Determines whether this instance is an equivalent state the specified other.
		/// </summary>
		/// <returns>
		/// <c>true</c> if this instance is an equivalent state the specified other; otherwise, <c>false</c>.
		/// </returns>
		/// <param name='other'>
		/// The other state to compare to.
		/// </param>
		public bool IsEquivalentState(State other)
		{
			if(other == this)
				return true;
			
			bool eq;
			eq = CurrStep == other.CurrStep
				&& PlayersTurn == other.PlayersTurn
				&& PlayerWithPriority == other.PlayerWithPriority
				&& AttackersDeclared == other.AttackersDeclared
				&& BlockersDeclared == other.BlockersDeclared
				&& LandsLeftToPlayThisTurn == other.LandsLeftToPlayThisTurn
				&& TurnNumber == other.TurnNumber
				&& Battlefield.Count == other.Battlefield.Count
				&& Stack.Count == other.Stack.Count;
			
			if( !eq)
				return false;
			
			foreach(Card c in Battlefield)
			{
				eq &= other.Battlefield.Contains(c);	
			}
			foreach(StackObject so in Stack)
			{
				eq &= other.Stack.Contains(so);
			}
			foreach(Player p in ManaPools.Keys)
			{
				if(!other.ManaPools.ContainsKey(p))
					return false;
				
				eq &= ManaPools[p] == other.ManaPools[p]
					&& LifeTotals[p] == other.LifeTotals[p];
				
				foreach(Card c in Hands[p])
				{
					eq &= other.Hands[p].Contains(c);
				}
				foreach(Card c in Graveyards[p])
				{
					eq &= other.Graveyards[p].Contains(c);
				}
			}
			return eq;
		}
		
		public override int GetHashCode()
		{
			int hash;
			hash = CurrStep.GetHashCode() 
				^ PlayersTurn.GetHashCode()
				^ PlayerWithPriority.GetHashCode()
				^ AttackersDeclared.GetHashCode()
				^ BlockersDeclared.GetHashCode()
				^ AttackersDeclared.GetHashCode()
				^ BlockersDeclared.GetHashCode()
				^ LandsLeftToPlayThisTurn
				^ TurnNumber
				^ Battlefield.Count
				^ Stack.Count;
			
			return hash;
		}
		
		
		public class StateEqComp : EqualityComparer<State>
		{
			public override bool Equals(State s1, State s2)
			{
				return s1.IsEquivalentState(s2);
			}
			
			public override int GetHashCode(State s)
			{
				return s.GetHashCode();
			}
		}
	}		
}


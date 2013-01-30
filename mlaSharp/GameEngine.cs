using System;
using System.Collections.Generic;
using System.Linq;

namespace mlaSharp
{
	public class GameEngine
	{
		public const int STARTING_HAND_SIZE = 7;
		public const int STARTING_LIFE_TOTAL = 20;
		
		public int currentPlayerIndex;
		private State CurrState {get; set;}
		
		public List<Player> Players { get; private set; }
		public Dictionary<Player,Library> Libraries { get; private set; }
		public IDictionary<CreatureCard,IList<CreatureCard>> AttackersToBlockersDictionary { get; private set; }		
		public Player DefendingPlayer {get; private set;}
		private Dictionary<Player,int> initialHandSize;
		public Random rng;
		
		private List<Player> lostPlayers;
		
		public GameEngine ()
		{
			Players = new List<Player>();
			Libraries = new Dictionary<Player, Library>();
			rng = new CryptoRandom();
		}
		
		public List<ActionDescriptionTuple> EnumActions(Player p)
		{
			var actions = new List<ActionDescriptionTuple>();
			
			// sanity checking
			for(int i = 0; i < CurrState.Hands[CurrState.PlayerWithPriority].Count; i++)
			{
				for ( int j = i + 1; j < CurrState.Hands[CurrState.PlayerWithPriority].Count; j++)
				{
					if(CurrState.Hands[CurrState.PlayerWithPriority][i] == CurrState.Hands[CurrState.PlayerWithPriority][j])
					{
						throw new Exception("Card " + i + " and " + j + " are duplicates in " + CurrState.PlayerWithPriority.Name + "'s hand");	
					}
				}
			}
			
			// if the stack is empty, and that player has priority
			if(p == CurrState.PlayersTurn 
			   && p == CurrState.PlayerWithPriority 
			   && CurrState.Stack.Count == 0)
			{
							
				// move to the next step
				var moveToNextStep = new ActionDescriptionTuple()
				{
					GameAction = (Player player, State s) => { s.MoveToNextStep(); },
					ActionDescription = "Move to the next step."
				};
				actions.Add(moveToNextStep);
				
				// for convience, an end turn action is available
				var endTurn = new ActionDescriptionTuple()
				{
					GameAction = (Player player, State s) => {
						do
							s.MoveToNextStep();
						while(s.CurrStep != Steps.main1);
					},
					ActionDescription = "Move to the next turn"
					
				};
				actions.Add(endTurn);
				
				// if it's a main phase, sorcery speed effects can be played
				if((CurrState.CurrStep == Steps.main1 || CurrState.CurrStep == Steps.main2))
				{
					// AP can play lands
					if(CurrState.LandsLeftToPlayThisTurn > 0)
					{
						var lands = from c in CurrState.Hands[CurrState.PlayerWithPriority]
									where c.Type.Contains("Land") 
									select c;
						
						foreach(Card land in lands)
						{
							var actionDescription = new ActionDescriptionTuple()
								{
									GameAction = 
										(Player player, State s) => {
											s.Battlefield.Add(land);
											s.Hands[player].Remove(land);
											land.ControlTimestamp = s.TurnNumber;
											land.Controller = player;
											s.LandsLeftToPlayThisTurn--;
										},
									ActionDescription =
										"Play a " + land.Name + " (land)"	
								};
							actions.Add(actionDescription);
						}
					}
					
					
					// Cast sorceries and creatures
					// TODO: make this work for sorceries
					ManaPool floating = CurrState.ManaPools[CurrState.PlayerWithPriority];
					var sorcerySpeedSpells = from c in CurrState.Hands[CurrState.PlayerWithPriority]
											 where c.Type.Contains("Creature")
											 where c.Cost.CanCast(floating)
											 select c;
					
					foreach(Card spell in sorcerySpeedSpells)
					{
						// this doesn't work for sorceries
						StackObject so = new StackObject( StackObject.StackObjectType.Card, spell.Type, spell.Text, spell.Owner, spell.Owner, spell.Colors, 
						                       (Player Player, State s) => { 
													s.Battlefield.Add(spell); 
													spell.ControlTimestamp = s.TurnNumber;
													spell.Controller = Player;
												});
						var actionDescription = new ActionDescriptionTuple()
							{
								GameAction = 
									(Player player, State s) => {
										s.Stack.Add(so);										
										s.Hands[player].Remove(spell);
										RemoveManaFromPool(s.ManaPools[p],spell.Cost);
									},
								ActionDescription =
									"Cast " + spell.Name + " (creature)"
							};
						actions.Add(actionDescription);					
					}
				}
				
				// -- combat --
				// declareAtk
				if(p == CurrState.PlayersTurn 
				   && CurrState.CurrStep == Steps.declareAtk
				   && !CurrState.AttackersDeclared)
				{
					var possibleAttackers = (from c in CurrState.Battlefield
						where c.Controller == CurrState.PlayersTurn
						where c.Type.Contains("Creature")
						where c.ControlTimestamp < CurrState.TurnNumber
						select c as CreatureCard)
						.ToList();
					
					if(possibleAttackers.Count() > 0)
					{
						// TODO: change implementation to allow attacking planeswalkers, multiplayer, etc
						var chosenAttackers = p.ChooseAttackers(possibleAttackers);
						
						// create attackers to blockers dictionary that will be passed to defending player
						AttackersToBlockersDictionary = new Dictionary<CreatureCard,IList<CreatureCard>>();
						foreach(var creature in chosenAttackers)
						{
							AttackersToBlockersDictionary[creature] = new List<CreatureCard>();
						}
						DefendingPlayer = Players[(currentPlayerIndex + 1) % Players.Count];
					}
				}
				
				if(p == CurrState.PlayersTurn
				   && CurrState.CurrStep == Steps.declareBlk
				   && AttackersToBlockersDictionary != null
				   && AttackersToBlockersDictionary.Count > 0
				   && !CurrState.BlockersDeclared)
				{					
					var possibleBlockers = (from c in CurrState.Battlefield
						where c.Controller == DefendingPlayer
						where c.Type.Contains("Creature")
						select c as CreatureCard)
						.ToList();	
					
					if(possibleBlockers.Count() > 0)
					{
						List<CreatureCard> blockersCopy;
						IDictionary<CreatureCard,IList<CreatureCard>> atbCopy;
						// query player for legal blocks
						do
						{
							// copy the data structures in case defending player generates invalid blocks
							blockersCopy = possibleBlockers.ToList();
							atbCopy = DeepCopyDictionary(AttackersToBlockersDictionary);
							// query player for blocks
							DefendingPlayer.ChooseBlockers(atbCopy, blockersCopy);
						} while (!LegalBlocks(atbCopy));
						
						AttackersToBlockersDictionary = atbCopy;
						p.OrderBlockers(AttackersToBlockersDictionary);
						
					}
				}
				
			}
			
			// "instant" speed effects, such as activated abilities, instants
			if( p == CurrState.PlayerWithPriority)
			{
				// check for activated abilities
				
				// note, this where clause assumes that only the controllers of cards can activate their abilities
				// not true for Oona's Prowler, Mindslaver, etc
				var abilitiesAvailable = from card in CurrState.Battlefield
						where card.Controller == CurrState.PlayerWithPriority
						from ab in card.ActivatedAbilities
							where ab.AbilityAvailable(p,CurrState)
						select Tuple.Create(card,ab);
				
				foreach(var tuple in abilitiesAvailable)
				{
					var abilityAction = new ActionDescriptionTuple()
					{
						GameAction = tuple.Item2.AbilityAction,
						ActionDescription = tuple.Item1.Name + "'s activated ability"
					};
					actions.Add (abilityAction);
					
				}	
				
				// TODO: allow casting instants
			}
			
			return actions;
		}
		
		
		
		public State GetCurrState()
		{
			return CurrState;
		}
		
		public void PerformAction(GameActionDelegate action)
		{
			action(CurrState.PlayerWithPriority,CurrState);
		}
		
		/// <summary>
		/// Adds the player to the game.
		/// </summary>
		/// <param name='p'>
		/// The player to add.
		/// </param>
		/// <param name='deck'>
		/// The initialized deck to add.  The deck will be shuffled before starting.
		/// </param>
		public void AddPlayer(Player p, Library deck)
		{
			Players.Add(p);
			Libraries[p] = deck;			
		}
		
		/// <summary>
		/// Starts the game.
		/// </summary>
		/// <returns>
		/// The winning player, or null if the game ends in a draw.
		/// </returns>
		/// <exception cref='NotImplementedException'>
		/// Is thrown when a requested operation is not implemented for a given type.
		/// </exception>
		public Player StartGame()
		{
			if(Players.Count != 2)
				throw new NotImplementedException("The game engine currently only supports exactly 2 players");
			
			int n = Players.Count;
			
			var playOrder = Enumerable.Range(0,n).ToList();
			playOrder.Shuffle(rng);
			int k = playOrder[0];
			currentPlayerIndex = k;
			
			System.Console.WriteLine(String.Format("Game started.  Player {1} plays first.",k+1,Players[k].Name));
			
			// initialize state
			CurrState = new State(this);
			CurrState.PlayersTurn = Players[k];
			CurrState.PlayerWithPriority = Players[k];
			CurrState.CurrStep = Steps.main1;
			CurrState.LandsLeftToPlayThisTurn = 1;
			
			// shuffle libraries & initialize state dictionaries
			foreach(Player p in Players)
			{
				Libraries[p].Shuffle(rng);
				CurrState.ManaPools[p] = new ManaPool();
				CurrState.LifeTotals[p] = STARTING_LIFE_TOTAL;
				CurrState.Hands[p] = new List<Card>();
				CurrState.Graveyards[p] = new List<Card>();
			}
			
			lostPlayers = new List<Player>();
			
			// draw opening hands and resolve mulligans
			// create an ordered list of players who have not kept
			List<Player> notKept = new List<Player>(n);
			// create a dictionary of player -> hand size to keep track of how many cards to mulligan to
			initialHandSize = new Dictionary<Player, int>(n);
			// initialize both structures
			for(int i = 0; i < n; i++)
			{
				Player pcurr = Players[(i+k)%n];
				notKept.Add(pcurr);
				initialHandSize[pcurr] = STARTING_HAND_SIZE;
			}
			
			int j = 0;
			while(notKept.Count > 0)
			{
				Player p = notKept[j];
				Libraries[p].AddRange(CurrState.Hands[p]);
				CurrState.Hands[p].Clear();
				var hand = Libraries[p].Draw (initialHandSize[p]);
				CurrState.Hands[p].AddRange(hand);
								
				if(!p.MulliganHand())
				{
					notKept.Remove (p);
				}
				else if (--initialHandSize[p] <= 0)
				{
					notKept.Remove(p);
				}				
				
				
				j = (j+1) %  ((notKept.Count > 0) ? notKept.Count : 1);	
			}
			
			return MainGameLoop();
		}
		
		/// <summary>
		/// The main game loop.
		/// On every iteration, checks state-based actions, then polls the player with priority for an action.
		/// </summary>
		/// <returns>
		/// The winning player, or null if a draw.
		/// </returns>
		private Player MainGameLoop()
		{
			while(true)
			{
				// check for players losing
				IEnumerable<Player> losingPlayers = CheckPlayerLost();
				if(losingPlayers.Count() > 0)
				{
					foreach(Player p in losingPlayers)
					{
						Console.WriteLine(String.Format("Player {0} lost!",p.Name));
						lostPlayers.Add(p);
						Players.Remove(p);
					}
				}
				
				DoSBA();
				
				var action = CurrState.PlayerWithPriority.GetAction();
				
				try
				{
					action(CurrState.PlayerWithPriority,CurrState);
				
				} catch(PlayerLostException ex)
				{
					Player p = ex.losingPlayer;
					Console.WriteLine(String.Format("Player {0} lost from decking!",p.Name));
					lostPlayers.Add(p);
					Players.Remove(p);
				}
				
				// if no more than 1 player is left, the game has ended
				if(Players.Count <= 1)
				{
					Player winningPlayer = null;
					if(Players.Count == 0)
						Console.WriteLine("The game ends in a draw.");
					else
					{
						winningPlayer = Players.First();
						Console.WriteLine("Player {0} has won!",winningPlayer.Name);
					}
					
					return winningPlayer;
				}
				
				// because there is no priority passing right now, simply resolve the stack if it's not empty
				while(CurrState.Stack.Count > 0)
				{
					Resolve(CurrState.Stack.First());	
				}
			}
			
			//throw new ApplicationException("Should not have reached this code point, invalid exit from main loop");
			return null;
		}
		
		/// <summary>
		/// Performs State-based actions (except for a player losing).
		/// </summary>
		private void DoSBA()
		{
			// Check for lethal damage
			var creatures = from c in CurrState.Battlefield
							where c.Type.Contains("Creature")
							select c as CreatureCard;
			
			// have to separate iterating through the enumeration and removing
			var markedForDestruction = new HashSet<CreatureCard>();
			foreach ( CreatureCard c in creatures)
			{
				if(c.DamageMarked >= c.T)
				{
					markedForDestruction.Add(c);
				}
			}
			
			foreach(CreatureCard c in markedForDestruction)
			{
				CurrState.Battlefield.Remove(c);
				CurrState.Graveyards[c.Owner].Add(c);
				// TODO: check dying triggers?
				
				// for debugging, print deaths
				Console.WriteLine(c.Name + ", owned by " + c.Owner + ", died.");
			}
			
			/// TODO: check for other SBAs
			
			// debug : sanity check battlefield for duplicates
			for(int i = 0; i < CurrState.Battlefield.Count; i++)
			{
				for ( int j = i + 1; j < CurrState.Battlefield.Count; j++)
				{
					if(CurrState.Battlefield[i] == CurrState.Battlefield[j])
					{
						throw new Exception("Battlefield should not have duplicates");	
					}
				}
			}
		}
		
		/// <summary>
		/// Returns a list of players that have lost.
		/// </summary>
		/// <returns>
		/// The players who have lost, or an empty list if none have lost.
		/// </returns>
		private IEnumerable<Player> CheckPlayerLost()
		{
			List<Player> lost = new List<Player>();
			foreach(Player p in Players)
			{
				if(CurrState.LifeTotals[p] <= 0)
					lost.Add(p);
			}
			return lost;
		}
		
		private void RemoveManaFromPool(ManaPool pool, ManaCost cost)
		{
			// pay colored costs
			var ColorsEnumValues = Enum.GetValues(typeof(ColorsEnum));
			foreach(ColorsEnum color in ColorsEnumValues)
			{
				pool[color] -= cost[color];
			}
			
			// pay generic costs
			int genericCost = cost.GenericMana;
			
			// if can pay with only generic mana, do so
			if(pool.Generic - genericCost >= 0)
			{
				pool.Generic -= genericCost;
				return;
			}
			
			// otherwise, pay for generic costs by looking through each color
			genericCost -= pool.Generic;
			pool.Generic = 0;
			foreach(ColorsEnum color in ColorsEnumValues)
			{
				// if there's enough mana in the current color to pay the rest of the cost,
				// pay it and break
				// otherwise, subtract what we can
				if(pool[color] - genericCost >= 0)
				{
					pool[color] -= genericCost;
					genericCost = 0;
					break;
				}
				
				genericCost -= pool[color];
				pool[color] -= pool[color];				
			}
		}
		
		private void Resolve(StackObject spell)
		{
			/*
			// resolve instants and sorceries
			if(spell.Type.Contains("Instant") || spell.Type.Contains("Sorcery"))
			{
				throw new NotImplementedException("Have not implemented resolving instants and sorceries");	
				return;
			}
			
			// resolve permanents
			CurrState.Stack.Remove(spell);
			CurrState.Battlefield.Add(spell);
			foreach(var triggeredAbility in spell.TriggeredAbilities)
			{
				if(triggeredAbility.AbilityAvailable(spell.Owner,CurrState))
				{
					CurrState.Stack.Add(new StackObject(	
				}
			}
			*/
			spell.ResolutionAction(spell.Owner,CurrState);
			CurrState.Stack.Remove(spell);
		}
		
		private bool LegalBlocks(IDictionary<CreatureCard,IList<CreatureCard>> attackersToBlockersDictionary)
		{
			// doesn't take into account creatures that can block multiple attackers, banding, etc
			HashSet<CreatureCard> creaturesInCombat = new HashSet<CreatureCard>();
			foreach(CreatureCard attacker in attackersToBlockersDictionary.Keys)
			{
				if(!creaturesInCombat.Add(attacker))
				{
					Console.WriteLine("Illegal block!");
					return false;
				}
				foreach(var blocker in attackersToBlockersDictionary[attacker])
				{
					if(!creaturesInCombat.Add(blocker))
					{
						Console.WriteLine("Illegal block!");
						return false;
					}
				}
			}
			return true;
		}
		
		/// <summary>
		/// Deep-copies dictionary, creating new references for the block lists.
		/// </summary>
		/// <returns>
		/// The copied dictionary.
		/// </returns>
		/// <param name='dict'>
		/// Dictionary to copy.
		/// </param>
		/// <remarks>
		/// Use this instead of the copy constructor when the value references have to be distinct.
		/// </remarks>
		public IDictionary<CreatureCard, IList<CreatureCard>> DeepCopyDictionary (IDictionary<CreatureCard, IList<CreatureCard>> dict)
		{
			var copy = new Dictionary<CreatureCard,IList<CreatureCard>>();
			foreach(var key in dict.Keys)
			{
				copy[key] = new List<CreatureCard>(dict[key]);
			}
			return copy;
		}
		
		/// <summary>
		/// Shuffles player p's library.
		/// </summary>
		/// <param name='p'>
		/// The player whose library to shuffle.
		/// </param>
		public void ShuffleLibrary(Player p)
		{
			Libraries[p].Shuffle(rng);
		}
	}
	
	
}


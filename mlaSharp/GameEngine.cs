using System;
using System.Collections.Generic;
using System.Linq;
using log4net;
using log4net.Config;
using System.Reflection;
using System.Text;

namespace mlaSharp
{
	public class GameEngine
	{
		public const int STARTING_HAND_SIZE = 7;
		public const int STARTING_LIFE_TOTAL = 20;
		
		public int currentPlayerIndex;
		public bool SimulationMode = false;
		
		
		public List<Player> Players { get; private set; }
		public IDictionary<CreatureCard,IList<CreatureCard>> AttackersToBlockersDictionary { get; private set; }		
		public Player DefendingPlayer {get; private set;}
		private Dictionary<Player,int> initialHandSize;
		public Random rng;
		public Player StartingPlayer;
		
		private List<Player> lostPlayers;
		private State _currState;
		private readonly ILog logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
		
		public GameEngine ()
		{
			Players = new List<Player>();
			rng = new CryptoRandom();		
			_currState = new State(this);			
			_currState.TurnNumber = -1;
		}
		
		/// <summary>
		/// Initializes a new instance of the <see cref="mlaSharp.GameEngine"/> class used by CloneToNewPlayers
		/// </summary>
		/// <param name='playerCount'>
		/// Player count.
		/// </param>
		/// <param name='rng'>
		/// Rng.
		/// </param>
		private GameEngine (int playerCount, Random rng)
		{
			Players = new List<Player>(playerCount);
			initialHandSize = new Dictionary<Player, int>(playerCount);
			this.rng = rng;
			_currState = new State(this);
			_currState.TurnNumber = -1;
		}
		
		private GameEngine(GameEngine toCopy)
		{
			Players = new List<Player>(toCopy.Players);
			initialHandSize = new Dictionary<Player, int>(toCopy.initialHandSize);
			StartingPlayer = toCopy.StartingPlayer;
			DefendingPlayer = toCopy.DefendingPlayer;
			_currState = toCopy._currState;
			currentPlayerIndex = toCopy.currentPlayerIndex;
			rng = toCopy.rng;	// don't need to recreate RNGs
			
			if(toCopy.AttackersToBlockersDictionary != null)
				AttackersToBlockersDictionary = new Dictionary<CreatureCard, IList<CreatureCard>>(toCopy.AttackersToBlockersDictionary);
			if(toCopy.lostPlayers != null)
				lostPlayers = new List<Player>(toCopy.lostPlayers);
		}
		
		/// <summary>
		/// Gets or sets the current game state.
		/// </summary>
		/// <value>
		/// The current game state.
		/// </value>
		/// <exception cref='ApplicationException'>
		/// <see cref="T:System.ApplicationException" /> is thrown if an attempt to set CurrState is made while not in Simulation Mode.
		/// </exception>
		public State CurrState
		{
			get { return _currState; }
			set
			{
				if(!SimulationMode)
					throw new ApplicationException("The game engine must be in simulation mode to set game state");
				_currState = value;
			}
		}
		
		/// <summary>
		/// Enumerate all possible actions from CurrState for a given player.
		/// </summary>
		/// <returns>
		/// The list of actions.
		/// </returns>
		/// <param name='p'>
		/// The player for whom to enumerate the actions.
		/// </param>
		public List<ActionDescriptionTuple> EnumActions(Player p)
		{
			var actions = new List<ActionDescriptionTuple>();
					
			
			// if the stack is empty, and that player has priority
			if(p == CurrState.PlayersTurn 
			   && p == CurrState.PlayerWithPriority 
			   && CurrState.Stack.Count == 0)
			{
							
				#region Attacks
				// declareAtk
				if(p == CurrState.PlayersTurn 
				   && CurrState.CurrStep == Steps.declareAtk
				   && !CurrState.AttackersDeclared)
				{					
					var possibleAttackers = (from c in CurrState.Battlefield
						where c.Controller == CurrState.PlayersTurn
						where c.Type.Contains("Creature")
						where c.ControlTimestamp < CurrState.TurnNumber	// TODO: represent summoning sickness correctly
					    where (c.Status & Card.StatusEnum.Tapped) == 0		// i.e. untapped
						select c as CreatureCard)
						.ToList();
					
					if(possibleAttackers.Count() > 0)
					{
						AttackersToBlockersDictionary = new Dictionary<CreatureCard,IList<CreatureCard>>();						
						DefendingPlayer = Players[(currentPlayerIndex + 1) % Players.Count];
						
						// TODO: make choosing attackers actually consistent, fix this hack
						// present the list of actions as comprising only of choosing attackers
						// warning - number of actions is exponential in number of attackers
						if(true || p is MctsPlayer)
						{
							var allPossibleAttacks = possibleAttackers.PowerSet();
							StringBuilder descSb = new StringBuilder();
							foreach(var atkSubset in allPossibleAttacks)
							{		
								var atkSubsetClosureVariable = atkSubset;	// needed so the closures point to distinct subsets instead of just the last
								descSb.Clear();
								descSb.Append("Attack with ");
								foreach(CreatureCard c in atkSubset)
								{
									descSb.Append(c.Name);
									descSb.Append(", ");
								}
								if(atkSubset.Count == 0)
									descSb.Append("nothing");	// complete description for attacking with nothing
								else
									descSb.Remove(descSb.Length-2,2);	// remove trailing ", "
								
								var possibleAttack = new ActionDescriptionTuple()
								{
									GameAction = (Player player, State s) =>
										{
											foreach(CreatureCard c in atkSubsetClosureVariable) {
												AttackersToBlockersDictionary[c] = null;
												c.Status |= Card.StatusEnum.Tapped;		// TODO: allow for vigilance
											}
											// TODO: allow "fast" effects to be played, but for now, skip priority pass
											
											// Give "priority" to defending player so they can assign blocks
											CurrState.PlayerWithPriority = DefendingPlayer;
											s.MoveToNextStep();
										},
									ActionDescription = descSb.ToString()
								};
								actions.Add(possibleAttack);
							}							
							CurrState.AttackersDeclared = true;
							
							return actions;
						}
						
						// TODO: change implementation to allow attacking planeswalkers, multiplayer, etc
						var chosenAttackers = p.ChooseAttackers(possibleAttackers);
						
						// create attackers to blockers dictionary that will be passed to defending player
						string attackersMsg = "Player " + p.Name + " attacks with ";
						
						foreach(var creature in chosenAttackers)
						{
							AttackersToBlockersDictionary[creature] = new List<CreatureCard>();
							
							attackersMsg += creature.Name + ", ";
						}
						if(chosenAttackers.Count > 0)
							logger.Debug(attackersMsg);
					}
				}
				#endregion
				
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
						var spellClosureVar = spell; // needed so the closure refers to this spell, not just the last one.
						// this doesn't work for sorceries
						StackObject so = new StackObject( StackObject.StackObjectType.Card, spell.Type, spell.Text, spell.Owner, spell.Owner, spell.Colors, 
						                       (Player Player, State s) => { 
													s.Battlefield.Add(spellClosureVar); 
													spellClosureVar.ControlTimestamp = s.TurnNumber;
													spellClosureVar.Controller = Player;
												});
						var actionDescription = new ActionDescriptionTuple()
							{
								GameAction = 
									(Player player, State s) => {
										s.Stack.Add(so);										
										s.Hands[player].Remove(spellClosureVar);
										RemoveManaFromPool(s.ManaPools[p],spellClosureVar.Cost);
									},
								ActionDescription =
									"Cast " + spellClosureVar.Name + " (creature)"
							};
						actions.Add(actionDescription);					
					}
				}
				
				
				
			}
			
			#region Blocks
			if(CurrState.AttackersDeclared
			   	   && p == DefendingPlayer
				   && CurrState.CurrStep == Steps.declareBlk
				   && !CurrState.BlockersDeclared)
			{					
				var possibleBlockers = (from c in CurrState.Battlefield
					where c.Controller == DefendingPlayer
					where c.Type.Contains("Creature")
				    where (c.Status & Card.StatusEnum.Tapped) == 0
					select c as CreatureCard)
					.ToList();	
				
				if(AttackersToBlockersDictionary == null)
					throw new NullReferenceException("AttackersToBlockersDictionary was unexpectedly null");
				
				if(possibleBlockers.Count() == 0)	// give "priority" back to active player
				{
					actions.Add(new ActionDescriptionTuple()
					           {
									GameAction = (Player player, State s) =>
										{
											CurrState.BlockersDeclared = true;
											CurrState.PlayerWithPriority = CurrState.PlayersTurn;
											
											// TODO: while there are no "fast" effects, skip priority pass
											CurrState.MoveToNextStep();
										},
									ActionDescription = "No blocks"
					});
				}
				else
				{
					// TODO: make choosing blockers actually consistent, fix this hack
					// present the list of actions as comprising only of choosing blocks
					// warning - number of actions is exponential in the number of attackers AND blockers
					if(true || p is MctsPlayer)
					{
						var attackers = AttackersToBlockersDictionary.Keys.ToList();
						var possibleBlocks = GetPossibleBlocks(attackers,possibleBlockers);
						StringBuilder descSb = new StringBuilder();
						foreach(var block in possibleBlocks)
						{
							var blockClosureVariable = block;	// needed so the closures point to distinct subsets instead of just the last
							descSb.Clear();
							GameActionDelegate ga = (Player player, State s) => {
								foreach(var t in blockClosureVariable)
									AttackersToBlockersDictionary[t.Attacker] = t.Blockers;
								// TODO: allow "fast" effects to be played, but for now, skip priority pass
								
								// give priority back to active player
								CurrState.PlayerWithPriority = CurrState.PlayersTurn;
								s.MoveToNextStep();
							};
									
							foreach(var atbTuple in block)
							{		
								AttackersToBlockersDictionary[atbTuple.Attacker] = atbTuple.Blockers;
								descSb.Append("Block ");
								descSb.Append(atbTuple.Attacker.ToString());
								descSb.Append(" with ");
								if(atbTuple.Blockers.Count == 0)
									descSb.Append("nothing; ");
								else
								{
									foreach(var blocker in atbTuple.Blockers)
									{
										descSb.Append(blocker.ToString());
										descSb.Append(" & ");
									}
									descSb.Remove(descSb.Length-3,3);	// remove trailing " & "
									descSb.Append("; ");
								}
							}
							//descSb.Remove(descSb.Length-2,2);	// remove trailing "; "
							
							var possibleBlockDescription = new ActionDescriptionTuple() {
								GameAction = ga,
								ActionDescription = descSb.ToString()
							};
							actions.Add(possibleBlockDescription);
						}
						CurrState.BlockersDeclared = true;
						
						return actions;
					}
					
					List<CreatureCard> blockersCopy;
					IDictionary<CreatureCard,IList<CreatureCard>> atbCopy;
					// query player for legal blocks
					do
					{
						// copy the data structures in case defending player generates invalid blocks
						blockersCopy = possibleBlockers.ToList();
						atbCopy = AttackersToBlockersDictionary.DeepCopy();
						// query player for blocks
						DefendingPlayer.ChooseBlockers(atbCopy, blockersCopy);
					} while (!LegalBlocks(atbCopy));
					
					AttackersToBlockersDictionary = atbCopy;
					p.OrderBlockers(AttackersToBlockersDictionary);
					
					// display blocks
					string blockersMsg = "Player " + DefendingPlayer.Name + " blocks:\n";
					foreach(var attacker in AttackersToBlockersDictionary.Keys)
					{
						// only display attackers that are blocked
						if(AttackersToBlockersDictionary[attacker].Count > 0)
						{
							blockersMsg += attacker.Name + " is blocked by ";
							foreach(var blockers in AttackersToBlockersDictionary[attacker])
							{
								blockersMsg += blockers.Name + ", ";	
							}
							blockersMsg += "\n";
						}
					}
					logger.Debug(blockersMsg);
				}
			}
			#endregion
			
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
			_currState.Libraries[p] = deck;			
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
			this.StartingPlayer = Players[k];
				
			logger.Info(String.Format("Game started.  Player {1} plays first.",k+1,Players[k].Name));
			
			// initialize state
			CurrState.PlayersTurn = Players[k];
			CurrState.PlayerWithPriority = Players[k];
			CurrState.CurrStep = Steps.main1;
			CurrState.LandsLeftToPlayThisTurn = 1;
			CurrState.TurnNumber = 1;
			
			// shuffle libraries & initialize state dictionaries
			foreach(Player p in Players)
			{
				CurrState.Libraries[p].Shuffle(rng);
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
				CurrState.Libraries[p].AddRange(CurrState.Hands[p]);
				CurrState.Hands[p].Clear();
				var hand = CurrState.Libraries[p].Draw (initialHandSize[p]);
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
		/// Starts and runs the game to completion from a given starting state.
		/// </summary>
		/// <returns>
		/// The winning player, or null if a draw.
		/// </returns>
		/// <param name='startingState'>
		/// Starting state.
		/// </param>
		public Player StartGame(State startingState)
		{
			if(startingState.TurnNumber == 0)
				return StartGame();
			
			_currState = startingState;
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
						logger.Info(String.Format("Player {0} lost!",p.Name));
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
					logger.Info(String.Format("Player {0} lost from decking!",p.Name));
					lostPlayers.Add(p);
					Players.Remove(p);
				}
				
				// if no more than 1 player is left, the game has ended
				if(Players.Count <= 1)
				{
					Player winningPlayer = null;
					if(Players.Count == 0)
						logger.Info("The game ends in a draw.");
					else
					{
						winningPlayer = Players.First();
						logger.Info(String.Format("Player {0} has won!",winningPlayer.Name));
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
				logger.Debug(c.Name + ", owned by " + c.Owner + ", died.");
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
					logger.Debug("Illegal block!");
					return false;
				}
				foreach(var blocker in attackersToBlockersDictionary[attacker])
				{
					if(!creaturesInCombat.Add(blocker))
					{
						logger.Debug("Illegal block!");
						return false;
					}
				}
			}
			return true;
		}

		public static IEnumerable<BlockDescription> GetPossibleBlocks (IList<CreatureCard> attackers, IList<CreatureCard> possibleBlockers)
		{
			var allBlocksList = new List<BlockDescription>();
			var blockersToUse = possibleBlockers.PowerSet();	
			
			
			allBlocksList.Add(new BlockDescription(attackers));
			//GetPossibleBlocksAux(attackers,possibleBlockers,possibleBlockers.Count - 1, emptyBlocks, allBlocksList);
			foreach(var blkSet in blockersToUse)
			{
				if(blkSet.Count == 0)
					continue;
				
				BlockDescription emptyBlocks = new BlockDescription(attackers);		
				GetPossibleBlocksAux(attackers,blkSet,blkSet.Count-1,emptyBlocks,allBlocksList);
			}
			
			return allBlocksList;
		}
		
		/// <summary>
		/// Recursive helper function to get the possible blocks and add them to blockDescriptions.
		/// </summary>
		/// <param name='atkSet'>
		/// List of attackers to consider.
		/// </param>
		/// <param name='blkSet'>
		/// List of blockers to consider.
		/// </param>
		/// <param name='blkSetEndIndex'>
		/// The index of the last item in blkSet not yet assigned.
		/// </param>
		/// <param name='parent'>
		/// The parent <see cref="BlockDescription"/> for the current recursive call to consider.
		/// </param>
		/// <param name='blockDescriptions'>
		/// A list that will be filled with the <see cref="BlockDescription"/>s built
		/// </param>
		private static void GetPossibleBlocksAux(IList<CreatureCard> atkSet, IList<CreatureCard> blkSet, int blkSetEndIndex, BlockDescription parent, IList<BlockDescription> blockDescriptions)
		{
			// base case - no more blockers to assign, so add this block description to the list
			if(blkSetEndIndex < 0)
			{
				blockDescriptions.Add(parent);
				return;
			}
			
			// contraction - remove last blocker and assign it to all possible attackers
			CreatureCard lastBlocker = blkSet[blkSetEndIndex--];			
			
			foreach(var attacker in atkSet)
			{
				var newBlockingChoice = parent.DeepCopy();
				newBlockingChoice[attacker].Blockers.Add(lastBlocker);
				GetPossibleBlocksAux(atkSet,blkSet,blkSetEndIndex,newBlockingChoice,blockDescriptions);
			}
		}
				
		/// <summary>
		/// Shuffles player p's library.
		/// </summary>
		/// <param name='p'>
		/// The player whose library to shuffle.
		/// </param>
		public void ShuffleLibrary(Player p)
		{
			CurrState.Libraries[p].Shuffle(rng);
		}
		
		public GameEngine Clone()
		{
			return new GameEngine(this);
		}
		
		/// <summary>
		/// Clones the game engine and current state to new players of type T.
		/// </summary>
		/// <returns>
		/// A new game engine with everything mapped to the new players.
		/// </returns>
		/// <param name='T'>
		/// The type of the new players to create.
		/// </param>
		public GameEngine CloneToNewPlayers<T>() where T : Player
		{
			
			GameEngine newEngine = new GameEngine(this.Players.Count,this.rng);
			newEngine.currentPlayerIndex = currentPlayerIndex;
			
			// copy over state
			for(int i = 0; i < Players.Count; i++)
			{
				Player op = Players[i];
				Player np = Activator.CreateInstance<T>();
				np.Env = newEngine;
				np.Name = op.Name;
				
				if(op == this.StartingPlayer)
					newEngine.StartingPlayer = np;
				if(op == this.DefendingPlayer)
					newEngine.DefendingPlayer = np;
				
				newEngine.Players[i] = np;
				newEngine.initialHandSize[np] = initialHandSize[op];
				newEngine._currState.ManaPools[np] = new ManaPool(_currState.ManaPools[op]);	
				newEngine._currState.LifeTotals[np] = _currState.LifeTotals[op];
				newEngine._currState.Hands[np] = new List<Card>(_currState.Hands[op]);
				newEngine._currState.Graveyards[np] = new List<Card>(_currState.Graveyards[op]);
			}
			
			if(AttackersToBlockersDictionary != null)
				newEngine.AttackersToBlockersDictionary = new Dictionary<CreatureCard, IList<CreatureCard>>(AttackersToBlockersDictionary);
			if(lostPlayers != null)
				newEngine.lostPlayers = new List<Player>(lostPlayers);
			
			return newEngine;
		}
		
	}
	
	
}


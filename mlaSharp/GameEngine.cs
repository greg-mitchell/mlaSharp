using System;
using System.Collections.Generic;
using System.Linq;

namespace mlaSharp
{
	public class GameEngine
	{
		public const int STARTING_HAND_SIZE = 7;
		public const int STARTING_LIFE_TOTAL = 20;
		
		private State CurrState {get; set;}
		
		public List<Player> Players { get; private set; }
		private Dictionary<Player,Library> libraries;
		private Dictionary<Player,int> initialHandSize;
		private Random rng;
		
		private int currentPlayerIndex;
		private List<Player> lostPlayers;
		
		public GameEngine ()
		{
			CurrState = new State(this);
			Players = new List<Player>();
			libraries = new Dictionary<Player, Library>();
			rng = new CryptoRandom();
		}
		
		public List<ActionDescriptionTuple> EnumActions(Player p)
		{
			var actions = new List<ActionDescriptionTuple>();
			// if the stack is empty, and that player has priority
			if(p == CurrState.PlayersTurn 
			   && p == CurrState.PlayerWithPriority 
			   && CurrState.Stack.Count == 0)
			{
				
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
											land.Controller = player;
											s.LandsLeftToPlayThisTurn--;
										},
									ActionDescription =
										"Play a " + land.Name	
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
						StackObject so = new StackObject( StackObject.StackObjectType.Card, spell.Type, spell.Text, spell.Owner, spell.Controller, spell.Colors, 
						                       (Player Player, State s) => { s.Battlefield.Add(spell); spell.Controller = Player;});
						var actionDescription = new ActionDescriptionTuple()
							{
								GameAction = 
									(Player player, State s) => {
										s.Stack.Add(so);										
										s.Hands[player].Remove(spell);
										// TODO: remove mana from mana pool
									},
								ActionDescription =
									"Cast " + spell.Name	
							};
						actions.Add(actionDescription);					
					}
				}
				
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
					GameAction = (Player Player, State s) => {
						s.MoveToNextStep();
						currentPlayerIndex = ++currentPlayerIndex % Players.Count;
						s.PlayersTurn = Players[currentPlayerIndex];
						s.PlayerWithPriority = s.PlayersTurn;						
						s.Untap();
						s.Hands[s.PlayersTurn].Add(libraries[s.PlayersTurn].Draw());
						s.CurrStep = Steps.main1;
						s.LandsLeftToPlayThisTurn = 1;
					},
					ActionDescription = "Move to the next turn"
					
				};
				actions.Add(endTurn);
			}
			
			// "instant" speed effects, such as activated abilities, instants
			if( p == CurrState.PlayerWithPriority)
			{
				// check for activated abilities
				foreach(Card c in CurrState.Battlefield)
				{
					// note, this assumes that only the controllers of cards can activate their abilities
					// not true for Oona's Prowler, Mindslaver, etc
					if(c.Controller != p)
						continue;
					
					foreach(var ability in c.ActivatedAbilities)
					{
						if(ability.AbilityAvailable(p,CurrState))
						{
							var abilityAction = new ActionDescriptionTuple()
							{
								GameAction = ability.AbilityAction,
								ActionDescription = c.Name + "'s activated ability"
							};
							actions.Add (abilityAction);
						}
					}
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
			libraries[p] = deck;			
		}
		
		public void StartGame()
		{
			if(Players.Count != 2)
				throw new NotImplementedException("The game engine currently only supports exactly 2 players");
			
			int n = Players.Count;
			
			var playOrder = Enumerable.Range(0,n).ToList();
			playOrder.Shuffle(rng);
			int k = playOrder[0];
			currentPlayerIndex = k;
			
			System.Console.WriteLine(String.Format("Game started.  Player {0} plays first.",k+1,Players[k].Name));
			
			// initialize state
			CurrState.PlayersTurn = Players[k];
			CurrState.PlayerWithPriority = Players[k];
			CurrState.CurrStep = Steps.main1;
			CurrState.LandsLeftToPlayThisTurn = 1;
			
			// shuffle libraries & initialize state dictionaries
			foreach(Player p in Players)
			{
				libraries[p].Shuffle(rng);
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
				libraries[p].AddRange(CurrState.Hands[p]);
				CurrState.Hands[p].Clear();
				var hand = libraries[p].Draw (initialHandSize[p]);
				CurrState.Hands[p].AddRange(hand);
								
				if(!p.MulliganHand())
				{
					notKept.Remove (p);
				}
				else
				{
					initialHandSize[p]--;
				}				
				
				
				j = (j+1) %  ((notKept.Count > 0) ? notKept.Count : 1);	
			}
			
			MainGameLoop();
		}
		
		/// <summary>
		/// The main game loop.
		/// On every iteration, checks state-based actions, then polls the player with priority for an action.
		/// </summary>
		private void MainGameLoop()
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
					
				}
				
				// if no more than 1 player is left, the game has ended
				if(Players.Count <= 1)
				{
					if(Players.Count == 0)
						Console.WriteLine("The game ends in a draw.");
					else
						Console.WriteLine("Player {0} has won!",Players.First().Name);
					
					break;
				}
				
				// because there is no priority passing right now, simply resolve the stack if it's not empty
				while(CurrState.Stack.Count > 0)
				{
					Resolve(CurrState.Stack.First());	
				}
			}
		}
		
		/// <summary>
		/// Performs State-based actions (except for a player losing).
		/// </summary>
		private void DoSBA()
		{
			/// TODO: check for SBAs
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
		
		/// <summary>
		/// Shuffles player p's library.
		/// </summary>
		/// <param name='p'>
		/// The player whose library to shuffle.
		/// </param>
		public void ShuffleLibrary(Player p)
		{
			libraries[p].Shuffle(rng);
		}
	}
	
	
	/// <summary>
	/// A function that performs a game action
	/// </summary>
	public delegate void GameActionDelegate(Player p, State s);
	
	/// <summary>
	/// Action description tuple.
	/// </summary>
	public struct ActionDescriptionTuple
	{
		public GameActionDelegate GameAction;
		public String ActionDescription;
	}
	
	/// <summary>
	/// Sequential steps in the game
	/// </summary>
	public enum Steps
	{
		untap,
		upkeep,
		draw,
		main1,
		beginCombat,
		declareAtk,
		declareBlk,
		damage,
		endCombat,
		main2,
		end,
		cleanup
	}
}


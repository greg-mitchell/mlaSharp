using System;
using System.Collections.Generic;
using System.Linq;

namespace mlaSharp
{
	public class ConsolePlayer : Player
	{
		public ConsolePlayer(GameEngine env, string name = "")
			: base(env, name)
		{ }
		
		public override bool MulliganHand()
		{
			Console.Write("Player " + this.Name + "'s Hand:");
			foreach(Card c in Env.CurrState.Hands[this])
				Console.Write(String.Format("\n\t{0},",c.Name));
			Console.WriteLine();
			while(true)
			{
				Console.WriteLine("Mulligan hand? (y/n):");
				Console.Write ("mla> ");
				string input = Console.In.ReadLine().ToLower().Trim();
				if(input == "n")
					return false;
				if(input == "y")
					return true;
			}
		}
		
		public override GameActionDelegate GetAction() 
		{
			var actions = Env.EnumActions(this);
			PrintActions(actions);
			
			Console.WriteLine ();
			int actionNum = -1;
			while(true)
			{
				Console.Write("mla> ");
				string input = Console.In.ReadLine().Trim();
				
				// if user simply presses enter or white space, repeat list of actions
				if(String.IsNullOrEmpty(input))
				{
					PrintActions(actions);
					continue;
				}
				
				// attempt to parse as a number.  If is a number in range, break from loop
				if(Int32.TryParse(input,out actionNum))
				{
					if(actionNum >= 0 && actionNum < actions.Count())
						break;
					
					Console.Write("Select an action from 0 to ");
					Console.WriteLine(actions.Count() - 1);
					continue;
				}
				
				ParseCommands(input);
			}
			return actions[actionNum].GameAction;
		}
		
		public override ISet<CreatureCard> ChooseAttackers (IList<CreatureCard> possibleAttackers)
		{
			if (possibleAttackers.Count() == 0)
				return null;
			
			var chosenSet = new HashSet<CreatureCard>();
			PrintPossibleAttackers(possibleAttackers);
			Console.WriteLine ();
			
			while(true)
			{
				Console.Write("mla> ");
				string input = Console.In.ReadLine().Trim();
				
				// if user simply presses enter or white space, repeat list of possible attackers and chosen attackers
				if(String.IsNullOrEmpty(input))
				{
					PrintPossibleAttackers(possibleAttackers);
					PrintChosenCreatures(chosenSet);
					continue;
				}
				
				// attempt to parse as a number.  If is a number in range, add to set
				int chosen;
				if(Int32.TryParse(input,out chosen))
				{
					if(chosen == -1)
						break;
					
					// if the chosen number is in range, add it to the set
					// if not in the set already, or remove it if it is in the set.
					// otherwise, tsk-tsk the user
					if(chosen >= 0 && chosen < possibleAttackers.Count())
					{
						bool added = chosenSet.Add(possibleAttackers[chosen]);
						if(!added)
							chosenSet.Remove(possibleAttackers[chosen]);						
					}
					else
					{
						Console.Write("Select a number from -1 to ");
						Console.WriteLine(possibleAttackers.Count() - 1);
					}
					PrintChosenCreatures(chosenSet);
					continue;
				}
				
				ParseCommands(input);
			}
			
			return chosenSet;
		}
		
		public override void ChooseBlockers (IDictionary<CreatureCard, IList<CreatureCard>> attackersToBlockersDictionary, IList<CreatureCard> possibleBlockers)
		{
			if(attackersToBlockersDictionary == null 
			   || attackersToBlockersDictionary.Count == 0
			   || possibleBlockers == null
			   || possibleBlockers.Count == 0)
				return;
			
			var attackers = attackersToBlockersDictionary.Keys.ToList();
			PrintPossibleBlockers(attackers, possibleBlockers);
			Console.WriteLine ();
			
			while(true)
			{
				Console.Write("mla> ");
				string input = Console.In.ReadLine().Trim();
				
				// if user simply presses enter or white space, repeat list of possible attackers and chosen attackers
				if(String.IsNullOrEmpty(input))
				{
					PrintPossibleBlockers(attackers,possibleBlockers);
					PrintChosenCreatures(attackersToBlockersDictionary);
					continue;
				}
					   
				// attempt to parse as a number to break out of loop
				int chosen;
				if(Int32.TryParse(input,out chosen))
				{
					if(chosen == -1)
						break;
					
					PrintPossibleBlockers(attackers, possibleBlockers);
					PrintChosenCreatures(attackersToBlockersDictionary);
					continue;
				}
				
				// attempt to parse as <letter>:<number> to add a blocker
				string[] splitInput = input.Split(new char[] {':'},2);
				if(splitInput.Length < 2)
				{
					PrintPossibleBlockers(attackers, possibleBlockers);
					PrintChosenCreatures(attackersToBlockersDictionary);
					continue;
				}
				int attacker,blocker;
				if(Int32.TryParse(splitInput[0],out attacker) && Int32.TryParse(splitInput[1],out blocker)) 
				{
					if(attacker >= 0 && attacker < attackers.Count
					   && blocker >= 0 && blocker < possibleBlockers.Count)
					{
						if(attackersToBlockersDictionary[attackers[attacker]].Contains(possibleBlockers[blocker]))
						{
							attackersToBlockersDictionary[attackers[attacker]].Remove(possibleBlockers[blocker]);
						}
						else
						{
							attackersToBlockersDictionary[attackers[attacker]].Add(possibleBlockers[blocker]);	
						}
						continue;
					}
				}
				
				ParseCommands(input);
			}
		}
		
		public override void OrderBlockers (IDictionary<CreatureCard, IList<CreatureCard>> attackersToBlockersDictionary)
		{
			return;
		}
		
		/// <summary>
		/// Parses the input for valid non-game-action commands.
		/// </summary>
		private void ParseCommands(string input)
		{
			// parse minimal non-ambiguous strings
			// 'print' command
			if(input.StartsWith("p") && input.Contains(' '))
			{
				string arg = input.Split(' ')[1];
				
				// "state"
				if(arg.StartsWith( "stat"))
				{
					Console.Write(Env.CurrState.PrintState());
				}
				// "battlefield", "field", "play"
				else if(arg.StartsWith("b") || arg.StartsWith("f") || arg.StartsWith("p"))
				{
					foreach(Player p in Env.Players)
					{
						Console.WriteLine("Player " + p.Name + "'s board:");
						var controlledCards = from c in this.Env.CurrState.Battlefield
												where c.Controller == p
												orderby c.Type ascending
												select c;
							
						foreach(Card c in controlledCards)
						{
							string cardInfo = c.Name;
							if((c.Status & Card.StatusEnum.Tapped) != 0)
								cardInfo += " (tapped)";
							
							Console.WriteLine(cardInfo + ",");
						}
					}
				}
				// "stack"
				else if(arg.StartsWith("stac"))
				{
					if(Env.CurrState.Stack.Count == 0)
						Console.WriteLine("Stack is empty.");
					else
					{
						Console.WriteLine("Top");
						for(int i = Env.CurrState.Stack.Count - 1; i >= 0; i--)
						{
							Console.WriteLine(Env.CurrState.Stack[i].Text + ",");
						}
						Console.WriteLine("Bottom");
					}
				}
				// "hand"
                else if (arg.StartsWith("ha"))
                {
                    Console.WriteLine(this.Name + "'s hand:");
                    foreach (Card c in Env.CurrState.Hands[this])
                    {
                        Console.Write(c.Name + ", ");
                    }
                    Console.WriteLine();
                }
				// no match, print help
                else
                {
                    PrintHelp();
                }
			}
			// no valid matches, print help
			else 
			{
				PrintHelp();
			}
		}
		
		private void PrintPossibleAttackers(IList<CreatureCard> possibleAttackers)
		{
			Console.WriteLine (this.Name + ", Select/Deselect attackers:");
			
			Console.Write("\t-1:Done selecting attackers");
			for(int i = 0; i < possibleAttackers.Count(); i++)
				Console.Write ("\n\t " + i + ":" + possibleAttackers[i].ToString());
			Console.WriteLine ();
		}
		
		private void PrintPossibleBlockers(IList<CreatureCard> attackers, IList<CreatureCard> possibleBlockers)
		{
			Console.WriteLine("Attackers:");
			Console.Write("\t-1:Done selecting blockers");
			for(int i = 0; i < attackers.Count; i++)
			{
				Console.Write("\n\t " + i + ":" + attackers[i].ToString());
			}
			Console.WriteLine();
			
			Console.Write ("Possible blockers:");
			
			for(int i = 0; i < possibleBlockers.Count(); i++)
				Console.Write ("\n\t " + i + ":" + possibleBlockers[i].ToString());
			Console.WriteLine ("\n" + this.Name + ", Select/Deselect blocker by <attacker>:<blocker>");
		}
		
		private void PrintChosenCreatures(ISet<CreatureCard> chosenSet)
		{
			Console.Write ("Chosen creatures:");
			foreach(CreatureCard c in chosenSet)
				Console.Write (" " + c.Name + ",");
			Console.WriteLine();
		}
		
		private void PrintChosenCreatures(IDictionary<CreatureCard, IList<CreatureCard>> dict)
		{
			Console.Write ("Chosen blocks:");
			foreach(CreatureCard atk in dict.Keys)
			{
				if(dict[atk] == null || dict[atk].Count == 0)
					Console.Write(" " + atk.Name + ":<none>;");
				else
				{
					Console.Write(" " + atk.Name + ":");
					foreach(CreatureCard blk in dict[atk])
						Console.Write (blk.Name + ",");
					Console.Write(";");
				}
			}
			Console.WriteLine();
		}
		
		private void PrintHelp()
		{
			Console.WriteLine("Available \"print\" commands:");
            Console.WriteLine("\tbattlefield\tThe contents and state of the battlefield");
            Console.WriteLine("\tstack\t\tThe contents of the stack");
            Console.WriteLine("\tstate\t\tThe current step, active player, and floating mana");
            Console.WriteLine("\thand\t\tThe contents of the current player's hand");
		}
		
		private void PrintActions(IList<ActionDescriptionTuple> actions)
		{		
			Console.WriteLine("Step is " + Env.CurrState.CurrStep.ToString() + ".  " + this.Name + ", Select Action:");
			for(int i = 0; i < actions.Count(); i++)
				Console.Write("\n\t" + i + ":" + actions[i].ActionDescription);	
			Console.WriteLine();
		}
	}
}


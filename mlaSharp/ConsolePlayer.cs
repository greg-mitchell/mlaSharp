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
			foreach(Card c in Env.GetCurrState().Hands[this])
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
				
				// parse minimal non-ambiguous strings
				// 'print' command
				if(input.StartsWith("p") && input.Contains(' '))
				{
					string arg = input.Split(' ')[1];
					
					// "state"
					if(arg.StartsWith( "stat"))
					{
						Console.Write(Env.GetCurrState().PrintState());
					}
					// "battlefield", "field", "play"
					else if(arg.StartsWith("b") || arg.StartsWith("f") || arg.StartsWith("p"))
					{
						foreach(Player p in Env.Players)
						{
							Console.WriteLine("Player " + p.Name + "'s board:");
							var controlledCards = from c in this.Env.GetCurrState().Battlefield
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
						if(Env.GetCurrState().Stack.Count == 0)
							Console.WriteLine("Stack is empty.");
						else
						{
							Console.WriteLine("Top");
							for(int i = Env.GetCurrState().Stack.Count - 1; i >= 0; i--)
							{
								Console.WriteLine(Env.GetCurrState().Stack[i].Text + ",");
							}
							Console.WriteLine("Bottom");
						}
					}
					// "hand"
                    else if (arg.StartsWith("ha"))
                    {
                        Console.WriteLine(this.Name + "'s hand:");
                        foreach (Card c in Env.GetCurrState().Hands[this])
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
			return actions[actionNum].GameAction;
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
			Console.WriteLine(this.Name + ", Select Action:");
			for(int i = 0; i < actions.Count(); i++)
				Console.Write("\n\t" + i + ":" + actions[i].ActionDescription);	
			Console.WriteLine();
		}
	}
}


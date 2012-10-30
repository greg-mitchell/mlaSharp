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
			Console.WriteLine("\nMulligan hand? (y/n):");	
			Console.Write ("mla> ");
			string input = Console.In.ReadLine();
			if(input.Contains("n"))
				return false;
			
			return true;			
		}
		
		public override GameActionDelegate GetAction() 
		{
			var actions = Env.EnumActions(this);
			Console.WriteLine(this.Name + ", Select Action:");
			for(int i = 0; i < actions.Count(); i++)
				Console.Write("\n\t" + i + ":" + actions[i].ActionDescription);
			
			Console.WriteLine ();
			int actionNum = -1;
			while(true)
			{
				Console.Write("mla> ");
				string input = Console.In.ReadLine();
				if(Int32.TryParse(input,out actionNum) && actionNum >= 0 && actionNum < actions.Count())
					break;
				
				if(input.StartsWith("print "))
				{
					string arg = input.Substring(6);
					if(arg == "state")
						Console.Write(Env.GetCurrState().PrintState());
					if(arg == "battlefield")
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
					if(arg == "stack")
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
					if(arg == "hand")
					{
						Console.WriteLine (this.Name + "'s hand:");
						foreach(Card c in Env.GetCurrState().Hands[this])
						{
							Console.Write(c.Name + ", ");
						}
						Console.WriteLine();
					}
				}
			}
			return actions[actionNum].GameAction;
		}
	}
}


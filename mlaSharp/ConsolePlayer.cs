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
			Console.WriteLine("Select Action:");
			for(int i = 0; i < actions.Count(); i++)
				Console.WriteLine("\n\t" + i + ":" + actions[i]);
			
			int actionNum = -1;
			do
				Console.WriteLine("mla> ");
			while (!(Int32.TryParse(Console.In.ReadLine(),out actionNum)) && actionNum >= 0 && actionNum < actions.Count());
			
			return actions[actionNum];
		}
	}
}


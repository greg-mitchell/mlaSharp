using System;

namespace mlaSharp
{
	public class ConsolePlayer : Player
	{
		public ConsolePlayer(GameEngine env, string name = "")
			: base(env, name)
		{ }
		
		public override bool MulliganHand()
		{
			Console.WriteLine(" > Hand:");
			Console.Write(" > ");
			foreach(Card c in Hand)
				Console.Write(String.Format("{0};",c.Name));
			Console.WriteLine(" > Mulligan hand? (y/n):");	
			string input = Console.In.ReadLine();
			if(input.Contains("n"))
				return false;
			
			return true;			
		}
		
		public override GameActionDelegate GetAction() 
		{
			return null;
		}
	}
}


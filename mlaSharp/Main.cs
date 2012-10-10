using System;

namespace mlaSharp
{
	class MainClass
	{
		public static void Main (string[] args)
		{
			GameEngine env = new GameEngine();
			
			Console.WriteLine ("Hello World!");
			for(int i = 0; i < 2; i++)
			{
				Console.WriteLine(" > Player " + i + " name: ");
				string name = Console.In.ReadLine();
				Player player = new ConsolePlayer(env,name);
				Library lib = Library.ParseDecklist("40 Goblin Piker\n20 Mountain",player);
				env.AddPlayer(player,lib);
			}
		}
	}
}

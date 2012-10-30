using System;
using System.Linq;
using mlaSharp;
using CardDatabase;
using NUnit.Framework;

namespace mlaSharpTests
{
	
	[TestFixture]
	public class LibraryTest
	{
		ConsolePlayer p1, p2;
		GameEngine env;
		public LibraryTest ()
		{
			env = new GameEngine();
			p1 = new ConsolePlayer(env,"p1");
			p2 = new ConsolePlayer(env,"p2");			
		}
		
		[Test]
		public void ParseDecklistTest()
		{
			string decklist = "40 Mountain\n20 Goblin Piker";
			var l1 = Library.ParseDecklist(decklist, p1);
			Assert.AreEqual(60,l1.Count);
			Assert.AreEqual(40, (
				from Card c in l1
				where c.GetType() == typeof(Mountain)
				select c
				).Count());
		}
		
		[Test]
		public void DrawTest()
		{
				
		}
	}
}


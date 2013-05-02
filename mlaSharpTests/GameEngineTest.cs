using System;
using System.Collections.Generic;
using mlaSharp;
using NUnit.Framework;
using System.Linq;
using System.Text;

namespace mlaSharpTests
{
	[TestFixture]
	public class GameEngineTest
	{
		RandomPlayer p1, p2;
		GameEngine env;
		public GameEngineTest ()
		{
			env = new GameEngine();
			p1 = new RandomPlayer(env,env.rng,"p1");
			p2 = new RandomPlayer(env,env.rng,"p2");
			
			string decklist = "40 Mountain\n20 Goblin Piker";
			var l1 = Library.ParseDecklist(decklist, p1);
			env.AddPlayer (p1,l1);
			
			var l2 = Library.ParseDecklist(decklist, p2);
			env.AddPlayer(p2,l2);
		}
		
		[Test]
		public void BlockTest()
		{
			int[,] setSizes = {{1,1},{2,1},{1,2},{2,2},{3,2},{2,3},{3,3}};
			int[] counts = {2,3,4,9,16,27,64};
			
			for(int i  = 0; i < setSizes.GetLength(0); i++)
			{
				var attackers = (from c in env.CurrState.Libraries[p1]
								where c.Type.Contains("Creature")
								select (c as CreatureCard))
								.Take(setSizes[i,0]).ToList();
				
				var blockers = (from c in env.CurrState.Libraries[p2]
				                where (c is CreatureCard)
				                select (c as CreatureCard))
								.Take(setSizes[i,1]).ToList();
				
				var blocks = GameEngine.GetPossibleBlocks(attackers,blockers);
				
				StringBuilder msg = new StringBuilder();
				msg.Append("{");
				foreach(var bd in blocks)
				{
					msg.Append("{");
					
					foreach(var atbTuple in bd)
					{
						// <creature's-id>
						msg.Append(atbTuple.Attacker.UniqueID);
						
						// " : "
						msg.Append(" : ");
						
						// [list-of-creature-ids]
						if(atbTuple.Blockers.Count == 0)
						{
							msg.Append("[]; ");
							continue;
						}
						
						msg.Append("[");
						foreach(var blk in atbTuple.Blockers)
						{
							msg.Append(blk.UniqueID);
							msg.Append(',');
						}
						msg.Remove(msg.Length - 1, 1);	// remove trailing ','
						msg.Append("]; ");
					}
					msg.Remove(msg.Length - 2, 2);	// remove trailing ','
					msg.Append("},");
				}
				msg.Remove(msg.Length - 1, 1);	// remove trailing ','
				msg.Append("}");
				
				Console.WriteLine(msg.ToString());
				
				Assert.AreEqual(counts[i], blocks.Count(),"Test" + i);
			}
		}
	}
}


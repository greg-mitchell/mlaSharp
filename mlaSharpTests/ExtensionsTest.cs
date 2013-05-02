using System;
using System.Collections.Generic;
using NUnit.Framework;
using System.Linq;
using mlaSharp;
using CardDatabase;
using System.Text;
using System.IO;

namespace mlaSharpTests
{
	[TestFixture]
	public class ExtensionsTest
	{
		
		System.Diagnostics.Stopwatch sw;
		
		public ExtensionsTest ()
		{
			sw = new System.Diagnostics.Stopwatch();
		}
		
		[Test]
		public void MulticolorTest()
		{
			
		}
		
		[Test]
		public void PowerSetTest()
		{
			System.Collections.Generic.List<int> testList = new System.Collections.Generic.List<int>() 
				{1,2,3,4, 5,6,7,8,9,10,11,12,13,14,15 }; //,16,17,18,19,20};			
			int correctCount = (int)Math.Pow(2, testList.Count);
			//string fpBA = "PowerSetTestBAResults.txt", fpIt="PowerSetTestItResults.txt";
			
			long ps1Time, ps2Time;
			sw.Start();
			var ps1 = testList.PowerSet();
			sw.Stop();
			ps1Time = sw.ElapsedMilliseconds;
			Console.WriteLine(String.Format("Binary arithmetic powerset (elapsed time {0}ms):",ps1Time));
			PrintPowerSet(ps1);
			Assert.AreEqual(correctCount, ps1.Count(),"PowerSetBA Count"); 
			
			sw.Restart();
			var ps2 = testList.PowerSet2();
			sw.Stop();
			ps2Time = sw.ElapsedMilliseconds;			
			Console.WriteLine(String.Format("Iterative powerset (elapsed time {0}ms):",ps2Time));
			PrintPowerSet(ps2);
			Assert.AreEqual(correctCount, ps2.Count(), "PowerSetIt Count");
						
		}
		
		private void PrintPowerSet<T>(IEnumerable<IList<T>> ps)
		{
			sw.Restart();
			StringBuilder msg = new StringBuilder();
			msg.Append("{");
			foreach(IList<T> subset in ps)
			{
				msg.Append( "{");
				foreach(T i in subset)
				{
					msg.Append(i.ToString());
					msg.Append( ",");
				}
				msg.Remove(msg.Length-1,1);	// remove trailing ','
				msg.Append("},");
			}
			msg.Remove(msg.Length-1,1); // remove trailing ','
			msg.Append("}");
			Console.WriteLine(msg.ToString());
			sw.Stop();
			Console.WriteLine("({0}ms elapsed to write out powerset)",sw.ElapsedMilliseconds);
		}
		
	}
	
}


using System;
using System.Collections.Generic;

namespace mlaSharp
{
	public class State
	{
		public Steps CurrStep { get; set;}
		public Player PlayerWithPriority { get; set;}
		public List<Card> Battlefield { get; private set;}
		public List<StackObject> Stack { get; private set;}
		
		public State ()
		{			
			Battlefield = new List<Card>();
			Stack = new List<StackObject>();
		}
	}
}


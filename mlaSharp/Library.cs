using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace mlaSharp
{
	public class Library : List<Card>
	{
		public Player Owner {get; private set;}
		
		public Library (Player owner)
		{
			Owner = owner;
		}
		
		public static Library ParseDecklist(string decklist, Player owner)
		{
			Library newLib = new Library(owner);
			string[] lines = decklist.Split (new char[] {'\n'});
			foreach(string line in lines)
			{
				int whiteIndex = line.IndexOf(' ');
				int count = Int32.Parse(line.Substring(0,whiteIndex));
				Type cardType = CardDatabase.CardLookup.LookUpCardType(line.Substring(whiteIndex+1));
				
				for(int i = 0; i < count; i++)
				{
					newLib.Add((Card)Activator.CreateInstance(cardType,owner));
				}
			}
			
			return newLib;
		}
		
		/// <summary>
		/// Draw a card.
		/// </summary>
		public Card Draw()
		{
			Card item = this[0];
			this.RemoveAt(0);	
			return item;
		}
		
		/// <summary>
		/// Draw x cards.
		/// </summary>
		/// <param name='x'>
		/// The number of cards to draw
		/// </param>
		public IEnumerable<Card> Draw(int x)
		{
			var cards = this.Take(x);
			this.RemoveRange(0,x);
			return cards;
		}
		
		
	}
}


using System;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;
using mlaSharp;

namespace CardDatabase
{
	public static class CardLookup
	{
		private static Dictionary<string,mlaSharp.Card> _db;
		
		static CardLookup()
		{
			_db = new Dictionary<string, Card>();
			var asm = Assembly.GetAssembly(typeof(CardDatabase.CardLookup));
			var cardTypes =	from type in asm.GetTypes()
							where type.GetCustomAttributes(typeof(CardAttribute),false).Length != 0
							select type;			
			
			foreach(Type t in cardTypes)
			{
				mlaSharp.Card c = (mlaSharp.Card)Activator.CreateInstance (t,null);	
				_db[c.Name] = c;
			}
		}
		
		public static Type LookUpCardType(string cardName)
		{
			return _db[cardName].GetType();
		}
	}
}


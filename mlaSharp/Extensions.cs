using System;
using System.Collections.Generic;
using System.Security.Cryptography;

namespace mlaSharp
{
	public static class Extensions
	{
		public static bool Multicolor(this ColorsEnum c)
		{
			throw new NotImplementedException("oops");
		}
		
		/// <summary>
		/// Sets the colors.
		/// </summary>
		/// <returns>
		/// The colors.
		/// </returns>
		/// <param name='c'>
		/// The ColorEnum to set.
		/// </param>
		/// <param name='manaCost'>
		/// Mana cost string to parse.
		/// </param>
		public static ColorsEnum SetColors(this ColorsEnum c, string manaCost)
		{
			string lmanaCost = manaCost.ToLower();
			if(lmanaCost.Contains("w"))
				c |= ColorsEnum.White;
			if(lmanaCost.Contains("u"))
				c |= ColorsEnum.Blue;
			if(lmanaCost.Contains("b"))
				c |= ColorsEnum.Black;
			if(lmanaCost.Contains("r"))
				c |= ColorsEnum.Red;
			if(lmanaCost.Contains("g"))
				c |= ColorsEnum.Green;
			
			return c;
		}
		
		/// <summary>
		/// Extension method to shuffle the specified list using the Fisher–Yates shuffle.
		/// </summary>
		/// <param name='list'>
		/// List.
		/// </param>
		/// <param name='rng'>
		/// The random number provider to use.
		/// </param>
		/// <typeparam name='T'>
		/// The 1st type parameter.
		/// </typeparam>
		 public static void Shuffle<T>(this IList<T> list, Random rnd) 
		 {
	        int n = list.Count;
	        
	        while (n > 1) {
	            int k = rnd.Next(0, n);
	            n--;
	            T value = list[k];
	            list[k] = list[n];
	            list[n] = value;
	        }
		}
    }	
}


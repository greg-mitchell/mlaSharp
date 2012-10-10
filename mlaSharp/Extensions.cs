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
		public static void Shuffle<T>(this List<T> list, RandomNumberGenerator rng)
		{
			if(list.Count >= Byte.MaxValue) throw new ArgumentOutOfRangeException("The list to shuffle must contain less than 255 elements");
		    for(int n = list.Count; n > 1; n--)
		    {
				
		        byte[] box = new byte[1];
				
				// ensure box[0] % n is unbiased
		        do rng.GetBytes(box);
		        while (!(box[0] < n * (Byte.MaxValue / n)));
								
				// swap element 'n' with a random earlier element 'k' (or itself)
		        int k = (box[0] % n);
		        T value = list[k];
		        list[k] = list[n];
		        list[n] = value;
		    }

		}
	}
}


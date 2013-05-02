using System;
using System.Linq;
using System.Collections.Generic;
using System.Security.Cryptography;

namespace mlaSharp
{
	public static class Extensions
	{
		/// <summary>
		/// Returns true if c is multicolored
		/// </summary>
		/// <param name='c'>
		/// The ColorEnum to check.
		/// </param>
		public static bool Multicolor(this ColorsEnum c)
		{
			// determine if it's multicolor by checking if c is a power of 2 or a power of 2 + 1
			double cInDouble = (double)c;
			double logC = Math.Log (cInDouble,2);
			double logCMinus1 = Math.Log (cInDouble - 1, 2);
			if(cInDouble != 0 && (logC - Math.Truncate(logC) == 0 || logCMinus1 - Math.Truncate(logCMinus1) == 0))
				return true;
			return false;
		}
		
		/// <summary>
		/// An iterator over the powerset of this list using binary arithmatic.
		/// </summary>
		/// <returns>
		/// The powerset.
		/// </returns>
		/// <param name='list'>
		/// List.
		/// </param>
		/// <typeparam name='T'>
		/// The 1st type parameter.
		/// </typeparam>
		/// <remarks>
		/// Adapted from http://jetblackrob.wordpress.com/2012/09/11/power-sets-in-c/
		/// Last accessed 2013-04-27
		/// </remarks>
		public static IEnumerable<IList<T>> PowerSet<T>(this IList<T> list)
		{
			int n = 1 << list.Count;
			for (int i = 0; i < n; ++i)
			{
				IList<T> set = new List<T>();
				for (int bits = i, j = 0; bits != 0; bits >>= 1, ++j)
				{
					if ((bits & 1) != 0)
				    	set.Add(list[j]);
				}
				yield return set;
			}
		}
		
		/// <summary>
		/// Returns the powerset of this list using an iterative method and building the whole list.
		/// </summary>
		/// <returns>
		/// The powerset.
		/// </returns>
		/// <param name='list'>
		/// List.
		/// </param>
		/// <typeparam name='T'>
		/// The 1st type parameter.
		/// </typeparam>
		/// <remarks>
		/// Adapted from http://jetblackrob.wordpress.com/2012/09/11/power-sets-in-c/
		/// Last accessed 2013-04-27
		/// </remarks>
		public static IEnumerable<IList<T>> PowerSet2<T>(this IList<T> list)
		{
			IList<IList<T>> powerset = new List<IList<T>>() { new List<T>() };
			foreach (T item in list)
			{
				foreach (IList<T> set in powerset.ToArray())
				{ 
					var newSet = new List<T>(set) { item };
					powerset.Add(newSet);
				}
			}
			return powerset;
		}
		
		/// <summary>
		/// Deep-copies a dictionary of lists, creating new references for each key's list.
		/// </summary>
		/// <returns>
		/// The copied dictionary.
		/// </returns>
		/// <param name='dict'>
		/// Dictionary to copy.
		/// </param>
		/// <remarks>
		/// Use this instead of the copy constructor when the value references have to be distinct.
		/// </remarks>
		public static IDictionary<T, IList<T>> DeepCopy<T>(this IDictionary<T, IList<T>> dict)
		{
			var copy = new Dictionary<T,IList<T>>();
			foreach(var key in dict.Keys)
			{
				copy[key] = new List<T>(dict[key]);
			}
			return copy;
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


using System;

namespace mlaSharp
{
	#region Enums
	/// <summary>
	/// Sequential steps in the game
	/// </summary>
	public enum Steps
	{
		untap,
		upkeep,
		draw,
		main1,
		beginCombat,
		declareAtk,
		declareBlk,
		damage,
		endCombat,
		main2,
		end,
		cleanup
	}
	
	[Flags]
	public enum ColorsEnum
	{
		Colorless = 0,
		White = 1,
		Blue = 2,
		Black = 4,
		Red = 8,
		Green = 16
	}
	#endregion
	
	/// <summary>
	/// A function that performs a game action
	/// </summary>
	public delegate void GameActionDelegate(Player p, State s);
	
	/// <summary>
	/// A fuction that should return true if the ability's prereqs are fulfilled
	/// </summary>
	public delegate bool AbilityAvailablePredicate(Player p, State s);
	
	
	/// <summary>
	/// Action description tuple.
	/// </summary>
	public struct ActionDescriptionTuple
	{
		public GameActionDelegate GameAction;
		public String ActionDescription;
	}
	
	/// <summary>
	/// A static utility class that provides conversion functions
	/// </summary>
	public static class Conversions
	{
		public const int NUMBER_OF_COLOR_FIELDS = 6;
		/// <summary>
		/// Returns a consistent index into an array size NUMBER_OF_COLORS.
		/// </summary>
		/// <returns>
		/// The index value for the color
		/// </returns>
		/// <param name='c'>
		/// The color to index into
		/// </param>
		/// <exception cref='NotImplementedException'>
		/// Is thrown if <see cref="ColorsEnum"/> is not consistent with this implementation.
		/// </exception>
		public static int ColorsEnumToIndex(ColorsEnum c)
		{
			switch(c)
			{
				case ColorsEnum.White:
					return 0;
				case ColorsEnum.Blue:
					return 1;
				case ColorsEnum.Black:
					return 2;
				case ColorsEnum.Red:
					return 3;
				case ColorsEnum.Green:
					return 4;
				case ColorsEnum.Colorless:
					return 5;
			}
			
			throw new NotImplementedException("Update this function to match ColorsEnum!");
		}
	}
}
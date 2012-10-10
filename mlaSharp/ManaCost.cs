using System;

namespace mlaSharp
{
	/// <summary>
	/// A class encapsulating a mana cost and its properties.
	/// </summary>
	public class ManaCost
	{
		/// <summary>
		/// Gets or sets the Converted Mana Cost (CMC).
		/// </summary>
		/// <value>
		/// The Converted Mana Cost (CMC).
		/// </value>
		public int CMC { get; private set; }		
		public ColorsEnum Colors {get; private set; }		
		public string ManaCostString { get; private set;}
		/// <summary>
		/// Gets or sets a value indicating whether this <see cref="mlaSharp.ManaCost"/> is an unpayable mana cost.
		/// </summary>
		/// <value>
		/// <c>true</c> if unpayable mana cost; otherwise, <c>false</c>.
		/// </value>
		/// <remarks>
		/// <c>true</c> for cards like Ancestral Vision, Restore Balance, etc.
		/// </remarks>
		public bool UnpayableManaCost { get; private set;}
		
		/// <summary>
		/// Initializes a new instance of the <see cref="mlaSharp.ManaCost"/> class.
		/// </summary>
		/// <param name='manaCostString'>
		/// Mana cost string.
		/// </param>
		public ManaCost (string manaCostString)
		{
			ManaCostString = manaCostString;
			Colors = this.Colors.SetColors(manaCostString);
			UnpayableManaCost = false;
			CMC = 0;
			setCmc();			
		}
		
		private void setCmc()
		{
			/* grammar for mana cost strings:
			 * <s> 		-> <nums> | {}
			 * <nums>	-> {digit}<nums> | <lets>
			 * <lets>	-> {w|u|b|r|g}<lets> | {}
			 */ 
			
			// if the ManaCostString is null or empty, the mana cost is unpayable
			// e.g. Ancestral Vision, Restore Balance
			if(String.IsNullOrEmpty(ManaCostString))
			{
				CMC = 0;
				UnpayableManaCost = true;
				return;
			}
						
			// find the length until the first letter
			int len = 0;
			int numResult = 0;
			bool parseSuccessful = true;
			while(len < ManaCostString.Length && parseSuccessful) 
			{
				parseSuccessful = Int32.TryParse(ManaCostString.Substring(0,++len), out numResult);
			}
			
			// no numbers
			if(len == 1 && !parseSuccessful)
			{
				CMC = ManaCostString.Length;
				return;
			}
			
			// only numbers
			if(len == ManaCostString.Length && parseSuccessful)
			{
				CMC = numResult;
				return;
			}
			
			// numbers and letters
			int generic = Int32.Parse(ManaCostString.Substring(0,--len));
			int letters = ManaCostString.Substring(len).Length;
			CMC = generic + letters;
		}
	}
}


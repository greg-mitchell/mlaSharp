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
		
		public int GenericMana { get; private set; }
		
		private int[] coloredSymbols = new int[Conversions.NUMBER_OF_COLOR_FIELDS];
		
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
			setCmcAndCost();	
			
		}
		
		/// <summary>
		/// Gets the number of mana symbols of the specified <see cref="ColorsEnum">color</see>
		/// </summary>
		/// <param name='color'>
		/// Color.
		/// </param>
		public int this[ColorsEnum color]
		{
			get { return coloredSymbols[Conversions.ColorsEnumToIndex(color)]; }
			private set { coloredSymbols[Conversions.ColorsEnumToIndex(color)] = value; }
		}
		
		public bool CanCast(ManaPool floating)
		{
			if(UnpayableManaCost)
				return false;
			
			ManaPool subtractedPool = new ManaPool(floating);
			bool coloredOk = true;
			foreach(ColorsEnum color in Enum.GetValues(typeof(ColorsEnum)))
			{
				coloredOk &= (subtractedPool[color] -= this[color]) >= 0;
			}
			
			if(!coloredOk)
				return false;
			
			subtractedPool.Generic += subtractedPool.W
				+ subtractedPool.U
				+ subtractedPool.B
				+ subtractedPool.R
				+ subtractedPool.G;
			
			return subtractedPool.Generic >= GenericMana;
		}
		
		private void setCmcAndCost()
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
				parseManaCostStringLetters(ManaCostString);
				return;
			}
			
			// only numbers
			if(len == ManaCostString.Length && parseSuccessful)
			{
				CMC = numResult;
				GenericMana += numResult;
				return;
			}
			
			// numbers and letters
			int generic = Int32.Parse(ManaCostString.Substring(0,--len));
			int letters = ManaCostString.Substring(len).Length;
			CMC = generic + letters;
			GenericMana += generic;
			parseManaCostStringLetters(ManaCostString.Substring(len));
		}
		
		private void parseManaCostStringLetters(string toParse)
		{
			string manaCostStringLower = toParse.ToLower();
			foreach(char c in manaCostStringLower)
			{
				switch(c)
				{
				case 'w':
					this[ColorsEnum.White]++;
					break;
				case 'u':
					this[ColorsEnum.Blue]++;
					break;
				case 'b':
					this[ColorsEnum.Black]++;
					break;
				case 'r':
					this[ColorsEnum.Red]++;
					break;
				case 'g':
					this[ColorsEnum.Green]++;
					break;
				}				
			}
		}
	}
}


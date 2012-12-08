using mlaSharp;

namespace mlaSharp
{
	public class ManaPool
	{
		public int W
		{
			get { return pool[Conversions.ColorsEnumToIndex(ColorsEnum.White)]; }
			set { pool[Conversions.ColorsEnumToIndex(ColorsEnum.White)] = value; }
		}
		public int U
		{
			get { return pool[Conversions.ColorsEnumToIndex(ColorsEnum.Blue)]; }
			set { pool[Conversions.ColorsEnumToIndex(ColorsEnum.Blue)] = value; }
		}
		public int B
		{
			get { return pool[Conversions.ColorsEnumToIndex(ColorsEnum.Black)]; }
			set { pool[Conversions.ColorsEnumToIndex(ColorsEnum.Black)] = value; }
		}
		public int R
		{
			get { return pool[Conversions.ColorsEnumToIndex(ColorsEnum.Red)]; }
			set { pool[Conversions.ColorsEnumToIndex(ColorsEnum.Red)] = value; }
		}
		public int G
		{
			get { return pool[Conversions.ColorsEnumToIndex(ColorsEnum.Green)]; }
			set { pool[Conversions.ColorsEnumToIndex(ColorsEnum.Green)] = value; }
		}
		
		public int Generic;
		
		// one element for each color
		private int[] pool = new int[Conversions.NUMBER_OF_COLOR_FIELDS];
		
		public ManaPool()
		{
			
		}
		
		/// <summary>
		/// Copy constructor for the <see cref="mlaSharp.ManaPool"/> class.
		/// </summary>
		/// <param name='mp'>
		/// ManaPool to copy
		/// </param>
		public ManaPool(ManaPool mp)
		{
			this.W = mp.W;
			this.U = mp.U;
			this.B = mp.B;
			this.R = mp.R;
			this.G = mp.G;
			this.Generic = mp.Generic;
		}
		
		public int this[ColorsEnum color]
		{
			get { return pool[Conversions.ColorsEnumToIndex(color)]; }
			set { pool[Conversions.ColorsEnumToIndex(color)] = value; }
		}
		
		public void Clear()
		{
			W = U = B = R = G = Generic = 0;	
		}
		
		public bool Equals (ManaPool other)
		{
			return (W == other.W)
				&& (U == other.U)
				&& (B == other.B)
				&& (R == other.R)
				&& (G == other.G)
				&& (Generic == other.Generic);
		}
		
		public override string ToString ()
		{
			return string.Format("W={0}, U={1}, B={2}, R={3}, G={4}, Generic={5}",W,U,B,R,G,Generic);
		}
	}
		                
		                
}

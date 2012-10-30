
namespace mlaSharp
{
	public class ManaPool
	{
		public int W;
		public int U;
		public int B;
		public int R;
		public int G;
		public int Generic;
		
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

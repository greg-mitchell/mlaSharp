using System;
using System.Collections.Generic;

namespace mlaSharp
{
	
		
		/// <summary>
		/// Block description.  Provides an implementation of a complete description of a blocking situation.
		/// Use this instead of a dictionary to save space of Dictionary's hashtable.
		/// </summary>
		public class BlockDescription : IEnumerable<BlockDescription.AttackerToBlockersTuple>
		{
			private static CardComparer comp = new CardComparer();
		
			private SortedList<CreatureCard,AttackerToBlockersTuple> _blockMap;
			
			private BlockDescription()
			{
				
			}
			
			public BlockDescription(IList<CreatureCard> atkSet)
			{
				_blockMap = new SortedList<CreatureCard,AttackerToBlockersTuple>(atkSet.Count, comp);
				foreach(var cc in atkSet)
				{
					_blockMap.Add(cc, new AttackerToBlockersTuple(cc));
				}
			}
			
			/// <summary>
			/// Deep copies this instance of a BlockDescription
			/// </summary>
			/// <returns>
			/// The copy.
			/// </returns>
			public BlockDescription DeepCopy()
			{
				BlockDescription copy = new BlockDescription();
				copy._blockMap = new SortedList<CreatureCard,AttackerToBlockersTuple>(_blockMap.Count, comp);
				foreach(var cc in _blockMap.Keys)
				{
					copy._blockMap.Add(cc, new AttackerToBlockersTuple(cc,_blockMap[cc].Blockers));
				}
				return copy;
			}
			
			#region IEnumerable implementation
			public IEnumerator<AttackerToBlockersTuple> GetEnumerator ()
			{
				return _blockMap.Values.GetEnumerator();
			}
			
			System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator ()
			{
				return _blockMap.Values.GetEnumerator();
			}
			#endregion
			
			#region IList-like Methods
			public int IndexOf (AttackerToBlockersTuple item)
			{
				return _blockMap.IndexOfValue(item);
			}

			public void RemoveAt (int index)
			{
				_blockMap.RemoveAt(index);
			}

			public AttackerToBlockersTuple this[CreatureCard key] {
				get {
					return _blockMap[key];
				}
				set {
					_blockMap[key] = value;
				}
			}

			public void Clear ()
			{
				foreach(var kv in _blockMap)
				{
					kv.Value.Blockers.Clear();
				}
			}

			public bool Contains (CreatureCard attacker)
			{
				return _blockMap.ContainsKey(attacker);
			}
			
			public bool Remove (CreatureCard attacker)
			{
				return _blockMap.Remove(attacker);
			}

			public int Count {
				get {
					return _blockMap.Count;
				}
			}
			#endregion
			
			/// <summary>
			/// Compares cards based on unique ID.
			/// </summary>
			private class CardComparer : Comparer<Card>
			{				
				#region implemented abstract members of System.Collections.Generic.Comparer[Card]
				public override int Compare (Card x, Card y)
				{
					return Math.Sign(x.UniqueID - y.UniqueID);
				}
				#endregion
			}
		
			
		/// <summary>
		/// Attacker to blockers tuple.  Relates an attacker to a list of blockers.  Its hashable value is the attacker.
		/// </summary>
		public class AttackerToBlockersTuple
		{			
			/// <summary>
			/// Gets or sets the attacker.
			/// </summary>
			/// <value>
			/// The attacker.
			/// </value>
			public CreatureCard Attacker { get; set; }
			
			/// <summary>
			/// Gets or sets the list of blockers.
			/// </summary>
			/// <value>
			/// The blockers.
			/// </value>
			public IList<CreatureCard> Blockers { get; protected set; }
			
			
			/// <summary>
			/// Initializes a new instance of the <see cref="mlaSharp.GameEngine.AttackerToBlockersTuple"/> class.
			/// </summary>
			public AttackerToBlockersTuple()
				: this(null, null)
			{ }
			
			/// <summary>
			/// Initializes a new instance of the <see cref="mlaSharp.GameEngine.AttackerToBlockersTuple"/> class.
			/// </summary>
			/// <param name='attacker'>
			/// Attacker.
			/// </param>
			public AttackerToBlockersTuple(CreatureCard attacker)
				: this(attacker,null)
			{ }
			
			/// <summary>
			/// Initializes a new instance of the <see cref="mlaSharp.GameEngine.AttackerToBlockersTuple"/> class.
			/// </summary>
			/// <param name='attacker'>
			/// Attacker.
			/// </param>
			/// <param name='blockersList'>
			/// Blockers list to clone.
			/// </param>
			public AttackerToBlockersTuple(CreatureCard attacker, IList<CreatureCard> blockersList)
			{
				Attacker = attacker;
				if(blockersList == null)
				{
					Blockers = new List<CreatureCard>();
				}
				else
				{
					Blockers = new List<CreatureCard>(blockersList);
				}
			}
			
			public override int GetHashCode ()
			{
				return (Attacker == null) ? base.GetHashCode() : Attacker.GetHashCode();
			}			
		}
		}
}


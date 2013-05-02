using System;
using System.Collections.Generic;
using log4net;

namespace mlaSharp
{
	public class MctsNode
	{				
		/// <summary>
		/// Gets or sets the action taken to get from this node's parent to this node.
		/// </summary>
		/// <value>
		/// The action taken.
		/// </value>
		public GameActionDelegate ActionTaken {get; set;}
		
		/// <summary>
		/// Gets or sets the current evaluation v_i of this gamestate, the average of results of simulated games.
		/// </summary>
		/// <value>
		/// The value.
		/// </value>
		public double Evaluation { get; set;}
		
		/// <summary>
		/// Gets or sets the visit count.
		/// </summary>
		/// <value>
		/// The visit count.
		/// </value>
		public long VisitCount { get; set; }
		
		/// <summary>
		/// Gets or sets the children list.
		/// </summary>
		/// <value>
		/// The children list.
		/// </value>
		public IList<MctsNode> Children { get; protected set; }
		
		/// <summary>
		/// Gets or sets the parent of this node.
		/// </summary>
		/// <value>
		/// The parent.
		/// </value>
		public MctsNode Parent { get; protected set; }
		
		/// <summary>
		/// Gets or sets the gamestate this node represents and updates the possible actions
		/// </summary>
		/// <value>
		/// The gamestate.
		/// </value>
		public State GameState { get; set; }
				
		public MctsNode ()
		{
			Evaluation = 0.0;
			VisitCount = 0;
			Children = new List<MctsNode>();
		}
	}
}


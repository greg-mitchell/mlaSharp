using System;
using System.Threading;
using log4net;
using System.Reflection;
using System.Collections.Generic;
using System.Linq;

namespace mlaSharp
{
	public class MctsEvaluator
	{
		protected Func<MctsNode,MctsNode> SelectFunc;
		protected Func<MctsNode,IList<GameActionDelegate>,MctsNode> ExpandFunc;
		protected Func<MctsNode,double> SimulateFunc;
		protected Action<MctsNode,double> BackpropFunc;
		
		private readonly ILog logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

		// shared variables used by the evaluator
		private MctsNode bestChildSoFar;
		private MctsNode root;
		private bool keepSearching;
		private GameEngine stateMachine,simulationEngine;
		private Dictionary<State,List<GameActionDelegate>> actionsFromState;
		private int playerToMoveIndex;
		
		/// <summary>
		/// Initializes a new instance of the <see cref="mlaSharp.MctsEvaluator"/> class.
		/// </summary>
		/// <param name='ge'>
		/// Ge.
		/// </param>
		public MctsEvaluator(GameEngine ge)
			: this(ge, null, null, null,null)
		{
			
		}
		
		/// <summary>
		/// Initializes a new instance of the <see cref="mlaSharp.MctsEvaluator"/> class.  Each function has a default implementation in this class.
		/// </summary>
		/// <param name='selectFunc'>
		/// Select function.
		/// </param>
		/// <param name='expandFunc'>
		/// Expand func.
		/// </param>
		/// <param name='simulateFunc'>
		/// Simulate func.
		/// </param>
		/// <param name='backpropFunc'>
		/// Backprop func.
		/// </param>
		public MctsEvaluator (GameEngine ge, Func<MctsNode,MctsNode> selectFunc, Func<MctsNode,IList<GameActionDelegate>,MctsNode> expandFunc, Func<MctsNode,double> simulateFunc, Action<MctsNode,double> backpropFunc)
		{
			SelectFunc = selectFunc ?? SelectLeaf;
			ExpandFunc = expandFunc ?? ExpandNode;
			SimulateFunc = simulateFunc ?? SimulateGames;
			BackpropFunc = backpropFunc ?? BackpropagateFrom;
			
			if(selectFunc == null)
			{
				var sec = new State.StateEqComp();
				actionsFromState = new Dictionary<State, List<GameActionDelegate>>(sec);
				stateMachine = ge.Clone();
				ge.SimulationMode = true;
			}
		}		
		
		/// <summary>
		/// Finds the best child action of the MCTS game tree in the allotted time budget.
		/// </summary>
		/// <returns>
		/// The best action found
		/// </returns>
		/// <param name='rootState'>
		/// State to start the search from
		/// </param>
		/// <param name='budget_ms'>
		/// The amount of time in the computation budget, in ms
		/// </param>
		public GameActionDelegate FindBestChild(State rootState, int budget_ms)
		{
			const int JOIN_TIMEOUT_MS = 50;
			this.root = new MctsNode() {
				GameState = rootState
			};			
			this.bestChildSoFar = null;
			this.keepSearching = true;
			this.playerToMoveIndex = rootState.HostGameEngine.Players.IndexOf(rootState.PlayerWithPriority);
			this.simulationEngine = rootState.HostGameEngine.CloneToNewPlayers<RandomPlayer>();
			
			// spawn a new thread to perform the search
			Thread t = new Thread(new ThreadStart(MctsLoop));
			t.Start();
			Thread.Sleep(budget_ms);
			
			keepSearching = false;
			t.Join(JOIN_TIMEOUT_MS);
			t.Abort();
			
			return bestChildSoFar.ActionTaken;
		}
		
		protected void MctsLoop()
		{
			try
			{
				while(keepSearching)
				{
					MctsNode leaf = SelectFunc(root);
					double delta = SimulateFunc(leaf);
					BackpropFunc(leaf,delta);
				}
			}
			catch (ThreadAbortException)
			{
				logger.Debug("Aborted MCTS search thread.");
			}
		}
		
		/// <summary>
		/// Selects a leaf to explore using UCT (implements the Tree Policy)
		/// and adds it to the tree.
		/// </summary>
		/// <returns>
		/// The selected leaf.
		/// </returns>
		/// <param name='node'>
		/// Node of the game tree to search from
		/// </param>
		private MctsNode SelectLeaf(MctsNode node)
		{
			double Cp = 1/Math.Sqrt(2);	// TODO: determine if this is a good Constant of exploration
			
			while(node.Children.Count != 0)
			{
				// determine if node is fully expanded
				var actions = GetActionsFromState(node.GameState);
				if (actions.Count != node.Children.Count)
					return ExpandFunc(node,actions);			// if node is not fully expanded, expand the node and select the new child
				node = BestChild(node, actions, Cp);	// otherwise if fully expanded, continue exploring down						
			}
			return node;
		}
		
		/// <summary>
		/// Gets a list of actions available from the state s.  Uses a Hashtable lookup to minimize computational cost.
		/// </summary>
		/// <returns>
		/// The actions from state s.
		/// </returns>
		/// <param name='s'>
		/// S.
		/// </param>
		private List<GameActionDelegate> GetActionsFromState(State s)
		{
			List<GameActionDelegate> actions;
			if(actionsFromState.ContainsKey(s))
			{
				actions = actionsFromState[s];
			}
			else
			{
				stateMachine.CurrState = s;
				var actionDescriptions = stateMachine.EnumActions(s.PlayerWithPriority);
				actions = actionDescriptions.Select((ad) => ad.GameAction).ToList();
				actionsFromState[s] = actions;
			}
			return actions;
		}
		
		/// <summary>
		/// Finds the best child of the parent node to explore using UCB1.  Assumes parent is nonterminal.
		/// </summary>
		/// <remarks>
		/// </remarks>
		/// <returns>
		/// The best child.
		/// </returns>
		/// <param name='parent'>
		/// Parent node.
		/// </param>
		/// <param name='actions'>
		/// List of actions from this state.
		/// </param>
		/// <param name='Cp'>
		/// Constant of exploration, Cp > 0
		/// </param>
		private MctsNode BestChild(MctsNode parent, List<GameActionDelegate> actions, double Cp)
		{
			MctsNode best = parent.Children.First();
			double bestValue = 0.0;
			foreach(var curr in parent.Children)
			{
				double currValue = curr.Evaluation / curr.VisitCount 
					+ Cp * Math.Sqrt(2 * Math.Log(parent.VisitCount) / curr.VisitCount);
				if (currValue > bestValue)
				{
					bestValue = currValue;
					best = curr;
				}
			}
			return best;
		}
		
		/// <summary>
		/// Expands the node.  Creates a child node using the first untried action and adds it to the tree.
		/// </summary>
		/// <returns>
		/// The new child node.
		/// </returns>
		/// <param name='node'>
		/// Node to expand.
		/// </param>
		/// <param name='actionsFromState'>
		/// List of actions from the state.
		/// </param>
		private MctsNode ExpandNode(MctsNode node, IList<GameActionDelegate> actionsFromState)
		{
			// choose an untried action from node
			GameActionDelegate untriedAction = null;
			LinkedList<MctsNode> childrenToVerify = new LinkedList<MctsNode>(node.Children);
			foreach(var a in actionsFromState)
			{
				bool taken = false;
				var child = childrenToVerify.First;
				
				while(child.Next != null)
				{
					if(child.Value.ActionTaken == a)
					{
						taken = true;
						childrenToVerify.Remove (child);
						break;
					}
					child = child.Next;
				}
				if(!taken)
				{
					untriedAction = a;
					break;
				}
			}
			
			if(untriedAction == null)
				throw new ArgumentNullException("Parameter node must not be fully expanded");
			
			// create and add the child
			stateMachine.CurrState = node.GameState;
			stateMachine.PerformAction(untriedAction);
			MctsNode newChild = new MctsNode() 
			{ 
				ActionTaken = untriedAction,
				GameState = stateMachine.CurrState,
			};
			node.Children.Add(newChild);
			return newChild;
		}
		
		private double SimulateGames(MctsNode node)
		{
			Player winner = simulationEngine.StartGame(node.GameState);
			if(winner == null)
				return 0.0;
			int simulatedWinnerIndex = simulationEngine.Players.IndexOf(winner);
			if(simulatedWinnerIndex == playerToMoveIndex)
				return 1.0;
			else
				return -1.0;
		}
		
		/// <summary>
		/// Backpropagates the new simulation value delta from the child node to its parent.
		/// </summary>
		/// <param name='child'>
		/// Child node with an updated value.
		/// </param>
		/// <param name='delta'>
		/// The updated simulation value.
		/// </param>
		private void BackpropagateFrom(MctsNode child,double delta)
		{ 
			Player toMove = child.GameState.PlayerWithPriority;
			while(child != null)
			{
				child.VisitCount++;
				child.Evaluation += delta * ((child.GameState.PlayerWithPriority == toMove) ? 1 : -1);
				child = child.Parent;				
			}
		}
	}
}


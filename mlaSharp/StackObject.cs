using System;

namespace mlaSharp
{
	/// <summary>
	/// Encapsulation of a card or ability on the stack.
	/// </summary>
	public class StackObject
	{
		/// <summary>
		/// Gets or sets the type of the object on the stack - either a Card, an Activated Ability, or a Triggered Ability
		/// </summary>
		/// <value>
		/// The type of the object.
		/// </value>
		public StackObjectType ObjectType { get; private set;}
		/// <summary>
		/// Gets or sets the type string.  This may be null for an activated or triggered ability.
		/// </summary>
		/// <value>
		/// The type string.  E.g. "Artifact Creature - Construct"
		/// </value>
		public string TypeString { get; private set;}
		/// <summary>
		/// Gets or sets the text of the object.
		/// </summary>
		/// <value>
		/// The text.
		/// </value>
		public string Text { get;private set;}
		/// <summary>
		/// Gets or sets the owner of the stack object.
		/// </summary>
		/// <value>
		/// The owner.
		/// </value>
		public Player Owner { get; private set;}
		/// <summary>
		/// Gets or sets the controller of the stack object.
		/// </summary>
		/// <value>
		/// The controller.
		/// </value>
		public Player Controller { get; private set;}
		/// <summary>
		/// Gets or sets the colors of the stack object.
		/// </summary>
		/// <value>
		/// The colors.
		/// </value>
		public ColorsEnum Colors { get; private set;}
		
		/// <summary>
		/// Gets or sets the action that occurs on the resolution of the stack object (e.g. putting a creature onto the battlefield; giving a creature +1/+0)
		/// </summary>
		/// <value>
		/// The resolution action.
		/// </value>
		public GameActionDelegate ResolutionAction { get; private set;}
		
		public StackObject(StackObjectType objectType, string typeString, string text, 
		                   Player owner, Player controller, ColorsEnum colors, 
		                   GameActionDelegate resolutionAction)
		{
			ObjectType = objectType;
			TypeString = typeString;
			Text = text;
			Owner = owner;
			Controller = controller;
			Colors = colors;
			ResolutionAction = resolutionAction;
		}
		
		/// <summary>
		/// The type of the object on the stack
		/// </summary>
		public enum StackObjectType {
			Card,
			TriggeredAbility,
			ActivatedAbility
		}
	}
}


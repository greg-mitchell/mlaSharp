using System;
using mlaSharp;

namespace CardDatabase
{
	[Card("Mountain")]
	public class Mountain : Card
	{
		internal Mountain()
			: base("Mountain","Basic Land - Mountain", "", "{t}: Add {r} to your mana pool.")
		{ }
		
		public Mountain(Player owner)
			: this()
		{
			Owner = owner;
			
			ActivatedAbilities.Add(new Ability(	
				(Player p, State s) => 
					{
						return (this.Status & Card.StatusEnum.Tapped) == Card.StatusEnum.Default;
					},
				(Player p, State s) =>
					{
						s.ManaPools[p].R++;
					}
			));
		}
	}
	
	[Card("Goblin Piker")]
	public class GoblinPiker : CreatureCard
	{
		internal GoblinPiker()
			: base("Goblin Piker", "Creature - Goblin", "1R", "", 2, 1)
		{ }
		
		public GoblinPiker(Player owner)
			: this()
		{
			Owner = owner;
		}
	}
}


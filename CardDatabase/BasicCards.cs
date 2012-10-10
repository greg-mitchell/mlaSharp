using System;
using mlaSharp;

namespace CardDatabase
{
	public class Mountain : Card
	{
		public Mountain(Player owner)
			: base("Mountain","Basic Land - Mountain", "", "{t}: Add {r} to your mana pool.")
		{
			ActivatedAbilities.Add(new Ability(	
				(Player p, State s) => 
					{
						return (int)(this.Status & Card.StatusEnum.Tapped) == 0;
					},
				(Player p, State s) =>
					{
						p.ManaPool.R++;
					}
			));
		}
	}
	
	public class GoblinPiker : CreatureCard
	{
		public GoblinPiker(Player owner)
			: base("Goblin Piker", "Creature - Goblin", "1R", "", 2, 1)
		{
			
		}
	}
}


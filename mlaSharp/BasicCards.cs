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
						this.Status |= Card.StatusEnum.Tapped;
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
	
	[Card("Grey Ogre")]
	public class GreyOgre : CreatureCard
	{
		internal GreyOgre()
			: base("Grey Ogre", "Creature - Giant", "2R", "", 2, 2)
		{ }
		
		public GreyOgre(Player owner)
			: this()
		{
			Owner = owner;
		}		
	}
	
	[Card("Hill Giant")]
	public class HillGiant : CreatureCard
	{
		internal HillGiant()
			: base("Hill Giant", "Creature - Giant", "3R", "", 3, 3)
		{ }
		
		public HillGiant(Player owner)
			: this()
		{
			Owner = owner;
		}		
	}
}


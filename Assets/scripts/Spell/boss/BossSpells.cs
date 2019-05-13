using System.Collections.Generic;
using Animations;
using Animations.boss;
using Animations.warriors;

namespace Spells.boss
{
    public class BossSpells
    {
        public static readonly Spell normalAttack;

        static BossSpells()
        {
            normalAttack = createNormalAttackSpell();
        }

        private static Spell createNormalAttackSpell()
        {
            //todo: here we need to refactor the animation status so that each kind of role has their own animation status.
            return new Spell("melee_attack", 0, BossAnimationStatus.Attack51)
            {
                physicalAttackStrengh = 10
            };
        }

        //todo: several methods to get lists of different kinds of spells, this is useful when we need to display 
        //todo: different lists of spells in the group rule making interface.
        public List<Spell> getPhysicalSpells()
        {
            return null;
        }
    }
}
using System.Collections.Generic;
using Animations;

namespace Spells.warriors
{
    public class RogueSpells
    {
        public static readonly Spell normalAttack;

        static RogueSpells()
        {
            normalAttack = createNormalAttackSpell();
        }

        private static Spell createNormalAttackSpell()
        {
            return new Spell("melee_attack", 0, AnimationStatus.Attack51)
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
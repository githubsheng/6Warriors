using System.Collections.Generic;
using Buffs;

namespace Spells
{
    public class Spell {
        public int physicalAttackStrengh;
        public int magicalAttackPower;
        public int magicType;
        public int hpHeal;
        public int manaHeal;
        public CharacterBuff buff;

        public static readonly int MAGIC_TYPE_FIRE = 1;
        public static readonly int MAGIC_TYPE_ICE = 2;

        public static Spell createPhysicalAttack(int physicalAttackStrength) {
            Spell spell = new Spell {physicalAttackStrengh = physicalAttackStrength};
            return spell;
        }

    }
    
}
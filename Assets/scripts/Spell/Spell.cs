using System.Collections.Generic;

namespace Spells
{
    public class Spell
    {
        public int physicalAttackStrengh;
        public int magicalAttackPower;
        public int magicType;
        public int hpHeal;
        public int manaHeal;
        public string name;
        public CharacterBuff buff;
        public float spellEffectiveRange;
        public List<int> animationStatusList;
        private int animationStatusIndex = 0;

        public static readonly int MAGIC_TYPE_FIRE = 1;
        public static readonly int MAGIC_TYPE_ICE = 2;

        public Spell(string name, float spellEffectiveRange, int animationStatus)
        {
            this.name = name;
            this.spellEffectiveRange = spellEffectiveRange;
            animationStatusList = new List<int>{animationStatus};
        }

        public Spell(string name, float spellEffectiveRange, List<int> animationStatusList)
        {
            this.name = name;
            this.spellEffectiveRange = spellEffectiveRange;
            this.animationStatusList = animationStatusList;
        }

        public int getAnimationStatus()
        {
            if (animationStatusList.Count == 1) return animationStatusList[0];
            int ret = animationStatusList[animationStatusIndex];
            animationStatusIndex++;
            if (animationStatusIndex == animationStatusList.Count) animationStatusIndex = 0;
            return ret;
        }
    }
}
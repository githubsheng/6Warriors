using System.Collections.Generic;
using Buffs;
using UnityEngine;

namespace Spells
{
    public class Spell {
        public string name;
        public float attackStrength;
        public int magicType;
        public float hpHeal;
        public float manaHeal;
        public float manaConsumed;
        public float manaGenerated;
        public CharacterBuff buff;
        public GameObject prefab;
        public GameObject originPrefab;
        public int penetration = 1;

        public static readonly int MAGIC_TYPE_FIRE = 1;
        public static readonly int MAGIC_TYPE_ICE = 2;
        public static readonly int MAGIC_TYPE_HOLY = 3;
        
        public static Spell createNormalAttack(float attackStrength) {
            Spell spell = new Spell {attackStrength = attackStrength };
            return spell;
        }


    }
    
}
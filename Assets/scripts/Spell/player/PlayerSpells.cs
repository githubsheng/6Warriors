using UnityEngine;

namespace Spells.ArrowAttack {
    public class PlayerSpells {

        private static Spell getBaseDemonArrow(GameObject prefab) {
            return new Spell {
                attackStrength = 10f,
                manaGenerated = 5f,
                prefab = prefab
            };
        }
        
        private static Spell getBaseFireArrow(GameObject prefab) {
            return new Spell {
                attackStrength = 18f,
                manaConsumed = 3f,
                magicType = Spell.MAGIC_TYPE_FIRE,
                prefab = prefab
            };
        }
        
        private static Spell getBaseIceArrow(GameObject prefab) {
            return new Spell {
                attackStrength = 18f,
                manaConsumed = 2f,
                magicType = Spell.MAGIC_TYPE_ICE,
                prefab = prefab
            };
        }
        
        private static Spell getBasePoisonArrow(GameObject prefab) {
            return new Spell {
                attackStrength = 15,
                manaConsumed = 2f,
                magicType = Spell.MAGIC_TYPE_POISION,
                prefab = prefab
            };
        }

        private static void adjustStrength(Spell spell, float weaponAddition, int playerLevel) {
            spell.attackStrength *= playerLevel - 1; 
            spell.attackStrength += weaponAddition;
        }

        public static Spell getDemonArrow(float weaponAddition, int playerLevel, GameObject prefab) {
            Spell spell = getBaseDemonArrow(prefab);
            adjustStrength(spell, weaponAddition, playerLevel);
            return spell;
        }
        
        public static Spell getFireArrow(float weaponAddition, int playerLevel, GameObject prefab) {
            Spell spell = getBaseFireArrow(prefab);
            adjustStrength(spell, weaponAddition, playerLevel);
            return spell;
        }

        
        public static Spell getIceArrow(float weaponAddition, int playerLevel, GameObject prefab) {
            Spell spell = getBaseIceArrow(prefab);
            adjustStrength(spell, weaponAddition, playerLevel);
            return spell;
        }
        
        public static Spell getPoisonArrow(float weaponAddition, int playerLevel, GameObject prefab) {
            Spell spell = getBasePoisonArrow(prefab);
            adjustStrength(spell, weaponAddition, playerLevel);
            return spell;
        }
        

    }
}
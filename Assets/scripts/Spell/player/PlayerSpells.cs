using CharacterControllers;
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

        private static void adjustStrength(Spell spell, CharacterStatus characterStatus) {
            spell.attackStrength += characterStatus.playerLevel - 1; 
            spell.attackStrength += characterStatus.weaponAddition;
        }

        public static Spell getDemonArrow(CharacterStatus characterStatus, GameObject prefab) {
            Spell spell = getBaseDemonArrow(prefab);
            adjustStrength(spell, characterStatus);
            return spell;
        }
        
        public static Spell getFireArrow(CharacterStatus characterStatus, GameObject prefab) {
            Spell spell = getBaseFireArrow(prefab);
            adjustStrength(spell, characterStatus);
            return spell;
        }

        
        public static Spell getIceArrow(CharacterStatus characterStatus, GameObject prefab) {
            Spell spell = getBaseIceArrow(prefab);
            adjustStrength(spell, characterStatus);
            return spell;
        }
        
        public static Spell getPoisonArrow(CharacterStatus characterStatus, GameObject prefab) {
            Spell spell = getBasePoisonArrow(prefab);
            adjustStrength(spell, characterStatus);
            return spell;
        }
        

    }
}
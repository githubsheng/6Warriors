using Buffs.player;
using CharacterControllers;
using UnityEngine;

namespace Spells {
    public class PlayerSpells {

        private static Spell getBaseDemonArrow(GameObject prefab, GameObject originPrefab) {
            return new Spell {
                name = "demon_arrow",
                attackStrength = 10f,
                manaGenerated = 5f,
                prefab = prefab,
                originPrefab = originPrefab
            };
        }
        
        private static Spell getBaseHolyArrow(GameObject prefab, GameObject originPrefab) {
            return new Spell {
                name = "holy_arrow",
                attackStrength = 18f,
                manaConsumed = 3f,
                magicType = Spell.MAGIC_TYPE_HOLY,
                prefab = prefab,
                originPrefab = originPrefab,
                buff = new HolyStack()
            };
        }
        
        private static Spell getBaseIceArrow(GameObject prefab, GameObject originPrefab) {
            return new Spell {
                name = "ice_arrow",
                attackStrength = 18f,
                manaConsumed = 2f,
                magicType = Spell.MAGIC_TYPE_ICE,
                prefab = prefab,
                originPrefab = originPrefab,
                buff = new Frozen()
            };
        }
        
        private static Spell getBaseTripleArrows(GameObject prefab, GameObject originPrefab) {
            return new Spell {
                name = "triple_arrows",
                attackStrength = 18f,
                manaConsumed = 2f,
                penetration = 4,
                magicType = Spell.MAGIC_TYPE_ICE,
                prefab = prefab,
                originPrefab = originPrefab
            };
        }
        
        private static Spell getBasePowerShot(GameObject prefab) {
            return new Spell {
                name = "power_shot",
                attackStrength = 100f,
                manaConsumed = 20f,
                magicType = Spell.MAGIC_TYPE_ICE,
                prefab = prefab
            };
        }
        

        private static void adjustStrength(Spell spell, CharacterStatus characterStatus) {
            spell.attackStrength += characterStatus.playerLevel - 1; 
            spell.attackStrength += characterStatus.gearAttackStrengh;
        }

        public static Spell getDemonArrow(CharacterStatus characterStatus, GameObject prefab, GameObject originPrefab) {
            Spell spell = getBaseDemonArrow(prefab, originPrefab);
            adjustStrength(spell, characterStatus);
            return spell;
        }
        
        public static Spell getHolyArrow(CharacterStatus characterStatus, GameObject prefab, GameObject originPrefab) {
            Spell spell = getBaseHolyArrow(prefab, originPrefab);
            adjustStrength(spell, characterStatus);
            return spell;
        }

        
        public static Spell getIceArrow(CharacterStatus characterStatus, GameObject prefab, GameObject originPrefab) {
            Spell spell = getBaseIceArrow(prefab, originPrefab);
            adjustStrength(spell, characterStatus);
            return spell;
        }
        
        public static Spell getTripleArrows(CharacterStatus characterStatus, GameObject prefab, GameObject originPrefab) {
            Spell spell = getBaseTripleArrows(prefab, originPrefab);
            adjustStrength(spell, characterStatus);
            return spell;
        }
        
        public static Spell getPowerShot(CharacterStatus characterStatus, GameObject prefab) {
            Spell spell = getBasePowerShot(prefab);
            adjustStrength(spell, characterStatus);
            return spell;
        }

    }
}
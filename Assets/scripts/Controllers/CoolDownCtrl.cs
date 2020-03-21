using System;
using System.Collections.Generic;
using UnityEngine;

namespace Controllers {
    public class CoolDownCtrl : MonoBehaviour {

        public float stopWatch;
        public float iceShield;
        public float darkTrap;
        public float magicalTurret;
        public float powerShot;
        
        private Dictionary<string, float> cooldowns = new Dictionary<string, float>();
        private Dictionary<string, float> cooldownStarts = new Dictionary<string, float>();


        private void Awake() {
            //try registering all player spells here...minion spells are simple and do not need this
            registerCooldown("stop_watch", stopWatch);
            registerCooldown("ice_shield", iceShield);
            registerCooldown("dark_trap", darkTrap);
            registerCooldown("magical_turret", magicalTurret);
            registerCooldown("power_shot", powerShot);
        }

        public void registerCooldown(string spellName, float time) {
            cooldowns[spellName] = time;
        }

        public void cooldown(string spellName) {
            cooldownStarts[spellName] = Time.time;
        }

        public float getCooldownRatio(string spell) {
            if (!cooldownStarts.ContainsKey(spell)) return 0;
            float diff = Time.time - cooldownStarts[spell];
            float cooldown = cooldowns[spell];
            float ratio = (cooldown - diff) / cooldown;
            return Math.Max(0, ratio);
        }
        
    }
}
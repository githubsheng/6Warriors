using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace Controllers {
    
    
    public class ActionBarCtrl : MonoBehaviour {
        
        private CoolDownCtrl coolDownCtrl;

        public Image stopWatchCoolDown;
        public Image iceShield;
        public Image darkTrap;
        public Image magicalTurret;
        public Image powerShot;
        
        private void Start() {
            coolDownCtrl = GameObject.Find("cool_down_ctrl").GetComponent<CoolDownCtrl>();
        }

        private void Update() {
            stopWatchCoolDown.fillAmount = coolDownCtrl.getCooldownRatio("stop_watch");
            iceShield.fillAmount = coolDownCtrl.getCooldownRatio("ice_shield");           
            darkTrap.fillAmount = coolDownCtrl.getCooldownRatio("dark_trap");
            magicalTurret.fillAmount = coolDownCtrl.getCooldownRatio("magical_turret");
            powerShot.fillAmount = coolDownCtrl.getCooldownRatio("power_shot");
        }
    }
}
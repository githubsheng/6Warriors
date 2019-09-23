using UnityEngine;
using UnityEngine.UI;

namespace Gui.BufferIndicator {
    public class BufferIndicator : UnityEngine.MonoBehaviour {
        
        public Image shieldBroken;
        public Image sword01;
        public Image sword02;
        public Image sword03;
        public Image sword04;
        public Image sword05;
        public Image frost;
        private Image[] swords;

        private void Awake() {
            swords = new[] {sword01, sword02, sword03, sword04, sword05};
        }

        public void enableShieldBroken() {
            if(!gameObject.activeSelf) gameObject.SetActive(true);
            disableAll();
            shieldBroken.enabled = true;
        }

        public void enableSword(int count) {
            if(!gameObject.activeSelf) gameObject.SetActive(true);
            disableAll();
            for (int i = 0; i < count; i++) {
                Debug.Log("setting sword " + i + " to be true");
                swords[i].enabled = true;
            }
        }

        public void enableFrost() {
            if(!gameObject.activeSelf) gameObject.SetActive(true);
            disableAll();
            frost.enabled = true;
        }

        private void disableAll() {
            shieldBroken.enabled = false;
            foreach (Image sword in swords) {
                sword.enabled = false;
            }

            frost.enabled = false;
        }

        public void removeAll() {
            disableAll();
            gameObject.SetActive(false);
        } 
               

    }
}
using UnityEngine;
using UnityEngine.UI;

namespace Gui.HealthBar {
    public class FloatingHealthBar : UnityEngine.MonoBehaviour {

        private Vector3 offsetUpwards;
        private bool isReady;
        private Image healthBarImage;
        private Camera mainCamera;
        private Transform toFollow;
        
        public GameObject healthBarImageUI;
        
        private void Start() {
            mainCamera = Camera.main;
            transform.position += offsetUpwards;
            healthBarImage = healthBarImageUI.GetComponent<Image>();
        }

        public void setHealthBarAttrib(float offsetUpwards) {
            this.offsetUpwards = new Vector3(0, offsetUpwards, 0);
            isReady = true;
        }

        void LateUpdate()
        {
            transform.LookAt(transform.position + mainCamera.transform.rotation * Vector3.forward,
                mainCamera.transform.rotation * Vector3.up);
        }


        public void setHealth(float percentage) {
            healthBarImage.fillAmount = percentage;
        }

    }
}
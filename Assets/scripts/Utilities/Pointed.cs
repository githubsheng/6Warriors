using CharacterControllers;
using UnityEngine;

namespace Utilities {
    public class Pointed : UnityEngine.MonoBehaviour {

        public GameObject empty;
        private GameObject pointed;
        private int characterMask = 1 << 9;
        public int camRayLength;
        Ray ray;
        RaycastHit hit;
        private Camera mainCamera;

        private void Start() {
            mainCamera = Camera.main;
            pointed = empty;
        }

        void Update()
        {
            ray = mainCamera.ScreenPointToRay(Input.mousePosition);
            if(Physics.Raycast(ray, out hit, camRayLength, characterMask)) {
                GameObject hitted = hit.collider.gameObject;
                if (hitted != pointed && hitted.CompareTag("Enemy")) {
                    if(pointed != empty) pointed.GetComponent<MinionCtrl>().disablePointedIndicator();
                    hitted.GetComponent<MinionCtrl>().enablePointedIndicator();
                    pointed = hitted;
                }
            } else {
                if (pointed != empty) {
                    if(pointed) pointed.GetComponent<MinionCtrl>().disablePointedIndicator();
                    pointed = empty;
                }
            }
        }
    }
}
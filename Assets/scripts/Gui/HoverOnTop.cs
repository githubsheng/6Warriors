using UnityEngine;

namespace Gui {
    
    
    public class HoverOnTop : UnityEngine.MonoBehaviour {
        
        private Vector3 offsetUpwards;
        private Camera mainCamera;
        
        private void Start() {
            mainCamera = Camera.main;
            transform.position += offsetUpwards;
        }
        
        void LateUpdate()
        {
            transform.LookAt(transform.position + mainCamera.transform.rotation * Vector3.forward,
                mainCamera.transform.rotation * Vector3.up);
        }
    }
}
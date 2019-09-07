using UnityEngine;

namespace Spells.ArrowAttack {
    
    public class ArrowSpinning : MonoBehaviour {
        public float spinSpeed;
        
        private void Update() {
            gameObject.transform.Rotate(0, 0, spinSpeed * Time.deltaTime, Space.Self);
        }
    }
}
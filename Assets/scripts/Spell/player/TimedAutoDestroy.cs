using UnityEngine;

namespace Spells {
    public class TimedAutoDestroy : MonoBehaviour {

        public float destroyAfterSeconds;
        private float destroyTime;
        
        private void Start() {
            destroyTime = Time.time + destroyAfterSeconds;
        }

        private void Update() {
            if(Time.time > destroyTime) Destroy(gameObject);
        }
    }
}
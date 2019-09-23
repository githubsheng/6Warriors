using System.Numerics;
using CharacterControllers;
using Spells;
using UnityEngine;
using Quaternion = UnityEngine.Quaternion;
using Vector3 = UnityEngine.Vector3;

namespace Spells.ArrowAttack {
    public class ArrowAttack : MonoBehaviour {
        private Vector3 direction;
        private Spell spell;
        private bool isReady;
        public float speed;
        public GameObject onHitParticlePreb;

        public void setAttackAttrib(Spell spell, Vector3 direction) {
            this.spell = spell;
            this.direction = Vector3.Normalize(direction);
            isReady = true;
        }

        private void Update() {
            if (!isReady) return;
            transform.position += speed * direction * Time.deltaTime;
        }

        private void OnTriggerEnter(Collider other) {
            if (other.CompareTag("Enemy")) {
                MinionCtrl ctrl = other.gameObject.GetComponent<MinionCtrl>();
                ctrl.receiveSpell(spell);
                Instantiate(onHitParticlePreb, transform.position, Quaternion.LookRotation(-transform.forward));
                spell.penetration--;
                if (spell.penetration == 0) {
                    Destroy(gameObject);                    
                }
//                Vector3 contactPoint = transform.position + transform.forward;
//                Instantiate(onHitParticlePreb, contactPoint, Quaternion.LookRotation(-transform.forward));
            }
            else if (other.CompareTag("Wall")) {
                Instantiate(onHitParticlePreb, transform.position, Quaternion.LookRotation(-transform.forward));
                Destroy(gameObject);
            }
        }

        void OnBecameInvisible() {
            Destroy(gameObject);
        }
    }
}
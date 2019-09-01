using CharacterControllers;
using Spells;
using UnityEngine;

namespace Spells.ArrowAttack {
    public class ArrowAttack : MonoBehaviour {
        private Vector3 direction;
        private Spell spell;
        private bool isReady;
        public float speed;

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
                Destroy(gameObject);
                //todo: spawn a hit particle system
            }
            else if (other.CompareTag("Wall")) {
                //todo: spawn a hit particle system
                Destroy(gameObject);
            }
        }

        void OnBecameInvisible() {
            Destroy(gameObject);
        }
    }
}
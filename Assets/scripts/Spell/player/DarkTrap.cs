using System.Collections;
using System.Collections.Generic;
using CharacterControllers;
using UnityEngine;

namespace Spells {
    public class DarkTrap : MonoBehaviour {
        public float baseDarkTrapAttackStrength;
        public float weaponAttackStrengh;
        public GameObject darkTrapExplosion;
        public GameObject darkTrapTrigger;
        private bool isTriggered = false;
        private HashSet<GameObject> inside = new HashSet<GameObject>();
        private float triggerDistance = 1f;
        private void OnTriggerEnter(Collider other) {
            if (other.CompareTag("Enemy")) {
                inside.Add(other.gameObject);
            }
        }

        private void OnTriggerExit(Collider other) {
            if (other.CompareTag("Enemy")) {
                inside.Remove(other.gameObject);
            }
        }

        private void Start() {
            StartCoroutine(checkTrigger());
        }

        private IEnumerator checkTrigger() {
            //a trap can only be triggered after half second later.
            yield return new WaitForSeconds(0.5f);
            while (!isTriggered) {
                foreach (GameObject enemy in inside) {
                    if (enemy && Vector3.Distance(enemy.transform.position, transform.position) < triggerDistance) {
                        isTriggered = true;
                        Destroy(darkTrapTrigger);
                        Instantiate(darkTrapExplosion, transform.position, Quaternion.identity);
                        break;
                    }
                }

                if (isTriggered) {
                    foreach (GameObject enemy in inside) {
                        if (enemy) {
                            enemy.GetComponent<MinionCtrl>().receiveSpell(Spell.createNormalAttack(baseDarkTrapAttackStrength + weaponAttackStrengh));                               
                        }
                    }
                    Destroy(gameObject);
                }
                else {
                    yield return new WaitForSeconds(0.1f);
                }
            }
        }
    }
}
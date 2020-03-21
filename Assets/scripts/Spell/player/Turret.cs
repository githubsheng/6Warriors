using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using CharacterControllers;
using Spells;

namespace Spells {
    public class Turret : UnityEngine.MonoBehaviour {
        public GameObject lootStartFX;
        public GameObject lootBaseFX;
        public GameObject lootObjectFX;
        public GameObject lootEndFX;
        public GameObject loot_Object;

        public ParticleSystem ground_Glow;
        public GameObject ground_Glow_Group;
        public ParticleSystem ground_Beam;
        public ParticleSystem ground_Rings;
        public GameObject loot_Glow_Group;
        public ParticleSystem loot_Rings;
        public GameObject loot_Rings_Group;
        public ParticleSystem loot_Sparks;
        public ParticleSystem loot_Electricity;

        public Light loot_Light;
        private float fadeStart = 2.0f;
        private float fadeEnd = 0;
        private float fadeTime = 1; 
        private float t = 0.0f;

        private bool lootActive = false;

        private Transform target;
        private MinionCtrl targetMinionCtrl;
        private Queue<Transform> targetCandidates = new Queue<Transform>();
        private HashSet<Transform> inRange = new HashSet<Transform>();
        private float freezeUntil;
        public float attackInterval;
        public GameObject arrowPrefab;
        public float baseAttackPower;
        private Vector3 targetToTurret;
        public float attackRange;
        public float arrowOutAnimationDelay;
        private Animator weaponAnimator;
        private static readonly int Shoot = Animator.StringToHash("shoot");
        private bool operational;
        public float lifeSpan;
        
        void Start() {
            lootStartFX.SetActive(false);
            lootBaseFX.SetActive(false);
            lootObjectFX.SetActive(false);
            lootEndFX.SetActive(false);
            StartCoroutine("initTurretVisual");
            StartCoroutine("endTurretVisual");
            weaponAnimator = loot_Object.GetComponentInChildren<Animator>();
        }

        private void Update() {
            if (!operational) return;
            if (Time.time <= freezeUntil) return;
            //if there is no target, then choose a target, chooseTarget returns null if no target can be found
            if (!target) {
                target = chooseTarget();
                if (!target) return; //no new target can be found.
                //cache the ctrl here to avoid calling getComponent in every update.
                targetMinionCtrl = target.gameObject.GetComponent<MinionCtrl>();
            }
            //so up until this point, we must have a valid target, and a minion ctrl.
            if (targetMinionCtrl.isDead()) {
                //set target to be null so next update, we will try to select a new one.
                target = null;
                return;
            };
            
            if (Vector3.Distance(target.position, transform.position) > attackRange) return;
            tryTurn();
            tryAttack();  
        }

        private void OnTriggerEnter(Collider other) {
            Transform targetTransform = other.transform;
            if (other.CompareTag("Enemy")) {
                targetCandidates.Enqueue(targetTransform);
                inRange.Add(targetTransform);
            }
        }

        private void OnTriggerExit(Collider other) {
            if (other.CompareTag("Enemy")) {
                inRange.Remove(other.transform);
            }
        }

        private Transform chooseTarget() {
            while (true) {
                if (targetCandidates.Count == 0) return null;
                Transform targetTransform = targetCandidates.Dequeue();
                //this candidate may have been killed and...then null
                if (inRange.Contains(targetTransform)) {
                    return targetTransform;
                }
            }
        }

        private void tryTurn() {
            Vector3 selfPos = transform.position;
            targetToTurret = target.position - selfPos;
            targetToTurret.y = 0;
            targetToTurret = Vector3.Normalize(targetToTurret);
            Quaternion newRotation = Quaternion.LookRotation(targetToTurret);
            loot_Object.transform.rotation = newRotation;
        }
        
        private void tryAttack() {
            if (Time.time <= freezeUntil) return;
            Spell spell = Spell.createNormalAttack(baseAttackPower);
            freezeUntil = Time.time + attackInterval;
            weaponAnimator.SetTrigger(Shoot);
            //attack interval is the attack animation length (see how it is calculated). and I need the arrow
            //to be spawned at nearly the end of the attack animation.
            StartCoroutine(executeSpellAfter(spell, arrowOutAnimationDelay, loot_Object.transform.position, targetToTurret));           
        }
        
        private void spawnArrow(Spell spell, Vector3 spawnPos, Vector3 hitToSpawn) {
            GameObject arrow = Instantiate(arrowPrefab, spawnPos, Quaternion.LookRotation(hitToSpawn));
            ArrowAttack arrowAttack = arrow.GetComponent<ArrowAttack>();
            arrowAttack.setAttackAttrib(spell, hitToSpawn);  
        }
        
        private IEnumerator executeSpellAfter(Spell spell, float delay, Vector3 spawnPos, Vector3 hitToSpawn)
        {
            yield return new WaitForSeconds(delay);
            spawnArrow(spell, spawnPos, hitToSpawn);
            if (spell.originPrefab) Instantiate(spell.originPrefab, spawnPos, Quaternion.LookRotation(hitToSpawn));
        }
        
        IEnumerator initTurretVisual() {
            ground_Glow_Group.SetActive(true);
            loot_Glow_Group.SetActive(true);
            loot_Rings_Group.SetActive(true);
            loot_Object.SetActive(true);

            lootActive = true;

            yield return new WaitForSeconds(0.1f);

            lootStartFX.SetActive(true);

            yield return new WaitForSeconds(0.2f);

            lootBaseFX.SetActive(true);

            yield return new WaitForSeconds(1.0f);

            lootObjectFX.SetActive(true);
            operational = true;
        }


        IEnumerator endTurretVisual() {
            operational = false;
            yield return new WaitForSeconds(lifeSpan);
            lootEndFX.SetActive(true);

            ground_Glow_Group.SetActive(false);
            loot_Glow_Group.SetActive(false);
            loot_Rings_Group.SetActive(false);
            loot_Object.SetActive(false);

            StartCoroutine("FadeLight");

            ground_Rings.Stop();
            ground_Beam.Stop();
            loot_Rings.Stop();
            loot_Sparks.Stop();
            loot_Electricity.Stop();


            yield return new WaitForSeconds(2.5f);


            lootStartFX.SetActive(false);
            lootBaseFX.SetActive(false);
            lootObjectFX.SetActive(false);
            lootEndFX.SetActive(false);

            lootActive = false;
            Destroy(gameObject);
        }

        IEnumerator FadeLight() {
            while (t < fadeTime) {
                t += Time.deltaTime;
                loot_Light.intensity = Mathf.Lerp(fadeStart, fadeEnd, t / fadeTime);
                yield return 0;
            }

            t = 0;
        }
    }
}
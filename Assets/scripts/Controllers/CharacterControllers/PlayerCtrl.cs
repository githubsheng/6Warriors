using System.Collections;
using System.Collections.Generic;
using Buffs;
using Buffs.player;
using guiraffe.SubstanceOrb;
using Spells;
using Spells.ArrowAttack;
using UnityEngine;
using UnityEngine.UI;
using Quaternion = UnityEngine.Quaternion;
using Vector3 = UnityEngine.Vector3;

namespace CharacterControllers {
    //todo: I need to get rid of the animation event and find an alternative
    public class PlayerCtrl : MonoBehaviour {

        private Animator animator;
        private CharacterStatus characterStatus;
        private CharacterController unityCharacterController;
        
        private int commonAnimationParam = Animator.StringToHash("animationStatus");
        private static readonly int PowerShotTrigger = Animator.StringToHash("powerShotTrigger");

        private int turnDetectionMask = 1 << 10;
        private int arrowDirectionMask = 1 << 11;
        private int charactersMask = 1 << 9;
        private Vector3 playerToMouse;
        private float freezeUntil;
        private int attackAnimationValUsed;
        private Transform arrowSpawnPos;
        private Vector3 playerChestPosition;
        private OrbFill hpOrbFill;
        private OrbAnimator hpOrbAnimator;
        private OrbFill mpOrbFill;
        private OrbAnimator mpOrbAnimator;
        private float attackInterval;
        
        private Vector3 moveDirection = Vector3.zero;
        private float gravity = 60f;

        private float slowModeUntil = float.MaxValue;

        private GameObject iceShieldInstance;
        private Dictionary<string, CharacterBuff> selfBuffers = new Dictionary<string, CharacterBuff>();
        
        private Camera mainCamera;

        
        
        public Image slowModeIndicator;
        public int runAnimationVal;
        //todo: if no input for a time, go back to idle mode
        public int idleAnimationVal;
        public int readyAnimationVal;
        public int attack01AnimationVal;
        public int attack02AnimationVal;
        //when doing powershot, set commonAnimationParam to this so that animation does not enter any other state.
        private int invalidAnimationValForPowerShot = -1;
        public int dieAnimationVal;
        
        public float maxBaseHp;
        public float maxBaseMana;
        public float baseAttackStrength;
        public float baseMagicPower;
       
        public int camRayLength;
        
        public int movementSpeed;

        public GameObject demonArrowPrefab;
        public GameObject demonArrowOriginPrefab;
        public GameObject holyArrowPrefab;
        public GameObject holyArrowOriginPrefab;
        public GameObject iceArrowPrefab;
        public GameObject iceArrowOriginPrefab;
        public GameObject iceShieldPrefab;


        public GameObject powerShotWingsPrefab;
        public GameObject powerShotArrowPrefab;
        public GameObject powerShotOriginPrefab;
        private Vector3 powerWingsLocalAdjustment = new Vector3(0, 0.479f, -0.366f);
        public float powerShotInterval;
        public float powerShotArrowSpawnDelay;

        public GameObject darkTrapPrefab;

        private void Start() {
            unityCharacterController = gameObject.GetComponent<CharacterController>();
            animator = gameObject.GetComponentInChildren<Animator>();
            characterStatus = new CharacterStatus(maxBaseHp, maxBaseMana, baseAttackStrength);
            arrowSpawnPos = transform.Find("arrow_spawn_pos");
            mainCamera = Camera.main;
            hpOrbFill = GameObject.Find("player_hp_fill").GetComponent<OrbFill>();
            mpOrbFill = GameObject.Find("player_mp_fill").GetComponent<OrbFill>();
            hpOrbAnimator = GameObject.Find("player_hp_fill").GetComponent<OrbAnimator>();
            mpOrbAnimator = GameObject.Find("player_mp_fill").GetComponent<OrbAnimator>();
            
            attackInterval = getAttackAnimationLength();
        }

        private float getAttackAnimationLength() {
            AnimationClip[] clips = animator.runtimeAnimatorController.animationClips;
            for (int i = 0; i < clips.Length; i++) {
                if (clips[i].name == "AttackSeq01") {
                    return clips[i].length / 1.5f;
                }
            }
            return 100f;
        }
       
        private void Update() {
            if(Time.time > slowModeUntil) outOfSlowMode();
            
            characterStatus.reEvaluateStatusEverySecond();
            reEvaluateBuff();
            hpOrbFill.Fill = characterStatus.hp / characterStatus.maxHp;
            mpOrbFill.Fill = characterStatus.mana / characterStatus.maxMana;
            if (characterStatus.isDead) {
                onKilled();
                return;
            }

            tryBecomeReady();
            tryTurn();
            tryMove();
            trySpecialSpell();
            tryAttack();  
        }
        
        private bool isAttackKeyPressed() {
            return Input.GetKey(KeyCode.Q) || Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.E) || Input.GetKey(KeyCode.F) || Input.GetKey(KeyCode.A);
        }
        
        private void tryBecomeReady() {
            if (Time.time <= freezeUntil) return;
            if(!Input.GetKey(KeyCode.Space) && !isAttackKeyPressed()) animator.SetInteger(commonAnimationParam, readyAnimationVal);
        }

        private void tryTurn() {
            if (Time.time <= freezeUntil) return;
            if (Input.GetKey(KeyCode.Space) || isAttackKeyPressed()) {
                Ray camRay = mainCamera.ScreenPointToRay(Input.mousePosition);
                RaycastHit hit;
                if (Physics.Raycast(camRay, out hit, camRayLength, turnDetectionMask)) {
                    //todo: add comments why we need chest position.
                    //todo: make 1.31f constant
                    Vector3 pos = transform.position;
                    playerChestPosition.Set(pos.x, 1.31f, pos.z);
                    //cache the moue pos to avoid extra ray casts
                    playerToMouse = hit.point - playerChestPosition;
                    playerToMouse.y = 0;
                    playerToMouse = Vector3.Normalize(playerToMouse);
                    Quaternion newRotation = Quaternion.LookRotation(playerToMouse);
                    gameObject.transform.rotation = newRotation;
                }
            }
        }

        private void tryMove() {
            if (Time.time <= freezeUntil) return;
            //if both attack and run are pressed, we attack
            moveDirection = Vector3.zero;
            if (Input.GetKey(KeyCode.Space) && !isAttackKeyPressed()) {
                animator.SetInteger(commonAnimationParam, runAnimationVal);
                moveDirection = movementSpeed * playerToMouse;
            }
            //apply gravity regardless of user pressing the move button or not
            moveDirection.y = -gravity * Time.deltaTime;
            unityCharacterController.Move(moveDirection * Time.deltaTime);
        }
                
        private IEnumerator executeSpellAfter(Spell spell, float delay, Vector3 spawnPos, Vector3 hitToSpawn)
        {
            yield return new WaitForSeconds(delay);
            spawnArrow(spell, spawnPos, hitToSpawn);
            if (spell.originPrefab) Instantiate(spell.originPrefab, spawnPos, Quaternion.LookRotation(hitToSpawn));
        }

        private IEnumerator executeTripleArrowAfter(Spell spell, float delay, Vector3 spawnPos, Vector3 hitToSpawn)
        {
            yield return new WaitForSeconds(delay);
            spawnArrow(spell, spawnPos, hitToSpawn);
            //need to instantiate two more...
            Vector3 slightlyLeft = Quaternion.AngleAxis(-10f, Vector3.up) * hitToSpawn;
            spawnArrow(spell, spawnPos, slightlyLeft);
            Vector3 slightlyRight = Quaternion.AngleAxis(10f, Vector3.up) * hitToSpawn;
            spawnArrow(spell, spawnPos, slightlyRight);
            if (spell.originPrefab) Instantiate(spell.originPrefab, spawnPos, Quaternion.LookRotation(hitToSpawn));
        }

        private void spawnArrow(Spell spell, Vector3 spawnPos, Vector3 hitToSpawn) {
            GameObject arrow = Instantiate(spell.prefab, spawnPos, Quaternion.LookRotation(hitToSpawn));
            ArrowAttack arrowAttack = arrow.GetComponent<ArrowAttack>();
            arrowAttack.setAttackAttrib(spell, hitToSpawn);  
        }
        
        private void castSpell(Spell spell, float delayAfterAnimationPlay) {
            characterStatus.mana -= (int)spell.manaConsumed;
            characterStatus.mana += (int) spell.manaGenerated;
            Vector3 spawnPos = arrowSpawnPos.position;
            //todo: first we need to check if the mouse is pointing at a enemy, only if it missed out, will we
            //todo: resolve to direction mask.
            Ray camRay = mainCamera.ScreenPointToRay(Input.mousePosition);
            Vector3 hitPos;          
            RaycastHit characterHit;
            if (Physics.Raycast(camRay, out characterHit, camRayLength, charactersMask)) {
                hitPos = characterHit.point;
            }
            else {
                RaycastHit arrowPlaneHit; 
                Physics.Raycast(camRay, out arrowPlaneHit, camRayLength, arrowDirectionMask);
                hitPos = arrowPlaneHit.point;
            }
            //cache the moue pos to avoid extra ray casts
            Vector3 hitToSpawn = hitPos - spawnPos;
            hitToSpawn.y = 0;
            hitToSpawn = Vector3.Normalize(hitToSpawn);
            if (spell.name.Equals("triple_arrows")) {
                StartCoroutine(executeTripleArrowAfter(spell, delayAfterAnimationPlay, spawnPos, hitToSpawn));
            }
            else {
                StartCoroutine(executeSpellAfter(spell, delayAfterAnimationPlay, spawnPos, hitToSpawn));                
            }
        }

        private void tryAttack() {
            if (Time.time <= freezeUntil) return;
            if (!isAttackKeyPressed()) return;
            if (Time.timeScale < 1f) outOfSlowMode();
            Spell spell = getSpell();
            if (characterStatus.mana < spell.manaConsumed) return;
            if (spell.name.Equals("power_shot")) {
                freezeUntil = Time.time + powerShotInterval;
                GameObject wings = Instantiate(powerShotWingsPrefab, transform);
                Instantiate(powerShotOriginPrefab, arrowSpawnPos.position, arrowSpawnPos.rotation);
                wings.transform.localPosition = powerWingsLocalAdjustment;
                animator.SetInteger(commonAnimationParam, invalidAnimationValForPowerShot);
                animator.SetTrigger(PowerShotTrigger);
                castSpell(spell, powerShotArrowSpawnDelay);
            } else {
                //todo: attackInterval, needs to be spell specific..? or maybe we have common attackInterval, because powershot has a different interval
                freezeUntil = Time.time + attackInterval;
                int attackAnimVal = getAttackAnimationVal();
                animator.SetInteger(commonAnimationParam, attackAnimVal);
                //attack interval is the attack animation length (see how it is calculated). and I need the arrow
                //to be spawned at nearly the end of the attack animation.
                castSpell(spell, attackInterval - 0.1f);         
            }
        }

        private int getAttackAnimationVal() {
            //check the animator controller to see how this code make sense...
            //TODO: add comments about the weird behaviors
            AnimatorStateInfo currentAnimation = animator.GetCurrentAnimatorStateInfo(0);
            return currentAnimation.IsName("Attack01") ? attack02AnimationVal : attack01AnimationVal;
        }

        private void trySpecialSpell() {
            if (Input.GetKeyUp(KeyCode.D)) {
                intoSlowMode();
            } else if (Input.GetKeyUp(KeyCode.S)) {
                addBuff(new IceShield());
            } else if (Input.GetKeyUp(KeyCode.X)) {
                Instantiate(darkTrapPrefab, transform.position + new Vector3(0, 0.1f, 0), Quaternion.identity);
            }
        }

        private void intoSlowMode() {
            Debug.Log("trying to get into slow mode");
            if (Time.timeScale < 1f) return;
            Debug.Log("into slow mode");
            slowModeIndicator.enabled = true;
            Time.timeScale = 0.1f;
            animator.speed *= 10;
            movementSpeed *= 10;
            gravity *= 100;
            hpOrbFill.setTimeScaleOffset(10f);
            hpOrbAnimator.setTimeScaleOffset(10f);
            mpOrbFill.setTimeScaleOffset(10f);
            mpOrbAnimator.setTimeScaleOffset(10f);
            //slow mode lasts for 2 seconds if nothing (like attack interrupts)
            //remember, we have changed the timeScale to 0,1f. so 0.2f is actually 2 seconds in our time.
            slowModeUntil = Time.time + 0.2f;
        }

        private void outOfSlowMode() {
            Debug.Log("getting out of slow mode");
            slowModeIndicator.enabled = false;
            Time.timeScale = 1f;
            animator.speed = 1f;
            movementSpeed /= 10;
            gravity /= 100;
            hpOrbFill.removeTimeScaleOffset();
            hpOrbAnimator.removeTimeScaleOffset();
            mpOrbFill.removeTimeScaleOffset();
            mpOrbAnimator.removeTimeScaleOffset();
            slowModeUntil = float.MaxValue;
        }
        
        private Spell getSpell() {
            //todo: weapon addition, needs to get from player gear...
            if (Input.GetKey(KeyCode.Q)) {
                return PlayerSpells.getDemonArrow(characterStatus, demonArrowPrefab, demonArrowOriginPrefab);
            } else if (Input.GetKey(KeyCode.W)) {
                return PlayerSpells.getHolyArrow(characterStatus, holyArrowPrefab, holyArrowOriginPrefab);
            } else if (Input.GetKey(KeyCode.E)) {
                return PlayerSpells.getIceArrow(characterStatus, iceArrowPrefab, iceArrowOriginPrefab);
            } else if (Input.GetKey(KeyCode.A)) {
                return PlayerSpells.getPowerShot(characterStatus, powerShotArrowPrefab);
            } else {
                //is F, supposed to be penetration, but for now holy so it compiles
                return PlayerSpells.getTripleArrows(characterStatus, demonArrowPrefab, demonArrowOriginPrefab);
            }
        }
        
        public void receiveSpell(Spell spell) {
            characterStatus.onReceivingSpell(spell);
        }
        
         private void addBuff(CharacterBuff newBuff) {
            selfBuffers.Add(newBuff.name, newBuff);
            switch (newBuff.name) {
                case "ice_shield":
                    iceShieldInstance = Instantiate(iceShieldPrefab, transform);
                    break;
                case "recover":
                    //todo: others
                    break;
            }
        }

        private void removeBuff(string bufferName) {
            selfBuffers.Remove(bufferName);
            switch (bufferName) {
                case "ice_shield":
                    Destroy(iceShieldInstance);
                    break;
                case "recover":
                    break;
            }  
        }


        private void reEvaluateBuff() {
            List<string> needsRemoved = new List<string>();
            foreach(KeyValuePair<string, CharacterBuff> entry in selfBuffers) {
                CharacterBuff buffer = entry.Value;
                if (buffer.isExpired())
                {
                    Debug.Log("removing buff due to expire");
                    needsRemoved.Add(buffer.name);
                }
                else
                {
                    if (buffer.isEffective()) {
                        buffer.updateNextEffectiveTime();
                        characterStatus.changeHp(buffer.hpChange);
                        characterStatus.changeMana(buffer.manaChange);
                    }
                }
            }

            foreach (string bufferName in needsRemoved) {
                removeBuff(bufferName);
            }
        }
        
        private void onKilled()
        {
            animator.SetInteger(commonAnimationParam, dieAnimationVal);
            gameObject.SetActive(false);
        }
    }
}
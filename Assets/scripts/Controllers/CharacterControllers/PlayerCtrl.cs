using System.Numerics;
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
        public Image slowModeIndicator;
        
        private Camera mainCamera;
        
        public int runAnimationVal;
        //todo: if no input for a time, go back to idle mode
        public int idleAnimationVal;
        public int readyAnimationVal;
        public int attack01AnimationVal;
        public int attack02AnimationVal;
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
                if (clips[i].name == "AttackCrossbow_fixed [10]") {
                    return clips[i].length;
                }
            }
            return 100f;
        }
       
        private void Update() {
            if(Time.time > slowModeUntil) outOfSlowMode();
            
            characterStatus.reEvaluateStatusEverySecond();
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
            return Input.GetKey(KeyCode.Q) || Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.E) || Input.GetKey(KeyCode.F);
        }
        
        private void tryBecomeReady() {
            if (Time.time <= freezeUntil) return;
            if(!Input.GetKey(KeyCode.Space) && !isAttackKeyPressed()) animator.SetInteger(commonAnimationParam, readyAnimationVal);
        }

        private void tryTurn() {
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
        
        private void castSpell(Spell spell, int animationVal) {
            characterStatus.mana -= (int)spell.manaConsumed;
            characterStatus.mana += (int) spell.manaGenerated;
            animator.SetInteger(commonAnimationParam, animationVal);
            Vector3 spawnPos = arrowSpawnPos.position;
            //todo: first we need to check if the mouse is pointing at a enemy, only if it missed out, will we
            //todo: resolve to direction mask.
            Ray camRay = mainCamera.ScreenPointToRay(Input.mousePosition);
            Vector3 hitPos;
            bool characterHitRes;            
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
            GameObject arrow = Instantiate(spell.prefab, spawnPos, Quaternion.LookRotation(hitToSpawn));
            ArrowAttack arrowAttack = arrow.GetComponent<ArrowAttack>();
            arrowAttack.setAttackAttrib(spell, hitToSpawn);  
            if (spell.name.Equals("triple_arrows")) {
                Debug.Log("creating more...");
                //need to instantiate two more...
                Vector3 slightlyLeft = Quaternion.AngleAxis(-10f, Vector3.up) * hitToSpawn;
                GameObject arrow2 = Instantiate(spell.prefab, spawnPos, Quaternion.LookRotation(slightlyLeft));
                ArrowAttack arrowAttack2 = arrow2.GetComponent<ArrowAttack>();
                arrowAttack2.setAttackAttrib(spell, slightlyLeft);

                Vector3 slightlyRight = Quaternion.AngleAxis(10f, Vector3.up) * hitToSpawn;
                GameObject arrow3 = Instantiate(spell.prefab, spawnPos, Quaternion.LookRotation(slightlyRight));
                ArrowAttack arrowAttack3 = arrow3.GetComponent<ArrowAttack>();
                arrowAttack3.setAttackAttrib(spell, slightlyRight);  
            }
            if(spell.originPrefab) Instantiate(spell.originPrefab, spawnPos, Quaternion.LookRotation(hitToSpawn));
        }

        private void tryAttack() {
            if (Time.time <= freezeUntil) return;
            if (!isAttackKeyPressed()) return;
            if (Time.timeScale < 1f) outOfSlowMode();
            Spell spell = getSpell();
            if (characterStatus.mana < spell.manaConsumed) return;
            freezeUntil = Time.time + attackInterval;
            //check the animator controller to see how this code make sense...
            //TODO: add comments about the weird behaviors
            AnimatorStateInfo currentAnimation = animator.GetCurrentAnimatorStateInfo(0);
            if (currentAnimation.IsName("AttackCrossbow [10]")) {
                castSpell(spell, attack02AnimationVal);                
            } else {
                castSpell(spell, attack01AnimationVal);   
            }
        }

        private void trySpecialSpell() {
            if (Input.GetKeyUp(KeyCode.D)) {
                intoSlowMode();
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
            } else {
                //is F, supposed to be penetration, but for now holy so it compiles
                return PlayerSpells.getTripleArrows(characterStatus, demonArrowPrefab, demonArrowOriginPrefab);
            }
        }
        
        public void receiveSpell(Spell spell) {
            characterStatus.onReceivingSpell(spell);
        }
        
        private void onKilled()
        {
            animator.SetInteger(commonAnimationParam, dieAnimationVal);
            gameObject.SetActive(false);
        }
    }
}
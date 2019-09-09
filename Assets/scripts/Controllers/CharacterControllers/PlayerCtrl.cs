using Spells;
using Spells.ArrowAttack;
using UnityEngine;

namespace CharacterControllers {
    //todo: I need to get rid of the animation event and find an alternative
    public class PlayerCtrl : MonoBehaviour {

        private Animator animator;
        private CharacterStatus characterStatus;
        private CharacterController unityCharacterController;
        
        private int commonAnimationParam = Animator.StringToHash("animationStatus");

        private int turnDetectionMask = 1 << 10;
        private int arrowDirectionMask = 1 << 11;
        private Vector3 playerToMouse;
        private float freezeUntil;
        private int attackAnimationValUsed;
        private Transform arrowSpawnPos;
        private Vector3 playerChestPosition;
        private OrbFill hpOrbFill;
        private OrbFill mpOrbFill;
        private float attackInterval;

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
        public GameObject fireArrowPrefab;
        public GameObject iceArrowPrefab;
        public GameObject poisonArrowPrefab;
        
        private void Start() {
            unityCharacterController = gameObject.GetComponent<CharacterController>();
            animator = gameObject.GetComponentInChildren<Animator>();
            characterStatus = new CharacterStatus(maxBaseHp, maxBaseMana, baseAttackStrength);
            arrowSpawnPos = transform.Find("arrow_spawn_pos");
            mainCamera = Camera.main;
            hpOrbFill = GameObject.Find("player_hp_fill").GetComponent<OrbFill>();
            mpOrbFill = GameObject.Find("player_mp_fill").GetComponent<OrbFill>();
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
       
        private void FixedUpdate() {
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
                    //todo: make 1.7f constant
                    Vector3 pos = transform.position;
                    playerChestPosition.Set(pos.x, 1.7f, pos.z);
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
            if (Input.GetKey(KeyCode.Space) && !isAttackKeyPressed()) {
                animator.SetInteger(commonAnimationParam, runAnimationVal);
                unityCharacterController.SimpleMove(movementSpeed * playerToMouse);
            }
        }

        private void castSpell(Spell spell, int animationVal) {
            characterStatus.mana -= (int)spell.manaConsumed;
            characterStatus.mana += (int) spell.manaGenerated;
            Debug.Log(animationVal);
            animator.SetInteger(commonAnimationParam, animationVal);
            
            Vector3 spawnPos = arrowSpawnPos.position;
            //todo: first we need to check if the mouse is pointing at a enemy, only if it missed out, will we
            //todo: resolve to direction mask.
            Ray camRay = mainCamera.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;
            if (Physics.Raycast(camRay, out hit, camRayLength, arrowDirectionMask)) {
                //cache the moue pos to avoid extra ray casts
                Vector3 hitToSpawn = hit.point - spawnPos;
                hitToSpawn = Vector3.Normalize(hitToSpawn);
                Debug.Log("instatiated");
                GameObject darkArrow = Instantiate(spell.prefab, spawnPos, Quaternion.LookRotation(hitToSpawn));
                ArrowAttack arrowAttack = darkArrow.GetComponent<ArrowAttack>();
                arrowAttack.setAttackAttrib(spell, hitToSpawn);  
            }
        }

        private void tryAttack() {
            if (Time.time <= freezeUntil) return;
            if (!isAttackKeyPressed()) return;
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
        
        private Spell getSpell() {
            //todo: weapon addition, needs to get from player gear...
            if (Input.GetKey(KeyCode.Q)) {
                return PlayerSpells.getDemonArrow(characterStatus, demonArrowPrefab);
            } else if (Input.GetKey(KeyCode.W)) {
                return PlayerSpells.getFireArrow(characterStatus, fireArrowPrefab);
            } else if (Input.GetKey(KeyCode.E)) {
                return PlayerSpells.getIceArrow(characterStatus, iceArrowPrefab);
            } else {
                //is F
                return PlayerSpells.getPoisonArrow(characterStatus, poisonArrowPrefab);
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
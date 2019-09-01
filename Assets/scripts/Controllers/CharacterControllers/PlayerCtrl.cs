using Spells;
using Spells.ArrowAttack;
using UnityEngine;

namespace CharacterControllers {
    public class PlayerCtrl : MonoBehaviour {

        private Animator animator;
        private CharacterStatus characterStatus;
        private CharacterController unityCharacterController;
        
        private int commonAnimationParam = Animator.StringToHash("animationStatus");

        private int turnDetectionMask = 1 << 10;
        private Vector3 playerToMouse;
        private float freezeUntil;
        private int attackAnimationValUsed;
        private Transform arrowSpawnPos;

        private Camera mainCamera;
        
        public int runAnimationVal;
        //todo: if no input for a time, go back to idle mode
        public int idleAnimationVal;
        public int readyAnimationVal;
        public int attack01AnimationVal;
        public int attack02AnimationVal;
        public int dieAnimationVal;
        
        public int maxBaseHp;
        public int maxBaseMana;
        public int baseAttackStrength;
        public int baseMagicPower;
       
        public int camRayLength;
        
        public int movementSpeed;

        public GameObject darkArrowPrefab;
        
        private void Start() {
            unityCharacterController = gameObject.GetComponent<CharacterController>();
            animator = gameObject.GetComponentInChildren<Animator>();
            characterStatus = new CharacterStatus(maxBaseHp, maxBaseMana, baseAttackStrength, baseMagicPower);
            arrowSpawnPos = transform.Find("arrow_spawn_pos");
            mainCamera = Camera.main;
        }
       
        private void Update() {
            characterStatus.reEvaluateStatusEverySecond();
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
                    Debug.Log("hit...");
                    //cache the moue pos to avoid extra ray casts
                    //assumes mouse click detection plane and arrowSpawnPos has same y axis value
                    playerToMouse = hit.point - arrowSpawnPos.position;
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

        private void tryAttack() {
            if (Time.time <= freezeUntil) return;
            if (isAttackKeyPressed()) {
                freezeUntil = float.MaxValue;
                attackAnimationValUsed = attack01AnimationVal;
                animator.SetInteger(commonAnimationParam, attack01AnimationVal);
                
            }
        }

        public void onAttackBecomeEffective() {
            GameObject darkArrow = Instantiate(darkArrowPrefab, arrowSpawnPos.position, arrowSpawnPos.rotation);
            ArrowAttack arrowAttack = darkArrow.GetComponent<ArrowAttack>();
            Vector3 characterForward = gameObject.transform.forward;
            arrowAttack.setAttackAttrib(Spell.createPhysicalAttack(10), characterForward);
        }
        
        public void onAttackFinish()
        {
            if (isAttackKeyPressed()) {
                //keep "freeze" status
                attackAnimationValUsed = attackAnimationValUsed == attack01AnimationVal
                    ? attack02AnimationVal
                    : attack01AnimationVal;
                tryTurn();
                animator.SetInteger(commonAnimationParam, attackAnimationValUsed);
            }
            else {
                freezeUntil = Time.time;
            }
        }

        private void attack(int attackAnimationVal) {
            

        }
        
        private void onKilled()
        {
            animator.SetInteger(commonAnimationParam, dieAnimationVal);
            gameObject.SetActive(false);
        }
    }
}
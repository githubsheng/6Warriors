using System;
using Gui.HealthBar;
using Spells;
using UnityEngine;
using UnityEngine.AI;
using Vector3 = UnityEngine.Vector3;

namespace CharacterControllers {
    public class MinionCtrl : MonoBehaviour {
        private GameObject player;
        private PlayerCtrl playerCtrl;
        private NavMeshAgent agent;
        private Animator animator;
        private CharacterStatus characterStatus;
        private int commonAnimationParam = Animator.StringToHash("animationStatus");
        private int attackAnimationIdxUsed;
        private int[] attackAnimationVals;
        private float[] attackAnimationClipLengths;
        private float freezeUntil;
        private GameObject floatingHealthBar;
        private FloatingHealthBar healthBarCtr;
        private bool isInBattleStatus;
        private GameObject pointedIndicator;

        
        public int attackRange;
        public int maxHeightDifferenceForEffectiveAttack;
        
        public float maxBaseHp;
        public float maxBaseMana;
        public float baseAttackStrength;
        
        public int runAnimationVal;
        //todo: if no input for a time, go back to idle mode
        public int idleAnimationVal;
        public int readyAnimationVal;
        public int attack01AnimationVal;
        public int attack02AnimationVal;
        public int attack03AnimationVal;
        public int dieAnimationVal;

        public GameObject floatingHealthBarPrefab;
        public float floatingHealthBarUpwardsOffset;
        public float alertRange;

        private void Awake() {
            //todo: hmmm, considering changing this constructor..?
            characterStatus = new CharacterStatus(maxBaseHp, maxBaseMana, baseAttackStrength);
            characterStatus.baseHpRegerationPerSecond = 0;
        }


        private void Start() {
            player = GameObject.FindWithTag("Player");
            playerCtrl = player.GetComponent<PlayerCtrl>();
            animator = gameObject.GetComponentInChildren<Animator>();


            agent =  GetComponent<NavMeshAgent>();
            attackAnimationVals = new []{attack01AnimationVal, attack02AnimationVal, attack01AnimationVal, attack02AnimationVal, attack03AnimationVal};
            getAttackAnimationLength();
            floatingHealthBar = Instantiate(floatingHealthBarPrefab, transform);
            healthBarCtr = floatingHealthBar.GetComponent<FloatingHealthBar>();
            healthBarCtr.setHealthBarAttrib(floatingHealthBarUpwardsOffset);
            pointedIndicator = transform.Find("pointed").gameObject;
        }

        private void getAttackAnimationLength()
        {
            attackAnimationClipLengths = new float[5];
            AnimationClip[] clips = animator.runtimeAnimatorController.animationClips;
            for (int i = 0; i < clips.Length; i++)
            {
                Debug.Log(clips[i].name);
                switch(clips[i].name)
                {
                    case "AttackOne":
                        attackAnimationClipLengths[0] = clips[i].length;
                        attackAnimationClipLengths[2] = clips[i].length;
                        break;
                    case "AttackTwo":
                        attackAnimationClipLengths[1] = clips[i].length;
                        attackAnimationClipLengths[3] = clips[i].length;
                        break;
                    case "AttackThree":
                        attackAnimationClipLengths[4] = clips[i].length;
                        break;
                }
            }

            for(int i = 0; i < attackAnimationClipLengths.Length; i++)
            {
                Debug.Log(attackAnimationClipLengths[i]);
            }
        }

        private void Update() {
            if (isInBattleStatus) {
                characterStatus.reEvaluateStatusEverySecond();
                healthBarCtr.setHealth((float)characterStatus.hp / characterStatus.maxHp);
                if (characterStatus.isDead) {
                    onKilled();
                    return;
                }
                if (Time.time <= freezeUntil) return;
                if (isPlayerInRange(attackRange)) {
                    attack();
                    return;
                }
                moveAgentToTarget(player.transform);
            }
            else if (isPlayerInRange(alertRange)) {
                alertGroup();
            }
        }

        private void alertGroup() {
            MinionCtrl[] minionCtrls = transform.parent.GetComponentsInChildren<MinionCtrl>();
            for (int i = 0; i < minionCtrls.Length; i++) {
                minionCtrls[i].enterBattleStatus();
            }
        }

        public void enterBattleStatus() {
            isInBattleStatus = true;
        }

        private bool isPlayerInRange(float range) {
            Vector3 playerPos = player.transform.position;
            Vector3 selfPos = gameObject.transform.position;
            if (Math.Abs(playerPos.y - selfPos.y) > maxHeightDifferenceForEffectiveAttack) return false;
            return Vector3.Distance(playerPos, selfPos) < range;
        }

        private void attack() {
            //pause movement first
            agent.isStopped = true;
            attackAnimationIdxUsed = (++attackAnimationIdxUsed) % attackAnimationVals.Length;
            animator.SetInteger(commonAnimationParam, attackAnimationVals[attackAnimationIdxUsed]);
            float attackAnimationTime = attackAnimationClipLengths[attackAnimationIdxUsed];
            freezeUntil = Time.time + attackAnimationTime;
            //todo: should have different attacks props set by public fields.
            playerCtrl.receiveSpell(Spell.createPhysicalAttack(5));
        }
        
        private void moveAgentToTarget(Transform target)
        {
            //agent may be paused previously by setting isStopped to true, eg. player is in magic attack range, and character paused
            //to attack player. if after attack, player is out of range again, this character needs to chase player.
            agent.isStopped = false;
            agent.destination = target.position;
            animator.SetInteger(commonAnimationParam, runAnimationVal);
        }
        
        private void onKilled()
        {
            //todo: animation
//            animator.SetInteger(commonAnimationParam, dieAnimationVal);
            Destroy(gameObject);
        }

        public void receiveSpell(Spell spell) {
            if(!isInBattleStatus) alertGroup();
            characterStatus.onReceivingSpell(spell);
        }

        public void enablePointedIndicator() {
            Debug.Log("setting it to be active");
            pointedIndicator.SetActive(true);
        }

        public void disablePointedIndicator() {
            Debug.Log("setting it to be inactive");
            pointedIndicator.SetActive(false);
        }
    }
}
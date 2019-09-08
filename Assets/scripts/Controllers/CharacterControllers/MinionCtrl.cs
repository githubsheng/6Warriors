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
        private float freezeUntil;
        private GameObject floatingHealthBar;
        private FloatingHealthBar healthBarCtr;
        private bool isInBattleStatus;
        
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
            floatingHealthBar = Instantiate(floatingHealthBarPrefab, transform);
            healthBarCtr = floatingHealthBar.GetComponent<FloatingHealthBar>();
            healthBarCtr.setHealthBarAttrib(floatingHealthBarUpwardsOffset);
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
            freezeUntil = float.MaxValue;
            attackAnimationIdxUsed = (++attackAnimationIdxUsed) % attackAnimationVals.Length;
            animator.SetInteger(commonAnimationParam, attackAnimationVals[attackAnimationIdxUsed]);
            //todo: attack animation delay for each animation type, and for each minion type (public fields)
            playerCtrl.receiveSpell(Spell.createPhysicalAttack(5));
        }
        
        public void onAttackFinish()
        {
            freezeUntil = Time.time;
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
    }
}
using System;
using System.Collections.Generic;
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
        private Transform spellSpawnTransform;

        
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
        public bool isRanged;
        public GameObject rangedAttackPrefab01;
        public GameObject rangedAttackPrefab02;
        public GameObject rangedAttackPrefab03;
        private Dictionary<int, bool> isRangedAttackMap = new Dictionary<int, bool>();
        private Dictionary<int, GameObject> rangedAttackPrefabs = new Dictionary<int, GameObject>();
        public int dieAnimationVal;

        public GameObject floatingHealthBarPrefab;
        public float floatingHealthBarUpwardsOffset;
        public float alertRange;

        private void Awake() {
            //todo: hmmm, considering changing this constructor..?
            characterStatus = new CharacterStatus(maxBaseHp, maxBaseMana, baseAttackStrength);
            characterStatus.baseHpRegerationPerSecond = 0;
            if(isRanged)
            {
                rangedAttackPrefabs[1] = rangedAttackPrefab01;
                rangedAttackPrefabs[2] = rangedAttackPrefab02;
                rangedAttackPrefabs[3] = rangedAttackPrefab03;
                spellSpawnTransform = transform.Find("spell_spawn_pos");
            }

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
            if(isRanged)
            {
                //if is ranged, attack range may be very large, I need to cast an ray to see if there are walls between
                //the minion and the player.
                //todo: 
                return Vector3.Distance(playerPos, selfPos) < range;
            }
            return Vector3.Distance(playerPos, selfPos) < range;
        }

        private void attack() {
            //pause movement first
            agent.isStopped = true;
            attackAnimationIdxUsed = (++attackAnimationIdxUsed) % attackAnimationVals.Length;
            int attackVal = attackAnimationVals[attackAnimationIdxUsed];
            animator.SetInteger(commonAnimationParam, attackVal);
            float attackAnimationTime = attackAnimationClipLengths[attackAnimationIdxUsed];
            freezeUntil = Time.time + attackAnimationTime;
            Spell spell = Spell.createNormalAttack(characterStatus.attackStrengh);
            if (isRanged)
            {
                GameObject rangedAttackPrefab = rangedAttackPrefabs[attackVal];
                Vector3 spellSpawnPos = spellSpawnTransform.position;
                Vector3 playerToSpellSpawn = player.transform.position - spellSpawnPos;
                playerToSpellSpawn.y = 0;
                GameObject rangedAttack = Instantiate(rangedAttackPrefab, spellSpawnPos, Quaternion.LookRotation(playerToSpellSpawn));
                rangedAttack.GetComponent<MinionRangedAttack>().setAttackAttrib(spell, playerToSpellSpawn);
            }
            else
            {
                playerCtrl.receiveSpell(spell);
            }

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
            pointedIndicator.SetActive(true);
        }

        public void disablePointedIndicator() {
            pointedIndicator.SetActive(false);
        }
    }
}
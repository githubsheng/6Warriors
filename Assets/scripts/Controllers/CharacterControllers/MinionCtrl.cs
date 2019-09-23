using System;
using System.Collections.Generic;
using Buffs;
using Gui.BufferIndicator;
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
        private float waitUntilAttackBecomeEffective;
        private GameObject floatingHealthBar;
        private bool isInBattleStatus;
        private GameObject pointedIndicator;
        private Transform spellSpawnTransform;
        private float stopAttackUntil;
        private Dictionary<int, GameObject> rangedAttackPrefabs = new Dictionary<int, GameObject>();
        private Dictionary<int, float> attackDelays = new Dictionary<int, float>();
        private Material[] materials;
        
        private CharacterBuff buffer;
        private BufferIndicator bufferIndicator;
        
        public int attackRange;
        public int maxHeightDifferenceForEffectiveAttack;
        
        public float maxBaseHp;
        public float maxBaseMana;
        public float baseAttackStrength;
        public float attackCoolDown = 10f;
        
        public int runAnimationVal;
        //todo: if no input for a time, go back to idle mode
        public int idleAnimationVal;
        public int readyAnimationVal;
        public int attack01AnimationVal;
        public int attack02AnimationVal;
        public int attack03AnimationVal;
        public float attack01EffectiveDelay;
        public float attack02EffectiveDelay;
        public float attack03EffectiveDelay;
        public bool isRanged;
        public GameObject rangedAttackPrefab01;
        public GameObject rangedAttackPrefab02;
        public GameObject rangedAttackPrefab03;
        public int dieAnimationVal;
        public GameObject bufferIndicatorPrefab;
        public float alertRange;
        
        
        private static readonly int TintColor = Shader.PropertyToID("_TintColor");
        private static readonly Color holyTintColor = new Color(245f / 255, 143f / 255, 98f / 255);
        private static readonly int AddColor = Shader.PropertyToID("_AddColor");
        private static readonly Color frozenAddColor = new Color(111f/255, 111f/255, 111f/255);
        
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
                attackDelays[1] = attack01EffectiveDelay;
                attackDelays[2] = attack02EffectiveDelay;
                attackDelays[3] = attack03EffectiveDelay;
            }
            stopAttackUntil = Time.time;
            
            materials = GetComponentInChildren<Renderer>().materials;
        }


        private void Start() {
            player = GameObject.FindWithTag("Player");
            playerCtrl = player.GetComponent<PlayerCtrl>();
            animator = gameObject.GetComponentInChildren<Animator>();
            agent =  GetComponent<NavMeshAgent>();
            attackAnimationVals = new []{attack01AnimationVal, attack02AnimationVal, attack01AnimationVal, attack02AnimationVal, attack03AnimationVal};
            getAttackAnimationLength();
            pointedIndicator = transform.Find("pointed").gameObject;
            bufferIndicator = bufferIndicatorPrefab.GetComponent<BufferIndicator>();
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
                reEvaluateBuff();
                characterStatus.reEvaluateStatusEverySecond();
                if (characterStatus.isDead) {
                    onKilled();
                    return;
                }
                if(Time.time > waitUntilAttackBecomeEffective)
                {
                    this.makeAttackEffective();
                    return;
                }
                if (Time.time <= freezeUntil) return;
                bool isInAttackRange = isPlayerInRange(attackRange);
                if (!isInAttackRange)
                {
                    moveAgentToTarget(player.transform);
                    return;
                }
                else
                {
                    agent.isStopped = true;
                    //since agent is stopped, it won't continue to face the player, we need to face the player here
                    //but skip this in time slow mode...because it runs every frame.
                    if (Time.timeScale == 1f) {
                        Vector3 playerToSelf = player.transform.position - transform.position;
                        transform.rotation = Quaternion.LookRotation(playerToSelf);
                    }
                    
                    //player is in attack range, attack if not in cool down.
                    if (Time.time > stopAttackUntil)
                    {
                        stopAttackUntil = Time.time + attackCoolDown;
                        attack();
                    }
                    else
                    {
                        //not moving to player and not attacking player...then just get ready..
                        animator.SetInteger(commonAnimationParam, readyAnimationVal);
                    }
                }
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
            attackAnimationIdxUsed = (++attackAnimationIdxUsed) % attackAnimationVals.Length;
            int attackVal = attackAnimationVals[attackAnimationIdxUsed];
            animator.SetInteger(commonAnimationParam, attackVal);
            float attackAnimationTime = attackAnimationClipLengths[attackAnimationIdxUsed];
            freezeUntil = Time.time + attackAnimationTime;
            //now attack animation is playing, but we need to wait for a while to make the attack really effective.
            waitUntilAttackBecomeEffective = Time.time + attackDelays[attackVal];

        }

        private void makeAttackEffective()
        {
            waitUntilAttackBecomeEffective = float.MaxValue;
            Spell spell = Spell.createNormalAttack(characterStatus.attackStrengh);
            if (isRanged)
            {
                int attackVal = attackAnimationVals[attackAnimationIdxUsed];
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
            if (spell.buff != null) {
                Debug.Log("carries buffer " + spell.buff);
                CharacterBuff newBuff = spell.buff;
                if (buffer != null && newBuff.name.Equals(buffer.name)) {
                    buffer.resetExpireTime();
                    if (buffer.name == "holy_stack") {
                        buffer.count++;
                        if (buffer.count > 5) {
                            bufferIndicator.enableShieldBroken();
                            characterStatus.vulnerability = 1f + 0.1f * buffer.count;
                        }
                        else {
                            bufferIndicator.enableSword(buffer.count);
                            characterStatus.vulnerability = 2f;
                        }
                    }
                }
                else {
                    addBuff(newBuff);  
                }
          }
            
            
            if (!isInBattleStatus) alertGroup();
            characterStatus.onReceivingSpell(spell);
        }

        private void addBuff(CharacterBuff newBuff) {
            removeBuff();
            buffer = newBuff;
            switch (newBuff.name) {
                case "holy_stack":
                    Debug.Log("trying to add buff " + newBuff.name);
                    bufferIndicator.enableSword(newBuff.count);
                    characterStatus.vulnerability = 1.1f;

                    foreach (Material material in materials) {
                        material.SetColor(TintColor, holyTintColor);
                    }
                    break;
                case "frozen":
                    Debug.Log("trying to add buff " + newBuff.name);
                    bufferIndicator.enableFrost();
                    agent.speed *= 0.6f;
                    animator.SetFloat("walkSpeed", 0.6f);
                    foreach (Material material in materials) {
                        material.SetColor(AddColor, frozenAddColor);
                    }
                    break;
            }
        }

        private void removeBuff() {
            if (buffer == null) return;
            bufferIndicator.removeAll();
            resetMaterialColors();
            switch (buffer.name) {
                case "holy_stack":
                    characterStatus.vulnerability = 1f;
                    break;
                case "frozen":
                    Debug.Log("removing frozen debuff");
                    agent.speed /= 0.6f;
                    animator.SetFloat("walkSpeed", 1f);
                    break;
            }
            buffer = null;  
        }

        private void resetMaterialColors() {
            foreach (Material material in materials) {
                material.SetColor(TintColor, Color.white);
                material.SetColor(AddColor, Color.black);
            }
        }

        private void reEvaluateBuff() {
            if (buffer == null) return;
            if (buffer.isExpired())
            {
                Debug.Log("removing buff due to expire");
                removeBuff();
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
        
        public void enablePointedIndicator() {
            pointedIndicator.SetActive(true);
        }

        public void disablePointedIndicator() {
            pointedIndicator.SetActive(false);
        }
    }
}
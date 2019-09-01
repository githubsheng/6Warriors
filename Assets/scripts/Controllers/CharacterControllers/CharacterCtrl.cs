using System;
using Spells;
using UnityEngine;
using UnityEngine.AI;

namespace CharacterControllers
{
    public abstract class CharacterCtrl : MonoBehaviour
    {
        protected NavMeshAgent _agent;
        protected Animator _animator;
        protected CharacterStatus _characterStatus;
        protected int commonAnimationParam;
        protected int runAnimationVal;
        protected int idleAnimationVal;
        protected int readyAnimationVal;
        protected int attackAnimationVal;
        protected int dieAnimationVal;
        protected bool isUnInterruptible;
        
        //following are used to instantiate the characters status
        public int maxBaseHp;
        public int maxBaseMana;
        public int baseAttackStrength;
        public int baseMagicPower;


        public CharacterStatus getStatus()
        {
            return _characterStatus;
        }

        public virtual void Start() {
            _animator = gameObject.GetComponentInChildren<Animator>();
            _agent = GetComponent<NavMeshAgent>();
            _characterStatus = new CharacterStatus(maxBaseHp, maxBaseMana, baseAttackStrength, baseMagicPower);
            _animator.SetInteger(commonAnimationParam, idleAnimationVal);
        }

        //mainly used for npc to go from one place to another
        public void moveAgentToPlace(Vector3 destination)
        {
            _agent.isStopped = false;
            _agent.destination = destination;
            _animator.SetInteger(commonAnimationParam, runAnimationVal);
        }
    
        //mainly used for enemies to chase the player
        protected void moveAgentToTarget(Transform target)
        {
            //agent may be paused previously by setting isStopped to true, eg. player is in magic attack range, and character paused
            //to attack player. if after attack, player is out of range again, this character needs to chase player.
            _agent.isStopped = false;
            _agent.destination = target.position;
            _animator.SetInteger(commonAnimationParam, runAnimationVal);
        }

        protected void startAttack()
        {
            isUnInterruptible = true;
            _agent.isStopped = true;
        }

        protected abstract void createSpell();
        
        public void onAttackFinish()
        {
            isUnInterruptible = false;
            _animator.SetInteger(commonAnimationParam, readyAnimationVal);
        }

        public void onReceiveSpell(Spell spell)
        {
            _characterStatus.onReceivingSpell(spell);
            if (_characterStatus.isDead) onKilled();
        }

        protected void onKilled()
        {
            _animator.SetInteger(commonAnimationParam, dieAnimationVal);
        }

        public void onKilledAnimationEnd()
        {
            gameObject.SetActive(false);
            Destroy(gameObject);               
        }
    }    

}


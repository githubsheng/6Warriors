using System;
using Actions;
using Animations;
using Animations.warriors;
using UnityEngine;
using UnityEngine.AI;

namespace Controllers
{
    public class CharacterControl : MonoBehaviour
    {
        public GameObject self;
        public GameObject navDestinationPrefab;

        protected NavMeshAgent _agent;
        protected Animator _animator;
    
        private const float NavDestColliderTransHeight = 1.5f;
        protected ActionRulesEngine _rulesEngine;

        protected CharacterStatus _characterStatus;
        
        //following are used to instantiate the characters status
        public int maxBaseHp;
        public int maxBaseMana;
        public int baseAttackStrengh;
        public int baseMagicPower;

        public virtual void Start()
        {
            _animator = self.GetComponent<Animator>();
            _agent = GetComponent<NavMeshAgent>();
            _rulesEngine = new ActionRulesEngine(this);
            _characterStatus = new CharacterStatus(maxBaseHp, maxBaseMana, baseAttackStrengh, baseMagicPower);
        }
    
        public virtual void Update()
        {
            _characterStatus.reEvaluateStatusEverySecond();
            
            if (_characterStatus.isDead)
            {
                onGameObjectKilled();
                return;
            }
            
            _rulesEngine.run();
        }
        
        public void moveAgentToPlace(CharacterAction moveAction)
        {
            _agent.isStopped = false;
            _agent.destination = moveAction.userSpecifiedDestination;
            /*
            https://docs.unity3d.com/ScriptReference/AI.NavMeshAgent-destination.html
            the destination could be different from hit.point, if the hit.point is not reachable, then destination will be
            the closest reachable point
             */
            Vector3 actualDestination = _agent.destination;
            Vector3 navDestinationTransform = new Vector3(
                actualDestination.x,
                NavDestColliderTransHeight + actualDestination.y,
                actualDestination.z
            );
            //the actual position of the target destination, and hence the destination itself can only be calculated when we actually
            //execute this move action.
            moveAction.Target = Instantiate(navDestinationPrefab, navDestinationTransform, Quaternion.identity);
            _animator.SetInteger(RogueAnimationStatus.AniStat, RogueAnimationStatus.Run22);
        }
    
        public void moveAgentToTarget(CharacterAction moveToTargetAction)
        {
            _agent.isStopped = false;
            _agent.destination = moveToTargetAction.Target.transform.position;
            _animator.SetInteger(RogueAnimationStatus.AniStat, RogueAnimationStatus.Run22);
        }
    
        public void startSpell(CharacterAction characterAction)
        {
            _animator.SetInteger(RogueAnimationStatus.AniStat, characterAction.spell.getAnimationStatus());
        }
    
        public void onAttackBecomeEffective()
        {
            CharacterAction action = _rulesEngine.getCurrentRunningAction();
            if (action.IsTargetValid)
            {
                //target may not be valid (died) at this time
                action.CharacterControl.onReceiveSpell(action);                
            }
            
            //todo: rules engine should have a method that force an action to end, no matter if the action is interruptible.
            //todo: this may apply 

        }
    
        public void onAttackFinish()
        {
            _animator.SetInteger(RogueAnimationStatus.AniStat, RogueAnimationStatus.Ready56);
            _rulesEngine.setWaitTime(0.5f);
        }

        public void onReceiveSpell(CharacterAction action)
        {
            _characterStatus.onReceivingSpell(action.spell);
            
            if (_characterStatus.isDead)
            {
                onGameObjectKilled();
            }
        }
    
        public void reachDestination()
        {
            _agent.isStopped = true;
            _animator.SetInteger(RogueAnimationStatus.AniStat, RogueAnimationStatus.Ready56);
        }
    
        public void reachTarget()
        {
            _agent.isStopped = true;
            _animator.SetInteger(RogueAnimationStatus.AniStat, RogueAnimationStatus.Ready56);
        }
    
        void OnTriggerEnter(Collider other)
        {
            _rulesEngine.onTriggerEnter(other);
        }
    
        private void OnTriggerExit(Collider other)
        {
            _rulesEngine.onTriggerExit(other);
        }

        private void onGameObjectKilled()
        {
            //should play the die animation...and do other stuffs.
            //for now i will just disable the entire game object.
            self.SetActive(false);
            //i should also inform the level controller to remove this object somehow.
        }

        public bool IsDead
        {
            get { return _characterStatus.isDead; }
        }
    }    

}


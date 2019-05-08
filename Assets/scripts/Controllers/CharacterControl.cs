using System;
using Actions;
using Animations;
using UnityEngine;
using UnityEngine.AI;

namespace Controllers
{
    public class CharacterControl : MonoBehaviour
    {
        public GameObject self;
        public GameObject navDestinationPrefab;
    
        private NavMeshAgent _agent;
        private Animator _animator;
    
        private const float NavDestColliderTransHeight = 1.5f;
        private ActionRulesEngine _rulesEngine;

        private CharacterStatus _characterStatus;
        
        //following are used to instantiate the characters status
        public int maxBaseHp;
        public int maxBaseMana;
        public int baseAttackStrengh;
        public int baseMagicPower;
    
        void Start()
        {
            _animator = self.GetComponent<Animator>();
            _agent = GetComponent<NavMeshAgent>();
            //todo: need to figure out how i can get a spell generator....
            _rulesEngine = new ActionRulesEngine(this);
            _characterStatus = new CharacterStatus(maxBaseHp, maxBaseMana, baseAttackStrengh, baseMagicPower);
        }
    
        void Update()
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
            _animator.SetInteger(AnimationStatus.AniStat, AnimationStatus.Run22);
        }
    
        public void moveAgentToTarget(CharacterAction moveToTargetAction)
        {
            _agent.isStopped = false;
            _agent.destination = moveToTargetAction.Target.transform.position;
            _animator.SetInteger(AnimationStatus.AniStat, AnimationStatus.Run22);
        }
    
        public void startSpell(CharacterAction characterAction)
        {
            _animator.SetInteger(AnimationStatus.AniStat, characterAction.spell.getAnimationStatus());
        }
    
        public void onAttackBecomeEffective()
        {
            CharacterAction action = _rulesEngine.getCurrentRunningAction();
            action.CharacterControl.onReceiveSpell(action);
        }
    
        public void onAttackFinish()
        {
            _animator.SetInteger(AnimationStatus.AniStat, AnimationStatus.Ready56);
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
            _animator.SetInteger(AnimationStatus.AniStat, AnimationStatus.Ready56);
        }
    
        public void reachTarget()
        {
            _agent.isStopped = true;
            _animator.SetInteger(AnimationStatus.AniStat, AnimationStatus.Ready56);
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
    }    

}


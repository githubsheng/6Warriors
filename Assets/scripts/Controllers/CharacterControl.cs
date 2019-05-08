using Actions;
using Animations;
using UnityEngine;
using UnityEngine.AI;

namespace Controllers
{
    public class CharacterControl : MonoBehaviour
    {
        public GameObject body;
        public GameObject navDestinationPrefab;
    
        private NavMeshAgent _agent;
        private Animator _animator;
    
        private const float NavDestColliderTransHeight = 1.5f;
        private ActionRulesEngine _actionRules;
        
        
        
    
        void Start()
        {
            _animator = body.GetComponent<Animator>();
            _agent = GetComponent<NavMeshAgent>();
            //todo: need to figure out how i can get a spell generator....
            _actionRules = new ActionRulesEngine(this, null);
        }
    
        void Update()
        {
            _actionRules.run();
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
            moveAction.target = Instantiate(navDestinationPrefab, navDestinationTransform, Quaternion.identity);
            _animator.SetInteger(AnimationStatus.AniStat, AnimationStatus.Run22);
        }
    
        public void moveAgentToTarget(CharacterAction moveToTargetAction)
        {
            _agent.isStopped = false;
            _agent.destination = moveToTargetAction.target.transform.position;
            _animator.SetInteger(AnimationStatus.AniStat, AnimationStatus.Run22);
        }
    
        public void startSpell(CharacterAction characterAction)
        {
            _animator.SetInteger(AnimationStatus.AniStat, characterAction.spell.getAnimationStatus());
        }
    
        public void onAttackBecomeEffective()
        {
            //todo: 
        }
    
        public void onAttackFinish()
        {
            _animator.SetInteger(AnimationStatus.AniStat, AnimationStatus.Ready56);
            _actionRules.setWaitTime(0.5f);
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
            _actionRules.onTriggerEnter(other);
        }
    
        private void OnTriggerExit(Collider other)
        {
            _actionRules.onTriggerExit(other);
        }
    }    

}


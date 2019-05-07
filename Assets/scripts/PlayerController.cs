using UnityEngine;
using UnityEngine.AI;

public class PlayerController : MonoBehaviour
{
    public GameObject body;
    public GameObject navDestinationPrefab;

    private GameObject _navDestination;
    private NavMeshAgent _agent;
    private Animator _animator;

    private const float NavDestColliderTransHeight = 1.5f;
    private PlayActionRulesEngine _actionRules;
    
    
    

    void Start()
    {
        _animator = body.GetComponent<Animator>();
        _agent = GetComponent<NavMeshAgent>();
        //todo: need to figure out how i can get a spell generator....
        _actionRules = new PlayActionRulesEngine(this, null);
    }

    void Update()
    {
        _actionRules.run();
    }

    public void moveAgentToPlace(PlayerAction moveAction)
    {
        if (_navDestination) Destroy(_navDestination);
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
        _navDestination = Instantiate(navDestinationPrefab, navDestinationTransform, Quaternion.identity);
        _animator.SetInteger(AnimationStatus.AniStat, AnimationStatus.Run22);
    }

    public void moveAgentToTarget(PlayerAction moveToTargetAction)
    {
        if (_navDestination) Destroy(_navDestination);
        _agent.isStopped = false;
        _agent.destination = moveToTargetAction.target.transform.position;
        _animator.SetInteger(AnimationStatus.AniStat, AnimationStatus.Run22);
    }

    public void startAttack(PlayerAction playerAction)
    {
        _animator.SetInteger(AnimationStatus.AniStat, AnimationStatus.Attack51);
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
        Destroy(_navDestination);
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
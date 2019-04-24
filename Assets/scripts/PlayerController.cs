using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class PlayerController : MonoBehaviour
{
    public GameObject body;
    public GameObject navDestinationPrefab;

    private GameObject _none;
    private GameObject _navDestination;
    private NavMeshAgent _agent;
    private Animator _animator;

    private PlayerAction _pendingAction;
    private PlayerAction _actionInExecution;
    private GameObject _currentTarget;
    private HashSet<Collider> _inContacts;

    private const float NavDestColliderTransHeight = 1.5f;
    
    private float _nextActionTime = 0.0f;
    private float _period = 5f;

    void Start()
    {
        _inContacts = new HashSet<Collider>();
        _none = new GameObject();
        _currentTarget = _none;
        _animator = body.GetComponent<Animator>();
        _agent = GetComponent<NavMeshAgent>();
    }

    void Update()
    {
        if (Time.time > _nextActionTime)
        {
            _nextActionTime += _period;
            if (_actionInExecution != null)
            {
                Debug.Log(_actionInExecution.actionName);                
            }

        }

        if (Input.GetMouseButtonDown(0))
        {
            //check what kind of stuffs the user clicks on. is it a mob, a place or some other stuffs
            RaycastHit hit;
            if (Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out hit, 100))
            {
                if (hit.collider.CompareTag("small_mob"))
                {
                    //clicked on a mob
                    setCurrentTarget(hit.collider.gameObject);
                }
                else
                {
                    //clicked on a place
                    setMoveAction(hit.point);
                }
            }
        }



        if (_actionInExecution != null
            && _actionInExecution.actionName == "wait_action")
        {
            if (_actionInExecution.expireTime < Time.time)
            {
                _actionInExecution = null;
            }
        }
        
        PlayerAction nextAction = getNextAction();
        
        //check if we need to execute another action
        if (_actionInExecution == null || (_actionInExecution.isInterrupbitle && nextAction.tryInterrupt))
        {
            endAction(_actionInExecution);
            _actionInExecution = nextAction;
            _pendingAction = null;
            if (!_actionInExecution.isEvaluated)
            {
                _actionInExecution = evaluateRuleAction();
                if (_actionInExecution == null) return;
            }
            executeAction(_actionInExecution);
        } 
        
        //if not, continue execute the current action
        continousExecuteAction(_actionInExecution);
    }

    /**
     * try to end the current action (if any clean up needs to be done)
     */
    private void endAction(PlayerAction playerAction)
    {
        //todo: implement.
    }

    private PlayerAction evaluateRuleAction()
    {        
        /*
         * todo: rules:
         * if no target then find a target
         * if nothing can be target, then stay ready
         */

        //todo: this is a simplified rule for testing only
        if (_currentTarget != _none)
        {
            if (_inContacts.Contains(_currentTarget.GetComponent<Collider>()))
            {
                //within range, generate an attack action
                return PlayerAction.createMeleeAttackAction(_currentTarget, "normal_attack");                            
            }

            return PlayerAction.createPlayerMoveToMovingTargetAction(_currentTarget);
        }

        return null;
    }

    private PlayerAction getNextAction()
    {
        if (_pendingAction != null)
        {
            PlayerAction ret = _pendingAction;
            return ret;
        }

        return PlayerAction.createGenericRuleAction();
    }

    private void executeAction(PlayerAction playerAction)
    {
        switch (playerAction.actionName)
        {
            case "move":
                moveAgentToPlace(playerAction);
                break;
            case "move_to_moving_target":
                moveAgentToTarget(playerAction);
                break;
            case "melee_attack":
                startAttack(playerAction);
                break;
            default:
                break;
        }
    }

    private void continousExecuteAction(PlayerAction playerAction)
    {
        /*
         * todo: for some certain actions such as move to moving target action, we need to continue checking the target's
         * for now, all targets are static so we can get by....
         */
    }
    
    private void setMoveAction(Vector3 userSpecifiedMoveDestination)
    {
        PlayerAction moveAction = PlayerAction.createPlayerMoveAction(userSpecifiedMoveDestination);
        _pendingAction = moveAction;
    }
    
    private void setCurrentTarget(GameObject target)
    {
        _currentTarget = target;
        
    }

    private void resetCurrentTarget()
    {
        _currentTarget = _none;
    }

    private void moveAgentToPlace(PlayerAction moveAction)
    {
        resetCurrentTarget();
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

    private void moveAgentToTarget(PlayerAction moveToTargetAction)
    {
        if (_navDestination) Destroy(_navDestination);
        _agent.isStopped = false;
        _agent.destination = moveToTargetAction.target.transform.position;
        _animator.SetInteger(AnimationStatus.AniStat, AnimationStatus.Run22);
    }

    private void startAttack(PlayerAction playerAction)
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
        _actionInExecution = PlayerAction.createWaitAction(0.5f);
    }

    void OnTriggerEnter(Collider other)
    {
        _inContacts.Add(other);
       
        
        switch (_actionInExecution.actionName)
        {
            case "move":
                if (other.CompareTag("player_nav_destination"))
                {
                    Destroy(_navDestination);
                    _agent.isStopped = true;
                    _animator.SetInteger(AnimationStatus.AniStat, AnimationStatus.Ready56);
                    _actionInExecution = PlayerAction.createWaitAction(0.2f);
                }
                break;
            case "move_to_moving_target":
                if (other.gameObject == _actionInExecution.target)
                {
                    _agent.isStopped = true;
                    _animator.SetInteger(AnimationStatus.AniStat, AnimationStatus.Ready56);
                    _actionInExecution = PlayerAction.createWaitAction(0.2f);
                }
                break;
            default:
                break;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        _inContacts.Remove(other);
    }
}
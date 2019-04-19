using System;
using UnityEngine;
using UnityEngine.AI;

public class PlayerController : MonoBehaviour
{
    public GameObject body;
    public GameObject navDestinationPrefab;
    
    private GameObject navDestination;
    private NavMeshAgent agent;
    private Animator animator;

    private PlayerAction pendingAction;
    private PlayerAction actionInExecution;
    private GameObject defaultTarget;
    private bool isTouchingDefaultTarget;
    
    private static readonly int IsRunning = Animator.StringToHash("isRunning");
    private static readonly int IsAttacking = Animator.StringToHash("isAttacking");
    private const float navDestColliderTransHeight = 1.5f;

    void Start()
    {
        animator = body.GetComponent<Animator>();
        agent = GetComponent<NavMeshAgent>();
    }

    void Update()
    {   
        if (Input.GetMouseButtonDown(0))
        {
            //check what kind of stuffs the user clicks on. is it a mob, a place or some other stuffs
            RaycastHit hit;
            if (Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out hit, 100))
            {
                if (hit.collider.CompareTag("small_mob"))
                {
                    //clicked on a mob
                    setDefaultTarget(hit.collider.gameObject);
                }
                else
                {
                    //clicked on a place
                    setMoveAction(hit.point);
                }
            }
        }

        PlayerAction nextAction = getNextAction();
        //check if we need to execute another action
        if (this.actionInExecution == null || (this.actionInExecution.isInterrupbitle && nextAction.tryInterrupt))
        {
            //todo: try to end the current action (if any clean up needs to be done), use a method instead
            this.actionInExecution = nextAction;
            this.pendingAction = null;
            if (!actionInExecution.isEvaluated)
            {
                this.actionInExecution = evaluateRuleAction();
                this.executeAction(this.actionInExecution);
            }
        } 
        
        //if not, continue execute the current action
        
        //todo: continuously execute the current action, im thinking of creating another method other than executeAction
    }

    private PlayerAction evaluateRuleAction()
    {
        //todo: use the group action rules to evaluate a complete action, right now im simply trying to do a melee attack
        if (isTouchingDefaultTarget)
        {
            //within range, generate an attack action
            return PlayerAction.createMeleeAttackAction();            
        }
        else
        {
            //we have to move the player to the mob first. then after that, based on the group rule or pending action, player will do other stuff
            return PlayerAction.createPlayerMoveToMovingTargetAction(defaultTarget);
        }
        

    }

    private PlayerAction getNextAction()
    {
        if (this.pendingAction != null)
        {
            PlayerAction ret = this.pendingAction;
            this.pendingAction = null;
            return ret;
        }

        return PlayerAction.createGenericRuleAction();
    }

    private void executeAction(PlayerAction playerAction)
    {
        if (!playerAction.isEvaluated)
        {
            //first we need to evaluate this player action
            Debug.Log("evaluate the player action first");
            Debug.Log("may need to evaluate all the rules and decide what the action actually should be");
        }
        
        switch (playerAction.actionName)
        {
            case "move":
                Debug.Log("getting a move player action");
                actionInExecution = playerAction;
                MoveAgentToPlace(playerAction);
                break;
            case "melee_attack":
                Debug.Log("a melee attack action");
                break;
            default:
                break;
        }
    }

    private void continousExecuteAction(PlayerAction playerAction)
    {
        /*
         * todo: for some certain actions such as move to moving target action, we need to continue checking the target's
         * todo: position periodically.
         */
        Debug.Log("continue executing some certain action");
    }
    
    private void setMoveAction(Vector3 userSpecifiedMoveDestination)
    {
        PlayerAction moveAction = PlayerAction.createPlayerMoveAction(userSpecifiedMoveDestination);
        if (actionInExecution.isInterrupbitle) executeAction(moveAction);
    }

    private void MoveAgentToPlace(PlayerAction moveAction)
    {
        if (navDestination != null) Destroy(navDestination);
        agent.isStopped = false;
        agent.destination = moveAction.userSpecifiedDestination;
        /*
        https://docs.unity3d.com/ScriptReference/AI.NavMeshAgent-destination.html
        the destination could be different from hit.point, if the hit.point is not reachable, then destination will be
        the closest reachable point
         */
        Vector3 actualDestination = agent.destination;
        Vector3 navDestinationTransform = new Vector3(
            actualDestination.x,
            navDestColliderTransHeight + actualDestination.y,
            actualDestination.z
        );
        navDestination = Instantiate(navDestinationPrefab, navDestinationTransform, Quaternion.identity);
        animator.SetBool(IsRunning, true);
    }

    private void setDefaultTarget(GameObject target)
    {
        this.defaultTarget = target;
    }

//    private void MoveAgentBak()
//    {
//        RaycastHit hit;
//        if (Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out hit, 100))
//        {
//            if (hit.collider.CompareTag("small_mob"))
//            {
//                //clicked on a mob
//                if (navDestination != null) Destroy(navDestination);
//                agent.isStopped = false;
//                agent.destination = hit.collider.transform.position;
//            }
//            else
//            {
//                //clicked on a place
//
//            }
//
//
//
//        }
//    }

    void OnTriggerEnter(Collider other)
    {
      
        if (other.CompareTag("player_nav_destination"))
        {
            Destroy(navDestination);
            navDestination = null;
            agent.isStopped = true;
            animator.SetBool(IsRunning, false);
        } else if (other.CompareTag("small_mob"))
        {
            if (other.gameObject == defaultTarget)
            {
                isTouchingDefaultTarget = true;                
            }
            
            //todo: following logic is only for player moving to a mob, we need to refine it, as trigger detection is used in many other cases as well
            //stop the agent and start attacking!
            Debug.Log("hitting a mob collider!");
            agent.isStopped = true;
            animator.SetBool(IsRunning, false);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("small_mob"))
        {
            if (other.gameObject == defaultTarget)
            {
                isTouchingDefaultTarget = false;                
            }
        }
    }
}
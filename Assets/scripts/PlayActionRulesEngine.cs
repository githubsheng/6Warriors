using System.Collections.Generic;
using Conditions;
using DefaultNamespace;
using PlayerActionRules;
using UnityEngine;

public class PlayActionRulesEngine
{
    private PlayerAction _pendingAction;
    private PlayerAction _actionInExecution;
    private GameObject _defaultEnemyTarget;
    private GameObject _none;
    private GameObject _self;
    private HashSet<GameObject> _inContacts;
    private PlayerController _playerController;
    
    private List<Rule> rules;
    private SpellGenerator _spellGenerator;

    private void init(PlayerController playerController, SpellGenerator spellGenerator)
    {
        _inContacts = new HashSet<GameObject>();
        _none = new GameObject();
        _defaultEnemyTarget = _none;
        _playerController = playerController;
        _self = playerController.gameObject;
        _spellGenerator = spellGenerator;
    }
    
    //used by player characters. player can have 10 customizable rules.
    public PlayActionRulesEngine(PlayerController playerController, SpellGenerator spellGenerator)
    {
        init(playerController, spellGenerator);
        rules = new List<Rule>();
        for (int i = 0; i < 10; i++)
        {
            //by default create 10 empty rules. 
            rules.Add(new Rule());
        }
        
        //todo: for testing purpose im gonna put a "attack nearest enemy rule for player"
        rules[0].condition = new RuleNearestEnemeyCondition();
        rules[0].spellName = "normal_attack";
    }

    //fixed rules are usually used for npc or mobs
    public PlayActionRulesEngine(PlayerController playerController, List<Rule> fixedRules, SpellGenerator spellGenerator)
    {
        init(playerController, spellGenerator);
        rules = fixedRules;
    }

    public void run()
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
        GameObject actionTarget = _none;
        Rule targetRule = null;
        Condition targetCondition = null;
        for (int i = 0; i < rules.Count; i++)
        {
            Rule rule = rules[i];
            Condition condition = rule.condition;
            if (condition.evaluate(null, _defaultEnemyTarget, _self))
            {
                actionTarget = condition.getEvaluatedTarget();
                if (condition.isAnyEnemy() && _defaultEnemyTarget == _none)
                {
                    _defaultEnemyTarget = _none;
                }

                targetRule = rule;
                targetCondition = condition;
                break;
            }
        }
        
        if (targetRule == null) return null;

        if (targetCondition.isSelf())
        {
            //todo:
            //do something for the character itself
            Debug.Log(targetRule.spellName);            
        }
        else
        {
            //either target is an enemy or a teammate.
            /*
             * almost all actions requires the target to be within action range. depending on the action types, some needs to be in close range
             * (in contact), some needs to be within a certain distance (for example, range attack)
             */
            bool needToMoveToTarget = false;
            Spell spell = _spellGenerator.getSpellByName(targetRule.spellName);
            // <1f rather then == 0f to avoid the annoying ide complain
            if (spell.spellEffectiveRange < 1f)
            {
                //needs to be right next to target.
                needToMoveToTarget = isInContact(actionTarget);
            }
            else
            {
                needToMoveToTarget = 
                    Vector3.Distance(_self.transform.position, actionTarget.transform.position) < spell.spellEffectiveRange;
            }

            return needToMoveToTarget ? PlayerAction.createPlayerMoveToMovingTargetAction(_defaultEnemyTarget) : PlayerAction.createSpellAction(actionTarget, spell);

        }

        return null;
    }

    public void setWaitTime(float timeInSeconds)
    {
        _actionInExecution = PlayerAction.createWaitAction(timeInSeconds);
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
                resetCurrentTarget();
                _playerController.moveAgentToPlace(playerAction);
                break;
            case "move_to_moving_target":
                _playerController.moveAgentToTarget(playerAction);
                break;
            case "melee_attack":
                _playerController.startAttack(playerAction);
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
        _defaultEnemyTarget = target;
        
    }

    private void resetCurrentTarget()
    {
        _defaultEnemyTarget = _none;
    }

    public void onTriggerEnter(Collider other)
    {
        addInContact(other);
       
        switch (_actionInExecution.actionName)
        {
            case "move":
                if (other.CompareTag("player_nav_destination"))
                {
                    _playerController.reachDestination();
                    setWaitTime(0.2f);
                }
                break;
            case "move_to_moving_target":
                if (other.gameObject == _actionInExecution.target)
                {
                    _playerController.reachTarget();
                    setWaitTime(0.2f);
                }
                break;
            default:
                break;
        }
    }

    public void onTriggerExit(Collider other)
    {
        removeInContact(other);
    }

    public void addInContact(Collider other)
    {
        _inContacts.Add(other.gameObject);
    }

    public void removeInContact(Collider other)
    {
        _inContacts.Remove(other.gameObject);
    }

    public bool isInContact(GameObject other)
    {
        return _inContacts.Contains(other);
    }
}
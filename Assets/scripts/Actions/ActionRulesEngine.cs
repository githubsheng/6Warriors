using System;
using System.Collections.Generic;
using Conditions;
using ActionRules;
using Spells;
using Spells.warriors;
using UnityEngine;
using Object = UnityEngine.Object;

public class ActionRulesEngine
{
    private CharacterAction _pendingAction;
    private CharacterAction _actionInExecution;
    private GameObject _defaultEnemyTarget;
    private GameObject _none;
    private GameObject _self;
    private HashSet<GameObject> _inContacts;
    private CharacterController _characterController;
    
    private List<Rule> rules;

    private void init(CharacterController characterController)
    {
        _inContacts = new HashSet<GameObject>();
        _none = new GameObject();
        _defaultEnemyTarget = _none;
        _characterController = characterController;
        _self = characterController.gameObject;
    }
    
    //used by player characters. player can have 10 customizable rules.
    public ActionRulesEngine(CharacterController characterController)
    {
        init(characterController);
        rules = new List<Rule>();
        for (int i = 0; i < 10; i++)
        {
            //by default create 10 empty rules. 
            rules.Add(new Rule());
        }
        
        //todo: for testing purpose im gonna put a "attack nearest enemy rule for player"
        rules[0].condition = new RuleNearestEnemeyCondition();
        rules[0].spell = RogueSpells.normalAttack;
    }

    //fixed rules are usually used for npc or mobs
    public ActionRulesEngine(CharacterController characterController, List<Rule> fixedRules)
    {
        init(characterController);
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


        if (_actionInExecution != null  && _actionInExecution.actionName == "wait_action" && _actionInExecution.expireTime < Time.time)
        {
            //currently we have an wait action, and we have waited long enough. 
            //remove the wait action, allowing next action to become actionInExecution.
            _actionInExecution = null;
        }
        
        CharacterAction nextAction = getNextAction();
        
        //check if we need to execute another action
        if (_actionInExecution == null || (_actionInExecution.isInterrupbitle && nextAction.tryInterrupt))
        {
            _actionInExecution = nextAction;
            _pendingAction = null;
            if (!_actionInExecution.isEvaluated)
            {
                /*
                 * evaluated rule action can be null, indicating there is nothing to do. The GenericRuleAction is really a place holder
                 * for unevaluated rule action. More precisely, for performance reason, I am trying to delay evaluating a real action, thats
                 * why I use GenericRuleAction as a place holder, it just means: some action according to the rules. But based on the rules,
                 * there are cases where it turns out that there is nothing to do, so after `evaluateRuleAction`, we get _actionInExecution = null.
                 *
                 * note that if the gamer manually specify an action, it can never be null, nor can a pending action be null.
                 */
                _actionInExecution = evaluateRuleAction();
                if (_actionInExecution == null) return;
            }
            executeAction();
        } 
        
        //if not, continue execute the current action
        continousExecuteAction();
    }
    
    private CharacterAction getNextAction()
    {
        if (_pendingAction != null)
        {
            CharacterAction ret = _pendingAction;
            return ret;
        }

        return CharacterAction.createGenericRuleAction();
    }

    private CharacterAction evaluateRuleAction()
    {        
        GameObject actionTarget = _none;
        Rule targetRule = null;
        for (int i = 0; i < rules.Count; i++)
        {
            Rule rule = rules[i];
            Condition condition = rule.condition;
            if (condition.evaluate(null, _defaultEnemyTarget, _self))
            {
                actionTarget = condition.getEvaluatedTarget();
                if (condition.isAnyEnemy() && _defaultEnemyTarget == _none)
                {
                    _defaultEnemyTarget = actionTarget;
                }

                targetRule = rule;
                break;
            }
        }
        
        if (targetRule == null) return null;

        CharacterAction spellAction = CharacterAction.createSpellAction(actionTarget, targetRule.spell);
        
        
        //either target is an enemy or a teammate.
        /*
         * almost all actions requires the target to be within action range. depending on the action types, some needs to be in close range
         * (in contact), some needs to be within a certain distance (for example, range attack)
         */
        bool needToMoveToTarget = isActionInRange(spellAction);

        if (needToMoveToTarget)
        {
            CharacterAction moveToTarget = CharacterAction.createPlayerMoveToMovingTargetAction(spellAction.target);
            moveToTarget.subsequentAction = spellAction;
            return moveToTarget;
        }
        else
        {
            return spellAction;
        }
        
    }

    private bool isActionInRange(CharacterAction action)
    {
        Spell spell = action.spell;
        // <1f rather then == 0f to avoid the annoying ide complain
        if (spell.spellEffectiveRange < 1f)
        {
            //needs to be right next to target.
            return isInContact(action.target);
        }
        else
        {
            return Vector3.Distance(_self.transform.position, action.target.transform.position) < spell.spellEffectiveRange;
        }
    }

    public void setWaitTime(float timeInSeconds)
    {
        if (_actionInExecution.actionName == "move" || _actionInExecution.actionName == "move_to_moving_target")
        {
            throw new InvalidOperationException("running move and move to target action. they need to be properly terminated before we set _actionInExecution to something else");
        }
        _actionInExecution = CharacterAction.createWaitAction(timeInSeconds);
    }

    private void executeAction()
    {
        CharacterAction characterAction = _actionInExecution;
        switch (characterAction.actionName)
        {
            case "move":
                resetCurrentTarget();
                _characterController.moveAgentToPlace(characterAction);
                break;
            case "move_to_moving_target":
                _characterController.moveAgentToTarget(characterAction);
                break;
            case "spell_action":
                _characterController.startSpell(characterAction);
                break;
            default:
                break;
        }
    }

    private void continousExecuteAction()
    {
        switch (_actionInExecution.actionName)
        {
            case "move":
                examineMoveAction();
                break;
            case "move_to_moving_target":
                examineMoveToMovingTargetAction();
                break;
            default:
                break;
        }
    }
    
    private void examineMoveToMovingTargetAction()
    {
        CharacterAction moveToTarget = _actionInExecution;
        if (moveToTarget.subsequentAction == null) return;
        //it means we are moving to the target to execute the _pendingAction.
        if (isActionInRange(moveToTarget.subsequentAction))
        {
            _characterController.reachTarget();
            _actionInExecution = null;
            setWaitTime(0.2f);
            if (_pendingAction != null) _pendingAction = moveToTarget.subsequentAction;
        }
    }

    private void examineMoveAction()
    {
        CharacterAction moveAction = _actionInExecution;
        if (moveAction.target == null) return;
        if (isInContact(moveAction.target))
        {
            Object.Destroy(moveAction.target);
            _characterController.reachDestination();
            _actionInExecution = null;
            setWaitTime(0.2f);
        }
    }
    
    private void setMoveAction(Vector3 userSpecifiedMoveDestination)
    {
        CharacterAction moveAction = CharacterAction.createPlayerMoveAction(userSpecifiedMoveDestination);
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
        _inContacts.Add(other.gameObject);
    }

    public void onTriggerExit(Collider other)
    {
        _inContacts.Remove(other.gameObject);
    }

    private bool isInContact(GameObject other)
    {
        return _inContacts.Contains(other);
    }
}
using System;
using System.Collections.Generic;
using Conditions;
using ActionRules;
using Controllers;
using Spells;
using Spells.warriors;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Actions
{
    public class ActionRulesEngine
    {
        private CharacterAction _pendingAction;
        private CharacterAction _actionInExecution;
        //a default target can not be a friendly target, this is mostly used when deciding what to attack, not what / who to heal.
        private GameObject _defaultHostileTarget;
        private GameObject _self;
        private HashSet<GameObject> _inContacts;
        private CharacterControl _characterControl;
        private LevelController _levelController;
        
        private List<Rule> rules;
    
        private void init(CharacterControl characterControl, LevelController levelController)
        {
            _inContacts = new HashSet<GameObject>();
            _defaultHostileTarget = null;
            _characterControl = characterControl;
            _self = characterControl.gameObject;
            _levelController = levelController;
        }
        
        //used by player characters. player can have 10 customizable rules.
        public ActionRulesEngine(CharacterControl characterControl, LevelController levelController)
        {
            init(characterControl, levelController);
            rules = new List<Rule>();
            for (int i = 0; i < 10; i++)
            {
                //by default create 10 empty rules. 
                rules.Add(new Rule());
            }
            
            //todo: for testing purpose im gonna put a "attack nearest enemy rule for player"
            rules[0].condition = new RuleNearestEnemeyCondition();
            rules[0].spell = RogueSpells.normalAttack;
            rules[0].enabled = true;
        }
    
        //fixed rules are usually used for npc or mobs
        public ActionRulesEngine(CharacterControl characterControl, LevelController levelController, List<Rule> fixedRules)
        {
            init(characterControl, levelController);
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
                    if (hit.collider.CompareTag("monsters") || hit.collider.CompareTag("warriors"))
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
            Rule targetRule = null;
            List<GameObject> hostiles = _levelController.getHostiles(_self);
            List<GameObject> friendly = _levelController.getFriendly(_self);
            
            for (int i = 0; i < rules.Count; i++)
            {
                Rule rule = rules[i];
                Condition condition = rule.condition;
                //tries to evaluate each rule and see if it is met.
                if (rule.enabled && condition.evaluate(hostiles, friendly, _defaultHostileTarget, _self))
                {
                    targetRule = rule;
                    break;
                }
            }
            
            //if none of the rules matches, return null, indicating that no rules match
            if (targetRule == null) return null;
            
            //some rule matches, go on.
            
            //if the rule is about the caster himself, return the action, no need to first reach out for the target.
            if (targetRule.condition.EvaluatedTarget == _self)
            {
                return CharacterAction.createSpellAction(_self, targetRule.spell);
            } else
            {
                //target is either enemy or ally

                //if it is about an enemy, we may set this enemy to a default enemy, if there is no default enemy now.
                Condition condition = targetRule.condition;
                GameObject actionTarget = condition.EvaluatedTarget;
                if (isHostile(condition.EvaluatedTarget, _self) && !_defaultHostileTarget)
                {
                    _defaultHostileTarget = actionTarget;
                }

                //the rule's target is an enemy or an ally, we may potentially need to get closer to the target.
                CharacterAction spellAction = CharacterAction.createSpellAction(actionTarget, targetRule.spell);

                /*
                 * almost all actions requires the target to be within action range. depending on the action types, some needs to be in close range
                 * (in contact), some needs to be within a certain distance (for example, range attack)
                 */
                if (!isActionTargetInRange(spellAction))
                {
                    CharacterAction moveToTarget =
                        CharacterAction.createPlayerMoveToMovingTargetAction(spellAction.Target);
                    moveToTarget.subsequentAction = spellAction;
                    return moveToTarget;
                }
                else
                {
                    return spellAction;
                }

            }
        }

        private bool isHostile(GameObject obj1, GameObject obj2)
        {
            return !obj1.CompareTag(obj2.tag);
        }
    
        private bool isActionTargetInRange(CharacterAction action)
        {
            if (!action.Target)
            {
                throw new ArgumentException("needs to have a target to measure");
            }
            Spell spell = action.spell;
            // <1f rather then == 0f to avoid the annoying ide complain
            if (spell.spellEffectiveRange < 1f)
            {
                //needs to be right next to target.
                return isInContact(action.Target);
            } 
            return Vector3.Distance(_self.transform.position, action.Target.transform.position) < spell.spellEffectiveRange;
        }
    
        public void setWaitTime(float timeInSeconds)
        {
            _actionInExecution = CharacterAction.createWaitAction(timeInSeconds);
        }
    
        private void executeAction()
        {
            CharacterAction characterAction = _actionInExecution;
            switch (characterAction.actionName)
            {
                case "move":
                    resetCurrentTarget();
                    _characterControl.moveAgentToPlace(characterAction);
                    break;
                case "move_to_moving_target":
                    _characterControl.moveAgentToTarget(characterAction);
                    break;
                case "spell_action":
                    _characterControl.startSpell(characterAction);
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
                case "spell_action":
                    examineIfTargetIsStillValid();
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
            if (isActionTargetInRange(moveToTarget.subsequentAction))
            {
                _characterControl.reachTarget();
                _actionInExecution = null;
                setWaitTime(0.2f);
                if (_pendingAction != null) _pendingAction = moveToTarget.subsequentAction;
            }
        }
    
        private void examineMoveAction()
        {
            CharacterAction moveAction = _actionInExecution;
            if (moveAction.Target == null) return;
            if (isInContact(moveAction.Target))
            {
                Object.Destroy(moveAction.Target);
                _characterControl.reachDestination();
                _actionInExecution = null;
                setWaitTime(0.2f);
            }
        }

        private void examineIfTargetIsStillValid()
        {
//            if (_actionInExecution.TargetCharacterControl.IsDead)
//            {
//                //todo: if current action is a quick melee attack, or anything quick (like shooting an arrow) leave it to finish.
//                
//                //todo: if the character is still casting the spell (which typically take 2 seconds), then immediately cancel the cast animation.
//                
//            }
        }
        
        private void setMoveAction(Vector3 userSpecifiedMoveDestination)
        {
            CharacterAction moveAction = CharacterAction.createPlayerMoveAction(userSpecifiedMoveDestination);
            _pendingAction = moveAction;
        }
        
        private void setCurrentTarget(GameObject target)
        {
            _defaultHostileTarget = target;
            
        }
    
        private void resetCurrentTarget()
        {
            _defaultHostileTarget = null;
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

        public CharacterAction getCurrentRunningAction()
        {
            return _actionInExecution;
        }
    }    

}



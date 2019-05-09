using System;
using Controllers;
using Spells;
using UnityEngine;

namespace Actions
{
    public class CharacterAction
    {
        public String actionName;
        public bool isEvaluated;
        public bool tryInterrupt;
        public bool isInterrupbitle;
        
        //following props are for destinations
        public Vector3 userSpecifiedDestination;
        
        //following props are for attacking / casting spells
        private GameObject _target;
        private CharacterControl _characterControl;
        public Spell spell;
    
        public float expireTime;
        public CharacterAction subsequentAction;
    
        private static CharacterAction cachedRuleAction = null;
        private static CharacterAction cachedReadyAction = null;
    
        private CharacterAction(){}
        
        private CharacterAction(string actionName, bool isEvaluated, bool tryInterrupt, bool isInterrupbitle)
        {
            this.actionName = actionName;
            this.isEvaluated = isEvaluated;
            this.tryInterrupt = tryInterrupt;
            this.isInterrupbitle = isInterrupbitle;
        }
    
        public static CharacterAction createPlayerMoveAction(Vector3 userSpecifiedDestination)
        {
            CharacterAction moveAction = new CharacterAction("move", true, true, true);
            moveAction.userSpecifiedDestination = userSpecifiedDestination;
            return moveAction;
        }
        
        public static CharacterAction createPlayerMoveToMovingTargetAction(GameObject movingTarget)
        {
            CharacterAction moveToMovingTargetAction = new CharacterAction("move_to_moving_target", true, true, true);
            moveToMovingTargetAction.Target = movingTarget;
            return moveToMovingTargetAction;
        }
    
        public static CharacterAction createGenericRuleAction()
        {
            if (cachedRuleAction == null)
            {
                CharacterAction ruleAction = new CharacterAction("rule_action", false, false, false);
                cachedRuleAction = ruleAction;
                return ruleAction;
            }
            else
            {
                return cachedRuleAction;
            }
        }
    
        public static CharacterAction createSpellAction(GameObject target, Spell spell, bool tryInterrupt = false)
        {
            CharacterAction meleeAttackAction = new CharacterAction("spell_action", true, tryInterrupt, false);
            meleeAttackAction.Target = target;
            meleeAttackAction.spell = spell;
            return meleeAttackAction;
        }
    
        public static CharacterAction createWaitAction(float waitTime)
        {
            CharacterAction waitAction = new CharacterAction();
            waitAction.actionName = "wait_action";
            waitAction.isInterrupbitle = false;
            waitAction.expireTime = Time.time + waitTime;
            return waitAction;
        }
        
        
        public GameObject Target
        {
            // This is your getter.
            // it uses the accessibility of the property (public)
            get
            {
                return _target;
            }
            // this is your setter
            // Note: you can specify different accessibility
            // for your getter and setter.
            set
            {
                _target = value;
                _characterControl = _target.GetComponent<CharacterControl>();
            }
        }
        
        public CharacterControl CharacterControl
        {
            get
            {
                return _characterControl;
            }
        }

        public bool IsTargetValid
        {
            get { return _characterControl.IsDead; }
        }
    }    
}


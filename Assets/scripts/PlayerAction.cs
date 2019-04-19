using System;
using System.Runtime.ConstrainedExecution;
using UnityEngine;

public class PlayerAction
{
    public String actionName;
    public bool isEvaluated;
    public bool tryInterrupt;
    public bool isInterrupbitle;
    
    //following props are for destinations
    public Vector3 userSpecifiedDestination;
    
    //following props are for attacking / casting spells
    public GameObject target;
    public String spellName;

    private static PlayerAction cachedRuleAction = null;
    
    public static PlayerAction createPlayerMoveAction(Vector3 userSpecifiedDestination)
    {
        PlayerAction moveAction = new PlayerAction();
        moveAction.actionName = "move";
        moveAction.tryInterrupt = true;
        moveAction.isInterrupbitle = true;
        moveAction.userSpecifiedDestination = userSpecifiedDestination;
        return moveAction;
    }
    
    public static PlayerAction createPlayerMoveToMovingTargetAction(GameObject movingTarget)
    {
        PlayerAction moveToMovingTargetAction = new PlayerAction();
        moveToMovingTargetAction.actionName = "move_to_moving_target";
        moveToMovingTargetAction.tryInterrupt = true;
        moveToMovingTargetAction.isInterrupbitle = true;
        moveToMovingTargetAction.target = movingTarget;
        return moveToMovingTargetAction;
    }

    public static PlayerAction createGenericRuleAction()
    {
        if (cachedRuleAction == null)
        {
            PlayerAction ruleAction = new PlayerAction();
            ruleAction.actionName = "rule_action";
            ruleAction.isEvaluated = false;
            ruleAction.tryInterrupt = false;
            cachedRuleAction = ruleAction;
            return ruleAction;
        }
        else
        {
            return cachedRuleAction;
        }

    }

    public static PlayerAction createMeleeAttackAction()
    {
        return null;
    }
    
}
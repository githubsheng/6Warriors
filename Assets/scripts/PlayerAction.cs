using System;
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
    public Spell spell;

    public float expireTime;

    private static PlayerAction cachedRuleAction = null;
    private static PlayerAction cachedReadyAction = null;

    private PlayerAction(){}
    
    private PlayerAction(string actionName, bool isEvaluated, bool tryInterrupt, bool isInterrupbitle)
    {
        this.actionName = actionName;
        this.isEvaluated = isEvaluated;
        this.tryInterrupt = tryInterrupt;
        this.isInterrupbitle = isInterrupbitle;
    }

    public static PlayerAction createPlayerMoveAction(Vector3 userSpecifiedDestination)
    {
        PlayerAction moveAction = new PlayerAction("move", true, true, true);
        moveAction.userSpecifiedDestination = userSpecifiedDestination;
        return moveAction;
    }
    
    public static PlayerAction createPlayerMoveToMovingTargetAction(GameObject movingTarget)
    {
        PlayerAction moveToMovingTargetAction = new PlayerAction("move_to_moving_target", true, true, true);
        moveToMovingTargetAction.target = movingTarget;
        return moveToMovingTargetAction;
    }

    public static PlayerAction createGenericRuleAction()
    {
        if (cachedRuleAction == null)
        {
            PlayerAction ruleAction = new PlayerAction("rule_action", false, false, false);
            cachedRuleAction = ruleAction;
            return ruleAction;
        }
        else
        {
            return cachedRuleAction;
        }
    }

    public static PlayerAction createSpellAction(GameObject target, Spell spell, bool tryInterrupt = false)
    {
        PlayerAction meleeAttackAction = new PlayerAction("melee_attack", true, tryInterrupt, false);
        meleeAttackAction.target = target;
        meleeAttackAction.spell = spell;
        return meleeAttackAction;
    }

    public static PlayerAction createWaitAction(float waitTime)
    {
        PlayerAction waitAction = new PlayerAction();
        waitAction.actionName = "wait_action";
        waitAction.isInterrupbitle = false;
        waitAction.expireTime = Time.time + waitTime;
        return waitAction;
    }
    
}
using System;
using System.Collections.Generic;
using Spells;
using UnityEngine;

namespace ActionRules
{
    /*
     * The target of the rule can be obtained, by `condition.evaluatedTarget`. This value becomes available, after the call to
     * `condition.evaluate`.
     *
     * for player managed rules, the target in the condition is the same the target of the spell.
     * for example, if a target's hp becomes low, use heal spell on the same target.
     *
     * This way, the rule can be seen as two parts: 1) if target... 2) do ... to the target.
     *
     * If the spell's target can be different from the condition's target, the rule becomes too complicated, and is diffcult
     * for players to manager in the UI.
     *
     * There are some exceptions though, for fixed / built in rules, the spell's target and the condition's target can actually be different.
     *
     * This is useful, for boss's actions (ie, when boss's hp drops to half, unleash this special power on our heroes).
     *
     * And it is also practical because players do not need to handle and manage these rules in the UI, so complexity isn't
     * an issue here.
     */
    public class Rule
    {
        public Condition condition;
        public Spell spell;
        public bool enabled;
    }

    public abstract class Condition
    {

        protected GameObject evaluatedTarget;
        
        //is the evaluated target going to be an enemy
        virtual public bool isAnyEnemy()
        {
            return false;
        }

        //is the evaluated target going to be an ally
        virtual public bool isAnyTeammate()
        {
            return false;
        }

        //is the evaluated target going to be the caster itself.
        virtual public bool isSelf()
        {
            return false;
        }
       
        virtual public bool evaluate(List<GameObject> targets, GameObject defaultTarget, GameObject self)
        {
            return false;
        }

        public GameObject getEvaluatedTarget()
        {
            return evaluatedTarget;
        }
        
    }
    
    
   
}
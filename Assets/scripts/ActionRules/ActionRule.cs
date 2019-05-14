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
     * the evaluated target in the condition always become the target of the spell.
     * 
     * this makes sense, for rules like:
     * ___________________________________________________________
     *            condition            |            spell         
     *  any ally whose hp is below 80% |           use heal      
     *
     * the ally that meets the condition naturally becomes the target of our heal spell.
     * in this way, the rule becomes quite simple, as user only needs to manage two components, shown above.
     * all player managed rules are like the above
     *
     * there are exceptions though, when it comes to build in rules. Built in rules do not need to managed by players and
     * can afford to be more complicated, for example, following is a boss action rule:
     *
     * __________________________________________________________________________________________
     *            condition                |            spell           |        spell target
     *  any self whose hp is below 80%     |      use "powerful attack" |      nearest enemy
     *
     * In this case, the `condition.evaluate` not only checks the boss's hp, to decide if the condition is met, but also
     * goes through all enemies (in this cases, our warriors), and set the `evaluatedTarget` to be that warrior. As you can see,
     * the character that meets the condition (self) is not always the evaluated target (the warrior) here. However, this still
     * follow the rule of "the evaluated target in the condition always become the target of the spell.", and therefore I am
     * able to reuse the rules engine built for player managed rules to execute built-in rules.
     *
     * 
     */
    public class Rule
    {
        public Condition condition;
        public Spell spell;
        public bool enabled;
    }

    public abstract class Condition
    {
        private GameObject _evaluatedTarget;
        private bool _isEvaluatedTargetSet = false;
       
        virtual public bool evaluate(List<GameObject> targets, GameObject defaultTarget, GameObject self)
        {
            return false;
        }

        public GameObject EvaluatedTarget
        {
            get
            {
                if(_isEvaluatedTargetSet) throw new InvalidOperationException("evaluated target is not set, call evaluate first");
                return _evaluatedTarget;
            }
            set
            {
                _isEvaluatedTargetSet = true;
                _evaluatedTarget = value;
            }
        }
    }
    
    
   
}
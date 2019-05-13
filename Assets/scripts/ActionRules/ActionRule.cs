using System;
using System.Collections.Generic;
using Spells;
using UnityEngine;

namespace ActionRules
{
    public class Rule
    {
        public Condition condition;
        public Spell spell;
        public bool enabled;
    }

    public abstract class Condition
    {

        protected GameObject evaluatedTarget;
        
        virtual public bool isAnyEnemy()
        {
            return false;
        }

        virtual public bool isAnyTeammate()
        {
            return false;
        }

        virtual public bool isSelf()
        {
            return false;
        }
       

        virtual public bool evaluate(List<GameObject> targets, GameObject target, GameObject self)
        {
            return false;
        }

        public GameObject getEvaluatedTarget()
        {
            return evaluatedTarget;
        }
        
    }
    
    
   
}
using System;
using System.Collections.Generic;
using UnityEngine;

namespace PlayerActionRules
{
    public class Rule
    {
        public Condition condition;
        public string spellName;
        public bool enabled;
    }

    public abstract class Condition
    {

        protected GameObject evaluatedTarget;
        
        public bool isAnyEnemy()
        {
            return false;
        }

        public bool isAnyTeammate()
        {
            return false;
        }

        public bool isSelf()
        {
            return false;
        }
       

        public bool evaluate(List<GameObject> targets, GameObject target, GameObject self)
        {
            return false;
        }

        public GameObject getEvaluatedTarget()
        {
            return evaluatedTarget;
        }
        
    }
    
    
   
}
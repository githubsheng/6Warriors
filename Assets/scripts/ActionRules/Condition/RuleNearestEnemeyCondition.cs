using System;
using System.Collections.Generic;
using ActionRules;
using UnityEngine;

namespace Conditions
{
    public class RuleNearestEnemeyCondition : Condition
    {
        //todo: this value needs to be experimented and tuned later.
        private const float MaxAutoAttackRange = 1000;
        
        public override bool isAnyEnemy()
        {
            return true;
        }

        public override bool evaluate(List<GameObject> targets, GameObject currentTarget, GameObject self)
        {
            Vector3 selfPosition = self.transform.position;

            if (Vector3.Distance(currentTarget.transform.position, selfPosition) < MaxAutoAttackRange)
            {
                evaluatedTarget = currentTarget;
                return true;
            }
            
            GameObject nearest = null;
            //if all enemies are too far away then do nothing
            float nearestDistance = MaxAutoAttackRange;

            for (int i = 0; i < targets.Count; i++)
            {
                GameObject target = targets[i];
                Vector3 targetPosition = target.transform.position;
                float distance = Vector3.Distance(targetPosition, selfPosition);
                if (distance < nearestDistance)
                {
                    nearestDistance = distance;
                    nearest = target;
                }
            }

            if (nearest == null) return false;

            evaluatedTarget = nearest;

            return true;
        }
        
    }
}
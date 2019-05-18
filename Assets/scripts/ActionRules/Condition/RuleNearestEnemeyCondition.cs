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

        public override bool evaluate(List<GameObject> hostiles, List<GameObject> friendly, GameObject defaultTarget, GameObject self)
        {
            Vector3 selfPosition = self.transform.position;

            if (defaultTarget && Vector3.Distance(defaultTarget.transform.position, selfPosition) < MaxAutoAttackRange)
            {
                EvaluatedTarget = defaultTarget;
                return true;
            }
            
            GameObject nearest = null;
            //if all enemies are too far away then do nothing
            float nearestDistance = MaxAutoAttackRange;

            for (int i = 0; i < hostiles.Count; i++)
            {
                GameObject target = hostiles[i];
                Vector3 targetPosition = target.transform.position;
                float distance = Vector3.Distance(targetPosition, selfPosition);
                if (distance < nearestDistance)
                {
                    nearestDistance = distance;
                    nearest = target;
                }
            }

            if (nearest == null) return false;

            EvaluatedTarget = nearest;

            return true;
        }
        
    }
}
using System.Collections.Generic;
using ActionRules;
using Controllers;
using UnityEngine;

namespace Conditions
{
    public class HpBelow80PercentageCondition : Condition
    {
        public override bool evaluate(List<GameObject> hostiles, List<GameObject> friendly, GameObject defaultTarget, GameObject self)
        {
            CharacterControl cc = self.GetComponent<CharacterControl>();
            EvaluatedTarget = self;
            return (float)cc.CharacterStatus.hp / cc.CharacterStatus.maxHp < 0.8f;
        }
        
    }
}
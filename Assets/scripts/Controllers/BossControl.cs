using System.Collections.Generic;
using ActionRules;
using Actions;
using Conditions;
using Spells.boss;
using UnityEngine;
using UnityEngine.AI;

namespace Controllers
{
    public class BossControl : CharacterControl
    {

        public override void Start()
        {
            _animator = self.GetComponent<Animator>();
            _agent = GetComponent<NavMeshAgent>();
            _rulesEngine = new ActionRulesEngine(this, initBossActionRules());
            _characterStatus = new CharacterStatus(maxBaseHp, maxBaseMana, baseAttackStrengh, baseMagicPower);
        }

        private List<Rule> initBossActionRules()
        {
            //a list of fixed rules for boss control
            List<Rule> rules = new List<Rule>();

            Rule attackNearestEnemy = new Rule
            {
                condition = new RuleNearestEnemeyCondition(),
                enabled = true,
                spell = BossSpells.normalAttack
            };
            
            rules.Add(attackNearestEnemy);
            
            //hint: perhaps some rules to attack the warrior that casted "taunt"

            return rules;
        }
        
        public override void Update()
        {
            Debug.Log("yo...");
        }


        
    }
}
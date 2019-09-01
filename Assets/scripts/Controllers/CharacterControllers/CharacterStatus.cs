using System;
using System.Collections.Generic;
using Buffs;
using Spells;
using UnityEngine;
using Random = System.Random;

namespace CharacterControllers
{
    public class CharacterStatus
    {
    
        public int maxBaseHp;
        public int maxBaseMana;
        public int maxHp;
        public int maxMana;
        public int baseAttackStrengh;
        public int baseMagicPower;
        public int baseHpRegerationPerSecond;
        public int baseManaRegeratoinPerSecond;
        public int baseArmor;
        public int baseMagicResistence;
        public int baseFireResistence;
        public int baseIceResistence;
        public int baseDodgeChance;
        
        public int hp;
        public int mana;
        public int attackStrengh;
        public int magicPower;
        public int armor;
        public int magicResistence;
        public int fireResistence;
        public int iceResistence;
        // 0 - 100
        public int dodgeChance;
        // by default it is 100, if blind, set to something like 20
        public int hitChance;
        
        public bool isDead;
        public int isPoisoned;
        public int isBlind;
    
        private float _expectedNextEvaluationTime;
    
        //includes debuffs
        public List<CharacterBuff> buffs = new List<CharacterBuff>();
        
        private Random _random = new Random();
        
        
        public CharacterStatus(int maxBaseHp, int maxBaseMana, int baseAttackStrengh, int baseMagicPower)
        {
            hp = maxBaseHp;
            mana = maxBaseMana;
            this.maxBaseHp = maxBaseHp;
            this.maxBaseMana = maxBaseMana;
            maxHp = maxBaseHp;
            maxMana = maxBaseMana;
            this.baseAttackStrengh = baseAttackStrengh;
            this.baseMagicPower = baseMagicPower;
            baseHpRegerationPerSecond = 5;
            baseManaRegeratoinPerSecond = 10;
            _expectedNextEvaluationTime = 0;
            hitChance = 100;
            //all base arm, (all kinds of) base magic resistance are 0 by default.
        }
    
        private bool isDodgingDamage()
        {
            var dodge = _random.Next(100);
            return dodge < dodgeChance;
        }
    
        private int calculateReceivedPhysicalDamage(Spell effect)
        {
            return effect.physicalAttackStrengh - armor;
        }
    
        private int calclulateReceivedMagicDamage(Spell effect)
        {
            return  effect.magicalAttackPower - magicResistence;
        }
    
        private int calculateReceivedIceMagicDamage(Spell effect)
        {
            return effect.magicalAttackPower - iceResistence - magicResistence;
        }
    
        private int calculateReceivedFireMagicDamage(Spell effect)
        {
            return effect.magicalAttackPower - fireResistence - magicResistence;
        }
    
        public void onReceivingSpell(Spell spell)
        {
            int damage = 0;
            if (isDodgingDamage()) return;
            if (spell.buff != null)
            {
                //todo: check if spell is already there. if it is, simply reset its duration
                buffs.Add(spell.buff);
                spell.buff.onAddingBuffer(this);
            }
            
            damage += calculateReceivedPhysicalDamage(spell);
            damage += calclulateReceivedMagicDamage(spell);
            damage += calculateReceivedIceMagicDamage(spell);
            damage += calculateReceivedFireMagicDamage(spell);
      
            hp -= damage;
            if (hp <= 0)
            {
                isDead = true;
                resetStatus();
            }
        }
    
        private void resetStatus()
        {
            buffs = new List<CharacterBuff>();
        } 
    
        public bool reEvaluateStatusEverySecond()
        {
            float now = Time.time;
            if (now < _expectedNextEvaluationTime) return false;
    
            _expectedNextEvaluationTime += 1;
            
            hp += baseHpRegerationPerSecond;
            mana += baseManaRegeratoinPerSecond;
           
            List<CharacterBuff> validBuffs = new List<CharacterBuff>(buffs.Count);
    
            for (int i = 0; i < buffs.Count; i++)
            {
                CharacterBuff characterBuff = buffs[i];
                if (characterBuff.isExpired())
                {
                    //this buff has expired, we need to revert all the effects of this buff
                    characterBuff.onRemovingBuffer(this);
                }
                else
                {
                    if (characterBuff.isEffective()) characterBuff.onBufferBecomeEffective(this);
                    validBuffs.Add(characterBuff);
                }
            }
    
            buffs = validBuffs;         
            //when we remove a buff that increase max hp, the current hp may be higher than new max hp.
            hp = Math.Min(hp, maxHp);
            mana = Math.Min(mana, maxMana);

            if (hp <= 0)
            {
                resetStatus();
                isDead = true;
            }
            return true;
        }
        
    }    
}


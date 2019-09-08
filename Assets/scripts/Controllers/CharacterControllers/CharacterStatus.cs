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
    
        public float maxBaseHp;
        public float maxBaseMana;
        public float maxHp;
        public float maxMana;
        public float baseAttackStrengh;
        public float baseHpRegerationPerSecond;
        public float baseManaRegeratoinPerSecond;
        public float baseArmor;
        public float baseMagicResistence;
        public float baseFireResistence;
        public float baseIceResistence;
        public float baseDodgeChance;
        
        public float hp;
        public float mana;
        public float attackStrengh;
        public float armor;
        public float poisonResistence;
        public float fireResistence;
        public float iceResistence;
        public bool isDead;
        public int isPoisoned;
    
        private float _expectedNextEvaluationTime;
    
        //includes debuffs
        public List<CharacterBuff> buffs = new List<CharacterBuff>();
        
        private Random _random = new Random();
        
        
        public CharacterStatus(float maxBaseHp, float maxBaseMana, float baseAttackStrengh)
        {
            hp = maxBaseHp;
            mana = maxBaseMana;
            this.maxBaseHp = maxBaseHp;
            this.maxBaseMana = maxBaseMana;
            maxHp = maxBaseHp;
            maxMana = maxBaseMana;
            this.baseAttackStrengh = baseAttackStrengh;
            baseHpRegerationPerSecond = 3;
            baseManaRegeratoinPerSecond = 3;
            _expectedNextEvaluationTime = 0;
        }
    
        private float calculateReceivedDamage(Spell effect)
        {
            float damage = effect.attackStrength - armor;
            if (effect.magicType == Spell.MAGIC_TYPE_ICE) damage -= iceResistence;
            if (effect.magicType == Spell.MAGIC_TYPE_FIRE) damage -= fireResistence;
            if (effect.magicType == Spell.MAGIC_TYPE_POISION) damage -= poisonResistence;
            return damage;
        }

    
        public void onReceivingSpell(Spell spell)
        {
            if (spell.buff != null)
            {
                //todo: check if spell is already there. if it is, simply reset its duration
                buffs.Add(spell.buff);
                spell.buff.onAddingBuffer(this);
            }
            hp -=  calculateReceivedDamage(spell);
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


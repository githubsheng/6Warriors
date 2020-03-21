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
        public int playerLevel = 1;
        //todo: should provide methods to add/subtract weapon addition, gear hp/mana regeration
        //todo: these methods are called when we put on/off gears.
        public float gearAttackStrengh = 0;
        public float gearHpRegerationPerSecond = 0;
        public float gearManaRegerationPerSecond = 0;
        public float armor;
        public float maxBaseHp;
        public float maxBaseMana;
        public float maxHp;
        public float maxMana;
        public float baseAttackStrengh;
        public float baseHpRegerationPerSecond;
        public float baseManaRegeratoinPerSecond;
        public float baseMagicResistence;
        public float baseFireResistence;
        public float baseIceResistence;
        public float baseDodgeChance;
        public float hp;
        public float mana;
        public float attackStrengh;
        public float holyResistence;
        public float fireResistence;
        public float iceResistence;
        public bool isDead;
        public float vulnerability = 1f;
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
            this.attackStrengh = baseAttackStrengh;
            baseHpRegerationPerSecond = 3;
            baseManaRegeratoinPerSecond = 3;
            _expectedNextEvaluationTime = 0;
        }
    
        private float calculateReceivedDamage(Spell effect)
        {
            float damage = effect.attackStrength - armor;
            if (effect.magicType == Spell.MAGIC_TYPE_ICE) damage -= iceResistence;
            if (effect.magicType == Spell.MAGIC_TYPE_FIRE) damage -= fireResistence;
            if (effect.magicType == Spell.MAGIC_TYPE_HOLY) damage -= holyResistence;
            damage = Math.Max(0, damage);
            return damage * vulnerability;
        }

    
        public void onReceivingSpell(Spell spell)
        {
            hp -=  calculateReceivedDamage(spell);
            if (hp <= 0)
            {
                isDead = true;
            }
        }
    
        public void reEvaluateStatusEverySecond()
        {
            float now = Time.time;
            if (now < _expectedNextEvaluationTime) return;
    
            _expectedNextEvaluationTime += 1;
            
            hp = hp + baseHpRegerationPerSecond + gearHpRegerationPerSecond;
            mana = mana + baseManaRegeratoinPerSecond + gearManaRegerationPerSecond;

            //cos hp / man keeps regerating
            hp = Math.Min(maxHp, hp);
            mana = Math.Min(maxMana, mana);

            if (hp <= 0)
            {
                isDead = true;
            }
        }


        public void changeHp(float change) {
            hp += change;
            hp = Math.Min(maxHp, hp);
            if (hp <= 0)
            {
                isDead = true;
            }
        }

        public void changeMana(float change) {
            mana += change;
            mana = Math.Min(maxMana, mana);
        }
    }    
}


using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Spells;
using UnityEngine;
using Random = System.Random;

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
    public int dodgeChance;
    
    public bool isDisabled;
    public bool isDead;
    public bool isPoisoned;
    public bool isPetryfied;
    public bool isBlind;

    private float _expectedNextEvaluationTime;

    //includes debuffs
    public List<CharacterBuff> buffs = new List<CharacterBuff>();
    
    private Random _random = new Random();
    
    
    public CharacterStatus(int maxBaseHp, int maxBaseMana)
    {
        hp = maxBaseHp;
        mana = maxBaseMana;
        this.maxBaseHp = maxBaseHp;
        this.maxBaseMana = maxBaseMana;
        baseHpRegerationPerSecond = 5;
        baseManaRegeratoinPerSecond = 10;
        _expectedNextEvaluationTime = 0;
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

    public void onReceivingBattleEffect(Spell effect)
    {
        int damage = 0;
        if (isDodgingDamage()) return;
        if (effect.buff != null)
        {
            buffs.Add(effect.buff);
        }
        
        damage += calculateReceivedPhysicalDamage(effect);
        damage += calclulateReceivedMagicDamage(effect);
        damage += calculateReceivedIceMagicDamage(effect);
        damage += calculateReceivedFireMagicDamage(effect);
  
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
        //todo:
    } 

    public int[] reEvaluateStatusEverySecond()
    {
        float now = Time.time;
        if (now < _expectedNextEvaluationTime) return null;

        _expectedNextEvaluationTime += 1;
        
        int hpIncrease = baseHpRegerationPerSecond;
        int manaIncrease = baseManaRegeratoinPerSecond;
        int dotDamage = 0;
        int manaBurned = 0;

        List<CharacterBuff> validBuffs = new List<CharacterBuff>(buffs.Count);

        for (int i = 0; i < buffs.Count; i++)
        {
            CharacterBuff characterBuff = buffs[i];
            if (characterBuff.isExpired())
            {
                //this buff has expired, we need to revert all the effects of this buff
                maxHp -= characterBuff.maxHpChange;
                maxMana -= characterBuff.maxManaChange;
                armor -= characterBuff.armorChange;
                magicResistence -= characterBuff.magicResistenceChange;
                fireResistence -= characterBuff.fireResistenceChange;
                iceResistence -= characterBuff.iceResistenceChange;
                attackStrengh -= characterBuff.attackStrenghChange;
                magicPower -= characterBuff.magicPowerChange;
                dodgeChance -= characterBuff.dodgeChanceChange;
            }
            else
            {
                if (characterBuff.isEffective())
                {
                    hpIncrease += characterBuff.hpIncrease;
                    manaIncrease += characterBuff.manaIncrease;
                    dotDamage += characterBuff.hpDecrease;
                    manaBurned += characterBuff.manaDecrease;
                    characterBuff.updateNextEffectiveTime();
                }
                validBuffs.Add(characterBuff);
            }
        }

        buffs = validBuffs;
      
        hp = hpIncrease + dotDamage;
        hp = Math.Max(hp, 0);
        //when we remove a buff that increase max hp, the current hp may be higher than new max hp.
        hp = Math.Min(hp, maxHp);
        
        mana = manaIncrease + manaBurned;
        mana = Math.Max(mana, 0);
        mana = Math.Min(mana, maxMana);
        
        if (hp == 0)
        {
            isDisabled = true;
            resetStatus();
        }
        return new[]{hpIncrease, manaIncrease, dotDamage, manaBurned};
    }
    

}
//also includes debuff

using System;
using UnityEngine;

public class CharacterBuff
{
    //these are one time effects, ie, only applied once
    public int maxHpChange;
    public int maxManaChange;
    public int attackStrenghChange;
    public int magicPowerChange;
    public int armorChange;
    public int magicResistenceChange;
    public int fireResistenceChange;
    public int iceResistenceChange;
    public int dodgeChanceChange;

    //these effects are applied every time the buff become effective
    public int hpIncrease;
    public int manaIncrease;
    //the decrease needs to be negative
    public int hpDecrease;
    public int manaDecrease;
    
    public string name;
    //todo: need to think about it.
    public string buffIconName;
    public bool isDebuff;
    public string explaination;
    
    
    public int intervalInSeconds;
    public float durationInSeconds;
    public float buffExpireTime;
    public float nextEffectiveTime;

    public CharacterBuff(string name, string buffIconName, bool isDebuff, string explaination, float durationInSeconds)
    {
        this.name = name;
        this.buffIconName = buffIconName;
        this.isDebuff = isDebuff;
        this.explaination = explaination;
        this.durationInSeconds = durationInSeconds;
    }

    public bool isExpired()
    {
        return buffExpireTime < Time.time;
    }

    public bool isEffective()
    {
        return nextEffectiveTime < Time.time;
    }

    public void updateNextEffectiveTime()
    {
        nextEffectiveTime += intervalInSeconds;
    }

}
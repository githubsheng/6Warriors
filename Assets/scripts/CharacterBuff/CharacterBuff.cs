//also includes debuff

using System;
using CharacterControllers;
using UnityEngine;

namespace Buffs
{
    public abstract class CharacterBuff
    {
        //same buff may be accumulated. if 0, means, buffer cannot be accumulated
        public int count;
        //todo: make these int to floats....

        //these are one time effects, ie, only applied once
        public int maxHpChange;
        public int maxManaChange;
        public int attackStrenghChange;
        public int magicPowerChange;
        public int armorChange;

        //these effects are applied every time the buff become effective
        public int hpChange;
        public int manaChange;
    
        public string name;
        public bool isDebuff;
        public string explaination;
    
        public int intervalInSeconds;
        public float durationInSeconds;
        public float buffExpireTime;
        public float nextEffectiveTime;

        public CharacterBuff(string name, bool isDebuff, string explaination, float durationInSeconds)
        {
            this.name = name;
            this.isDebuff = isDebuff;
            this.explaination = explaination;
            this.durationInSeconds = durationInSeconds;
            resetExpireTime();
        }

        public void resetExpireTime()
        {
            buffExpireTime = Time.time + durationInSeconds;
        }

        public bool isExpired()
        {
            return buffExpireTime < Time.time;
        }

        public bool isEffective() {
            if (intervalInSeconds == 0) return true;
            return nextEffectiveTime < Time.time;
        }

        public void updateNextEffectiveTime()
        {
            nextEffectiveTime += intervalInSeconds;
        }

    }
}


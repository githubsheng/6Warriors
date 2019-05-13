using Controllers;
using UnityEngine;

namespace Buffs.warriors.rogue
{
    public class Poison : CharacterBuff
    {
        //todo: here the names, buffIconName, and everything should be static for this point buff.
        public Poison(string name, string buffIconName, bool isDebuff, string explaination, float durationInSeconds) :
            base(name, buffIconName, isDebuff, explaination, durationInSeconds)
        {
            hpDecrease = -3;
            intervalInSeconds = 2;
            makeBlind = 1;
        }

        public override void onAddingBuffer(CharacterStatus status)
        {
            if (status.isPoisoned == 0)
            {
                //todo:
                Debug.Log("add the poisoned shader");
            }

            status.isPoisoned += makeBlind;
            //immediately deduct hp.
            onBufferBecomeEffective(status);
        }

        public override void onRemovingBuffer(CharacterStatus status)
        {
            status.isPoisoned -= makeBlind;
            
            if (status.isPoisoned == 0)
            {
                //todo:
                Debug.Log("remove the poisoned shader");
            }
        }

        public override void onBufferBecomeEffective(CharacterStatus status)
        {
            status.hp += hpDecrease;
            updateNextEffectiveTime();
        }
    }
}
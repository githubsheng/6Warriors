using System.Collections;
using System.Collections.Generic;
using Controllers;
using UnityEngine;

namespace Animations
{

    public class AnimationEventsHandler: MonoBehaviour
    {
        private CharacterControl _characterControl;
    
        void Start()
        {
            _characterControl = GetComponentInParent<CharacterControl>();
        }

        public void Attack51EffectiveCallback()
        {
            _characterControl.onAttackBecomeEffective();
        }
    
        public void Attach51FinishCallback()
        {
            _characterControl.onAttackFinish();
        }

        public void Attach2FinishCallback()
        {

        }
    }    

}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimationEventsHandler: MonoBehaviour
{
    private CharacterController _characterController;
    
    void Start()
    {
        _characterController = GetComponentInParent<CharacterController>();
    }

    public void Attack51EffectiveCallback()
    {
        _characterController.onAttackBecomeEffective();
    }
    
    public void Attach51FinishCallback()
    {
        _characterController.onAttackFinish();
    }

    public void Attach2FinishCallback()
    {

    }
}

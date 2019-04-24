using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimationEventsHandler: MonoBehaviour
{
    private PlayerController playerController;
    
    void Start()
    {
        playerController = GetComponentInParent<PlayerController>();
    }

    public void Attack51EffectiveCallback()
    {
        playerController.onAttackBecomeEffective();
    }
    
    public void Attach51FinishCallback()
    {
        playerController.onAttackFinish();
    }

    public void Attach2FinishCallback()
    {

    }
}

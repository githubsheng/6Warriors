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
    
    public void Attach51FinishCallback()
    {

    }

    public void Attach2FinishCallback()
    {

    }
}

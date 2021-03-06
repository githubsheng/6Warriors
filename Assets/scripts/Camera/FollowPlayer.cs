﻿using UnityEngine;

public class FollowPlayer : MonoBehaviour {
    public float followXOffset;
    public float followYOffset;
    public float followZOffset;
    public Transform target;

    void Start()
    {
        Vector3 targetPos = target.transform.position;
        transform.position = new Vector3(targetPos.x + followXOffset, targetPos.y + followYOffset, targetPos.z + followZOffset);
    }

    void Update(){
        Vector3 targetPos = target.transform.position;
        transform.position = new Vector3(targetPos.x + followXOffset, targetPos.y + followYOffset, targetPos.z + followZOffset);
    }
}

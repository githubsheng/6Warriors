using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FollowPlayer : MonoBehaviour
{
    public float followYOffset;
    public float followZOffset;
    public Transform target;

    void Start()
    {
        Vector3 targetPos = target.transform.position;
        transform.position = new Vector3(targetPos.x, followYOffset, targetPos.z + followZOffset);
    }

    void Update(){
        Vector3 targetPos = target.transform.position;
        transform.position = new Vector3(targetPos.x, followYOffset, targetPos.z + followZOffset);
    }
}

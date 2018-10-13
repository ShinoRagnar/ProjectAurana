using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraMovement : MonoBehaviour, Initiates{

    public static readonly float CAMERA_REPOS_TIME = 2;

    public Transform target;
   // public Vector3 offset;
    private Vector3 currentOffset;
    private Vector3 targetOffset;
    private Vector3 fromOffset;

    public float currentReposTime = 0;

    public void Start()
    {
        Initiate();
    }

    public void Initiate()
    {
        if (Global.IsAwake)
        {
            currentOffset = Global.CAMERA_DISTANCE;
            targetOffset = currentOffset;
        }
        else
        {
            Global.initiates.Add(this);
        }
    }

    public void SetTarget(Vector3 target)
    {
        targetOffset = target;
        currentReposTime = 0;
        fromOffset = currentOffset;
    }

    // Update is called once per frame
    void Update () {
        if (Global.IsAwake)
        {
            if (targetOffset != currentOffset)
            {
                float t = currentReposTime / CAMERA_REPOS_TIME;
                t = t * t * t * (t * (6f * t - 15f) + 10f);

                currentOffset = Vector3.Lerp(fromOffset, targetOffset, t);

                currentReposTime += Time.deltaTime;
            }


            if (transform.position != target.position + currentOffset)
            {
                transform.position = target.position + currentOffset;
            }
            transform.LookAt(target);
        }
    }
}

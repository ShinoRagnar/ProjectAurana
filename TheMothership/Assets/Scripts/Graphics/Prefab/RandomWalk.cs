using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RandomWalk : MonoBehaviour {



    public Vector3 current;
    public Vector3 direction = Vector3.zero;
    float currentTime = 0;

    public float duration = 2;
    public float moveFactor = 1;
    public bool forward = true;

    public void OnEnable()
    {
        current = this.transform.localPosition;
        direction = current+new Vector3(Random.Range(0f, 1f) * moveFactor, Random.Range(0f, 1f) * moveFactor, Random.Range(0f, 1f) * moveFactor);
    }

    public void Update()
    {
        if (forward && currentTime > duration)
        {
            forward = false;
        }
        if (!forward && currentTime < 0)
        {
            forward = true;
        }

        currentTime += (forward? 1 : -1)*Time.deltaTime;

        float t = currentTime / duration;

        transform.localPosition = Vector3.Lerp(current, direction, t);

    }
}

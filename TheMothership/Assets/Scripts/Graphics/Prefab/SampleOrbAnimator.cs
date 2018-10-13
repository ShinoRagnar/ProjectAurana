using UnityEngine;
using System.Collections;

public class SampleOrbAnimator : MonoBehaviour
{
    public float Delay = 5.0f;

    OrbColor[] orbColors;

    float startTime = 0.0f;
    int currentIndex = -1;

    void Awake()
    {
        orbColors = GetComponents<OrbColor>();
        currentIndex = orbColors.Length - 1;
    }

    void Update()
    {
        if(Time.time > startTime + Delay)
        {
            startTime = Time.time;
            orbColors[currentIndex].enabled = false;
            currentIndex += 1;
            currentIndex %= orbColors.Length;
            orbColors[currentIndex].enabled = true;
        }
    }
}

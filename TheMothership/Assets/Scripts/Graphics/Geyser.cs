using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Geyser : MonoBehaviour {

    public ParticleSystem steam;
    public ParticleSystem splash;
    public ParticleSystem gas;

    public float delayBeforeRelease = 10;
    public float releaseDuration = 5;
    public Vector2 randomVariance = new Vector2(0.5f, 2);

    private float thisRoundDelayBeforeRelease;
    private float thisRoundReleaseDuration;

    private bool isReleasing = false;

    private float currentWaitDuration = 0;
    private float currentReleaseDuration = 0;

    public void Start()
    {
        NewRound();
        steam.Stop();
        splash.Stop();
    }

    public void Update()
    {
        if (isReleasing)
        {
            currentReleaseDuration += Time.deltaTime;

   
            if (currentReleaseDuration > thisRoundReleaseDuration)
            {
                isReleasing = false;
                currentReleaseDuration = 0;
                currentWaitDuration = 0;

                NewRound();

                steam.Stop(true);
            }

        }
        else {

            currentWaitDuration += Time.deltaTime;

            if (currentWaitDuration > thisRoundDelayBeforeRelease)
            {
                gas.transform.localScale = new Vector3(1, 1, GetRandom(1, randomVariance));

                isReleasing = true;

                steam.Play(true);
                splash.Clear(true);
                splash.Play(true);
            }
        }

    }

    public void NewRound()
    {
        thisRoundReleaseDuration = GetRandom(releaseDuration, randomVariance);
        thisRoundDelayBeforeRelease = GetRandom(delayBeforeRelease, randomVariance);
    }

    public float GetRandom(float val, Vector2 variance) {

        return Random.Range(variance.x, variance.y) * val;
    }
}

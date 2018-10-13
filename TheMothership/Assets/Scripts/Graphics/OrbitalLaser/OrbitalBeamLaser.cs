using UnityEngine;
using System.Collections;

public class OrbitalBeamLaser : MonoBehaviour {

public GameObject LaserEffects;
public ParticleSystem LaserSparks;
public ParticleSystem LaserSmoke;
public AudioSource LaserChargeAudio;
public AudioSource LaserAudio;
public AudioSource LaserStopAudio;
public GameObject LaserChargeBeam;
public GameObject SmokeAndSparks;
public GameObject ScorchMark;

private int LaserChargeFlag = 0;

/*void Start (){

    // Reset and stop all effects and audio

    LaserEffects.SetActive(false);

    LaserChargeBeam.SetActive(false);
    SmokeAndSparks.SetActive(false);

    LaserChargeAudio.Stop();
    LaserAudio.Stop();
    LaserStopAudio.Stop();
 
}*/

    public void OnEnable()
    {
        LaserEffects.SetActive(false);

        LaserChargeBeam.SetActive(false);
        SmokeAndSparks.SetActive(false);

        LaserChargeAudio.Stop();
        LaserAudio.Stop();
        LaserStopAudio.Stop();


        LaserChargeFlag = 0;
        LaserChargeAudio.Play();
        LaserChargeBeam.SetActive(true);
        StartCoroutine("LaserChargeWait");

    }

    /*
    void Update (){

    // Fire laser when left mouse button is pressed

    if (Input.GetButtonDown("Fire1"))
    {

        LaserChargeFlag = 0;
        LaserChargeAudio.Play();
        LaserChargeBeam.SetActive(true);
	    StartCoroutine("LaserChargeWait");

    }

    // Stop laser if left mouse button is released

    if (Input.GetButtonUp("Fire1"))
    {

        LaserChargeFlag = 1;
        LaserEffects.SetActive(false);
		LaserSparks.Stop();
		LaserSmoke.Stop();
        LaserAudio.Stop();
        LaserStopAudio.Play();
        LaserChargeBeam.SetActive(false);

    }

}*/

    IEnumerator LaserChargeWait()
    {

        // Wait for laser to charge
        yield return new WaitForSeconds(1.4f);

        if (LaserChargeFlag == 0)
        {
            LaserEffects.SetActive(true);
            SmokeAndSparks.SetActive(true);
            LaserSparks.Play();
            LaserSmoke.Play();

            LaserAudio.Play();
            ScorchMark.SetActive(true);
            LaserChargeFlag = 0;
        }

        yield return new WaitForSeconds(3f);

        LaserChargeFlag = 1;
        LaserEffects.SetActive(false);
        LaserSparks.Stop();
        LaserSmoke.Stop();
        LaserAudio.Stop();
        LaserStopAudio.Play();
        LaserChargeBeam.SetActive(false);
    }
}
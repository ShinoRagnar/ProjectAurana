using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OpenDoor : MonoBehaviour {


    public Transform doorOne;
    public Transform doorTwo = null;
    public GameObject light;
    public GameObject ground;

    public Vector3 gotoLocalDoorOne;
    public Vector3 gotoLocalDoorTwo;

    public Vector3 rotationToDoorOne;
    public Vector3 rotationToDoorTwo;

    private Vector3 gotoDoorOne;
    private Vector3 gotoDoorTwo;

    private float currentOpenDuration = 0;
    public float openDuration = 1;

    public bool opening = false;

    Vector3 doorOneStartPos;
    Vector3 doorTwoStartPos;

    Vector3 doorOneStartRotation;
    Vector3 doorTwoStartRotation;

    private float intensity = 1;
    private float alphaInside;
    private float alphaOutside;

    Light chosenLight;
    VLB.VolumetricLightBeam volumetricLightBeam;


    public void Start()
    {

        doorOneStartRotation = doorOne.localEulerAngles;
        doorTwoStartRotation = doorTwo.localEulerAngles;

        doorOneStartPos = doorOne.localPosition;
        doorTwoStartPos = doorTwo.localPosition;

        gotoDoorOne = doorOneStartPos + gotoLocalDoorOne;
        gotoDoorTwo = doorTwoStartPos + gotoLocalDoorTwo;

        if(light != null)
        {
            chosenLight = light.GetComponent<Light>();
            if(chosenLight != null)
            {
                intensity = chosenLight.intensity;
                volumetricLightBeam = light.GetComponent<VLB.VolumetricLightBeam>();

                chosenLight.intensity = 0;

                if (volumetricLightBeam != null)
                {
                    alphaInside = volumetricLightBeam.alphaInside;
                    alphaOutside = volumetricLightBeam.alphaOutside;

                    volumetricLightBeam.alphaInside = 0;
                    volumetricLightBeam.alphaOutside = 0;
                }
            }

        }


    }

    public void Update()
    {
        currentOpenDuration = Mathf.Clamp(currentOpenDuration+(opening ? 1 : -1)*Time.deltaTime,0,openDuration);

        float t = currentOpenDuration / openDuration;
        bool changed = false;

        if((opening && doorOne.localPosition != gotoDoorOne) || (!opening && doorOne.localPosition != doorOneStartPos))
        {
        doorOne.localPosition = Vector3.Lerp(doorOneStartPos, gotoDoorOne, t);
        doorOne.localEulerAngles = Vector3.Lerp(doorOneStartRotation, rotationToDoorOne, t);
            changed = true;


        }

        if (doorTwo != null && (opening && doorTwo.localPosition != gotoDoorTwo) || (!opening && doorTwo.localPosition != doorTwoStartPos))
        {
        doorTwo.localPosition = Vector3.Lerp(doorTwoStartPos, gotoDoorTwo, t);
        doorTwo.localEulerAngles = Vector3.Lerp(doorTwoStartRotation, rotationToDoorTwo, t);
            changed = true;
        }

        if (changed)
        {
            if (chosenLight != null)
            {
                chosenLight.intensity = t * intensity;
            }
            if (volumetricLightBeam != null)
            {
               
                volumetricLightBeam.alphaInside = t * alphaInside;
                volumetricLightBeam.alphaOutside = t * alphaOutside;
            }
        }



    }



    public void OnTriggerEnter(Collider col)
    {
        if (col != null)
        {
            GameUnitBodyComponent gb = col.gameObject.GetComponent<GameUnitBodyComponent>();
            //Debug.Log(col.gameObject.name);

            if (gb != null)
            {
               // Debug.Log("Opening door");
                opening = true;
                ground.SetActive(false);
            }
        }
    }

    private void OnTriggerExit(Collider col)
    {
        if (col != null)
        {
            GameUnitBodyComponent gb = col.gameObject.GetComponent<GameUnitBodyComponent>();
            //Debug.Log(col.gameObject.name);

            if (gb != null)
            {
                //Debug.Log("Closing door");
                opening = false;
                ground.SetActive(true);
            }
        }
    }
}

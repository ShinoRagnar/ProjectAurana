using UnityEngine;
using System.Collections;

public class FlickeringLight : MonoBehaviour {

public Light fuseLight;
private int fuseLightIntensity = 10;

void Start (){

}

void Update (){

    fuseLightIntensity = (Random.Range (5, 14));
    fuseLight.intensity = fuseLightIntensity;

}
}
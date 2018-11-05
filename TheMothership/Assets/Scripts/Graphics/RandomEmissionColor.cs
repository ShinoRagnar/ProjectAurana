using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RandomEmissionColor : MonoBehaviour {

    // Use this for initialization
    public Color[] randomRange = new Color[2];
    public Color[] randomLightRange = new Color[2];

    public Light light;

    void Start () {

        float rnd = Random.Range(0, 1f);

        Color randomEmission = Color.Lerp(randomRange[0], randomRange[1], rnd);
        Color randomLight = Color.Lerp(randomLightRange[0], randomLightRange[1], rnd);

        MeshRenderer mr = transform.GetComponent<MeshRenderer>();
        Material m = new Material(mr.material);
        m.SetColor("_EmissionColor", randomEmission);
        mr.material = m;

        light.color = randomLight;
    }
	

}

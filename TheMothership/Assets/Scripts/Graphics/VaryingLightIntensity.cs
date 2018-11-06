using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VaryingLightIntensity : MonoBehaviour {

    public bool varyEmission = true;
    public Color varyEmissionTo;
    public Light light;
    public float randomTime = 2f;
    public float minIntensity = 0.5f;
    public float variableIntensity = 0.5f;

    private float currentTime = 0;
    private float currentGoal = 1;
    private bool returning = false;

    private Color startColor;
    private Material mat;

    private void Start()
    {
        currentGoal = Random.Range(randomTime / 2f, randomTime);
        currentTime = Random.Range(currentGoal / 2f, currentGoal);

        if (varyEmission)
        {
            mat = transform.GetComponent<MeshRenderer>().material;
            startColor = mat.GetColor("_EmissionColor");
        }
    }
    // Update is called once per frame
    void Update() {




        if (returning)
        {
            currentTime -= Time.deltaTime;
            if (currentTime < 0) {
                currentGoal = Random.Range(randomTime / 2f, randomTime);
                currentTime = 0;
                returning = false;
            }
        }
        else {
            currentTime += Time.deltaTime;
            if (currentTime > currentGoal)
            {
                returning = true;
            }
        }

        float t = Mathf.Clamp01(currentTime / currentGoal);
        t = t * t * t * (t * (6f * t - 15f) + 10f);

        light.intensity = minIntensity + variableIntensity * t;

        if (varyEmission)
        {
            mat.SetColor("_EmissionColor", Color.Lerp(startColor, varyEmissionTo, 1f-t));
        }


    }
}

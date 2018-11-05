using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RandomScale : MonoBehaviour {

    public float randomMinScale = 1;
    public float randomMaxScale = 1;

    // Use this for initialization
    void Start () {
        float scale = Random.Range(randomMinScale, randomMaxScale);

        this.transform.localScale = new Vector3(scale, scale, scale);

	}
}

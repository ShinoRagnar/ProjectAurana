using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StartAnimationAtRandomtime : MonoBehaviour {

    public float random = 20;

    // Use this for initialization
    void Start() {
        transform.GetComponent<Animator>().Update(Random.Range(0, random));
	}
	

}

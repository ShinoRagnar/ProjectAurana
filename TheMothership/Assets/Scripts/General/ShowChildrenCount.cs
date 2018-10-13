using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShowChildrenCount : MonoBehaviour {

    private string objectName;
    private int lastCount;

	// Use this for initialization
	void Start () {
        objectName = this.gameObject.name;
	}
	
	// Update is called once per frame
	void Update () {
        int newCount = this.transform.childCount;
        if (newCount != lastCount)
        {
            this.gameObject.name = objectName + ": " + newCount;
        }
        lastCount = newCount;
	}
}

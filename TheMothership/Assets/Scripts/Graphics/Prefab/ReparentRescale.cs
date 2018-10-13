using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ReparentRescale : MonoBehaviour {

    public Transform parent;

	void Start () {
		if(parent != null)
        {
            transform.parent = parent;
        }
	}
	
}

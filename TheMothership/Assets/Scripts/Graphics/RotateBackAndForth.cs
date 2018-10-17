using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RotateBackAndForth : MonoBehaviour {

    public Transform searchFrom;
    public Transform searchTo;
    public GameObject search;

    public float duration = 1;
    public float currentDuration = 0;
    public bool back = true;

    // Use this for initialization
    void Start () {
        //currentRot = transform.localEulerAngles;
        currentDuration = 0;
        back = true;

        search = new GameObject();
        search.transform.parent = Global.References[SceneReferenceNames.NodeMarkers];
       
    }
	
	// Update is called once per frame
	void Update () {

        if (back)
        {
            currentDuration += Time.deltaTime;
        }
        else
        {
            currentDuration -= Time.deltaTime;
        }

        if(currentDuration > duration)
        {
            back = false;
        }else if(currentDuration < 0)
        {
            back = true;
        }

        float progress = currentDuration / duration;

        search.transform.position = Vector3.Lerp(searchFrom.position, searchTo.position, progress);

        transform.LookAt(search.transform);
		
	}
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SinkChildrenIntoGround : MonoBehaviour {

    public float duration = 2;
    public float currentDuration = 0;
    public float currentSinkDuration = 0;
    public float sinkDuration = 2;
    public float sinkDepth = 0.2f;

    public bool startedSinking = true;

    public DictionaryList<Transform, Vector3> children = new DictionaryList<Transform, Vector3>();

    public void Start()
    {
        currentDuration = 0;
        currentSinkDuration = 0;
    }
    // Update is called once per frame
    void Update () {
		if(currentDuration < duration)
        {
            currentDuration += Time.deltaTime;
            startedSinking = true;
        }
        else
        {
            if (startedSinking)
            {
                Rigidbody[] rb = GetComponentsInChildren<Rigidbody>();
                foreach(Rigidbody r in rb)
                {
                    Destroy(r);
                }
                startedSinking = false;
                foreach(Transform t in transform)
                {
                    children.AddIfNotContains(t, t.position);
                }
            }
            if(currentSinkDuration < sinkDuration)
            {
                currentSinkDuration += Time.deltaTime;

                float prcntg = currentSinkDuration / sinkDuration;

                foreach(Transform child in children)
                {
                    child.position = Vector3.Lerp(children[child], children[child] + new Vector3(0, -sinkDepth), prcntg);
                }
                
            }
            else
            {
                Global.removeMe.AddIfNotContains(this.gameObject);
            }
        }

	}
}

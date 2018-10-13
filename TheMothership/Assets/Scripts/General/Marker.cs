using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Marker{

    public GameObject marker;

	public Marker(Vector3 pos, bool alwaysInvisible)
    {
        marker = new GameObject();
        marker.transform.parent = Global.References[SceneReferenceNames.NodeMarkers];// Global.instance.X_MARKERS;
        if (!alwaysInvisible && DevelopmentSettings.SHOW_MARKERS)
        {
            GameObject s = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            s.GetComponent<SphereCollider>().enabled = false;
            s.transform.parent = marker.transform;
        }
        marker.transform.position = pos;
    }
}

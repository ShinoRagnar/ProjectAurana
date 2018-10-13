using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CornerCollisionDetector : MonoBehaviour {

    public CornerCollidable receiver = null;

    private void OnTriggerEnter(Collider col)
    {
        if(receiver != null)
        {
            //col.gameObject.isStatic
            receiver.CornerCollided(transform.position, col.gameObject.isStatic);
        }
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameEventTrigger : MonoBehaviour {

    public GameEvent triggerEvent;

    public void OnTriggerEnter(Collider other)
    {
        if(other != null)
        {
            GameUnitBodyComponent gbc = other.gameObject.GetComponent<GameUnitBodyComponent>();

            if(gbc != null)
            {
                Global.TriggerEnter(gbc.owner, this.gameObject, triggerEvent);
            }
        }
    }

    public void OnTriggerExit(Collider other)
    {
        if (other != null)
        {
            GameUnitBodyComponent gbc = other.gameObject.GetComponent<GameUnitBodyComponent>();

            if (gbc != null)
            {
                Global.TriggerExit(gbc.owner, this.gameObject, triggerEvent);
            }
        }
    }
}

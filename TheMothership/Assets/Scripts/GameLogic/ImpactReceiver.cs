using UnityEngine;

using System.Collections;

public class ImpactReceiver : GameUnitBodyComponent
{
    public float mass = 3.0F; // defines the character mass
    Vector3 impact = Vector3.zero;

    private CharacterController character;
   // LegMovement pm;

    //public GameUnit owner;

    // Use this for initialization
   /* void Start()
    {

        //character = GetComponent<CharacterController>();
        //pm = GetComponent<LegMovement>();
        //if(pm != null) { 
        //    owner = pm.owner;
        //}
    }*/

    // Update is called once per frame
    void Update()
    {
        // apply the impact force:
        if (impact.magnitude > 0.2F)
        {
           // if(owner.)
            //owner.impact.AddImpact()
            if(owner.movement != null)
            {
                
                owner.movement.AddImpact(owner.uniqueName, impact);
            }
            else
            {
                character.Move(impact * Time.deltaTime);
            }       
        }
        // consumes the impact energy each cycle:
        impact = Vector3.Lerp(impact, Vector3.zero, 5 * Time.deltaTime);
    }

    // call this function to add an impact force:
    public void AddImpact(string source, Vector3 dir, float force)
    {
        if (DevelopmentSettings.ENABLE_CAMERA_SHAKE)
        {
            //Only shake on three times force than mass
            if (owner.isPlayer && (force / mass) > 3)
            {
                float multip = Mathf.Min((force / mass) / 10, 2);
                CameraRumbler.Instance.ShakeOnce(2*multip, 2*multip, 0.1f*multip, 0.5f*multip);
            }
        }

        //Debug.Log(force / mass);
        //CameraRumbler.Instance.ShakeOnce(4, 4, 0.1f, 1);

        dir.Normalize();
        if (dir.y < 0) dir.y = -dir.y; // reflect down force on the ground
        impact += dir.normalized * force / mass;

       // Debug.Log("Impacting: " + owner.uniqueName + " " + dir.ToString()+" "+force);
    }
}
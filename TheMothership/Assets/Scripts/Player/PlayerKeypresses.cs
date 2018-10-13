using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerKeypresses : AIBasic {

    public static readonly float CROSSHAIR_Z_DISTANCE = -1F;

    static Plane ZPlane = new Plane(new Vector3(0, 0, -1), Vector3.zero);

    public static Vector3 GetMousePositionOnZPlane(Vector3 mousePosition)
    {
        float distance;
        Ray ray = Camera.main.ScreenPointToRay(mousePosition);
        if (ZPlane.Raycast(ray, out distance))
        {
            Vector3 hitPoint = ray.GetPoint(distance);
            //Just double check to ensure the y position is exactly zero
            hitPoint.z = 0;
            return hitPoint;
        }
        return Vector3.zero;

    }
    // Update is called once per frame
    void Update () {
		
        if(owner != null)
        {

            bool jumping = Input.GetButton("Jump");
           // bool thrusters = false;
            float axis = Input.GetAxis("Horizontal");
            bool horizontal = Input.GetButtonDown("Horizontal");
            bool turn = Input.GetButton("Fire3");
            bool transform = Input.GetKeyUp(KeyCode.T);
           // bool block = Input.GetKey(KeyCode.Q);
           // bool attack = shooting;
            float actionspeed = owner.stats.GetCurrentValue(Stat.ActionSpeed);
            
            if (Global.Console.Showing)
            {
                owner.movement.UpdateMovement(false, 0, false, false ,actionspeed);
            }
            else
            {
                //Movement
                owner.movement.UpdateMovement(
                    jumping,
                    axis,
                    transform,//owner.movement.DetectDoubleTap(horizontal, axis > 0, axis < 0),
                    turn,
                    actionspeed
                    );

                if(Global.Inventory.selectedGameUnits.Count > 0)
                {
                    target = Global.Inventory.selectedGameUnits.Get(0);
                }
                else
                {
                    target = null;
                }

                Vector3 mousePos = Input.mousePosition;
                

                owner.mech.Tick(actionspeed, mousePos, GetMousePositionOnZPlane(mousePos), target);

            }
        }
	}


}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FootstepEventReceiver : MonoBehaviour {

    public static readonly string FOOTSTEP = "Footstep";

    public static readonly int FRONT_RIGHT = 1;
    public static readonly int FRONT_LEFT = 2;
    public static readonly int BACK_RIGHT = 3;
    public static readonly int BACK_LEFT = 4;



    public AnimationEventReceiver receiver;

    public void Footstep(int i)
    {
        if(receiver != null)
        {
            PointOfInterest pos = PointOfInterest.FrontRightFoot1;
            if(i == FRONT_LEFT)
            {
                pos = PointOfInterest.FrontLeftFoot1;
            }else if (i == BACK_RIGHT)
            {
                pos = PointOfInterest.BackRightFoot1;
            }else if (i == BACK_LEFT)
            {
                pos = PointOfInterest.BackLeftFoot1;
            }
            receiver.Receive(FOOTSTEP,pos);
        }
    }
}

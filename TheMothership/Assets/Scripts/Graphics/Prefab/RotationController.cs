using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RotationController : MonoBehaviour {


	private float currentRotation = 0;
    public float turnDuration = 2;
    public float turnSpeed = 1;
    private bool clockWise = true;

    public bool xAxis = false;
    public bool yAxis = false;
    public bool zAxis = false;

    public bool backandforward = false;
    public bool rotateOnce = false;
    private bool rotatedOnce = false;
    // public int rotateTimes = 0;
    // private int rotate = 0;

    private void OnEnable()
    {
        currentRotation = 0;
        rotatedOnce = false;
        clockWise = true;
   //     rotate = 0;
    }

    // Update is called once per frame
    void Update () {
        //if(rotateTimes == 0 || rotate < rotateTimes)
        //{
        if(!rotateOnce || !rotatedOnce)
        {
            if (currentRotation > turnDuration)
            {
                rotatedOnce = true;
                clockWise = false;
                //    rotate++;
            }
            else if (currentRotation < 0)
            {
                rotatedOnce = true;
                clockWise = true;
                //  rotate++;
            }

            float t = Mathf.Min(Mathf.Max(currentRotation / turnDuration, 0), 1);
            t = t * t * t * (t * (6f * t - 15f) + 10f);

            if (clockWise)
            {
                currentRotation += Time.deltaTime * turnSpeed;
            }
            else
            {
                currentRotation -= Time.deltaTime * turnSpeed;
            }

            if (backandforward)
            {
                float g = Mathf.Lerp(-345, 345, t);

                this.transform.eulerAngles = new Vector3(
                     xAxis ? g : 0,
                     yAxis ? g : 0,
                     zAxis ? g : 0);
            }
            else
            {
                this.transform.Rotate(
                    xAxis ? Time.deltaTime * turnSpeed : 0,
                    yAxis ? Time.deltaTime * turnSpeed : 0,
                    zAxis ? Time.deltaTime * turnSpeed : 0
                    );
            }
        }

        //}
    }
}

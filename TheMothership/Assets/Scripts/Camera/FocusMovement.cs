using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FocusMovement : MonoBehaviour {

    public static readonly float CHASE_TIME = 2;
    public static readonly float MIN_BOUNDS = 7;

    //Tracking
    public GameUnit owner;

    
    public bool visible;

    private float timeOffScreen;
    private float toTheRightX;
    private float toTheLeftX;
    private float upY;
    private float downY;

    private Vector3 playerScaleLastFrame = Vector3.zero;
    public Vector3 snapShotPlayerRelativeCameraPos = Vector3.zero;
    bool outOfBounds = false;

    void Start(){
        timeOffScreen = 0;
        if (!visible){
            GetComponent<MeshRenderer>().enabled = false;
        }
    }

    private Vector3 playerPositionLastFrame = Vector3.zero;

    void Update() {

        if(owner.body.localScale != playerScaleLastFrame)
        {
            transform.localScale = new Vector3(
                                        Mathf.Max(MIN_BOUNDS, owner.body.localScale.x*2), 
                                        Mathf.Max(MIN_BOUNDS, owner.body.localScale.y*2),
                                        -0.9f);
        }

        float width = transform.localScale.x;
        float height = transform.localScale.y;
        float playerWidth = owner.body.transform.localScale.x;
        float playerHeight = owner.body.transform.localScale.y;

        toTheRightX = (owner.body.position.x - (playerWidth / 2)) - (transform.position.x + (width / 2));
        toTheLeftX = (transform.position.x - (width / 2)) - (owner.body.position.x + (playerWidth / 2));
        upY = (owner.body.position.y - (playerHeight / 2))  - (transform.position.y + (height / 2));
        downY = (transform.position.y - (height / 2)) - (owner.body.position.y + (playerHeight / 2));

        if (!outOfBounds && (toTheRightX > 0 || toTheLeftX > 0 || upY > 0 || downY > 0))
        {
            outOfBounds = true;
            snapShotPlayerRelativeCameraPos = owner.body.position - transform.position;
        }

        if (outOfBounds)
        {
            timeOffScreen += Time.deltaTime;
        }

        if (snapShotPlayerRelativeCameraPos != Vector3.zero)
        {
            float t = timeOffScreen / CHASE_TIME;
            //Ease out
            t = Mathf.Sin(t * Mathf.PI * 0.5f);

            if (timeOffScreen > CHASE_TIME)
            {
                this.transform.position = owner.body.position;

                //Stop chasing if we haven't moved
                if(playerPositionLastFrame == owner.body.position) {
                    outOfBounds = false;
                    timeOffScreen = 0;
                    snapShotPlayerRelativeCameraPos = Vector3.zero;
                }
            }
            else
            {
                this.transform.position = owner.body.position - Vector3.Lerp(snapShotPlayerRelativeCameraPos, Vector3.zero, t);
            }

            playerPositionLastFrame = owner.body.position;
        }

        playerScaleLastFrame = owner.body.localScale;
    }
   /* void setGreen() {
        rend.material.shader = Shader.Find("_Color");
        rend.material.SetColor("_Color", Color.green);
        rend.material.shader = Shader.Find("Specular");
        rend.material.SetColor("_SpecColor", Color.green);
    }
    void setRed(){
        rend.material.shader = Shader.Find("_Color");
        rend.material.SetColor("_Color", Color.red);
        rend.material.shader = Shader.Find("Specular");
        rend.material.SetColor("_SpecColor", Color.red);
    }
    void setYellow()
    {
        rend.material.shader = Shader.Find("_Color");
        rend.material.SetColor("_Color", Color.yellow);
        rend.material.shader = Shader.Find("Specular");
        rend.material.SetColor("_SpecColor", Color.yellow);
    }*/
}

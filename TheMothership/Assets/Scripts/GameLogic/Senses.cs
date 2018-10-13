using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum Sense
{
    Seeing,
    Hearing
}
public class Senses {

    public struct Hit
    {
        public RaycastHit hit;
        public GameUnit unit;
        public Vector3 pos;
        public bool didHit;

        public Hit(RaycastHit hitVal, GameUnit hitUnit, Vector3 posVal)
        {
            hit = hitVal;
            unit = hitUnit;
            pos = posVal;
            didHit = true;
        }
    }

    public static int MAX_RAY_CASTS_WHEN_TRY_TO_SEE = 3;

    public float visionRangeX;
    public float visionRangeY;
    public float hearingRangeX;
    public float hearingRangeY;

    private float reactionTimeMax;
    private float reactionTime;
   

    public GameUnit owner;

	public Senses(float visionRangeXVal, float visionRangeYVal, float hearingRangeXVal, float hearingRangeYVal, float reactionTimeVal)
    {
        this.visionRangeX = visionRangeXVal;
        this.visionRangeY = visionRangeYVal;
        this.hearingRangeX = hearingRangeXVal;
        this.hearingRangeY = hearingRangeYVal;
        this.reactionTimeMax = reactionTimeVal;

    }
    public float GetReactionTime()
    {
        if(this.reactionTime == 0 && reactionTimeMax != 0)
        {
            this.reactionTime = reactionTimeMax / 2f+ (float)Global.instance.rand.NextDouble() * reactionTimeMax / 2f;
            //Debug.Log(reactionTime*1000f);
        }
        return reactionTime;
    }

    public bool CanSee(GameUnit target)
    {
        return SeeOrHear(Sense.Seeing, target,true);
    }
    public bool CanHear(GameUnit target)
    {
        return SeeOrHear(Sense.Hearing, target,true);

    }
    public static Ground GetGroundBelow(Vector3 fromPosition, float distance)
    {
        Hit hit = SeeGroundBelow(fromPosition, distance);
        if(hit.didHit)
        {
            if(hit.hit.collider.transform != null)
            {
                return Global.Grounds[hit.hit.collider.transform];
            }
        }
        return null;
    }
    public static Hit SeeGroundBelow(Vector3 fromPosition, float distance = Mathf.Infinity){
        int layerMask = LayerMask.GetMask(Global.LAYER_GROUND);
        RaycastHit hit;
        if (Physics.Raycast(fromPosition, Vector3.down, out hit, distance, layerMask))
        {
            return new Hit(hit,null,hit.point);
        }
        return new Hit();
    }


    public Hit TryToHit(Vector3 fromPosition, GameUnit target, int tries, bool hitShields)
    {
        RaycastHit hit;
        Hit othr = new Hit(); 

        for(int i = 0; i < tries; i++) {

            Vector3 toPosition = target.GetCenterPos();

            if (hitShields)
            {
                toPosition += Random.insideUnitSphere * target.shield.gameObject.transform.localScale.x;
            }
            else
            {
                toPosition = GetRandomPointFrom(fromPosition, target, hitShields); //targetBody.position;
            }

            Vector3 direction = toPosition - fromPosition;


            int layerMask;

            if (hitShields)
            {
                layerMask = LayerMask.GetMask(Global.LAYER_SHIELDS);
            }
            else
            {
                layerMask = LayerMask.GetMask(Global.LAYERS_NONE_SHIELD_GAME_OBJECTS);
            }

            if (Physics.Raycast(fromPosition, direction, out hit, layerMask))
            {
                GameUnitBodyComponent co = hit.collider.transform.gameObject.GetComponent<GameUnitBodyComponent>();
                if (co != null)
                {
                    if (co.owner == target)
                    {
                        //Debug.DrawRay(fromPosition, direction, Color.green);
                        return new Hit(hit,target,hit.point);
                    }
                    else
                    {
                        othr = new Hit(hit, target, hit.point);
                    }
                }
            }
        }
        return othr;
        //return Vector3.negativeInfinity;
    }
    public Vector3 GetRandomPointFrom(Vector3 fromPosition, GameUnit target, bool hitShield = false)
    {
        float x = target.body.position.x;
        float y = target.body.position.y;
        float z = target.body.position.z;


        bool isMech = target.mech != null;

        //Aim for roof if above
        if (target.body.position.y + target.body.localScale.y < fromPosition.y)
        {
            if (isMech && !hitShield)
            {
                return target.mech.GetRandomSurfacePointFrom(Orientation.Above);
            }
            else
            {
                x += (float)Global.instance.rand.NextDouble() * target.body.localScale.x - target.body.localScale.x / 2;
                y += target.body.localScale.y / (isMech ? 2 : 1);
            }
        }
        else if (target.body.position.x < fromPosition.x)
        {
            if (isMech && !hitShield)
            {
                return target.mech.GetRandomSurfacePointFrom(Orientation.ToTheRightOf);
            }
            else
            {
                x += target.body.localScale.x / (isMech ? 2 : 1); ;
            }
        }
        else
        {
            if (isMech && !hitShield)
            {
                return target.mech.GetRandomSurfacePointFrom(Orientation.ToTheLeftOf);
            }
            else
            {
                x -= target.body.localScale.x / (isMech ? 2 : 1); ;
            }
        }
        //Aim for top half if  below
        if (target.body.position.y - target.body.localScale.y > fromPosition.y)
        {
            y += target.body.localScale.y / (isMech ? 2 : 1) - (float)Global.instance.rand.NextDouble() * target.body.localScale.y / (isMech ? 2 : 1);
        }
        else if (y == target.body.position.y)
        {
            y += (float)Global.instance.rand.NextDouble() * target.body.localScale.y - target.body.localScale.y / (isMech ? 2 : 1);
        }

        z += (float)Global.instance.rand.NextDouble() * target.body.localScale.z - target.body.localScale.z / (isMech ? 2 : 1);;

        return new Vector3(x, y, z);
    }

    protected bool SeeOrHear(Sense sens, GameUnit target, bool seeShields)
    {
        Transform selfBody = owner.body;
        Transform targetBody = target.body;

        float modifierX = visionRangeX;
        float modifierY = visionRangeY;

        if (sens == Sense.Hearing)
        {
            modifierX = hearingRangeX;
            modifierY = hearingRangeY;
        }

        if (
                (
                    (selfBody.forward.x < 0 || sens == Sense.Hearing)
                    && targetBody.transform.position.x < selfBody.transform.position.x
                    && targetBody.transform.position.x + modifierX > selfBody.position.x
                )
                ||
                (
                    (selfBody.forward.x > 0 || sens == Sense.Hearing)
                    && targetBody.transform.position.x > selfBody.transform.position.x
                    && targetBody.transform.position.x - modifierX < selfBody.position.x
                )
            )
        {
            if (
                (targetBody.transform.position.y < selfBody.transform.position.y
                    && targetBody.transform.position.y + modifierY > selfBody.position.y
                )
                ||
                (
                    targetBody.transform.position.y > selfBody.transform.position.y
                    && targetBody.transform.position.y - modifierY < selfBody.position.y
                )
            )
            {
                if (sens == Sense.Seeing)
                {
                    Vector3 fromPosition = selfBody.position;
                    fromPosition.y += selfBody.localScale.y*2; //2; //owner.headYOffset;
                    return TryToHit(fromPosition, target, MAX_RAY_CASTS_WHEN_TRY_TO_SEE, seeShields).unit != null;

                    /*Vector3 toPosition = GetRandomPointOn(owner.body.position,target); //targetBody.position;
                    Vector3 direction = toPosition - fromPosition;
                    RaycastHit hit;
                    


                    // Casts a ray against colliders in the scene
                    if (Physics.Raycast(fromPosition, direction, out hit))
                    {
                        ColliderOwner co = hit.transform.gameObject.GetComponent<ColliderOwner>();
                        if(co != null)
                        {
                            if(co.owner == target)
                            {
                                Debug.DrawRay(fromPosition, direction, Color.green);
                                return true;
                            }
                        }
                    }*/
                }
                else
                {
                    return true;
                }
            }
        }
        return false;
    }
    public Senses Clone()
    {
        return new Senses(visionRangeX, visionRangeY, hearingRangeX, hearingRangeY,reactionTimeMax);
    }
}

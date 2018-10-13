using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface Targeter
{
    GameUnit GetTarget();
    Transform GetTargetTransform();
    float GetLaunchAngle();
    Vector3 GetJointPos();

    //void LaunchProjectileAtTarget(Transform from);

}
public enum TargetingStage
{
    Idle = 0,
    TargetAcquired = 10,
   // TargetLocked = 20,
    ReadyForLaunch = 30
}
public class TurretTargeter : Targeter{

    //Statics
    public static readonly int LINE_ARRAY_BUFFER = 100; //5000;


    public static readonly float MAX_ANGLE_DISTANCE = 90;
    public static readonly float MAX_ANGLE_DIFFERENCE = 5;
    public static readonly float ROTATION_SPEED = 500;
    public static readonly float SMOOTH = 1.0f;
    public static readonly float TARGETING_START_SPEED = 1f;
    public static readonly float TARGETING_MAX_SPEED = 15f;
    public static readonly float TARGETING_ACCELERATION_SPEED = 2f;
 //   public static readonly float TIME_UNDER_TARGET_BEFORE_TARGET_LOCK = Gun.MAX_REACTION_TIME+ 0.25f;
 //   public static readonly float TIME_WHEN_LOCKED_BEFORE_LAUNCH = Gun.MAX_REACTION_TIME + 1f;
    public static readonly float PROJECTILE_GRAVITY = 9.8f;
    public static readonly float CURVE_WIDTH = 0.4f;
    public static readonly float STATIC_AIM_DISTANCE_MULTIPLIER = 4;
    public static readonly float PLAYER_INCREASED_TARGETING_SPEED = 2;
    public static readonly float TARGETER_ACCELERATION_FACTOR = 10;

    public static readonly float AIM_HIGH_POINT = 7;

    public static readonly float GROUND_TARGET_DISTANCE = 3;


    // Joint and jointangle
    public Transform joint;
    public float angle = 0;
    // Gun on this joint
    public Shootable shootable;
    public GunAnimator gunAnimator;
    // Adjusted target follows the gameunit
    public Marker adjustedTarget;
    // Simulates the arc
   // public Marker simulator;
    // Varying width depending in state
    public AnimationCurve curve;
    // Hunted target
    public GameUnit gameUnitTarget;
    // Line of the arc
    public LineRenderer line;
    // public bool isTargeting;
    public float currentTargetingSpeed = TARGETING_START_SPEED;
    // public float timeUnderTarget;
    // public float timeUnderLock;
    //Avoid uneccessary updates
    public float currentY;
    public float currentZ;
    public float currentLineWidth;
    public float currentLineGradient;
   /* public Vector3 currentTargetPos;
    public Vector3 currentFromPos;
    public float currentEulerAngle;*/
    //Current targeting stage
    public TargetingStage stage;

    //private List<Vector3> positions = new List<Vector3>();

    private Vector3[] linePositions = new Vector3[LINE_ARRAY_BUFFER];

    public GameUnit owner;


    public GameUnit GetTarget()
    {
        return gameUnitTarget;
    }
    
    public TurretTargeter(GameUnit owner, Shootable gunVal, Transform jointVal)//, Transform prefabValue)
    {
        this.owner = owner;
        this.shootable = gunVal;
        this.joint = jointVal;
        this.angle = 0;
        this.stage = TargetingStage.Idle;

        gunVal.RegisterTargeter(this);

        //Add animator
        this.gunAnimator = shootable.Item().visualItem.gameObject.GetComponent<GunAnimator>();
        this.gunAnimator.targeter = this;
        this.gunAnimator.shootable = shootable;
            
        //this.gunAnimator.EndShooting();
        
        // this.prefab = prefabValue;
        SetCurve(1);
    }

    public Vector3 GetJointPos()
    {
        return joint.position;
    }

    public void Hide()
    {

        if (this.adjustedTarget != null)
        {
            Global.Destroy(this.adjustedTarget.marker);
            adjustedTarget = null;
        }
        /*if (this.simulator != null)
        {
            Global.Destroy(this.simulator.marker);
            simulator = null;
        }*/
        if(line != null)
        {
            Global.Destroy(line);
            line = null;
        }
    }


    public void SetCurve(float time)
    {
        float min = 0.1f;
        float val = min + Mathf.Max(time - min, 0);

        if (val != currentLineWidth)
        {
            this.curve = new AnimationCurve();
            this.curve.AddKey(0.0f, 0.0f);
            this.curve.AddKey(0.25f, val);
            this.curve.AddKey(0.75f, val);
            this.curve.AddKey(1.0f, 0.0f);
            if (line != null)
            {
                line.widthCurve = curve;
            }
        }
        currentLineWidth = val;
    }
    public void AcquireTarget(GameUnit targ, Vector3 mousePosition, Vector3 mousePositionOnZPlane)
    {
        this.gameUnitTarget = targ;
        

        //if ((target != null && owner.senses.CanHear(target)) || owner.isPlayer){ //gun.owner.senses.CanHear(targ)) {

            if(this.adjustedTarget == null) { 
                this.adjustedTarget = new Marker(mousePositionOnZPlane, false);
            }
            /*if(this.simulator == null)
            {
                this.simulator = new Marker(Vector3.zero, true);
            }*/

            this.stage = TargetingStage.TargetAcquired;
           // this.timeUnderTarget = -shootable.GetTimeUntilCanShoot();//gun.reactionTime;
       // }

       // SetLineColor(0);
    }


   /* public Vector3 GetNoTargetPosition(Vector3 mousePosition)
    {
        return GetMousePositionOnZPlane(mousePosition);*/

        /*(shootable.GetBulletType() == BulletType.Mortar ? owner.GetFootPos() : joint.position) 
            + new Vector3(
                owner.body.localScale.x *
                STATIC_AIM_DISTANCE_MULTIPLIER *
                (owner.movement.facing == Facing.Right ? 1 : -1), 0);*/
   // }

    public void AdjustTarget(float actionspeed, Vector3 mousePosition, Vector3 mousePositionOnZPlane)
    {

       // bool adjustedTargetWasUnderTarget = false;

        //Move adjusted target;
        // if(stage == TargetingStage.TargetAcquired) { 

       // float acceleration = TARGETING_ACCELERATION_SPEED * Time.deltaTime;
       // this.currentTargetingSpeed = Mathf.Min(currentTargetingSpeed + acceleration, TARGETING_MAX_SPEED);

        //Easier reference
        Transform a = adjustedTarget.marker.transform;
        Vector3 targ;

        //Only target when there is a target
        if (gameUnitTarget != null && gameUnitTarget.body != null)
        {
            if (shootable.GetBulletTarget() == BulletTarget.GroundInFrontOfTarget && gameUnitTarget.IsGrounded())
            {
                targ = gameUnitTarget.GetFootPos() + new Vector3(((gameUnitTarget.body.localScale.x / 2) + GROUND_TARGET_DISTANCE)
                    * (gameUnitTarget.body.position.x > owner.body.position.x ? -1 : 1), 0);
            }
            else if (shootable.GetBulletTarget() == BulletTarget.GroundUnderTarget)
            {
                targ = gameUnitTarget.GetFootPos();

            }else { 

                targ = shootable.GetBulletType() == BulletType.Mortar ? gameUnitTarget.GetFootPos() : gameUnitTarget.GetHeadPos(); //gameUnitTarget.body.position;
            }
        }
        else
        {
            if (owner.isPlayer)
            {
                targ = mousePositionOnZPlane;
            }
            else
            {
                targ = owner.GetFootPos();
            }
            //a.position;
        }


        adjustedTarget.marker.transform.position = targ;

        //float y = ;

        //Mortars aim at the ground
        /*if (shootable.GetBulletType() == BulletType.Mortar)
        {
            if (gameUnitTarget != null && gameUnitTarget.body != null)
            {
                y -= gameUnitTarget.body.localScale.y / 2;
            }
            else
            {
                y -= owner.body.localScale.y / 2;//Senses.SeeGroundBelow(targ).point.y;
            }
        }*/

        //Move our aim towards the arget
        /*float tick = MoveTarget(
            a.position.x, targ.x, 
            owner.isPlayer ? 
            currentTargetingSpeed* PLAYER_INCREASED_TARGETING_SPEED : currentTargetingSpeed, 
            actionspeed);

        if (tick != 0 || targ.y != currentY || targ.z != currentZ)
        {
            adjustedTarget.marker.transform.position = new Vector3(a.position.x + tick, targ.y, targ.z);
        }

        currentY = targ.y;
        currentZ = targ.z;

        if (gameUnitTarget != null && gameUnitTarget.body != null)
        {
            //Try to lock on target
            if (a.position.x > gameUnitTarget.body.position.x - gameUnitTarget.body.localScale.x / 2
            && a.position.x < gameUnitTarget.body.position.x + gameUnitTarget.body.localScale.x / 2)
            {
                //timeUnderTarget += Time.deltaTime;
                adjustedTargetWasUnderTarget = true;
            }
            //Computer has to reset if he can't lock on target
            else if(!owner.isPlayer)
            {
                shootable.Reset();
                //timeUnderTarget = -shootable.GetTimeUntilCanShoot();
            }
        }
        else
        {
            //timeUnderTarget += Time.deltaTime;
            adjustedTargetWasUnderTarget = true;
        }*/

        //How much time we spent under the target
        //float prcntg = timeUnderTarget / TIME_UNDER_TARGET_BEFORE_TARGET_LOCK;
        //SetCurve(Mathf.Min(shootable.GetTimeUntilCanShootPercentage(), 1));
        SetLineColor(shootable.GetTimeUntilCanShootPercentage());

        if (shootable.CanShoot())
        {
            stage = TargetingStage.ReadyForLaunch;
        }
        //}
       // return adjustedTargetWasUnderTarget;
    }
    /*public void LockTarget()
    {
        Transform a = adjustedTarget.marker.transform;

        //After X seconds we lock on target
        if (shootable.CanShoot())//timeUnderTarget >= TIME_UNDER_TARGET_BEFORE_TARGET_LOCK)
        {
            ForceLock();
        }
    }
    public void ForceLock()
    {
        SetCurve(1);
        stage = TargetingStage.TargetLocked;
        //timeUnderLock = shootable.GetReactionTime();
    }*/

    /*public void LaunchProjectileAtTarget(Transform from)
    {
       

    }*/

    public Transform GetTargetTransform()
    {
        return adjustedTarget.marker.transform;
    }

    public float GetLaunchAngle()
    {
        return (360 - joint.rotation.eulerAngles.x);
    }

   /* public void GetReadyForLaunch()
    {
        timeUnderLock += Time.deltaTime;
        float prcntg = Mathf.Min(timeUnderLock / TIME_WHEN_LOCKED_BEFORE_LAUNCH, 1);
        if(prcntg == 1)
        {
            stage = TargetingStage.ReadyForLaunch;
        }
        SetLineColor(prcntg);
    }*/

    public void SetLineColor(float prcntg)
    {
        if(line != null) {  
            if (prcntg != currentLineGradient)
            {
                line.colorGradient = GetColorGradient(prcntg);
            }
            currentLineGradient = prcntg;
        }
    }


    public void HideArc()
    {
        if(line != null) { 
            if(line.enabled == true)
            {
                line.enabled = false;
            }
        }
    }

    public void ShowArc(Transform from)
    {
        if(line == null)
        {
            GameObject arc = new GameObject("Arc of "+from.gameObject.name);
            arc.transform.parent = from;
            line = arc.AddComponent<LineRenderer>();
            line.material = new Material(Global.Resources[MaterialNames.Line]); //Global.instance.MAT_LINE);
            line.colorGradient = GetColorGradient(0);
            line.widthCurve = curve;
            line.widthMultiplier = CURVE_WIDTH;
            line.positionCount = LINE_ARRAY_BUFFER;
            line.receiveShadows = false;
            line.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        }

        if(line.enabled == false)
        {
            line.enabled = true;
        }

        //float eulerAng = (360 - joint.rotation.eulerAngles.x);

        /*if(//eulerAng != currentEulerAngle 
            //|| 
            from.position != currentFromPos 
            || currentTargetPos != adjustedTarget.marker.transform.position)
        {*/
            GetLinePositions(shootable.GetOriginPoint(), adjustedTarget.marker.transform, joint);
            /*line.positionCount = GetLinePositions(
                shootable.GetBulletType(),
                0.1f,
                eulerAng,
                PROJECTILE_GRAVITY,
                from,
                simulator.marker.transform,
                adjustedTarget.marker.transform
            );
            // = positions.Count;
            line.SetPositions(linePositions);*/
        /*}
        else
        {
            //Debug.Log("Didnt even redraw bruh");
        }*/
        /*this.currentEulerAngle = eulerAng;
        this.currentFromPos = from.position;
        this.currentTargetPos = adjustedTarget.marker.transform.position;*/
    }

    public void GetLinePositions(Transform muzzle,  Transform target, Transform joint)
    {
        Vector3 p0 = GetBezierPoint(0, muzzle.position, target.position, joint.position); //muzzle.position;
        Vector3 p1 = GetBezierPoint(1, muzzle.position, target.position, joint.position);//muzzle.position + (muzzle.position - joint.position).normalized * AIM_HIGH_POINT;
        Vector3 p2 = GetBezierPoint(2, muzzle.position, target.position, joint.position);//new Vector3(p1.x + (p3.x - p1.x) / 2, p1.y, p3.z);
        Vector3 p3 = GetBezierPoint(3, muzzle.position, target.position, joint.position);//target.position;


        if (shootable.GetBulletType() == BulletType.Mortar)
        {

            for (int i = 0; i < linePositions.Length; i++)
            {
                float t = ((float)i) / ((float)linePositions.Length - 1);
                linePositions[i] = CalculateCubicBezierPoint(t, p0, p1, p2, p3);
            }
        }
        else{

            for (int i = 0; i < linePositions.Length; i++)
            {
                float t = ((float)i) / ((float)linePositions.Length - 1);
                linePositions[i] = Vector3.Lerp(p0,p3,t);
            }
        }

        line.SetPositions(linePositions);
    }

    public static Vector3 GetBezierPoint(int point, Vector3 muzzle, Vector3 target, Vector3 joint)
    {
        float yDiff = target.y - muzzle.y;
        Vector3 p1 = muzzle + (muzzle - joint).normalized * AIM_HIGH_POINT + new Vector3(0, Mathf.Abs(yDiff) / (yDiff < 0 ? 3 : 1));

        if (point == 0) { return muzzle; }
        if (point == 3) { return target; }
        if (point == 1) { return p1;
        } else {
            return new Vector3(p1.x + (target.x - p1.x) / 2, p1.y, target.z); }
    }

    public static Vector3 CalculateCubicBezierPoint(float t, Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3)
    {
        float u = 1 - t;
        float tt = t * t;
        float uu = u * u;
        float uuu = uu * u;
        float ttt = tt * t;
        Vector3 p = uuu * p0;
        p += 3 * uu * t * p1;
        p += 3 * u * tt * p2;
        p += ttt * p3;
        return p;
    }



    /*public int GetLinePositions(BulletType bulletType, float samplingrate, float firingAngle, float gravity, Transform myTransform, Transform simulator, Transform target)
    {
        //positions.Clear();
        int count = 1;
        //positions.Add(myTransform.position);
        linePositions[count-1] = myTransform.position;

        if (bulletType == BulletType.Mortar)
        {
            simulator.position = myTransform.position + new Vector3(0, 0, 0);

            // Calculate distance to target
            float target_Distance = Vector3.Distance(simulator.position, target.position);

            // Calculate the velocity needed to throw the object to the target at specified angle.
            float projectile_Velocity = target_Distance / (Mathf.Sin(2 * firingAngle * Mathf.Deg2Rad) / gravity);

            // Extract the X  Y componenent of the velocity
            float Vx = Mathf.Sqrt(projectile_Velocity) * Mathf.Cos(firingAngle * Mathf.Deg2Rad);
            float Vy = Mathf.Sqrt(projectile_Velocity) * Mathf.Sin(firingAngle * Mathf.Deg2Rad);

            // Calculate flight time.
            float flightDuration = target_Distance / Vx;

            // Rotate projectile to face the target.
            simulator.rotation = Quaternion.LookRotation(target.position - simulator.position);

            float elapse_time = 0;

            while (elapse_time < flightDuration)
            {
                simulator.Translate(0, (Vy - (gravity * elapse_time)) * samplingrate, Vx * samplingrate);

                elapse_time += samplingrate;
                count++;
                linePositions[count - 1] = simulator.position; 
                //positions.Add(new Vector3(simulator.position.x, simulator.position.y, simulator.position.z));
            }
            count++;
            linePositions[count - 1] = target.position;
            //positions.Add(target.position);
        }
        else
        {
            Vector3 targetPos = new Vector3(target.position.x, target.position.y, target.position.z);
            float distance = Vector3.Distance(myTransform.position, targetPos);
            for(float d = 0; d < distance; d++)
            {
                count++;
                linePositions[count - 1] = Vector3.Lerp(myTransform.position, targetPos, d / distance);

                //positions.Add(Vector3.Lerp(myTransform.position, targetPos, d/distance));
            }
        }
        return count;
        //Debug.Log(positions.Count);
        //return positions;//.//ToArray();
    }*/

    public float AdjustJointTowardsTarget(float actionspeed)
    {
        Transform t = joint;
        float targetAngle = 0;


        if(adjustedTarget != null)
        {
            if (shootable.GetBulletType() == BulletType.Mortar)
            {
                //Calculate target angle
                float targetXpos = Mathf.Min(Mathf.Max(t.position.x - MAX_ANGLE_DISTANCE, adjustedTarget.marker.transform.position.x), t.position.x + MAX_ANGLE_DISTANCE);
                float dist = (t.position.x + MAX_ANGLE_DISTANCE) - targetXpos;
                float prcntg = 1 - (dist / (MAX_ANGLE_DISTANCE * 2));
                targetAngle = prcntg * 180;
            }
            else
            {
                /*
                float dist = Vector3.Distance(t.position, adjustedTarget.marker.transform.position);
                Vector3 adjTarget = new Vector3(
                                             adjustedTarget.marker.transform.position.x,
                                             adjustedTarget.marker.transform.position.y);

                float hypoth = dist;
                float a = Mathf.Abs(t.position.x - adjTarget.x);
                float triAngle = Mathf.Acos(a / hypoth) * 100;

                if (adjTarget.x >= t.position.x && adjTarget.y >= t.position.y)
                {
                    triAngle = 90 + (90 - triAngle);
                }
                else if (adjTarget.x > t.position.x && adjTarget.y < t.position.y)
                {
                    triAngle = 180 + triAngle;

                }
                else if (adjTarget.y < t.position.y && adjTarget.x < t.position.x)
                {
                    triAngle *= -1;
                }

                targetAngle = triAngle;
                */
                /*
                // fast rotation
                float rotSpeed = 360f;

                // distance between target and the actual rotating object
                Vector3 D = adjustedTarget.marker.transform.position - t.position;
                D.z = t.position.z;

                // calculate the Quaternion for the rotation
                Quaternion rot = Quaternion.Slerp(t.rotation, Quaternion.LookRotation(D), rotSpeed * Time.deltaTime);

                //Apply the rotation 
                t.rotation = rot;

                // put 0 on the axys you do not want for the rotation object to rotate
               // t.eulerAngles = new Vector3(t.eulerAngles.x, t., 0);

                return 0;*/
                // 
                Vector3 relativeUp = Quaternion.Euler(0, 180, 0) * owner.body.forward; //new Vector3(0,0, owner.movement.facing == Facing.Right ? - 1 : 1);
                //Quaternion.AngleAxis(90, Vector3.up) * relativeUp;

                //if (owner.movement.facing != Facing.TurningRight && owner.movement.facing != Facing.TurningLeft)
                //{
                    Vector3 relativePos = adjustedTarget.marker.transform.position - t.position;
                    bool right = adjustedTarget.marker.transform.position.x > t.position.x;
                    right = owner.movement.facing == Facing.Left ? !right : right;

                    if (!right)
                    {
                        relativePos.y = relativePos.y * -1;
                    }    

                    t.rotation = Quaternion.LookRotation(relativePos, relativeUp);
                    
                  //  t.Rotate(0, 0, 90);

                        t.localEulerAngles = new Vector3((right ? 0 : 180 )+t.eulerAngles.x, 0,0);
         
               // Debug.Log(t.localEulerAngles.x);
                //}
                return 0;


                //Debug.Log(triAngle + " for: " + shootable.ToString()+ " joint: "+joint+ " adjusterTarget: "+ adjTarget.x);
                //Debug.Log("Diff:"+(angle-targetAngle));

            }
        }

        //Depending on rotation
        if (owner.movement != null)
        {
            if (owner.movement.facing == Facing.Right || owner.movement.facing == Facing.TurningRight)
            {
                targetAngle = 180 - targetAngle;
            }
        }

        //Get current angle
        float currentAngle = angle;

        //Get the angle amount to move
        float tick = MoveTarget(currentAngle, targetAngle, ROTATION_SPEED,actionspeed);
        
        //If we need to rotate only
        if (tick != 0)
        {
            angle += tick;
            //Debug.Log("Angle: " + angle + " Target angle: " + targetAngle + " Tick:" + tick);
            t.Rotate(-tick, 0, 0);
            //Debug.Log("Rotating: " + tick + " targetAngle: " + targetAngle+" for "+shootable.ToString());
        }

        return Mathf.Abs(angle - targetAngle);

    }

    public static float MoveTarget(
        float current, 
        float target, 
        float speed, 
        float actionspeed
        )
    {

        float tick = Time.deltaTime*speed*actionspeed;

        if(current == target)
        {
            return 0;
        }
        if (current > target)
        {
            if (current - tick < target)
            {
                tick = 0;
            }
            else
            {
                tick *= -1;
            }
        }
        else if (current < target && current + tick > target)
        {
            tick = 0;
        }
        return tick;
    }

    public Gradient GetColorGradient(float time)
    {
        Gradient g;
        GradientColorKey[] gck;
        GradientAlphaKey[] gak;
        g = new Gradient();
        gck = new GradientColorKey[4];
        gck[0].color = Color.red;
        gck[0].time = 0.0F;
        gck[1].color = Color.red;
        gck[1].time = Mathf.Max(time - 0.1f, 0);
        gck[2].color = Color.grey;
        gck[2].time = Mathf.Min(time + 0.1f, 1); ;
        gck[3].color = Color.grey;
        gck[3].time = 1.0F;
        gak = new GradientAlphaKey[4];
        gak[0].alpha = 1.0F;
        gak[0].time = 0.0F;
        gak[1].alpha = 1.0F;
        gak[1].time = 0.0F;
        gak[2].alpha = 1.0F;
        gak[2].time = 0.0F;
        gak[3].alpha = 0.0F;
        gak[3].time = 1.0F;
        g.SetKeys(gck, gak);
        return g;
    }

}

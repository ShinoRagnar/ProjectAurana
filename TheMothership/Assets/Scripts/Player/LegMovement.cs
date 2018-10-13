using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum Facing
{
    Left,
    Right,
    TurningLeft,
    TurningRight
}
public interface AnimationEventReceiver
{
    void Receive(string eventname, PointOfInterest pos);
}
public class LegMovement : AnimationEventReceiver{

    //Jump
    private static readonly float MIN_THRUST_FORCE = 1;
    private static readonly float MAX_THRUST_FORCE = 10;
    private static readonly float INCREMENT_THRUST_FORCE = 10;
    private static readonly float JUMP_FORCE = 18;
    private static readonly float IN_AIR_BONUS_SPEED = 1.1f;

    private static readonly float PLAYER_GRAVITY = 30;
    //Animator (based on controller settings)
    private static readonly float ANIMATOR_FORWARD_FACTOR = 4; //Dont't ever change
    private static readonly float ANIMATOR_TURN_FACTOR = 3; //Don't ever change
    private static readonly float ANIMATOR_SCALE_FACTOR_INCREMENT = 0.125f;
    private static readonly float ANIMATOR_SCALE_FACTOR_START = 0.75f;
    //Turn
    private static readonly float TURN_SPEED_WALKING = 1f; //1 = It takes 1 second to complete a turn
    private static readonly float TURN_SPEED_ROLLING = 0.4f;
    private static readonly float MOVE_SPEED_PENALTY_WHEN_TURNING = 0.6f; //0.4 = 40% penalty goes from 0.4-1 during the duration
    //JUMP
    private static readonly float THRUSTER_START_WHEN_ABSORBED = 1.25f;
    private static readonly float[] EXTRA_GRAVITY_WHEN_ABSORBED = new float[] { 0, 0.2f, 1 };
    private static readonly float[] EXTRA_GRAVITY = { 50f, 20f, 10 };
    private static readonly float MIN_FALL_SPEED = -22;

    //Gears
    private static readonly float[] WALKING_GEARS = new float[]         { 0, 0.5f }; // 0,1 = after 0 and after 1 second
    private static readonly float[] WALKING_GEARS_VALUES = new float[]  { 0.5f, 1 }; // Movement% threshold
    private static readonly float[] DRIVING_GEARS = new float[]         { 0,   0.75f, 2, 3f };
    private static readonly float[] DRIVING_GEARS_VALUES = new float[]  { 0.25f,0.5f,0.75f,1f };

    //Initial speed
    private static readonly float WALK_SPEED_INIT = 0.4f; //0.3 = We start at 30% movespeed

    //Visual impact
    private static readonly float VISUAL_IMPACT_LOW_LIMIT_SPEED = 15; //No effects below this impact speed
    private static readonly float VISUAL_IMPACT_HIGH_LIMIT_SPEED = 20; //Display the highest impact after this speed
    private static readonly float FOOTPRINT_RADIUS = 0.4f;

    //Movement
    private Vector3 playerMoveDirection;
    private float currentJumpForce = 1;
    private float x = 0f;
    private float y = 0f;
    private float z = 0f;
    private float currentTimeInMotion = 0;
    private float absorbedJumpForce = 0;

    //Statistics
    private float statisticsCurrentTimer = 0;
    private float statisticsPlayerMovement = 0;
    private float statisticsPlayerNoneMovement = 0;

    //Button presses
    float buttonCooler = 0.5f; // Half a second before reset
    float buttonCount = 0;

    //All accumulated impacts
    private List<Vector3> impacts;
    //private CharacterController playerController;
    private RectTransform timeBar;
    private UIContainer ui;


    //Player unit
    public GameUnit owner;

    public Facing facing;
    public float turningDuration;

    public Ground walkingOn;
    public Ground lastWalkedOn;

    //Skidmarks
    Transform frontRightSkidmark = null;
    Transform frontLeftSkidmark = null;
    Transform backRightSkidmark = null;
    Transform backLeftSkidmark = null;

    //public AudioSource transformerSource;
    //public AudioSource engineSource;

    public void AddImpact(string source, Vector3 impact)
    {
        impacts.Add(impact);
    }
    // Use this for initialization
    public LegMovement(GameUnit owner)
    {
        this.owner = owner;
        //Controller to move
       // playerController = owner.controller; //GetComponent<CharacterController>();
        owner.movement = this;
        // Debug.Log("Leg movement init");

        if (owner.isPlayer)
        {

            //Current facing
            facing = Facing.Right;

            //Prepare a jumping duration instance
            ui = new UIContainer(
                Global.NAME_TIME_BAR, 
                Global.References[SceneReferenceNames.PanelDuration],
                /*Global.instance.PANEL_DURATION*/
                //Global.instance.UI_DURATION_BIG
                Global.Resources[PrefabNames.DurationBar]
                );

            ui.Show();
            ui.Hide();
            timeBar = ui.GetComponent<RectTransform>(Global.NAME_TIME_BAR);
        }
        else
        {
            //Enemies always start off turned off left
            facing = Facing.Left;
            //enemies
        }

        //Impacts received list
        impacts = new List<Vector3>();

        //Debug.Log("Leg spawner end");
    }


    //&& !owner.mech.legs.movement.Contains((int)LegMovementType.NoMovement)

    public void Kill()
    {
        this.owner.mech.legs.animator.SetBool("Dead", true);
        this.owner.mech.legs.animator.speed = 1;
        this.owner.mech.jetpack.HideBeams();
        this.owner.mech.jetpack.HideExhausts();
    }

    public void Interrupt()
    {
        if (owner.controller.isGrounded)
        {
            this.owner.mech.legs.animator.SetTrigger("Hit");
            ShowFootCollision(false);
        }
    }

    // Update is called once per frame
    public void UpdateMovement(bool jumping, float heading, bool transformAction, bool dontTurn, float actionspeed) {

        bool thrusters = false;

        if (!owner.isActive)
        {
            Debug.Log("Trying to move dead unit: " + owner.uniqueName);
            return;
        }
        //If imovable then set all to null
        if (owner.mech.legs.movement.Contains((int)LegMovementType.NoMovement))
        {
            thrusters = false;
            heading = 0;
            transformAction = false;
            dontTurn = false;
        }
        //Get animator state
        AnimatorStateInfo curr = this.owner.mech.legs.animator.GetCurrentAnimatorStateInfo(0);//this.owner.animator.GetCurrentAnimatorStateInfo(0);
        AnimatorStateInfo next = this.owner.mech.legs.animator.GetNextAnimatorStateInfo(0); //this.owner.animator.GetNextAnimatorStateInfo(0);

        //Animation state
        bool spiderMode = this.owner.mech.legs.animator.GetBool("SpiderMode");
        bool transforming = curr.IsName("Transform_to_Roller")
            || curr.IsName("Transform_to_Spider")
            || next.IsName("Transform_to_Roller")
            || next.IsName("Transform_to_Spider");
        bool airBorne = this.owner.mech.legs.animator.GetBool("Jumping");
        bool backwards = this.owner.mech.legs.animator.GetBool("Backwards");

        //Movespeed
        float moveSpeed = 0;
        //Penalty when turning
        float movePenalty = 1;
        float turnSpeed = spiderMode ? TURN_SPEED_WALKING : TURN_SPEED_ROLLING;
        float scaleFactor = ANIMATOR_SCALE_FACTOR_INCREMENT * (this.owner.mech.legs.inventoryWidth) + ANIMATOR_SCALE_FACTOR_START;
        float moveability = owner.stats.GetCurrentValue(Stat.Moveability);
        //float actionspeed = owner.stats.GetCurrentValue(Stat.ActionSpeed);

        //Store X and Y momentum
        x = playerMoveDirection.x;
        y = playerMoveDirection.y;
        z = -owner.body.position.z;

        if(absorbedJumpForce >= JUMP_FORCE* THRUSTER_START_WHEN_ABSORBED && jumping)
        {
            thrusters = true;
        }

        //Ground check
        if (owner.controller.isGrounded)
        {
            if(walkingOn != null)
            {
                if (!walkingOn.IsOn(owner.body.position))
                {
                    walkingOn = Senses.GetGroundBelow(owner.body.position,owner.body.localScale.y);
                    if (walkingOn != null) { lastWalkedOn = walkingOn; }
                }
            }
            else
            {
                walkingOn = Senses.GetGroundBelow(owner.body.position, owner.body.localScale.y);
                if (walkingOn != null) { lastWalkedOn = walkingOn; }
            }
        }
        else
        {
            walkingOn = null;
        }
        

        //Initiates rotation if needed
        if (!airBorne && !backwards && ((facing == Facing.Right && heading < 0) || (facing == Facing.Left && heading > 0)))
        {
            currentTimeInMotion = 0;

            if (dontTurn)
            {
                this.owner.mech.legs.animator.SetBool("Backwards", true);
            }
            else
            {
                //Initiate turning
                facing = heading < 0 ? Facing.TurningLeft : Facing.TurningRight;
                this.owner.mech.legs.animator.SetInteger("Turning", heading < 0 ? -2 : 2);
                this.owner.stats.Kill(Stat.RollSpeed);
                this.owner.stats.SetValuePercentage(Stat.WalkSpeed, WALK_SPEED_INIT);
            }
        }
        else if (facing == Facing.TurningLeft || facing == Facing.TurningRight)
        {
            //Turn
            turningDuration += Time.deltaTime;
            float percentage = turningDuration / turnSpeed;
            this.owner.body.rotation = Quaternion.Slerp(
                Quaternion.Euler(0, facing == Facing.TurningLeft ? 0: 180, 0), 
                Quaternion.Euler(0, facing == Facing.TurningLeft ? 180 : 0, 0), 
                Mathf.Min(percentage, 1));

            //Movespeedmodifier when turning (ease in with pi)
            movePenalty = (1 - MOVE_SPEED_PENALTY_WHEN_TURNING) + (1f - Mathf.Cos(percentage * Mathf.PI * 0.5f)) * MOVE_SPEED_PENALTY_WHEN_TURNING;

            //Finish turning
            if (turningDuration > turnSpeed)
            {
                facing = facing == Facing.TurningLeft ? Facing.Left : Facing.Right;
                turningDuration = 0;
                movePenalty = 1;
                this.owner.mech.legs.animator.SetInteger("Turning", 0);
            }
            //Switch direction while switching direction
            else if ( (facing == Facing.TurningLeft && heading > 0)
                || (facing == Facing.TurningRight && heading < 0)){
                facing = facing == Facing.TurningLeft ? Facing.TurningRight : Facing.TurningLeft;
                this.owner.mech.legs.animator.SetInteger("Turning", facing == Facing.TurningLeft ? 2: -2);
                turningDuration = turnSpeed - turningDuration;
            }
        }else
        {
            //Can't transform mid air
            if (airBorne)
            {
                transformAction = false;
            }

            //Transform to roller
            if (spiderMode && transformAction)
            {
                this.owner.mech.legs.animator.SetBool("SpiderMode", false);
                transforming = true;
                owner.mech.soundEngine.TransformToRoller();
            }
            //Transform to spider
            else if(!spiderMode && (transformAction || thrusters))
            {
                this.owner.mech.legs.animator.SetBool("SpiderMode", true);
                transforming = true;
                owner.mech.soundEngine.TransformToSpider();
                this.owner.mech.jetpack.HideExhausts();
            }

            if (jumping && owner.IsGrounded())
            {
                if (!airBorne)
                {
                    this.owner.mech.legs.animator.SetBool("Jumping", true);
                }
                y += JUMP_FORCE;

                ShowJumpClouds();

            }
            //Thrusters
            if (thrusters && !transforming)
            {
                if (!airBorne)
                {
                    this.owner.mech.legs.animator.SetBool("Jumping", true);
                    
                }
               
                float jumpForce = owner.stats.GetCurrentValue(Stat.JumpForce)*actionspeed;
                float jumpForceMax = owner.stats.GetStat(Stat.JumpForce);

                if (jumpForce > 0)
                {
                    currentJumpForce += INCREMENT_THRUST_FORCE * Time.deltaTime;
                    currentJumpForce = Mathf.Min(currentJumpForce, MAX_THRUST_FORCE);
                    y = Mathf.Max(MIN_THRUST_FORCE, currentJumpForce);

                    owner.stats.Damage(Stat.JumpForce, Time.deltaTime);

                    if (owner.isPlayer)
                    {
                        UpdateJumpDuration(jumpForce, jumpForceMax);
                    }
                    //Turn beams up
                    this.owner.mech.jetpack.ShowBeams(heading,facing,true);
                }
                else
                {
                    thrusters = false;
                }
            }
            else
            {
                thrusters = false;
            }

        }

        if (!dontTurn)
        {
            this.owner.mech.legs.animator.SetBool("Backwards", false);
        }

        //Turn beams down
        if(!thrusters && airBorne)
        {
            this.owner.mech.jetpack.ShowBeams(heading, facing, false);
        }

        //Landing and being grounded
        if(!thrusters){

            currentJumpForce -= INCREMENT_THRUST_FORCE * Time.deltaTime;
            currentJumpForce = Mathf.Max(MIN_THRUST_FORCE, currentJumpForce);

            if (owner.controller.isGrounded)
            {
                if (airBorne)
                {
                    //Turn off beams
                    this.owner.mech.jetpack.HideBeams();
                    this.owner.mech.legs.animator.SetBool("Jumping", false);
                    owner.mech.soundEngine.Land(Mathf.Abs(playerMoveDirection.y));
                    ShowJumpImpact(Mathf.Abs(playerMoveDirection.y));
                }
                
                if (owner.stats.Heal(Stat.JumpForce, Time.deltaTime) != 0)
                {
                    if (owner.isPlayer)
                    {
                        UpdateJumpDuration(
                            owner.stats.GetCurrentValue(Stat.JumpForce),
                            owner.stats.GetStat(Stat.JumpForce));
                    }
                }
            }
        }

        //Moving left or right
        if (heading != 0)
        {
            this.owner.mech.legs.animator.SetBool("Walking", true);

            //Check if we are walking the way we are facing
            if(((facing == Facing.Right && heading > 0) || (facing == Facing.Left && heading < 0)) )
            {
                this.owner.mech.legs.animator.SetBool("Backwards", false);
            }
            //Accelerate movespeed
            if (spiderMode)
            {
                owner.stats.Heal(Stat.WalkSpeed, owner.stats.GetStat(Stat.WalkAcceleration, heading < 0 ? Global.CONDITION_MOVING_LEFT : Global.CONDITION_MOVING_RIGHT) * Time.deltaTime);
                moveSpeed = owner.stats.GetCurrentValue(Stat.WalkSpeed, heading < 0 ? Global.CONDITION_MOVING_LEFT : Global.CONDITION_MOVING_RIGHT);
            }
            else
            {
                owner.stats.Heal(Stat.RollSpeed, owner.stats.GetStat(Stat.RollAcceleration, heading < 0 ? Global.CONDITION_MOVING_LEFT : Global.CONDITION_MOVING_RIGHT) * Time.deltaTime);
                moveSpeed = owner.stats.GetCurrentValue(Stat.RollSpeed, heading < 0 ? Global.CONDITION_MOVING_LEFT : Global.CONDITION_MOVING_RIGHT);
                this.owner.mech.jetpack.ShowExhausts(heading, facing, true);
                
            }

            //Use gears to switch between speeds (Do not use gear in air)
            moveSpeed *= (owner.IsGrounded() ? GetGearPercentage(spiderMode) : 1) * actionspeed ;

            //Speed up animation when turning otherwise scale animation to movement
            if (facing != Facing.TurningLeft && facing != Facing.TurningRight)
            {
                this.owner.mech.legs.animator.speed = moveSpeed / (ANIMATOR_FORWARD_FACTOR*scaleFactor);
            }
            //Standing still
        }
        else
        {
            //Halt movement
            this.owner.stats.SetValuePercentage(Stat.WalkSpeed, WALK_SPEED_INIT);
            this.owner.stats.Kill(Stat.RollSpeed);
            this.owner.mech.legs.animator.SetBool("Walking", false);
            this.owner.mech.legs.animator.speed = actionspeed;

            //Decrease exhaust output when in car mode
            if (!spiderMode)
            {
                this.owner.mech.jetpack.ShowExhausts(heading, facing, false);
            }
        }

        //Keep speed while turning
        if (facing == Facing.TurningLeft || facing == Facing.TurningRight)
        {
            if (!transforming)
            {
                this.owner.mech.legs.animator.speed = ANIMATOR_TURN_FACTOR / (turnSpeed * scaleFactor);
            }
            else
            {
                this.owner.mech.legs.animator.speed = actionspeed;
            }
        }

        //Update skidmarks
        UpdateSkidmarks(airBorne, spiderMode, owner.controller.isGrounded,transforming);

        //Update x
        x = heading * moveSpeed * movePenalty * (airBorne ? IN_AIR_BONUS_SPEED : 1);

        if (x != 0)
        {
            //Handle gear switch
            int gear = GetGear(spiderMode);
            currentTimeInMotion += Time.deltaTime;
            if (gear != GetGear(spiderMode))
            {
                //Make gearchange sound
                owner.mech.soundEngine.GearChange(spiderMode);
                
                //Show effects when gearing up in rollermode
                if (!spiderMode)
                {
                    ShowFootCollision();
                    ShowGearSwitch(heading, facing);

                }
            }
        }
        else
        {
            //Reset gear switch
            currentTimeInMotion = 0;
        }


        //Gravity
        if (!owner.controller.isGrounded) {

            float extraGravity = 0;

            if (!jumping)
            {
                for (int i = 0; i < EXTRA_GRAVITY_WHEN_ABSORBED.Length; i++)
                {
                    if (absorbedJumpForce >= JUMP_FORCE * EXTRA_GRAVITY_WHEN_ABSORBED[i])
                    {
                        extraGravity = EXTRA_GRAVITY[i];
                    }
                }
            }

            float gravityThisFrame =  (extraGravity + PLAYER_GRAVITY) * Time.deltaTime;

            absorbedJumpForce += gravityThisFrame;

            y -= gravityThisFrame;

            if(y < MIN_FALL_SPEED)
            {
                y = MIN_FALL_SPEED;
            }
        }
        else if(owner.controller.isGrounded && y < 0)
        {
            y = 0;
        }

        //Jump force absorbtion
        if (owner.IsGrounded())
        {
            absorbedJumpForce = 0;
        }

        //Result
        playerMoveDirection = new Vector3(x, y, z);
        
        //Impact
        foreach (Vector3 impact in impacts)
        {
            //Debug.Log("absorbing: " + impacts.Count);
            if (owner.controller.isGrounded) { 
                playerMoveDirection += impact;
            }
            else
            {
                playerMoveDirection += impact * 2;
            }
        }
        impacts.Clear();

        //Move
        if (owner.controller.isGrounded 
            && playerMoveDirection.x == 0 
            && playerMoveDirection.y == 0 
            && playerMoveDirection.z == 0)
        {
            //We are grounded with no movement
            if (DevelopmentSettings.SHOW_STATISTICS)
            {
                statisticsPlayerNoneMovement++;
            }
        }
        else
        {
            //Debug.Log(playerMoveDirection * Time.deltaTime);

            owner.controller.Move(playerMoveDirection * Time.deltaTime * moveability);
            if (DevelopmentSettings.SHOW_STATISTICS)
            {
                //Debug.Log(playerController.isGrounded + " " + playerMoveDirection);
                statisticsPlayerMovement++;
            }
        }

        if (DevelopmentSettings.SHOW_STATISTICS)
        {
            statisticsCurrentTimer+=Time.deltaTime;
            if(statisticsCurrentTimer > 1)
            {
                statisticsCurrentTimer = 0;
                Debug.Log("Player: Movement: " + statisticsPlayerMovement + " NoneMovement: " + statisticsPlayerNoneMovement);
            }
        }


    }




    private void UpdateSkidmarks(bool airborne, bool spidermode, bool isGrounded, bool transforming)
    {
        if (!airborne 
            && walkingOn != null 
            && !spidermode 
            && isGrounded 
            && !transforming)
            
        {
            // 1 = 0.3
            // 3 = 0.5
            // 8 = 1
            float sizeScale = (0.2f + owner.mech.legs.inventoryWidth * 0.1f)*4;

            //ApplySkidmarks();
            if (walkingOn.IsOn(this.owner.mech.legs.GetPointOfInterest(PointOfInterest.FrontRightFoot1).position, FOOTPRINT_RADIUS))
            {
                ApplySkidmarkFrontRight(sizeScale);
            }
            else
            {
                ReturnFrontRightSkidmark();   
            }
            if (walkingOn.IsOn(this.owner.mech.legs.GetPointOfInterest(PointOfInterest.FrontLeftFoot1).position, FOOTPRINT_RADIUS))
            {
                ApplySkidmarkFrontLeft(sizeScale);
            }
            else
            {
                ReturnFrontLeftSkidmark();
            }
            if (walkingOn.IsOn(this.owner.mech.legs.GetPointOfInterest(PointOfInterest.BackRightFoot1).position, FOOTPRINT_RADIUS))
            {
                ApplySkidmarkBackRight(sizeScale);
            }
            else
            {
                ReturnBackRightSkidmark();
            }
            if (walkingOn.IsOn(this.owner.mech.legs.GetPointOfInterest(PointOfInterest.BackLeftFoot1).position, FOOTPRINT_RADIUS))
            {
                ApplySkidmarkBackLeft(sizeScale);
            }
            else
            {
                ReturnBackLeftSkidmark();
            }
        }
        else
        {
            ResetAllSkidmarks();
        }
    }

    private void ApplySkidmarkFrontRight(float scale)
    {
        if (frontRightSkidmark == null)
        {
            frontRightSkidmark = Global.Resources[EffectNames.Skidmark].Spawn(this.owner.mech.legs.GetPointOfInterest(PointOfInterest.FrontRightFoot1),0,0,0,scale);
        }
    }
    private void ApplySkidmarkFrontLeft(float scale)
    {
        if (frontLeftSkidmark == null)
        {
            frontLeftSkidmark = Global.Resources[EffectNames.Skidmark].Spawn(this.owner.mech.legs.GetPointOfInterest(PointOfInterest.FrontLeftFoot1),0,0,0,scale);
        }
    }
    private void ApplySkidmarkBackRight(float scale)
    {
        if (backRightSkidmark == null)
        {
            backRightSkidmark = Global.Resources[EffectNames.Skidmark].Spawn(this.owner.mech.legs.GetPointOfInterest(PointOfInterest.BackRightFoot1),0,0,0,scale);
        }
    }
    private void ApplySkidmarkBackLeft(float scale)
    {
        if (backLeftSkidmark == null)
        {
            //Global.instance.EFFECT_SKIDMARK.
            backLeftSkidmark = Global.Resources[EffectNames.Skidmark].Spawn(this.owner.mech.legs.GetPointOfInterest(PointOfInterest.BackLeftFoot1),0,0,0,scale);
        }
    }

    private void ReturnFrontRightSkidmark()
    {
        if (frontRightSkidmark != null)
        {
            //Global.instance.EFFECT_SKIDMARK
            Global.Resources[EffectNames.Skidmark].ReturnHandle(frontRightSkidmark);
            frontRightSkidmark = null;
        }
    }
    private void ReturnFrontLeftSkidmark()
    {
        if (frontLeftSkidmark != null)
        {
            //Global.instance.EFFECT_SKIDMARK
            Global.Resources[EffectNames.Skidmark].ReturnHandle(frontLeftSkidmark);
            frontLeftSkidmark = null;
        }
    }
    private void ReturnBackRightSkidmark()
    {
        if (backRightSkidmark != null)
        {
            // Global.instance.EFFECT_SKIDMARK
            Global.Resources[EffectNames.Skidmark].ReturnHandle(backRightSkidmark);
            backRightSkidmark = null;
        }
    }
    private void ReturnBackLeftSkidmark()
    {
        if (backLeftSkidmark != null)
        {
            //Global.instance.EFFECT_SKIDMARK
           Global.Resources[EffectNames.Skidmark].ReturnHandle(backLeftSkidmark);
            backLeftSkidmark = null;
        }
    }

    private void ResetAllSkidmarks()
    {
        ReturnFrontRightSkidmark();
        ReturnFrontLeftSkidmark();
        ReturnBackRightSkidmark();
        ReturnBackLeftSkidmark();
    }

    private void Step(Vector3 position, bool heavyImpact = false, bool playSound = true, bool smokeRing = false)
    {
        if (walkingOn != null)
        {
            if (walkingOn.IsOn(position, FOOTPRINT_RADIUS))
            {
                position.y = walkingOn.GetSurfaceY();
                if (playSound)
                {
                    owner.mech.soundEngine.Footsteps();
                }

                // 1 = 0.3
                // 3 = 0.5
                // 8 = 1
                float sizeScale = 0.2f + owner.mech.legs.inventoryWidth * 0.1f;

                //Global.instance.EFFECT_JUMP_IMPACT : Global.instance.EFFECT_FOOTSTEP

                if (smokeRing)
                {
                    Global.Resources[EffectNames.SmokeRing].Spawn(position, null, 0, 0, 0, sizeScale * 6);
                }
                else
                {
                    (heavyImpact ? Global.Resources[EffectNames.JumpImpact] : Global.Resources[EffectNames.Footstep]).Spawn(position, null, 0, 0, 0, sizeScale * 6);

                }

                //Global.instance.EFFECT_FOOTMARK
                //Global.Resources[EffectNames.Footmark].Spawn(position,null,-90,0,0, sizeScale);
            }
        }
    }
    public void Receive(string eventname, PointOfInterest pos)
    {
        if (eventname.Equals(FootstepEventReceiver.FOOTSTEP))
        {
            Step(this.owner.mech.legs.GetPointOfInterest(pos).position);
        }
    }
    public void ShowGearSwitch(float heading, Facing facing)
    {

        int rl = heading > 0 ? 180 : 0;

        //Going forward
        if (heading > 0 && facing == Facing.Right || heading < 0 && facing == Facing.Left)
        {
            //Global.instance.EFFECT_EXHAUST
            Global.Resources[EffectNames.Exhaust].Spawn(this.owner.mech.legs.GetPointOfInterest(PointOfInterest.ExhaustBack1).position,null, 0, rl, 0);
            //Global.instance.EFFECT_EXHAUST
            Global.Resources[EffectNames.Exhaust].Spawn(this.owner.mech.legs.GetPointOfInterest(PointOfInterest.ExhaustBack2).position,null, 0, rl, 0);
        }
        else
        {
            //Global.instance.EFFECT_EXHAUST
            Global.Resources[EffectNames.Exhaust].Spawn(this.owner.mech.legs.GetPointOfInterest(PointOfInterest.ExhaustFront1).position,null, 0, rl, 0);
            //Global.instance.EFFECT_EXHAUST
            Global.Resources[EffectNames.Exhaust].Spawn(this.owner.mech.legs.GetPointOfInterest(PointOfInterest.ExhaustFront1).position,null, 0, rl, 0);
        }
    }

    public void ShowJumpClouds()
    {
        Step(this.owner.mech.legs.GetPointOfInterest(PointOfInterest.FrontRightFoot1).position, false, false,true);
        Step(this.owner.mech.legs.GetPointOfInterest(PointOfInterest.FrontLeftFoot1).position, false, false, true);
        Step(this.owner.mech.legs.GetPointOfInterest(PointOfInterest.BackRightFoot1).position, false, false, true);
        Step(this.owner.mech.legs.GetPointOfInterest(PointOfInterest.BackLeftFoot1).position, false, false, true);
    }

    public void ShowFootCollision(bool heavyImpact = false)
    {
        Step(this.owner.mech.legs.GetPointOfInterest(PointOfInterest.FrontRightFoot1).position, heavyImpact,false);
        Step(this.owner.mech.legs.GetPointOfInterest(PointOfInterest.FrontLeftFoot1).position, heavyImpact,false);
        Step(this.owner.mech.legs.GetPointOfInterest(PointOfInterest.BackRightFoot1).position, heavyImpact,false);
        Step(this.owner.mech.legs.GetPointOfInterest(PointOfInterest.BackLeftFoot1).position, heavyImpact,false);
    }

    public void ShowJumpImpact(float speed)
    {
        float lowLimit = VISUAL_IMPACT_LOW_LIMIT_SPEED;
        float highLimit = VISUAL_IMPACT_HIGH_LIMIT_SPEED;

        if (!(speed <= lowLimit))
        {
            ShowFootCollision(!(speed > lowLimit && speed < highLimit));
        }
    }
    private float GetGearPercentage(bool spidermode)
    {

        float[] arr = spidermode ? WALKING_GEARS_VALUES : DRIVING_GEARS_VALUES;
        return arr[GetGear(spidermode) - 1]; // ((float)GetGear(spidermode)) / arr.Length;
    }

    private int GetGear(bool spidermode)
    {
        float[] arr = spidermode ? WALKING_GEARS : DRIVING_GEARS;
        int i;
        for (i = 0; i < arr.Length; i++)
        {
            if (currentTimeInMotion < arr[i])
            {
                break;
            }
        }
        return i;
    }

    public bool DetectDoubleTap(bool downPress, bool axisBigger, bool axisLess)
    {
        bool doubleT = false;

        if (        (
                    downPress && axisBigger
                    //Input.GetButtonDown("Horizontal") 
                    //&& Input.GetAxis("Horizontal") > 0
                    && facing == Facing.Right
                    )
                ||
                    (
                    downPress && axisLess
                    //(Input.GetButtonDown("Horizontal")
                    //&& Input.GetAxis("Horizontal") < 0
                    && facing == Facing.Left
                    )
                )
        {
            if (buttonCooler > 0 && buttonCount == 1/*Number of Taps you want Minus One*/)
            {
                doubleT = true;
            }
            else
            {
                buttonCooler = 0.5f;
                buttonCount += 1;
            }
        }

        if (buttonCooler > 0)
        {

            buttonCooler -= 1 * Time.deltaTime;

        }
        else
        {
            buttonCount = 0;
        }
        return doubleT;
    }
  
    private void UpdateJumpDuration(float jumpForce, float jumpForceMax)
    {
        /*if (timeBar == null)
        {
            timeBar = Global.FindDeepChild(jumpDuration, Global.NAME_TIME_BAR).GetComponent<RectTransform>();
        }*/


        int durPosition = ((int)((1 - (jumpForce / jumpForceMax)) * Global.UI_DURATION_BIG_TIME_LENGTH));
        int oldVal = ((int)timeBar.offsetMin.y);

        if (oldVal != durPosition)
        {
            timeBar.offsetMin = new Vector2(timeBar.offsetMin.x, (float)durPosition);
        }

        //Jump force show duration bar
        if (jumpForce != jumpForceMax && ui.hidden)//&& jumpDuration.gameObject.activeSelf != true)
        {
            ui.Show();
            //jumpDuration.gameObject.SetActive(true);
            //jumpDuration.localPosition = new Vector3(100, -100);
        }
        else if (jumpForce == jumpForceMax && !ui.hidden)//&& jumpDuration.gameObject.activeSelf == true)
        {
            ui.Hide();
            //jumpDuration.gameObject.SetActive(false);
        }

    }
}

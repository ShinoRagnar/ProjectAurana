using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class AIMech : AIBasic {

    public static readonly float IDLE_TIMER_START_WHEN_AT_DISTANCE = 30;
    public static readonly float TIME_UNTIL_IDLE = 4;

    public static readonly float PREFERRED_MORTAR_TIMING = 0.3f;
    public static readonly float PREFERRED_RIFLE_TIMING = 0.1f;


    public static readonly float FLEE_PREFERRED_DISTANCE = 50;
    public static readonly float RIFLE_PREFERRED_DISTANCE = 10;
    public static readonly float MORTAR_PREFERRED_DISTANCE = 15;
    public static readonly float MELEE_PREFERRED_DISTANCE = 0.5f;

    public static readonly float MELEE_DISTANCE_TOLERANCE_CLOSE = 0.5f;
    public static readonly float MORTAR_DISTANCE_TOLERANCE_CLOSE = 5;
    public static readonly float RIFLE_DISTANCE_TOLERANCE_CLOSE = 5;

    public static readonly float MELEE_DISTANCE_TOLERANCE_FAR = 0.5f;
    public static readonly float MORTAR_DISTANCE_TOLERANCE_FAR = 5;
    public static readonly float RIFLE_DISTANCE_TOLERANCE_FAR = 3;

    public static readonly float MELEE_RANGE = 4;
    public static readonly float PRECISION = 0.25f;
    public static readonly float MAX_TRANSITION_TIME = 4;
    public static readonly float MAX_STUCK_BEFORE_JUMP_TIME = 1;
    public static readonly float MAX_GET_UNSTUCK_TIME = 2;

    //bool hasInitiatedJumping = false;

    //int meleeWeaponsEquipped = 0;
    //int mortarsEquipped = 0;
    //int riflesEquipped = 0;
    //int shieldsEquipped = 0;


    float preferredDistance = 0;
    float distanceToleranceClose = 0;
    float distanceToleranceFar = 0;

    float currentTransitionTime = 0;
    float currentRelocationTime = 0;
    float currentStuckTime = 0;
    float currentTryingToGetUnstuckTime = MAX_GET_UNSTUCK_TIME;

    float currentIdleTimer = 0;

    //Vector3 locationLastFrame;

    bool isMelee = false;
    bool waitingForJumpRecharge = false;

    float xLastFrame = 0;

    List<Gun> rifles = new List<Gun>();
    List<Gun> mortarsAndML = new List<Gun>();
    List<Weapon> weapons = new List<Weapon>();
    List<Shield> shields = new List<Shield>();

    float lowestRifleTimer = 0;
    float currentRifleTimer = 0;
    float rifleTimer = 0;
    float currentMortarTimer = 0;
    float lowestMortarTimer = 0;
    float mortarTimer = 0;


    // Use this for initialization
    void Start () {

        foreach(Core c  in owner.mech.equippedCores)
        {
            //Debug.Log(c.uniqueItemName);

            if(c.sockets != null)
            {
                foreach (int i in c.sockets)
                {
                    if (c.sockets[i].occupant != null)
                    {

                        //Debug.Log(((MechItem)c.sockets[i].occupant).uniqueItemName);

                        if (c.sockets[i].occupant is Weapon)
                        {
                            weapons.Add((Weapon)c.sockets[i].occupant);
                            //meleeWeaponsEquipped++;
                        }
                        else if (c.sockets[i].occupant is Shield)
                        {
                            shields.Add((Shield)c.sockets[i].occupant);
                            //shieldsEquipped++;
                        }
                        else if (c.sockets[i].occupant is GunArray)
                        {
                            GunArray ga = (GunArray)c.sockets[i].occupant;
                            foreach(Gun g in ga.socketedGuns)
                            {
                                if(lowestRifleTimer == 0 || g.thisCycleReloadTime < lowestRifleTimer)
                                {
                                    lowestRifleTimer = g.thisCycleReloadTime;
                                }
                                rifles.Add(g);
                            }
                            rifleTimer = Mathf.Min(lowestRifleTimer / rifles.Count, PREFERRED_RIFLE_TIMING);
                            //riflesEquipped++;

                        }
                        else if (c.sockets[i].occupant is Gun)
                        {
                            Gun g = (Gun)c.sockets[i].occupant;
                            mortarsAndML.Add((Gun)c.sockets[i].occupant);

                            if (lowestMortarTimer == 0 || g.thisCycleReloadTime < lowestMortarTimer)
                            {
                                lowestMortarTimer = g.thisCycleReloadTime;
                            }

                            mortarTimer = Mathf.Min(lowestMortarTimer / rifles.Count, PREFERRED_MORTAR_TIMING);
                            //mortarsEquipped++;s
                        }
                    }
                }
            }

        }
        if(mortarsAndML.Count > 0)
        {
            preferredDistance = MORTAR_PREFERRED_DISTANCE;
            distanceToleranceClose = MORTAR_DISTANCE_TOLERANCE_CLOSE;
            distanceToleranceFar = MORTAR_DISTANCE_TOLERANCE_FAR;

        }else if(rifles.Count > 0)
        {
            preferredDistance = RIFLE_PREFERRED_DISTANCE;
            distanceToleranceClose = RIFLE_DISTANCE_TOLERANCE_CLOSE;
            distanceToleranceFar = RIFLE_DISTANCE_TOLERANCE_FAR;

        }else if(weapons.Count > 0)
        {
            preferredDistance = MELEE_PREFERRED_DISTANCE;
            distanceToleranceClose = MELEE_DISTANCE_TOLERANCE_CLOSE;
            distanceToleranceFar = MELEE_DISTANCE_TOLERANCE_FAR;
            isMelee = true;

        }else{
            preferredDistance = FLEE_PREFERRED_DISTANCE;
            distanceToleranceClose = FLEE_PREFERRED_DISTANCE / 2;
            distanceToleranceFar = FLEE_PREFERRED_DISTANCE/2;
        }

    }

    public void Shoot(List<Gun> guns, GameUnit target)
    {
        foreach(Gun g in guns)
        {
            if (g.IsReady())
            {
                g.Execute(target);
                return;
            }
        }
    }

    public void Attack(List<Weapon> weapons, GameUnit target)
    {
        foreach (Weapon w in weapons)
        {
            if (w.IsReady())
            {
                w.Execute(target);
                return;
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        float actionspeed = owner.stats.GetCurrentValue(Stat.ActionSpeed);

        timeSinceLastChange += Time.deltaTime;
        currentReactionTime += Time.deltaTime;

        bool jumping = false;
        float heading = 0;
        bool transformAction = false;
        bool shoot = false;
        bool attack = false;

        if (state == AIState.Idle)
        {
            
            if (timeSinceLastChange > IDLE_REACTION_TIME)
            {
                timeSinceLastChange = 0;
                GameUnit enemy = LookForEnemy(owner, owner.belongsToFaction.hostileFactions);
                if (enemy != null)
                {
                    state = AIState.Hunting;
                    target = enemy;
                    Global.Resources[EffectNames.ExclamationMark].Spawn(owner.GetHeadPos()+new Vector3(0, 2));
                }
            }
            return;
        }

        
        float scaleDistance = (owner.GetScale().x + target.GetScale().x) / 2;
        float distance = Vector3.Distance(owner.GetCenterPos(), target.GetCenterPos()) - scaleDistance;
        bool targetIsToTheRight = owner.GetCenterPos().x < target.GetCenterPos().x;
        bool targetIsToTheLeft = owner.GetCenterPos().x > target.GetCenterPos().x;
        float currentX = owner.GetCenterPos().x;
        float xProgressSinceLastFrame = currentX - xLastFrame;
      

        bool turning = false;

        Ground lastOn = owner.GetLastOnGround();

        if (state == AIState.InTransition)
        {
            //Do nothing
        }
        else
        {
            //Move if out of reach
            if (((preferredDistance - distanceToleranceClose > distance && !isMelee)
                || preferredDistance + distanceToleranceFar < distance)
                )
            {
                //If the current path is not valid
                if (!goingTo.valid ||
                    (!isMelee && preferredDistance - distanceToleranceClose > Vector3.Distance(target.GetCenterPos(), goingTo.worldpos)) ||
                    preferredDistance + distanceToleranceFar < Vector3.Distance(target.GetCenterPos(), goingTo.worldpos)
                   )
                {
                    goingTo = GetMoveAtDistanceFromTarget(preferredDistance,isMelee);
                }

                if (goingTo.valid)
                {
                    state = AIState.Relocating;
                    currentRelocationTime = 0;
                }
                else
                {
                    state = AIState.Hunting;
                }
            }
            else
            {
                state = AIState.Hunting;
            }
        }


        bool facingTarget = (
            targetIsToTheRight
            && owner.movement.facing == Facing.Right
            ) ||
            (
            targetIsToTheLeft
            && owner.movement.facing == Facing.Left
            );

        if (state == AIState.Hunting){


            //Turn towards the enemy
            if (targetIsToTheLeft && 
                (owner.movement.facing == Facing.Right || owner.movement.facing == Facing.TurningRight))
            {
                heading = -1;
                turning = true;
            }
            else if (targetIsToTheRight &&
               (owner.movement.facing == Facing.Left || owner.movement.facing == Facing.TurningLeft))
            {
                heading = 1;
                turning = true;
            }

        }

        if(state == AIState.InTransition || state == AIState.Relocating)
        {
            Ground next = lastOn.GetNextGroundTowards(goingTo.ground);
            Vector3 link = lastOn.GetTo(goingTo.ground);
            Vector3 destinationLinkPos = lastOn.startPointToEndPoint[link];

            bool linkFromLeftToRight = link.x < destinationLinkPos.x;

            float selfDistance = owner.GetScale().x/2;

            float targetX = link.x 
                            + (selfDistance) * (linkFromLeftToRight ? -1 : 1)
                            + (linkFromLeftToRight ? -PRECISION : PRECISION);

            bool linkIsToTheRight = currentX < targetX;

            if (DevelopmentSettings.SHOW_DESTINATIONS)
            {
                DrawLineTo(goingTo.worldpos, Color.green);
            }

            if (state == AIState.InTransition)
            {

                currentTransitionTime += Time.deltaTime;

                if (DevelopmentSettings.SHOW_DESTINATIONS)
                {
                    DrawLineTo(link, Color.red);
                    DrawLineTo(destinationLinkPos, Color.blue);
                }
                
                //DrawLineTo(new Vector3(targetX, destinationLinkPos.y), Color.yellow);

                //Steer towards the target
                if(currentTransitionTime > MAX_TRANSITION_TIME)
                {
                    state = AIState.Hunting;

                }else if (next.GetSurfaceY() < owner.GetCenterPos().y)
                {
                    if (
                        next.IsOn(owner.GetCenterPos() - new Vector3(selfDistance, 0))
                        &&
                        next.IsOn(owner.GetCenterPos() + new Vector3(selfDistance, 0))
                        )
                    {
                        if (owner.IsGrounded())
                        {
                            state = AIState.Hunting;
                        }
                        else
                        {
                            //Wait to land
                        }
                    }
                    else
                    {
                        heading = linkFromLeftToRight ? 1 : -1;
                    }
                }
                else if (owner.stats.GetValuePercentage(Stat.JumpForce) > 0)
                {
                    //Steer towards the ideal ascending spot
                    if (targetX + PRECISION > currentX && targetX - PRECISION < currentX)
                    {
                        heading = 0;
                    }
                    else if (linkIsToTheRight)
                    {
                        heading = 1;
                    }
                    else
                    {
                        heading = -1;
                    }
                }
                else
                {
                    state = AIState.Hunting;
                }

                jumping = true;

            }

            if (state == AIState.Relocating)
            {
                currentRelocationTime += Time.deltaTime;

                //Try jumping if stuck for too long
                if (!waitingForJumpRecharge)
                {
                    if (currentTryingToGetUnstuckTime < MAX_GET_UNSTUCK_TIME)
                    {
                        currentTryingToGetUnstuckTime += Time.deltaTime;

                        if (DevelopmentSettings.SHOW_DESTINATIONS)
                        {
                            DrawLineTo(owner.GetCenterPos() + new Vector3(0, 5), Color.red);
                        }

                        jumping = true;
                    }
                    else
                    {
                        if (xProgressSinceLastFrame == 0)
                        {
                            currentStuckTime += Time.deltaTime;
                        }
                        else
                        {
                            currentStuckTime = 0;
                        }

                        if (currentStuckTime > MAX_STUCK_BEFORE_JUMP_TIME && owner.stats.GetValuePercentage(Stat.JumpForce) == 1)
                        {
                            currentTryingToGetUnstuckTime = 0;
                        }

                    }
                }
                

                if (goingTo.valid)
                {
                    //Moving on the same ground
                    if (goingTo.ground == lastOn)
                    {
                        if(goingTo.worldpos.x+PRECISION > currentX && goingTo.worldpos.x - PRECISION < currentX)
                        {
                            heading = 0;
                            state = AIState.Hunting;
                            currentTryingToGetUnstuckTime = MAX_GET_UNSTUCK_TIME;
                            currentStuckTime = 0;
                        }
                        else if (goingTo.worldpos.x > owner.GetCenterPos().x)
                        {
                            heading = 1;
                        }
                        else
                        {
                            heading = -1;
                        }
                        waitingForJumpRecharge = false;
                    }
                    else
                    {
                        if (next.GetSurfaceY() < lastOn.GetSurfaceY())
                        {
                            if (linkFromLeftToRight)
                            {
                                heading = 1;
                            }
                            else
                            {
                                heading = -1;
                            }
                            waitingForJumpRecharge = false;
                        }
                        else
                        {
                            if (targetX + PRECISION > currentX && targetX - PRECISION < currentX)
                            {
                                //Wait to recharge jump force
                                if (owner.stats.GetValuePercentage(Stat.JumpForce) == 1)
                                {
                                    state = AIState.InTransition;
                                    currentTransitionTime = 0;
                                    currentStuckTime = 0;
                                    currentTryingToGetUnstuckTime = MAX_GET_UNSTUCK_TIME;
                                    waitingForJumpRecharge = false;
                                }
                                else
                                {
                                    waitingForJumpRecharge = true;
                                }
                            }
                            else if (linkIsToTheRight)
                            {
                                heading = 1;
                                waitingForJumpRecharge = false;
                            }
                            else
                            {
                                heading = -1;
                                waitingForJumpRecharge = false;
                            }

                        }
                    }
                }
            }
        }

        if(distance > IDLE_TIMER_START_WHEN_AT_DISTANCE)
        {
            currentIdleTimer += Time.deltaTime;
        }
        else
        {
            currentIdleTimer = 0;
        }
        if(currentIdleTimer > TIME_UNTIL_IDLE)
        {
            state = AIState.Idle;
            Global.Resources[EffectNames.QuestionMark].Spawn(owner.GetHeadPos()+ new Vector3(0,2));
        }
        else
        {

            shoot = facingTarget && !turning;
            attack = distance < MELEE_RANGE && facingTarget;

            if (rifles.Count > 0 && shoot)
            {
                currentRifleTimer += Time.deltaTime;
                if (currentRifleTimer > rifleTimer)
                {
                    Shoot(rifles,target);
                    currentRifleTimer = 0;
                }
            }
            if (mortarsAndML.Count > 0 && shoot)
            {
                currentMortarTimer += Time.deltaTime;
                if (currentMortarTimer > mortarTimer)
                {
                    Shoot(mortarsAndML,target);
                    currentMortarTimer = 0;
                }
            }
            if (weapons.Count > 0 && attack)
            {
                Attack(weapons,target);
            }

            xLastFrame = owner.GetCenterPos().x;

            owner.mech.Tick(actionspeed, Vector3.zero, Vector3.zero, target);
            //AttackShootBlock(shoot, attack, false, actionspeed);

            owner.movement.UpdateMovement(jumping, heading, transformAction, shoot, actionspeed);

            

        }
    }
}

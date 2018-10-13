using System.Collections;
using System.Collections.Generic;
using UnityEngine;



public class AISquad : AIBasic{

    public static readonly float DISTANCE_MAX_SEARCH = 50;
    public static readonly float DISTANCE_INCREASE_PER_SEARCH = 10;

    public static readonly float DISTANCE_PREFERRED = 15;
    public static readonly float DISTANCE_TOO_CLOSE = 10;
    public static readonly float DISTANCE_LAST_STAND = 1;


    public static readonly float RECALCULATE_POSITIONS = 0.75f;
    public static readonly float REACTION_RANDOM_VARIANCE = 0.25f;
    public static readonly float VISIBILITY_CHECK_TIME = 0.25f;
    public static readonly int NUMBER_OF_SQUAD_MEMBERS_TO_LOOK_WHEN_IDLE = 3;

    public Squad squad;


    private float timeSinceLastVisibilityCheck;
    public Faction enemyToThisSquad;

    public float idleTime = IDLE_REACTION_TIME;
    public float recalcPositionsTime = RECALCULATE_POSITIONS;
    

    //Framebased variables
    bool movedLastFrame = false;
    bool allAimingForTarget = false;

    public AISquad()
    {
        squad = new Squad();
        state = AIState.Idle;
        timeSinceLastChange = 0;
        enemyToThisSquad = Global.FACTION_PLAYER;
        idleTime = GetRandomVariance(IDLE_REACTION_TIME);
        recalcPositionsTime = GetRandomVariance(RECALCULATE_POSITIONS);
    }

    public GameUnit AddUnit(GameUnit gu)
    {
        if(gu.itemEquiper != null && gu.navMeshAgent != null && gu.body != null && gu.character != null)
        {
            squad.members.Add(gu);
            gu.squad = squad;
        }
        else
        {
            Debug.Log("Character cannot be AI-Controlled, missing objects: " + gu.uniqueName);
        }
        return gu;
    }
    public float GetRandomVariance(float orig)
    {
        return orig+(float)Global.instance.rand.NextDouble() * REACTION_RANDOM_VARIANCE;
    }
	
	// Update is called once per frame
	void Update () {

        timeSinceLastChange += Time.deltaTime;


        if (state == AIState.Idle)
        {
            if(timeSinceLastChange > idleTime)
            {
                timeSinceLastChange = 0;
                for(int i = 0; i < Mathf.Min(squad.members.Count, NUMBER_OF_SQUAD_MEMBERS_TO_LOOK_WHEN_IDLE); i++)
                {
                    GameUnit enemy = LookForEnemy(squad.members.RandomElement(), enemyToThisSquad);
                    if(enemy != null)
                    {
                        state = AIState.Hunting;
                        target = enemy;
                        squad.AssignReactionTimesToAllMembers(0.01f, 0.5f);
                        break;
                    }
                }
            }
        }
        else
        {
            if (timeSinceLastChange > recalcPositionsTime)
            {
                timeSinceLastChange = 0;

                if (IsTargetTooClose())
                {

                    Dictionary<Ground, float> possibleMoves;
                    int placed = 0;
                    int reserves = 0;

                    for (float i = DISTANCE_PREFERRED; i < DISTANCE_MAX_SEARCH; i += DISTANCE_INCREASE_PER_SEARCH)
                    {
                        possibleMoves = GetPossibleMovesAtDistanceFromTarget(i);
                        placed = 0;
                        reserves = 0;
                        foreach (Ground p in possibleMoves.Keys)
                        {
                            reserves = squad.currentFormation.Move(p, possibleMoves[p], placed, Squad.DEFAULT_SQUAD_SOLDIER_WIDTH);
                            placed = squad.members.Count - reserves;
                            //Debug.Log(placed + " soldiers going to: " + p.obj.name);
                            if (reserves == 0)
                            {
                                break;
                            }
                        }
                        if (possibleMoves.Count > 0 && reserves == 0)
                        {
                            squad.AssignReactionTimesToAllMembers(0.01f, 0.5f);
                            break;
                        }
                    }
                    squad.currentFormation.RecalculateClosestPositions();
                }else if (movedLastFrame) {
                    squad.currentFormation.RecalculateClosestPositions();
                }
            }

            if (state == AIState.Hunting)
            {
                //Only aim those that don't
                if (!allAimingForTarget)
                {
                    allAimingForTarget = squad.TellAllMembersToAimFor(target);
                }
                
                squad.TurnAllMembersTowardsTarget(target);

                timeSinceLastVisibilityCheck += Time.deltaTime;
                if (timeSinceLastVisibilityCheck > VISIBILITY_CHECK_TIME)
                {
                    squad.TellMembersThatCanSeeToShoot(target);
                    timeSinceLastVisibilityCheck = 0;
                }
            }

            movedLastFrame = squad.UpdateCharacterMove(target,DISTANCE_PREFERRED,DISTANCE_LAST_STAND);
        }
      
	}


    protected Dictionary<Ground, float> GetPossibleMovesAtDistanceFromTarget(float distance)
    {
        Dictionary<Ground, float> possibleMoves = new Dictionary<Ground, float>();
        // if (NavMeshAttachor.generated.ContainsKey(squad.currentFormation.currentlyOn))
        //{
        Ground currentGround = squad.currentFormation.currentlyOn; //NavMeshAttachor.generated[character.lastWalkedOn];
            if (CanMoveRightToPreferredDistance(currentGround))
            {
                possibleMoves.Add(currentGround, target.body.position.x + distance);
            }
            else if (CanMoveLeftToPreferredDistance(currentGround))
            {
                possibleMoves.Add(currentGround, target.body.position.x - distance);
            }

            Collider[] considerations = Physics.OverlapSphere(target.body.position, distance);

            foreach (Collider c in considerations)
            {
                if (Global.Grounds.Contains(c.transform))
                {
                    Ground consideration = Global.Grounds[c.transform];

                    foreach (Vector3 link in currentGround.links)
                    {
                        if (
                                //TODO: Should also take Y into consideration
                                //Move Right
                                (
                                squad.currentFormation.GetFormationCenter().x > target.body.position.x
                                && link.x > target.body.position.x
                                && currentGround.startPointToEndPoint[link].x >= link.x
                                && currentGround.distances[link].Contains(consideration)
                                )
                                ||
                                // Move left
                                (
                                squad.currentFormation.GetFormationCenter().x < target.body.position.x
                                && link.x < target.body.position.x
                                && currentGround.startPointToEndPoint[link].x <= link.x
                                && currentGround.distances[link].Contains(consideration)
                                )
                            )
                        {
                            // Should not always pick the midpoint
                            possibleMoves.Add(consideration, consideration.GetMidPoint().x);
                        }
                    }
                }
            }
       // }
        return possibleMoves;
    }
    private bool CanMoveRightToPreferredDistance(Ground on)
    {
        return squad.currentFormation.GetFormationCenter().x > target.body.position.x && target.body.position.x + DISTANCE_PREFERRED < on.obj.position.x + on.obj.localScale.x / 2;
    }
    private bool CanMoveLeftToPreferredDistance(Ground on)
    {
        return squad.currentFormation.GetFormationCenter().x < target.body.position.x && target.body.position.x - DISTANCE_PREFERRED > on.obj.position.x - on.obj.localScale.x / 2;
    }
    protected bool IsTargetTooClose()
    {
        Vector2 range = squad.GetXRangeOfMembers();
        return squad.IsTargetTooClose(
            target,
            range.x,
            range.y,
            squad.currentFormation.GetFormationCenter().y,
            squad.currentFormation.GetFormationCenter().y,
            DISTANCE_TOO_CLOSE
            );
        /*return  (
                    (target.body.position.x + DISTANCE_TOO_CLOSE > range.x && target.body.position.x < range.x)
                    ||
                    (target.body.position.x - DISTANCE_TOO_CLOSE < range.y && target.body.position.x > range.y)
                )
               &&
               Mathf.Abs(squad.currentFormation.GetFormationCenter().y - target.body.position.y) < DISTANCE_TOO_CLOSE
               ;*/
    }


}

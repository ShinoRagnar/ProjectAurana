using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum AIState
{
    Idle,
    Hunting,
    Relocating,
    InTransition,
    Fleeing
}
public class AIBasic : GameUnitBodyComponent
{

    public static readonly int MAX_MOVE_SEARCH_DISTANCE = 30;

    public struct Move
    {
        public bool valid;
        public Ground ground;
        public Vector3 worldpos;

        public Move(Ground ground, Vector3 worldpos)
        {
            this.ground = ground;
            this.worldpos = worldpos;
            this.valid = true;
        }
    }


    public static readonly float IDLE_REACTION_TIME = 0.25f;
    public static readonly float REACTION_TIME = 0.25f;
    public static readonly float REACTION_VARIANCE = 0.20f;

    public AIState state;
    protected float timeSinceLastChange;
    public GameUnit target;
    public Move goingTo;

    //public float actionSpeedLastFrame = 1;

    public float reactionTime = REACTION_TIME;
    public float currentReactionTime = 0;


    public void SetNewReactionTime()
    {
        currentReactionTime = 0;
        reactionTime = REACTION_TIME + Random.Range(-REACTION_VARIANCE / 2f, REACTION_VARIANCE / 2f);
    }


    public void DrawLineTo(Vector3 pos, Color color)
    {
        Vector3 direction = pos - owner.GetCenterPos();
        Debug.DrawRay(owner.GetCenterPos(), direction, color);
    }

    // public GameUnit owner;

   /* protected void AttackShootBlock(bool shooting, bool attack, bool block, float actionspeed)
    {
        if(owner.mech != null)
        {

            owner.mech.MountShake(actionspeed);

            foreach (Core c in owner.mech.equippedCores)
            {

                c.Tick(actionspeed);

                if(actionspeed != actionSpeedLastFrame && c.meleeAnimator != null)
                {
                    c.meleeAnimator.speed = actionspeed;
                }

                c.AimAndShootAt(target, shooting,actionspeed);
                c.AttackWithWeapon(attack);
                c.BlockWithShield(block);
            }
        }
        actionSpeedLastFrame = actionspeed;
    }*/

    protected GameUnit LookForEnemy(GameUnit looker, ListHash<Faction> lookForCharacterOfThisFaction)
    {
        if(lookForCharacterOfThisFaction != null)
        {
            foreach (Faction f in lookForCharacterOfThisFaction)
            {
                GameUnit found = LookForEnemy(looker, f);
                if (found != null)
                {
                    return found;
                }
            }
        }
        return null;
    }


    protected GameUnit LookForEnemy(GameUnit looker, Faction lookForCharacterOfThisFaction)
    {
        if(lookForCharacterOfThisFaction != null && GameUnit.unitsByFaction.Contains(lookForCharacterOfThisFaction))
        {
            ListHash<GameUnit> possibleTargets = GameUnit.unitsByFaction[lookForCharacterOfThisFaction];

            foreach (GameUnit possibleTarget in possibleTargets)
            {
                if (possibleTarget.body != null && looker.body != null)
                {
                    if (looker.senses.CanSee(possibleTarget))
                    {
                        return possibleTarget;
                    }
                    else if (looker.senses.CanHear(possibleTarget))
                    {
                        return possibleTarget;
                    }
                }
            }
        }
        return null;
    }

    protected Move GetMoveAtDistanceFromTarget(float preferredDistance, bool isMelee)
    {
        Ground targetGround = target.GetLastOnGround();
        Ground selfGround = owner.GetLastOnGround();

        if (targetGround != null && selfGround != null)
        {

            bool targetIsToTheRight = owner.GetCenterPos().x < target.GetCenterPos().x;
            bool targetIsToTheLeft = owner.GetCenterPos().x > target.GetCenterPos().x;

            //See if we can find a position at the same ground
            if (targetGround == selfGround || isMelee)
            {
                Vector3 leftOfTarget = target.GetCenterPos() - new Vector3(preferredDistance, 0);
                Vector3 rightOfTarget = target.GetCenterPos() + new Vector3(preferredDistance, 0);

                if (targetIsToTheRight)
                {
                    if (selfGround.IsOn(leftOfTarget))
                    {
                        return new Move(targetGround, leftOfTarget);

                    }else if (isMelee)
                    {
                        return new Move(targetGround, target.GetCenterPos());
                    }
                }
                else if (targetIsToTheLeft)
                {
                    if (selfGround.IsOn(rightOfTarget))
                    {
                        return new Move(targetGround, rightOfTarget);
                    }
                    else if (isMelee)
                    {
                        return new Move(targetGround, target.GetCenterPos());
                    }
                }
            }

            //Search for a ground at the specified distance
            ListHash<Ground> candidates = selfGround.GroundsAtDistanceFromTarget(target, preferredDistance, MAX_MOVE_SEARCH_DISTANCE);

            //Debug.Log("Candidates: "+candidates.Count+" preferred dist: "+ preferredDistance);

            Ground bestCandidate = null;
            Vector3 bestMovePos = new Vector3(0, 0, 0);
            float bestDistance = 0;

            foreach (Ground ret in candidates)
            {
                Vector3 left = ret.GetLeftPointAtDistance(target.GetCenterPos(), preferredDistance);
                Vector3 right = ret.GetRightPointAtDistance(target.GetCenterPos(), preferredDistance);
                float leftDist = Vector3.Distance(owner.GetCenterPos(), left);
                float rightDist = Vector3.Distance(owner.GetCenterPos(), right);

                if (ret.IsOn(left) && (leftDist < bestDistance || bestCandidate == null))
                {
                    bestMovePos = left;
                    bestDistance = leftDist;
                    bestCandidate = ret;
                }

                if (ret.IsOn(right) && (rightDist < bestDistance || bestCandidate == null))
                {
                    bestMovePos = right;
                    bestDistance = rightDist;
                    bestCandidate = ret;
                }

            }

            if(bestCandidate != null)
            {
                return new Move(bestCandidate, bestMovePos);
            }
        }
        return new Move();
    }

    /*protected Move GetPossibleMoveAtSameGroundAsTarget(float minDistance, bool canCrossPlayer)
    {
        Ground g = target.GetLastOnGround();

        bool targetIsToTheRight = owner.GetCenterPos().x < target.GetCenterPos().x;
        bool targetIsToTheLeft = owner.GetCenterPos().x > target.GetCenterPos().x;

        if (g == null) { return new Move(); }

        Vector3 closestDistanceInFront = new Vector3(target.GetCenterPos().x +
            (targetIsToTheRight ? -minDistance : minDistance),
            g.GetSurfaceY());

        Vector3 closestDistanceBehind = new Vector3(target.GetCenterPos().x +
            (targetIsToTheLeft ? minDistance : -minDistance),
            g.GetSurfaceY());

        float cdif = Vector3.Distance(closestDistanceInFront, owner.GetCenterPos());
        float cdb = Vector3.Distance(closestDistanceBehind, owner.GetCenterPos());

        if(cdif < cdb && 
            ((targetIsToTheRight || canCrossPlayer) && g.IsOn(closestDistanceInFront)))
        {
            return new Move(g, closestDistanceInFront);

        }else if(((targetIsToTheLeft || canCrossPlayer) && g.IsOn(closestDistanceInFront)))
        {
            return new Move(g, closestDistanceBehind);
        }

        return new Move();

    }*/


   /* protected Move GetPossibleMoveAtDistanceFromTarget(

       // bool onlySameGround,
        bool canCrossPlayer,
        float minDistance,
        float maxDistance

        // bool checkLineOfSight,
        //float minDistance,
        //float maxDistance

        )
    {

        if (target != null)
        {

            Collider[] considerations = Physics.OverlapSphere(target.body.position, minDistance);

            foreach (Collider c in considerations)
            {
                if (NavMeshAttachor.generated.Contains(c.transform))
                {
                    Ground consideration = NavMeshAttachor.generated[c.transform];


                }
            }




            //Ground g = target.GetLastOnGround();

            Move m = GetPossibleMoveAtSameGroundAsTarget(minDistance, canCrossPlayer);
            if (m.valid)
            {
                return m;
            }
            else if(owner.GetLastOnGround() != null && !onlySameGround)
            {
                
                ListHash<Ground> grounds = owner.GetLastOnGround().GroundsAtDistanceFromTarget(target, maxDistance, minDistance, MAX_MOVE_SEARCH_DISTANCE, null);

                Debug.Log("Grounds found: " +grounds.Count);
                //Sorted least steps to most steps
                foreach (Ground ground in grounds)
                {
                    for (float x = ground.GetLeftSide().x; x < ground.GetRightSide().x; x += 1f)
                    {
                        Vector3 pos = new Vector3(x, ground.GetSurfaceY(), 0);
                        float dist = Vector3.Distance(pos, target.GetCenterPos());

                        if (dist > minDistance && dist < maxDistance)
                        {
                            return new Move(ground, pos);
                        }
                    }
                }
            }
            else
            {
                Debug.Log("No ground");
            }
        }
        else
        {
            Debug.Log("No target");
        }
        return new Move();

       
    }*/

    }

/*Dictionary<Ground, float> possibleMoves = new Dictionary<Ground, float>();
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
       */

/* foreach (Collider c in considerations)
 {
     if (NavMeshAttachor.generated.Contains(c.transform))
     {
         Ground consideration = NavMeshAttachor.generated[c.transform];

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
 return possibleMoves;*/

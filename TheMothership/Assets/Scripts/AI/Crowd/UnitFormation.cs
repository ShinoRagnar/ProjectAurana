using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public enum PlacementStrategy
{
    MiddleAndOut,
    LeftToRight,
    RightToLeft
}
/*public enum Direction
{
    Towards,
    Against,
    Right,
    Left
}*/
public class UnitFormation  {

    public static float GROUND_MARGIN = 0.5f;

    public PlacementStrategy currentStrategy;

    public DictionaryList<int, Vector3> placements;

    public DictionaryList<GameUnit, int> placedUnits;
    DictionaryList<GameUnit, int> newPlacedUnits = new DictionaryList<GameUnit, int>();
    DictionaryList<Vector3, int> placementsReverse = new DictionaryList<Vector3, int>();

    public Ground currentlyOn;
    public float currentlyAtX;

    public int reserves;
    public int placed;


    public UnitFormation(PlacementStrategy strategy)
    {
        this.currentStrategy = strategy;
        this.placements = new DictionaryList<int, Vector3>();
        this.placedUnits = new DictionaryList<GameUnit,int>();
        this.reserves = 0;
        this.placed = 0;
    }
    public int Move(Ground ground, float xStart, int offset, float unitWidth)
    {
        return ProjectOn(ground, xStart, unitWidth, offset, placedUnits.Count);
    }
    public void Place(int i, GameUnit unit)
    {
        placedUnits.Add(unit,i);
    }
    public void RecalculateClosestPositions()
    {
        newPlacedUnits.Clear();
        placementsReverse.Clear();

        Vector3[] placSort = new Vector3[placements.Count];

        float memberSumX = 0;
        float placemSumX = 0;

        //Find average x pos for units - This determines the direction we are heading
        foreach(GameUnit gu in placedUnits)
        {
            if (gu.isActive)
            {
                memberSumX += gu.body.position.x;
            }
            else
            {
                placedUnits.RemoveLater(gu);
            }
        }
        placedUnits.Remove();

        // 
        foreach (int i in placements)
        {
            Vector3 plac = placements[i];
            if (!placementsReverse.Contains(plac))
            {
                placementsReverse.Add(plac, i);
            }
            placSort[i] = plac;
            placemSumX += plac.x;
        }

        // Going left to right
        if (memberSumX < placemSumX)
        {
            System.Array.Sort(placSort, LeftToRightMovement);
        }
        else
        {
            System.Array.Sort(placSort, RightToLeftMovement);
        }
        
        //Find closest pos
        for(int p = 0; p < placSort.Length; p++)
        {
            Vector3 plac = placSort[p];

            GameUnit closest = null;
            foreach (GameUnit g in placedUnits)
            {
                if (!newPlacedUnits.Contains(g))
                {
                    if (closest == null)
                    {
                        closest = g;
                    }
                    else if (Vector3.Distance(g.body.position, plac) < Vector3.Distance(closest.body.position, plac))
                    {
                        closest = g;
                    }
                }
            }
            if (closest != null)
            {
                
                newPlacedUnits.Add(closest, placementsReverse[plac]);
            }
        }

        placedUnits = newPlacedUnits;

    }
    public Vector3 GetMoveFor(GameUnit unit)
    {
        if (placedUnits.Contains(unit))
        {
            return placements[placedUnits[unit]];
        }
        return Vector3.zero;
    }
    public Vector3 GetFormationCenter()
    {
        float x = currentlyAtX;
        float y = currentlyOn.GetMidPoint().y;
        float z = 0;

        return new Vector3(x, y, z);
    }
    public int ProjectFormationOn(Ground ground, float xStart, float unitWidth, int numberOfUnits)
    {
        return ProjectOn(ground, xStart, unitWidth,0, numberOfUnits);
    }
    private int ProjectOn(Ground ground, float xStart, float unitWidth, int offset, int numberOfUnits)
    {

        reserves = 0;
        placed = offset;

        currentlyOn = ground;
        currentlyAtX = xStart;

        Vector3 center = GetFormationCenter();

        float x = center.x;
        float y = center.y;
        float z = center.z;

        float width = ground.obj.localScale.z - GROUND_MARGIN * 2;
        float unitsInRank = Mathf.FloorToInt(width / unitWidth);
        float maxRanks = ground.obj.localScale.x/ unitWidth;
        //Debug.Log("Width: " + width + " unitsinRank: " + unitsInRank + " maxRanks" + maxRanks);

        for (int i = 0; i < unitsInRank*(maxRanks); i++)
        {
            reserves = numberOfUnits - placed;
            if(reserves == 0)
            {
                break;
            }
            int currentRank = Mathf.FloorToInt(((float)i) / unitsInRank);
            int positionWithinRank = i % (int)unitsInRank;

            if (currentStrategy == PlacementStrategy.LeftToRight)
            {
                x = xStart + currentRank * unitWidth;
            }
            else if (currentStrategy == PlacementStrategy.LeftToRight)
            {
                x = xStart - currentRank * unitWidth;
            }
            else if (currentStrategy == PlacementStrategy.MiddleAndOut)
            {

                float rankLeft = Mathf.Ceil(((float)currentRank) / 2.0f);
                float rankRight = currentRank / 2;
                float rank = 0;

                if (currentRank % 2 == 1)
                {
                    rank = -rankLeft;
                }
                else
                {
                    rank = rankRight;
                }

                x = xStart - rank * unitWidth;
            }

            if (positionWithinRank % 2 == 1)
            {
                z = -Mathf.Ceil(((float)positionWithinRank) / 2.0f) * unitWidth;
            }
            else
            {
                z = (positionWithinRank / 2) * unitWidth;
            }
            Vector3 placement = new Vector3(x, y, z);
            if(CanPlace(ground, placement))
            {
                if (placements.Contains(placed))
                {
                    placements[placed] = placement;
                }
                else
                {
                    placements.Add(placed, placement);
                }
                placed++;
            }
        }
        
        return reserves;
    }
    

    private bool CanPlace(Ground g, Vector3 position)
    {
        if(Mathf.Abs(position.z-g.obj.position.z) > Mathf.Abs(g.obj.transform.localScale.z/2 - GROUND_MARGIN))
        {
            return false;
        }else if (Mathf.Abs(position.x-g.obj.position.x) > Mathf.Abs(g.obj.transform.localScale.x / 2 - GROUND_MARGIN))
        {
            return false;
        }
        return true;
    }
    private int LeftToRightMovement(Vector3 value1, Vector3 value2)
    {
        if (value1.x > value2.x)
        {
            return -1;
        }
        else if (value1.x == value2.x)
        {
                if (value1.z > value2.z)
                {
                    return -1;
                }
                else if (value1.z == value2.z)
                {
                    return 0;
                }
                else
                {
                    return 1;
                }

        }
        else
        {
            return 1;
        }
    }
    private int RightToLeftMovement(Vector3 value1, Vector3 value2)
    {
        if (value1.x < value2.x)
        {
            return -1;
        }
        else if (value1.x == value2.x)
        {
            if (value1.z > value2.z)
            {
                return -1;
            }
            else if (value1.z == value2.z)
            {
                return 0;
            }
            else
            {
                return 1;
            }

        }
        else
        {
            return 1;
        }
    }
    /*
    public static ArrayList ProjectFormationOn(PlacementStrategy strategy, Ground ground, float xStart, float unitWidth, int numberOfUnits)
    {
    }*/
}

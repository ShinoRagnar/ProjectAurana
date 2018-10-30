using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Ground {

    public GroundHints hints;
    public Transform obj;
    public DictionaryList<Vector3, Ground> links;
    public DictionaryList<Vector3, Vector3> startPointToEndPoint;
    public DictionaryList<Vector3, DictionaryList<Ground, int>> distances;

    // public ListHash<Ground> group;
    // public Color groupColor;

    private static ListHash<Ground> search = new ListHash<Ground>();
    private static ListHash<Ground> next = new ListHash<Ground>();
    private static ListHash<Ground> curr = new ListHash<Ground>();
    private static ListHash<Ground> fdd = new ListHash<Ground>();

    public Ground(Transform groundObject)
    {
        this.obj = groundObject;
        this.links = new DictionaryList<Vector3, Ground>();
        this.startPointToEndPoint = new DictionaryList<Vector3, Vector3>();
        this.distances = new DictionaryList<Vector3, DictionaryList<Ground, int>>();
    }
    public Vector3 GetMidPoint()
    {
        return new Vector3(obj.transform.position.x, obj.transform.position.y + obj.transform.localScale.y / 2);
    }
    public Vector3 GetLeftSide()
    {
        return new Vector3(obj.transform.position.x - obj.transform.localScale.x / 2,
            obj.transform.position.y + obj.transform.localScale.y / 2);
    }
    public float GetDepth()
    {
        return obj.transform.localScale.z;
    }

    public Vector3 GetRightSide()
    {
        return new Vector3(obj.transform.position.x + obj.transform.localScale.x / 2,
            obj.transform.position.y + obj.transform.localScale.y / 2);
    }

    public bool IsOnSameLevel(Ground g, bool yAxis) {

        if (yAxis) {

            float topY = GetSurfaceY(0);
            float bottomY = GetBottomY();

            return (g.GetSurfaceY(0) >= bottomY && g.GetSurfaceY(0) <= topY) || (g.GetBottomY() >= bottomY && g.GetBottomY() <= topY);
        }
        else
        {
            float leftX = GetLeftSide().x;
            float rightX = GetRightSide().x;

            return (g.GetLeftSide().x >= leftX && g.GetLeftSide().x <= rightX) || (g.GetRightSide().x >= leftX && g.GetRightSide().x <= rightX);
        }
    }

    public float Distance(Ground g, bool yAxis) {

        if (yAxis)
        {
            return
                Mathf.Min(
                    Mathf.Min(
                        Mathf.Abs(g.GetSurfaceY(0) - GetSurfaceY(0)),
                            Mathf.Min(
                                Mathf.Abs(g.GetBottomY() - GetSurfaceY(0)), 
                                Mathf.Abs(g.GetSurfaceY(0) - GetBottomY())
                                )
                            ),
                Mathf.Abs(g.GetBottomY() - GetBottomY())
                );

        }
        else {
            return
                Mathf.Min(
                    Mathf.Min(
                        Mathf.Abs(g.GetLeftSide().x - GetLeftSide().x),
                             Mathf.Min
                                (Mathf.Abs(g.GetLeftSide().x - GetRightSide().x), 
                                Mathf.Abs(g.GetRightSide().x - GetLeftSide().x))
                            ),
                Mathf.Abs(g.GetRightSide().x - GetRightSide().x)
                );


           ;
        }

    }

    public bool IsWithinOvarlappingDistance(Ground g, float distance) {

        bool ret = (IsOnSameLevel(g, true) && Distance(g, false) <= distance) || (IsOnSameLevel(g, false) && Distance(g, true) <= distance);

        if (false) {
            Debug.Log("(IsOnSameLevel(g, true) "+ IsOnSameLevel(g, true)+ " Distance(g, false) <= distance) "+ Distance(g, false));
            Debug.Log("(IsOnSameLevel(g, false) " + IsOnSameLevel(g, false) + " Distance(g, true) " + Distance(g, true));
        }

        return ret;
    }

    public Vector3 GetRightPointAtDistance(Vector3 from, float distance)
    {
        float y = Mathf.Abs(from.y - GetSurfaceY());
        float d = Mathf.Sqrt(distance * distance - y * y);
        return new Vector3(from.x + d, GetSurfaceY());
    }

    public Vector3 GetLeftPointAtDistance(Vector3 from, float distance)
    {
        float y = Mathf.Abs(from.y - GetSurfaceY());
        float d = Mathf.Sqrt(distance * distance - y * y);
        return new Vector3(from.x - d, GetSurfaceY());
    }

    public Ground GetNextGroundTowards(Ground g)
    {
        if(g == this) { return this; }

        return links[GetTo(g)];
    }

    public Vector3 GetTo(Ground g)
    {
        int shortest = -1;
        Vector3 retLink = links.GetFirst();

        foreach(Vector3 link in links)
        {
            if (distances[link].Contains(g))
            {
                if(shortest == -1 || distances[link][g] < shortest)
                {
                    shortest = distances[link][g];
                    retLink = link;
                }
            }
        }
        return retLink;
    }

    /*public void JoinGroupOf(Ground g)
    {
        group = g.group;
        groupColor = g.groupColor;
        obj.GetComponent<MeshRenderer>().material.color = groupColor;
        group.AddIfNotContains(this);
    }

    public void StartGroupWith(Ground g)
    {
        group = new ListHash<Ground>();
        groupColor = Random.ColorHSV();
        JoinGroupOf(this);
        g.JoinGroupOf(this);
    }*/



 /*   public int StepsToTravelToWithout(Ground to, Ground without = null)
    {
        if(without == null)
        {
            Vector3 link = distances.GetFirst();
            float dist = distances[link][to];

            foreach (Vector3 d in distances)
            {
                if (distances[d].Contains(to)){
                    if(distances[d][to] < dist)
                    {
                        link = d;
                        dist = distances[d][to];
                    }
                }
            }
            return distances[link][to];
        }

        search.Clear();
        curr.Clear();
        next.Clear();

        curr.Add(this);

        return StepsTo(search, curr, next, 0, without);
    }

    private int StepsTo(    ListHash<Ground> searched, 
                            ListHash<Ground> current, 
                            ListHash<Ground> nextStep, 
                            int steps,  
                            Ground without = null)
    {
        if(current.Count == 0)
        {
            return -1;
        }

        nextStep.Clear();

        foreach(Ground to in current)
        {
            foreach (Vector3 link in to.links)
            {
                Ground linkg = links[link];

                if(linkg != without && !searched.Contains(linkg))
                {
                    searched.Add(linkg);
                    nextStep.AddIfNotContains(linkg);
                }

                if (linkg == to) { return steps; }
            }
        }
        return StepsTo(searched, nextStep, current, steps+1, without);
    }
    */

    public ListHash<Ground> GroundsAtDistanceFromTarget(
         GameUnit target,
         float distance,
         int maxSteps,
         Ground without = null)
    {

        search.Clear();
        curr.Clear();
        next.Clear();
        fdd.Clear();

        curr.Add(this);

        //Found self
        if (IsOn(GetRightPointAtDistance(target.GetCenterPos(), distance))
            || 
            IsOn(GetLeftPointAtDistance(target.GetCenterPos(), distance)))
        {
            fdd.Add(this);
        }

        return GroundsAtDistanceFromTarget(target, search, curr, next, fdd, distance, 0, maxSteps, without);
    }
    
    private ListHash<Ground> GroundsAtDistanceFromTarget(
                        GameUnit target,
                        ListHash<Ground> searched,
                        ListHash<Ground> current,
                        ListHash<Ground> nextStep,
                        ListHash<Ground> found,
                        float distance,
                        int steps,
                        int maxSteps = 30,
                        Ground without = null)
    {
        if (current.Count == 0 || steps >= maxSteps)
        {
            return found;
        }

        nextStep.Clear();

        foreach (Ground to in current)
        {
            foreach (Vector3 link in to.links)
            {
                Ground linkg = to.links[link];

                if (linkg != without && !searched.Contains(linkg))
                {
                    searched.Add(linkg);
                    nextStep.AddIfNotContains(linkg);

                    if( linkg.IsOn(linkg.GetRightPointAtDistance(target.GetCenterPos(),distance))
                        || 
                        linkg.IsOn(linkg.GetLeftPointAtDistance(target.GetCenterPos(), distance))
                    )
                    {
                        found.Add(linkg);
                    }
                }
            }
        }
        return GroundsAtDistanceFromTarget(target, searched, nextStep, current, found, distance, steps+1, maxSteps, without);
    }
    
    public float GetLength()
    {
        return obj.transform.localScale.x;
    }

    public float GetSurfaceY(float margin = 0.1f)
    {
        return obj.transform.position.y + obj.transform.localScale.y / 2+ margin;
    }

    public float GetBottomY()
    {
        return obj.transform.position.y - obj.transform.localScale.y / 2;
    }

    public bool IsOn(Vector3 pos, float margin = 0, bool onlyXCheck = false)
    {

        float left = obj.transform.position.x - obj.transform.localScale.x / 2+margin;
        float right = obj.transform.position.x + obj.transform.localScale.x / 2-margin;
        //X-check
        if (pos.x < left || pos.x > right)
        {
            return false;
        }
        if (!onlyXCheck)
        {
            //Z-check
            float towards = obj.transform.position.z - obj.transform.localScale.z / 2 + margin;
            float against = obj.transform.position.z + obj.transform.localScale.z / 2 - margin;
            if (pos.z < towards || pos.z > against)
            {
                return false;
            }
        }
        return true;
    }


    public void GenerateDistanceLists()
    {
        foreach(Vector3 link in links)
        {
            DictionaryList<Ground, int> currentSearch = new DictionaryList<Ground, int>();
            AddDistance(links[link], 1, currentSearch);
            distances.Add(link, currentSearch);
        }
    }

    private void AddDistance(Ground search, int depth, DictionaryList<Ground, int> currentSearch)
    {
        currentSearch.Add(search, depth);

        foreach (Vector3 g in search.links)//Ground curr in search.links)
        {
            Ground curr = search.links[g];

            if (!currentSearch.Contains(curr) && curr != this)
            {
                AddDistance(curr, depth + 1, currentSearch);
            }
        }
    }

}

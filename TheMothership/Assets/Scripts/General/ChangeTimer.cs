using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChangeTimer<T>{

    private Dictionary<T, float> statUpdateTime = new Dictionary<T, float>();

    public ChangeTimer() { }

    public float StatUpdatedAt(T s)
    {
        if (statUpdateTime.ContainsKey(s))
        {
            return statUpdateTime[s];
        }
        return 0;
    }

    public void Change(T stat)
    {
        if (statUpdateTime.ContainsKey(stat))
        {
            statUpdateTime[stat] = Time.time;
        }
        else
        {
            statUpdateTime.Add(stat, Time.time);
        }
        //lastChangeTime = Time.time;
    }
}

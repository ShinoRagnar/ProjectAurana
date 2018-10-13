using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class Faction{

    public string name;
    public ListHash<Faction> hostileFactions =  new ListHash<Faction>();
    public ListHash<Faction> alliedFactions = new ListHash<Faction>();

    public Faction(string name)
    {
        this.name = name;
    }
    public Faction(string name, Faction hostileTo)
    {
        this.name = name;
        SetHostileTo(hostileTo);
    }
    public void SetHostileTo(Faction f)
    {
        SetHostile(f);
        f.SetHostile(this);
    }
    protected void SetHostile(Faction f)
    {
        hostileFactions.AddIfNotContains(f);
        alliedFactions.Remove(f);
    }
    public bool IsHostileTo(Faction f)
    {
        if(f == this)
        {
            return false;
        }
        if (hostileFactions.Contains(f) )
        {
            return true;
        }
        return false;

    }
    public bool IsAlliedTo(Faction f)
    {
        if (f == this)
        {
            return true;
        }
        if (alliedFactions.Contains(f))
        {
            return true;
        }
        return false;
    }
    public bool IsNeutralTo(Faction f)
    {
        if (f == this)
        {
            return false;
        }
        if (!IsHostileTo(f) && !IsAlliedTo(f))
        {
            return true;
        }
        return false;
    }





}

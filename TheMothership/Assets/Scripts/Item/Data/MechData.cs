using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public interface Saveable<U>
{
    bool Save(U t,string savename);
}
[Serializable]
public struct SocketMount
{
    public int[] pos;
    public SocketableNames socket;

    public SocketMount(int[] p, SocketableNames s)
    {
        pos = p;
        socket = s;
    }
}
[Serializable]
public struct CoreMount
{
    public Vector2 pos;
    public CoreNames core;
    public SocketMount[] sockets;

    public CoreMount(Vector2 p, CoreNames c, SocketMount[] sock)
    {
        pos = p;
        core = c;
        sockets = sock;
    }
}
[Serializable]
public struct LegMount
{
    public LegNames leg;
    public SocketMount[] sockets;

    public LegMount(LegNames l, SocketMount[] sock)
    {
        leg = l;
        sockets = sock;
    }
}
[Serializable]
public class MechData : ScriptableObject, Saveable<Mech>
{
    [SerializeField]
    public string enumName;
    [SerializeField]
    public CoreMount[] mounts;
    [SerializeField]
    public LegMount legs;

    /*public MechData(LegMount l, CoreMount[] sav)
    {
        mounts = sav;
        legs = l;
    }*/

    public bool Save(Mech m, string savename)
    {
        if(m == null)
        {
            return false;
        }
        List<CoreMount> coreMounts = new List<CoreMount>();
        foreach (Core c in m.equippedCores)
        {
            List<SocketMount> al = new List<SocketMount>();
            if (c.sockets != null)
            {
                SaveSockets(c, al, new List<int>());
            }
            coreMounts.Add(new CoreMount(m.equippedCores[c], c.origin, al.ToArray()));
        }
        //Legs
        List<SocketMount> legSocks = new List<SocketMount>();
        if (m.legs.sockets != null)
        {
            SaveSockets(m.legs, legSocks, new List<int>());
        }


        /*if(m.legs.sockets != null)
        {
            foreach (int sock in m.legs.sockets)
            {
                MechItem.SocketSlot slot = m.legs.sockets[sock];
                if (slot.occupant != null)
                {
                    legSocks.Add(new SocketMount(sock, slot.occupant.GetName()));
                }
            }
        }*/

        enumName = savename;
        legs = new LegMount(m.legs.origin, legSocks.ToArray());
        mounts = coreMounts.ToArray();

        return true;
    }

    public void SaveSockets(MechItem c, List<SocketMount> al, List<int> depth)
    {
        if (c.sockets != null)
        {
            foreach (int sock in c.sockets)
            {
                MechItem.SocketSlot slot = c.sockets[sock];
                if (slot.occupant != null)
                {
                    List<int> innerDepth = new List<int>();
                    foreach(int i in depth) { innerDepth.Add(i); }
                    innerDepth.Add(sock);

                    al.Add(new SocketMount(innerDepth.ToArray(), slot.occupant.GetName()));
                    SaveSockets((MechItem)slot.occupant, al, innerDepth);
                }
            }
        }
    }
}
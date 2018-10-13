using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class BuffHandler : GameUnitBodyComponent
{

    //Ignore steps of X on the duration of a debuff. Less drawcalls
    private static readonly int DURATION_SCALE_IGNORE = 5;
    private static readonly float BUFF_DISTANCE = 150;
    private static readonly float BUFF_START_DISTANCE = 75;

    //Privates
    private float currTime = 0;

    //Publics
    public ListDictionary<int, Buff> buffs = new ListDictionary<int, Buff>();
    //                 Stat
    public ChangeTimer<int> statUpdateTime = new ChangeTimer<int>();
    public float activeBuffs = 0;
   // public GameUnit owner;

    private void Change(Buff b)
    {
      foreach(Stat stat in b.affects)
        {
            statUpdateTime.Change((int)stat);
        }
    }

    public bool AddBuff(MechItem source, Buff b)
    {
        bool added = false;

        if (DevelopmentSettings.ADD_BUFFS)
        {


            if (buffs.Contains((int)b.origin)) //b.buffName))
            {

            }
            else
            {
                added = true;
                buffs.Add((int)b.origin,b.Clone()); //b.buffName, b.Clone());
                if (owner.isPlayer)
                {
                   // if (b.isDebuff)
                    //{
                        buffs[(int)b.origin].ui.Show(); //b.buffName].ui.Show();
                    //}
                }
                //Debug.Log("Buff added: " + b.buffName);
            }

            bool wasActive = buffs[(int)b.origin].AddStack(source); //[b.buffName].AddStack(source);

            if (!wasActive || added)
            {
                RefreshActiveBuffsCounter();

                if (owner.isPlayer)
                {
                    RecalculateBuffPositions();
                }

                added = true;
            }

            Change(buffs[(int)b.origin]);//b.buffName]);


        }

        return added;
    }

    public void RemoveStack(MechItem source, Buff b)
    {
        if (buffs.Contains((int)b.origin))
        {
            buffs[(int)b.origin].RemoveStack(source);
        }
    }

    public void Track(Buff b, OnHit.ActiveEffect ae)
    {
        if (buffs.Contains((int)b.origin)) //b.buffName))
        {
            buffs[(int)b.origin].effects.Add(ae); //b.buffName].effects.Add(ae);
        }
    }

    public void RefreshActiveBuffsCounter()
    {
        activeBuffs = 0;
        foreach (Buff bob in buffs)
        {
            if (bob.isActive)
            {
                activeBuffs++;
            }
        }
    }

    public void RecalculateBuffPositions()
    {
        float posX = 0;
        if (owner.isPlayer)
        {
            foreach (Buff buf in buffs)
            {
                if (buf.isActive)
                {
                    buf.ui.MoveX(BUFF_START_DISTANCE + posX * BUFF_DISTANCE); //MoveBuff(buf, posX);
                    posX++;
                }
            }
        }
    }
    
    public void UpdateBuff(Buff b)
    {
        if(b.isActive && b.ui.hidden)
        {
            b.ui.Show();
        }
        if (b.buffType != BuffType.NoDuration)
        {

            if (b.buffType == BuffType.DurationPerStack)
            {
                b.UpdateDurationPerStack();
            }

            //Change durationslider position
            b.ui.ScaleDuration(
                Global.UI_DURATION_TIME_LENGTH,
                (b.currentDuration / b.duration),
                DURATION_SCALE_IGNORE,
                true);

        }

        if (b.updatedStackLastFrame && b.ui.instantiated)
        {
            string newText = "" + (b.stacks.Count);

            //If stacks have gone up or down then change
            if (!b.ui.GetText(0).Equals(newText))
            {
                b.ui.GetText(0).text = newText;
                Change(b);
            }
            b.updatedStackLastFrame = false;
        }
    }
    
	// Update is called once per frame
	void Update () {

        //Development statistics
        if (DevelopmentSettings.SHOW_STATISTICS){ 
            currTime += Time.deltaTime;
            if(currTime > 3)
            {
                Debug.Log("Statistics Hits: " + owner.stats.cacheHit + " Misses:" + owner.stats.cacheMiss);
                currTime = 0;
            }
        }
        if (activeBuffs > 0) { 
		    foreach(Buff buf in buffs)
            {
                //Debug.Log("updatin");
                if (buf.isActive)
                {

                    if (buf.buffType != BuffType.NoDuration)
                    {
                        buf.currentDuration += Time.deltaTime;
                    }
                    if (buf.currentDuration > buf.duration || buf.stacks.Count == 0)
                    {
                        buf.DurationEnded(owner);
                        activeBuffs--;
                        Change(buf);

                        if (owner.isPlayer)
                        {
                            buf.ui.Hide();
                            RecalculateBuffPositions();
                        }
                    }
                    else if (owner.isPlayer)
                    {
                        UpdateBuff(buf);
                    }
                }
                /*else if (buf.updatedStackLastFrame)
                {
                   // if(buf.stacks =)
                    Change(buf);
                    UpdateBuff(buf);
                }*/
            }
            //}

        }
    }
}

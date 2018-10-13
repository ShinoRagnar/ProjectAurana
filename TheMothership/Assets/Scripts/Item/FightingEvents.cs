using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FightingEvents : MonoBehaviour {

    public Core core;

	public void SendEvent(string eve)
    {
        Debug.Log(eve);
    }

    public void BeginAttack()
    {
        Debug.Log("Begin attack");
        if(core != null && core.weapon != null)
        {
            core.weapon.BeginAttackPhase();
        }
    }
    public void EndAttack()
    {
        if (core != null && core.weapon != null)
        {
            core.weapon.EndAttackPhase();
        }
    }
    public void BeginBlocking()
    {
        if (core != null && core.shield != null)
        {
            core.shield.BeginBlockingPhase();
        }
    }
    public void EndBlocking()
    {
        if (core != null && core.shield != null)
        {
            core.shield.EndBlockingPhase();
        }
    }
    public void FirstBlockFrame(float time)
    {
        if (core != null && core.shield != null)
        {
            core.shield.SetBlockTime(time);
        }
        if (core != null && core.weapon != null)
        {
            core.weapon.SetSwingTime(time);
        }
    }
    public void FirstAttackFrame(float time)
    {
        if (core != null && core.weapon != null)
        {
            core.weapon.FirstAttackFrame();
            core.weapon.SetSwingTime(time);
        }
        if (core != null && core.shield != null)
        {
            core.shield.SetBlockTime(time);
        }
    }
}

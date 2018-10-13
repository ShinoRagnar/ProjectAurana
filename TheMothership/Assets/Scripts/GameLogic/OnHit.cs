using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public enum OnHitType
{
    OnHitOnly = 0,
    Duration = 10
}
public enum EndQualifier
{
    Death = 10,
    BuffExpires = 20
}

[Serializable]
public struct OnHitDelayedDamage
{
    public Damage[] damage;
    public DamageModifier[] modifiers;
    public float delay;
    public float radius;
    public float impact;
}
public class OnHit : Interaction{

    public struct ActiveEffect
    {
        public OnHit source;
        public Effect effect;
        public Transform toReturn;

        public ActiveEffect(OnHit source, Effect effect, Transform toReturn)
        {
            this.source = source;
            this.effect = effect;
            this.toReturn = toReturn;
        }
    }

    public OnHitNames origin;
   // public OnHitCondition[] conditions;
    public OnHitType type;
    //public OnSubsequentHits onSubsequentHits;
    public EndQualifier[] qualifiers;
    public List<Buff> transferringBuffs;
    public DictionaryList<EffectWhen, Effect> effectLibrary;
  //  public float onHitCollisionThreshold;
    public string name;
    public string description;

    public OnHitDelayedDamage[] delayedDamages;

    public OnHit(OnHitData data, OnHitNames origin) : this (
        data.onHitName, 
        data.onHitDescription,
        data.type,
       // data.onSubsequentHits,
       // data.onHitConditions,
        data.endQualifier,
     //   data.onHitConditionThreshold,
        Global.Resources[data.debuffs],
        MechItem.EffectWhenDataToDictionary(data.effectsWhen),
        origin,
        data.delayedDamage
        ){}

    public OnHit(
                    string name,
                    string description,
                    OnHitType type,
                    //  OnSubsequentHits onSubsequentHits,
                    // OnHitCondition[] conditions,
                    EndQualifier[] endQualifiers,
                    // float onHitCollisionThreshold,
                    List<Buff> transferringBuffs,
                    DictionaryList<EffectWhen, Effect> effectLibrary,
                    OnHitNames origin,
                    OnHitDelayedDamage[] delayedDam = null
                    )
    {
        this.delayedDamages = delayedDam;
        this.name = name;
        this.description = description;
        this.origin = origin;
       // this.onHitCollisionThreshold = onHitCollisionThreshold;
      //  this.conditions = conditions;
      //  this.onSubsequentHits = onSubsequentHits;
        this.qualifiers = endQualifiers;
        this.type = type;
        this.transferringBuffs = transferringBuffs;
        this.effectLibrary = effectLibrary;
    }

    public void Hit(MechItem source, GameUnit from, GameUnit target, Vector3 hitPos, Vector3 rotation, Vector3 delayedDamageOffset)
    {
        /*if (CheckOnHitConditions(from, target, damagePercentDealt))
        {*/
            bool showHit = false;

            //Duration types only shows hit when buffs are reapplied (freeze etc)
            if (TransferBuffs(source, from, target, transferringBuffs))
            {
                showHit = true;
            }

            //On hit only always shows hits
            if(type == OnHitType.OnHitOnly)
            {
                showHit = true;
            }

            if (showHit && effectLibrary.Contains(EffectWhen.OnHitLocal))
            {
                Transform effect = effectLibrary[EffectWhen.OnHitLocal].Spawn(target.GetCenterPos(), target.body, rotation.x, rotation.y, rotation.z, target.GetMaxScale());

                if (effectLibrary[EffectWhen.OnHitLocal].modeHandles)
                {
                    HandleEffect(effect, target, effectLibrary[EffectWhen.OnHitLocal]);
                }
            }
            else if (showHit && effectLibrary.Contains(EffectWhen.OnHitWorld))
            {
                Transform effect = effectLibrary[EffectWhen.OnHitWorld].Spawn(target != null ? target.GetCenterPos() : hitPos, null, rotation.x, rotation.y, rotation.z, target != null ? target.GetMaxScale() : 1);

                if (effectLibrary[EffectWhen.OnHitWorld].modeHandles)
                {
                    HandleEffect(effect, target, effectLibrary[EffectWhen.OnHitWorld]);
                }
            }

           if(delayedDamages != null)
           {
                foreach(OnHitDelayedDamage d in delayedDamages)
                {
                    Global.delayedDamage.Add(new Global.DelayedOnHitDamage(from, d, hitPos+ delayedDamageOffset, hitPos),0);
                }
           }

            
       // }
    }
    public void ReturnEffect(GameUnit target, Transform effect, Effect eff)
    {
        if (eff.ReturnHandle(effect))
        {
            if (effectLibrary.Contains(EffectWhen.OnDurationEnd))
            {
                effectLibrary[EffectWhen.OnDurationEnd].Spawn(target.GetCenterPos(), null, 0, 0, 0, target.GetMaxScale());
            }
        }
    }

    public void HandleEffect(Transform effect, GameUnit target, Effect eff)
    {
        foreach(EndQualifier eq in qualifiers)
        {
            ActiveEffect ae = new ActiveEffect(this, eff, effect);

            target.activeEffecs.Add(ae);

            if (eq == EndQualifier.BuffExpires)
            {
                foreach(Buff b in transferringBuffs)
                {
                    target.buffHandler.Track(b, ae);
                }
            }
        }
    }



    public OnHit Clone()
    {
        return new OnHit(name, description, type, /*conditions,*/ qualifiers, /*onHitCollisionThreshold,*/ transferringBuffs, effectLibrary, origin, delayedDamages);
    }
}

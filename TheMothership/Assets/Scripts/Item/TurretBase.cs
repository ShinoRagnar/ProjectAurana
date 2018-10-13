using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*public class TurretBase : Item{

    DictionaryList<Transform, TurretTargeter> mounted = new DictionaryList<Transform, TurretTargeter>();

    public TurretBase(
        string name,
        Transform item,
        Alignment align,
        DictionaryList<PointOfInterest,string> attachmentsVal
    ) :base(name,item,align,attachmentsVal)
    { }

    public TurretTargeter MountAndShow(PointOfInterest poi, Shootable g)
    {
        //Get joint position
        Transform joint = GetPointOfInterest(poi);
        //Equip item
        //ie.EquipItem(g.Item());
        ie.Equip(g.Item());
        //Show gun
        g.Item().Show(joint);
        //Add targeting
        TurretTargeter targ = new TurretTargeter(g, joint);
        mounted.Add(joint, targ);

        return targ;
       // Debug.Log(g.reactionTime);
    }




    public void ShootAt(GameUnit target)
    {
        foreach (Transform t in mounted)
        {
            //Get gun and targeting system
            TurretTargeter targ = mounted[t];
            Shootable g = targ.shootable;

            //Try to shoot from muzzle
            Transform from = g.GetOriginPoint();//g.GetPointOfInterest(PointOfInterest.Muzzle);
            if (from == null)
            {
                from = t;
            }

            //Tick reloadtime etc
            g.Tick();

            if (targ.stage == TargetingStage.Idle && g.WillReact()) //&& g.currentReactionTime > g.reactionTime) 
            {
                targ.AcquireTarget(target);
            }
            else if(targ.stage == TargetingStage.TargetAcquired)
            {
                bool targeted = targ.AdjustTarget();
                float diff = targ.AdjustJointTowardsTarget();
                BulletType type = g.GetBulletType();
                
                if(type == BulletType.Gunshot)
                {
                    if (g.CanDetect(target))
                    {
                        targ.gunAnimator.ResumeShooting();
                    }
                    else
                    {
                        targ.AcquireTarget(target);
                    }
                }else if (type == BulletType.MissileLauncher)
                {
                    if(diff < TurretTargeter.MAX_ANGLE_DIFFERENCE && targeted)
                    {
                        targ.ForceLock();
                        targ.ShowArc(from);
                    }
                }
                else if(type == BulletType.Mortar)
                {
                    targ.LockTarget();
                    targ.ShowArc(from);
                }
            }
            else if(targ.stage == TargetingStage.TargetLocked)
            {
                targ.GetReadyForLaunch();
                targ.ShowArc(from);
            }else if(targ.stage == TargetingStage.ReadyForLaunch)
            {
                targ.ShowArc(from);
                targ.gunAnimator.ResumeShooting();
                targ.AcquireTarget(target);
            }

        }
    }

    public new TurretBase Clone()
    {
        return new TurretBase(itemName, prefab, alignment, pointsOfInterestPreShowing.CloneSimple());
    }



}*/

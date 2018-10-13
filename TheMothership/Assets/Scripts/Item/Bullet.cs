using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum BulletType
{
    Array = 0,
    Gunshot = 10,
    Mortar = 20,
    MissileLauncher = 30
}
public enum ProjectileRole
{
    Main
}
public enum BulletTarget
{
    Target =0,
    GroundUnderTarget = 1,
    GroundInFrontOfTarget = 2
}
/*public enum ImpactType
{
    Default,
    //Hit effects
    ShieldHit,
    HealthHit,
    GroundHit,
    //Crater effects
    Crater,
    CraterCreation
}*/
public class Bullet : Interaction, IGameClone<Bullet>{



    //Projectiles in pool
    private Stack<Transform> availableProjectiles = new Stack<Transform>();
    private DictionaryList<Transform, float> inTransitProjectiles = new DictionaryList<Transform, float>();

    public BulletTarget target;
    public BulletType type;
    public DictionaryList<EffectWhen, Effect> impactEffects;
  //  public DictionaryList<Buff, float> transferringBuffs;
    public AudioContainer bulletSounds;
    public Damage[] damage;
    public OnHitWhen[] onHit;
    public DamageModifier[] damageModifiers;

    //public float damage;
    public float impact;
    public float explosionRadius;
    public float waitBeforeParticleSystemFinishTime;

    Transform projectilePrefab;

    public Gun originator;


    public Bullet(BulletData data) : this(
        data.type,
        data.target,
        EffectWhenDataToDictionary(data.effectsWhen),
        data.damage,
        MechItem.OnHitWhenDataToOnHitWhen(data.onHit),
        data.damageModifiers,
        data.impact,
        data.waitForParticlesTime,
        GetMainProjectile(data.projectiles),
        data.explosionRadius,
        Global.Resources[data.audio]){}


    public Bullet(
        BulletType typeVal,
        BulletTarget target,
        DictionaryList<EffectWhen, Effect> impactEffectsVal,
        Damage[] damage,
        OnHitWhen[] onHit,
        DamageModifier[] damageModifiers,
        float impactVal,
        float waitBeforeParticleSystemFinishTime,
        Transform projectilePrefabVal = null,
        float explosionRadiusVal = 0,
        AudioContainer bulletSoundsVal = null)
    {
        this.waitBeforeParticleSystemFinishTime = waitBeforeParticleSystemFinishTime;
        this.target = target;
        this.type = typeVal;
        this.impactEffects = impactEffectsVal;
        this.damage = damage;
        this.onHit = onHit;
        this.damageModifiers = damageModifiers;
        this.impact = impactVal;
        this.projectilePrefab = projectilePrefabVal;
        this.explosionRadius = explosionRadiusVal;
        this.bulletSounds = bulletSoundsVal;
    }

    public void Hit(
        GameUnit from, 
        GameUnit to, 
        Transform lookAt, 
       // Forge3D.Forcefield forc, 
        Vector3 impactPos,
        bool groundHit,
        float damagePercentage)
    {
        Hit(originator,from, to,originator.shootingConditions, lookAt, impactPos, impactPos, damagePercentage, groundHit, damage, impact, explosionRadius,onHit,damageModifiers, impactEffects); //, transferringBuffs);
    }

    public void DoOnHit(MechItem source, GameUnit from, GameUnit to, Vector3 hitPos, float damageDealt, bool groundHit)
    {
        if(onHit != null && onHit.Length > 0)
        {
            DoOnHit(
                    source,
                    onHit,
                    from,
                    to,
                    hitPos,
                    (from.body.position.x > hitPos.x && target == BulletTarget.GroundInFrontOfTarget) ?
                        new Vector3(0, 180, 0) : Vector3.zero,
                    target == BulletTarget.GroundInFrontOfTarget ?
                        new Vector3(TurretTargeter.GROUND_TARGET_DISTANCE * (from.body.position.x < hitPos.x ? 1 : -1), 0) : Vector3.zero,
                    damageDealt,
                    groundHit);
        }

    }


    public Transform GetAvailableProjectile(Transform parent)
    {
        Transform ret;

        if (availableProjectiles.Count == 0)
        {
            if(projectilePrefab != null)
            {
                ret = Global.PoolOrCreate(projectilePrefab, parent);
            }
            else
            {
                GameObject go = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                go.GetComponent<SphereCollider>().enabled = false;
                go.transform.parent = parent;
                ret = go.transform;
            }
        }
        else
        { 
            //Pop one from the stack
            ret = availableProjectiles.Pop();
            NeedsDisabling ps = ret.gameObject.GetComponentInChildren<NeedsDisabling>();
            if(ps != null)
            {
                ps.Enable();
            }

            ret.gameObject.SetActive(true);

            //Debug.Log("Reusing projectile");
        }
        //Add sound source
        /*AudioSource ass = ret.GetComponent<AudioSource>();
        if (ass == null)
        {
           ass = ret.gameObject.AddComponent<AudioSource>();
        }*/

        //Add detonator
        Detonator det = ret.GetComponent<Detonator>();
        if(det == null)
        {
            det = ret.gameObject.AddComponent<Detonator>();
        }
        //Make sure we will detonate as this bullet
        det.owner = originator.owner.uniqueName;
        det.bullet = this;
        det.hasDetonated = false;
        //det.source = ass;

        //Add audioSource
        /*if (ret.GetComponent<AudioSource>() == null)
        {
            ret.gameObject.AddComponent<AudioSource>();
        }*/
        return ret;
    }
    public void ReturnProjectile(Transform proj)
    {
        //Push one on top of the stack
        NeedsDisabling ps = proj.gameObject.GetComponentInChildren<NeedsDisabling>();
        if (ps != null)
        {
            ps.Disable();
        }
        availableProjectiles.Push(proj);
        proj.gameObject.SetActive(false);
    }



    public Bullet Clone()
    {
        return new Bullet(
            type,
            target,
            impactEffects, 
            //transferringBuffs.CloneGameKeys(), 
            damage,
            onHit,
            damageModifiers,
            impact,
            waitBeforeParticleSystemFinishTime,
            projectilePrefab,
            explosionRadius,
            bulletSounds
            );
    }


    public void TransferImpact(Rigidbody ri, float damagePercentage, Vector3 impactPos)
    {
        TransferImpact(ri, damagePercentage, impactPos, impact, explosionRadius);
    }




    /*public void ShowImpact(EffectWhen type, Vector3 pos, Transform lookAt = null, float xRot = 0, float yRot = 0, float zRot = 0)
    {

        Effect impact = impactEffects[type];

        if(impact == null)
        {
            impact = impactEffects[EffectWhen.Default];
        }
        if (impact != null)
        {

            Transform spawned = impact.Spawn(pos, null, xRot, yRot, zRot, explosionRadius * BULLET_EFFECT_RADIUS_SCALE);

            if (lookAt != null)
            {
                spawned.LookAt(lookAt);
            }
            //Debug.Log(impact.visualItem.gameObject);
            //impact.visualItem.parent = parent;
            //impact.visualItem.position = pos+ new Vector3(impact.alignment.x,impact.alignment.y,impact.alignment.z); //healthHit.point;
            //impact.visualItem.LookAt(lookAt);
            //impact.ReEnable();
        }
        else
        {
            Debug.Log("Missing impact effect for: " + originator.uniqueItemName + " : " + type);
        }

    }*/

}

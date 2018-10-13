using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Interaction {

    public static readonly float IMPACT_INTERRUPT_THRESHOLD = 5000;
    public static readonly float IMPACT_DAMPENING = 10;
    public static readonly float SHOW_DAMAGE_Y_OFFSET = 2;
    public static readonly float SHOW_DAMAGE_X_OFFSET = 0.5f;
    public static readonly float BULLET_EFFECT_RADIUS_SCALE = 4;
    public static readonly float RANDOM_SHIELD_VISUAL_DAMPENING = 50;

    public struct UnitsHit
    {
        public ListHash<GameUnit> unitsHit;
        public ListHash<Rigidbody> rigidBodiesHit;

        public UnitsHit(ListHash<GameUnit> unitsHit, ListHash<Rigidbody> rigidBodiesHit)
        {
            this.unitsHit = unitsHit;
            this.rigidBodiesHit = rigidBodiesHit;
        }
    }

    // ----------------------- STATIC -----------------------------------------------------------------------------------

    //Return the percentage of damage dealt
    public static GameUnit.DamageResponse Hit(
        MechItem source,
        GameUnit from,
        GameUnit to,
        ListHash<int> attackConditions,
        Transform lookAt,
        Vector3 impactPos,
        Vector3 shieldHitPos,

        float damagePercentage,
        bool groundHit,
        
        Damage[] damage,

        float impact,
        float explosionRadius,

        OnHitWhen[] onHits,
        DamageModifier[] modifiers,

        DictionaryList<EffectWhen, Effect> impactEffects = null,

        float shake = 0
        //DictionaryList<Buff, float> transferringBuffs = null
        )
    {
        //Damage player
        GameUnit.DamageResponse response = Damage(from, to, attackConditions, impactPos, damagePercentage, damage, modifiers, impact, explosionRadius, false);
        impact += response.impactAdded;


        EffectWhen effectImpact = EffectWhen.ShieldHit;

        if (impactEffects != null)
        {
            effectImpact = response.damagedHealth ? EffectWhen.HealthHit : effectImpact;
        }
        
        Vector3 point = shieldHitPos;

        if (response.damagedHealth)
        {
            point = from.senses.GetRandomPointFrom(from.isActive ? from.body.position : to.body.position, to);
        }
        else
        {
            ShowShieldVisualsInner(to.shield, shieldHitPos, response.healthDamageDealt+response.shieldDamageDealt, true);
        }

        if(impactEffects != null && impactEffects.Contains(effectImpact))
        {
            impactEffects[effectImpact].Spawn(point).LookAt(lookAt);
        }

        TransferImpact(from.isActive ? from.body :  to.body, to, from.uniqueName, impact, damagePercentage);

        DoOnHit(source, onHits,from, to, impactPos, Vector3.zero, Vector3.zero, response.healthDamageDealt + response.shieldDamageDealt, groundHit);

        if (from.isPlayer && shake > 0 && DevelopmentSettings.ENABLE_CAMERA_SHAKE)
        {
            shake += response.shakeAdded;
            CameraRumbler.Instance.ShakeOnce(2 * shake, 2 * shake, 0.1f * shake, 0.5f * shake);
        }

        return response;
    }

    public static GameUnit GetUnit(Collider col)
    {
        if (col != null)
        {
            GameUnitBodyComponent owner = col.gameObject.GetComponent<GameUnitBodyComponent>();
            if (owner != null)
            {
                return owner.owner;
            }
        }
        return null;
    }

    public static MechItem GetMechItem(Collider col)
    {
        if (col != null)
        {
            MechItemDetached mi = col.gameObject.GetComponent<MechItemDetached>();
            if (mi != null)
            {
                return mi.mechitem;
            }
        }
        return null;
    }

    public static UnitsHit GetUnitsHitInRadius(
        Vector3 position, 
        float radius,
        ListHash<GameUnit> unitHits,
        ListHash<Rigidbody> rigidBodies)
    {
        Collider[] hitColliders = Physics.OverlapSphere(position, radius);
        //GameUnit original = GetUnit(orig);

        //Hit the unit
        foreach (Collider col in hitColliders)
        {
            GameUnit gu = GetUnit(col);
            if (gu != null && !unitHits.Contains(gu))
            {
                //HitGameUnit(gu, col, original);
                unitHits.Add(gu);

            }
            else
            {
                Rigidbody[] mi = col.gameObject.GetComponentsInChildren<Rigidbody>();
                if (mi != null)
                {
                    foreach (Rigidbody r in mi)
                    {
                        if (!rigidBodies.Contains(r))
                        {
                            //bullet.TransferImpact(r, 1, transform.position);
                            rigidBodies.Add(r);
                        }
                    }
                }
            }
        }
        return new UnitsHit(unitHits, rigidBodies);
    }

    public static void DoOnHit(MechItem source, OnHitWhen[] onHits, GameUnit from, GameUnit to, Vector3 hitPos, Vector3 rotation, Vector3 delayedDamageOffset, float damageDealt, bool groundHit)
    {
        if (onHits != null)
        {
            foreach (OnHitWhen oh in onHits)
            {
                oh.HitIfConditions(source, from,to,hitPos,rotation, delayedDamageOffset,damageDealt, groundHit);
            }
        }
    }


    public static GameUnit.DamageResponse Damage(
        GameUnit from, 
        GameUnit to, 
        ListHash<int> conditions,
        Vector3 impactPos,
        float damagePercentage,
        Damage[] damage, 
        DamageModifier[] modifiers, 
        float impact, 
        float explosionRadius,
        bool isOnHitDamage
        )
    {
        //Calculate damage
        float anyDamage = 0;
        float shieldDamage = 0;
        float healthDamage = 0;
        float addedShake = 0;
        float addedImpact = 0;

        //Get local damage stat
        if (damage != null)
        {
            foreach (Damage dam in damage)
            {
                if (dam.type == DamageType.Physical) { anyDamage += dam.GetDamage(from, conditions); } //dam.amount; }
                else if (dam.type == DamageType.EMP) { shieldDamage += dam.GetDamage(from, conditions); } //dam.amount; }
                else if (dam.type == DamageType.Corruption) { healthDamage += dam.GetDamage(from, conditions); } //dam.amount; }
            }
        }
        //Check local damage modifiers
        if (modifiers != null)
        {
            foreach (DamageModifier modifier in modifiers)
            {
                bool satisfied = true;
                foreach (InteractionCondition ic in modifier.conditions)
                {
                    if (!ic.IsSatisified(from, to)) { satisfied = false; break; }
                }
                if (satisfied)
                {
                    anyDamage *= modifier.damageModifier;
                    shieldDamage *= modifier.damageModifier;
                    healthDamage *= modifier.damageModifier;
                    addedShake += modifier.shakeAddition;
                    addedImpact += modifier.impactAddition;
                }
            }
        }
        //Check global damage modifiers
        //anyDamage *= from.stats.GetCurrentValue(Stat.DamageDealt, conditions);
        //shieldDamage *= from.stats.GetCurrentValue(Stat.DamageDealt, conditions) + (1-from.stats.GetCurrentValue(Stat.EMPDamageDealt, conditions));
        //healthDamage *= from.stats.GetCurrentValue(Stat.DamageDealt, conditions) + (1-from.stats.GetCurrentValue(Stat.CorruptionDamageDealt, conditions));

        GameUnit.DamageResponse response = to.Damage(anyDamage* damagePercentage, shieldDamage*damagePercentage, healthDamage*damagePercentage, from, isOnHitDamage);
        response.impactAdded = addedImpact;
        response.shakeAdded = addedShake;

        if (response.killed)
        {
            impact += addedImpact;

            foreach (Rigidbody mi in response.detachedItems)
            {
                TransferImpact(mi, damagePercentage, impactPos, impact, explosionRadius);
            }
        }
        else
        {
            Flash(to);
        }

        ShowDamage(-1*(int)response.healthDamageDealt, -1*(int)response.shieldDamageDealt, to, !response.damagedHealth);

        return response;
    }

    public static void TransferImpact(Rigidbody ri, float damagePercentage, Vector3 impactPos, float impact, float explosionRadius)
    {
        ri.AddExplosionForce((impact / IMPACT_DAMPENING) * damagePercentage, impactPos, explosionRadius);
    }

    public static void Flash(GameUnit to)
    {
        to.Flash(Global.Resources[MaterialNames.Red], Global.FLASH_TIME);
    }

    public static void ShowDamage(int damageHealth, int damageShield, GameUnit to, bool shieldHit)
    {
        if (damageHealth != 0)
        {
            Global.Resources[EffectNames.TextDamageTaken].Spawn(
                to.body.position
                + new Vector3(SHOW_DAMAGE_X_OFFSET, to.body.localScale.y / 2 + SHOW_DAMAGE_Y_OFFSET)
                , null, 0, 0, 0, 1, damageHealth.ToString());
        }

        if(damageShield != 0)
        {
            Global.Resources[EffectNames.TextDamageShield].Spawn(
                to.body.position
                + new Vector3(-SHOW_DAMAGE_X_OFFSET, to.body.localScale.y / 2 + SHOW_DAMAGE_Y_OFFSET)
                , null, 0, 0, 0, 1, damageShield.ToString());
        }

        //Blood splash
        if (to.mech == null && !shieldHit)
        {
            Global.Resources[EffectNames.BloodSplash].Spawn(to.GetCenterPos(), null, 0, 0, 0, BULLET_EFFECT_RADIUS_SCALE);
        }
        // Debug.Log("Showed damage: " + damage);
    }
    public static void TransferImpact(Transform from, GameUnit to, string giver, float impact, float prcntg = 1)
    {
        TransferImpact(from.position, to, giver, impact, prcntg);
    }

    public static void TransferImpact(Vector3 from, GameUnit to, string giver, float impact, float prcntg = 1)
    {
        if (to.impact != null)
        {
            to.impact.AddImpact(giver,
                new Vector3(to.body.position.x, to.body.position.y, 0) -
                new Vector3(from.x, to.body.position.y, 0),
                impact * prcntg);
        }
        if (to.mech != null)
        {
            to.mech.MountShakeInit(impact * prcntg);

            if (impact*prcntg > IMPACT_INTERRUPT_THRESHOLD)
            {
                foreach (Core c in to.mech.equippedCores)
                {
                    if(c.meleeAnimator != null)
                    {
                        c.Interrupt();
                    }
                }
                to.movement.Interrupt();
            }
        }
    }

    /*public static void TransferImpact(GameUnit from, GameUnit to, string giver, float prcntg = 1)
    {
        TransferImpact(from.body, to, giver, prcntg);
    }*/

    public static bool TransferBuffs(MechItem source, GameUnit from, GameUnit to, List<Buff> transferringBuffs)//DictionaryList<Buff, float> transferringBuffs)
    {
        bool added = false;
        if (transferringBuffs != null)
        {
            foreach (Buff buff in transferringBuffs)
            {
                if (to.buffHandler.AddBuff(source, buff))
                {
                    added = true;
                }
            }
        }
        return added;
    }

    public static void RetractStacks(MechItem source, GameUnit from, List<Buff> transferringBuffs)//DictionaryList<Buff, float> transferringBuffs)
    {
        if(transferringBuffs != null)
        {
            foreach (Buff buff in transferringBuffs)
            {
                from.buffHandler.RemoveStack(source, buff);
            }
        }
    }

    public static void ShowShieldVisuals(RaycastHit shieldHit, float power)
    {
        Forge3D.Forcefield ffHit = shieldHit.collider.transform.GetComponentInParent<Forge3D.Forcefield>();
        /*float hitPower = Random.Range(-power, power);
        if (!randomPower)
        {
            hitPower = power;
        }*/
        ShowShieldVisualsInner(ffHit, shieldHit.point, power);
    }
    public static void ShowShieldVisuals(Collider col, Vector3 closest, float power)
    {
        Forge3D.Forcefield ffHit = col.transform.GetComponentInParent<Forge3D.Forcefield>();
        /*float hitPower = Random.Range(-power, power);
        if (!randomPower)
        {
            hitPower = power;
        }*/

        ShowShieldVisualsInner(ffHit, col.ClosestPoint(closest), power);

        //Show extra visuals
        for (int i = 0; i < power / RANDOM_SHIELD_VISUAL_DAMPENING; i++)
        {
            ShowShieldVisualsRandomPoint(ffHit, col, col.ClosestPoint(closest), power);
        }
        //Debug.Log("Showed " + i + " extra shield visuals");
    }
    public static void ShowShieldVisualsRandomPoint(Forge3D.Forcefield forc, Collider col, Vector3 closest, float power)
    {
        float randomWalkRange = power / RANDOM_SHIELD_VISUAL_DAMPENING;

        ShowShieldVisualsInner(forc, col.ClosestPoint(
            closest
            + new Vector3(
                Random.Range(-randomWalkRange, randomWalkRange),
                Random.Range(-randomWalkRange, randomWalkRange),
                Random.Range(-randomWalkRange, randomWalkRange))
                )
                , power
                , true
            );
    }

    protected static void ShowShieldVisualsInner(Forge3D.Forcefield forc, Vector3 pos, float power, bool ignoreReactionLimit = false)
    {
        if (forc != null)
        {
            forc.OnHit(pos, power, 1, ignoreReactionLimit);
        }
        else
        {
            Debug.Log("Couldnt get shield for bullet");
        }
    }


    public static DictionaryList<EffectWhen, Effect> EffectWhenDataToDictionary(EffectsWhenData[] data)
    {
        DictionaryList<EffectWhen, Effect> ret = new DictionaryList<EffectWhen, Effect>();
        foreach (EffectsWhenData dat in data)
        {
            ret.Add(dat.when, Global.Resources[dat.effect]);
        }
        return ret;
    }

    /*public static DictionaryList<Buff, float> BuffTransferDataToDictionary(BuffTransferData[] data)
    {
        DictionaryList<Buff, float> ret = new DictionaryList<Buff, float>();
        foreach (BuffTransferData dat in data)
        {
            ret.Add(Global.Resources[dat.buff], dat.direction);
        }
        return ret;
    }*/

    public static Transform GetMainProjectile(ProjectileRoleData[] data)
    {
        foreach (ProjectileRoleData dat in data)
        {
            if (dat.role == ProjectileRole.Main)
            {
                return Global.Resources[dat.projectile];
            }
        }
        return null;
    }
}

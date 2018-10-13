using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//using EZCameraShake;

public class Detonator : MonoBehaviour {

    public string owner;
    public Bullet bullet;
    //public AudioSource source;
    public bool hasDetonated = false;
    ListHash<GameUnit> alreadyCollidedWith = new ListHash<GameUnit>();
    ListHash<Rigidbody> alreadyCollidedRigid = new ListHash<Rigidbody>();


    public void OnTriggerEnter(Collider col)
    {
        GameUnit gu = Interaction.GetUnit(col);
        bool detonate = true;

        //Some bullets only collide with the ground
        if(gu != null && (bullet.target == BulletTarget.GroundInFrontOfTarget || bullet.target == BulletTarget.GroundUnderTarget))
        {
            detonate = false;

        //Don't collide with self
        }else if(bullet.originator.owner == gu)
        {
            detonate = false;

        }else if(gu != null)
        {
            //Check for shield collision
            if (Global.LAYER_SHIELDS.Equals(LayerMask.LayerToName(col.gameObject.layer)))
            {
                if (gu.stats != null)
                {
                    //If we collide with a shield that is zero (turned off) then ignore
                    if (gu.stats.GetCurrentValue(Stat.Shield) == 0)
                    {
                        detonate = false;
                    }
                    else 
                    {
                       // Bullet.ShowShieldVisuals(col, this.transform.position,bullet.damage);
                    }
                }
            }
        }

        //Detonate
        if (detonate)
        {
            Detonate(col);
        }
       
    }


    public void Detonate(Collider orig)
    {
        if (!hasDetonated)
        {
            // Debug.Log("explosion belonging to: " + bullet.originator.owner.uniqueName);

            alreadyCollidedWith.Clear();
            alreadyCollidedRigid.Clear();

            //Collider[] hitColliders = Physics.OverlapSphere(this.transform.position, bullet.explosionRadius);
            GameUnit original = Interaction.GetUnit(orig);

            Interaction.UnitsHit unitshit = Interaction.GetUnitsHitInRadius(this.transform.position, bullet.explosionRadius, alreadyCollidedWith, alreadyCollidedRigid);

            foreach (GameUnit gu in unitshit.unitsHit)
            {
                HitGameUnit(gu, original);
            }
            foreach(Rigidbody r in unitshit.rigidBodiesHit)
            {
                bullet.TransferImpact(r, 1, transform.position);
            }
            //Hit the unit
            /*foreach (Collider col in hitColliders)
            {
                GameUnit gu = GetUnit(col);
                if (gu != null && !alreadyCollidedWith.Contains(gu))
                {
                    HitGameUnit(gu, col,original);
                    alreadyCollidedWith.Add(gu);

                }
                else
                {
                    Rigidbody[] mi = col.gameObject.GetComponentsInChildren<Rigidbody>();
                    if (mi != null)
                    {
                        foreach (Rigidbody r in mi)
                        {
                            if (!alreadyCollidedRigid.Contains(r))
                            {
                                bullet.TransferImpact(r, 1, transform.position);
                                alreadyCollidedRigid.Add(r);
                            }
                        }
                    }
                }
            }*/
            //Default impact
            EffectWhen type = EffectWhen.Default;

            //Check if we did hit ground
            if (original == null)
            {
                
                Senses.Hit hit = Senses.SeeGroundBelow(this.transform.position+new Vector3(0,1));
                if(hit.didHit)
                {
                    if(hit.hit.distance < 2)
                    {
                        type = EffectWhen.GroundHit;
                        //Create crater on ground
                        bullet.impactEffects[EffectWhen.Crater].Spawn(hit.pos,null, -90, 0,0,bullet.explosionRadius*2);
                        bullet.impactEffects[EffectWhen.CraterCreation].Spawn(hit.pos,null, 0, 0, 0, bullet.explosionRadius);

                        //bullet.ShowImpact(EffectWhen.Crater, null, null, hit.point);
                    }
                }
            }
            //Show explosion
            bullet.impactEffects[type].Spawn(this.transform.position, null, 0, 0, 0, bullet.explosionRadius * Bullet.BULLET_EFFECT_RADIUS_SCALE);
            //bullet.ShowImpact(type, Global.instance.X_CLONES, this.transform, this.transform.position);

            if(bullet.bulletSounds != null)
            {

                bullet.bulletSounds.PlaySound(SoundWhen.Exploding, bullet.originator.audioSource, false);
            }

            //Trigger ground hits
            bullet.DoOnHit(bullet.originator,bullet.originator.owner, null,this.transform.position, 0, true);

        }
        hasDetonated = true;
    }

    public void HitGameUnit(GameUnit curr, GameUnit originalhit)
    {

        if (curr != null)
        {

            float damagePercentage;

            //Closest impact possible
            if (curr == originalhit)
            {
                damagePercentage = 1;
            }
            else
            {
                //Get the damage distance
                float dist = Mathf.Max(Vector3.Distance(curr.body.position, this.transform.position)-(curr.body.localScale.x)/2,0);
                float prcnt = Mathf.Min(dist/(bullet.explosionRadius*2), 1);
                damagePercentage = 1 - prcnt;
            }

            bullet.Hit(bullet.originator.owner, curr, this.transform, 
                this.transform.position,false, damagePercentage);
        }
    }



}

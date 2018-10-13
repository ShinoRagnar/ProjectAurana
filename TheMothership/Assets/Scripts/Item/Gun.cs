using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface Shootable
{
    BulletType GetBulletType();
    BulletTarget GetBulletTarget();
    Transform GetOriginPoint();
   // float GetReactionTime();
   // bool CanDetect(GameUnit target);
    Item Item();
    void Shoot(GameUnit target);//Targeter targeter);
    void RegisterTargeter(Targeter targeter);
    void Reset();
    void Reload();
    float GetTimeUntilCanShootPercentage();
    void Tick(float actionspeed, Vector3 mousePosition, GameUnit target, Vector3 mousePositionOnZPlane);
    bool CanShoot();

}
public class Gun : MechItem, Shootable, Socketable, Damager, IGameClone<Gun>{

    //Statics
    public static readonly int RAYCASTS_WHEN_SHOOTING = 1;
    public static readonly float MAX_REACTION_TIME = 0.5f;

    public static readonly float ANIMATION_RECOIL_LENGTH = 0.5f;

    //Ideal duration
    //Worst case percentage when we can't fit the furation into the ideal frame
    public static readonly float ANIMATION_RECOIL_DURATION_PERCENTAGE = 0.15f;
    public static readonly float ANIMATION_RETURN_DURATION_PERCENTAGE = 0.45f;

    //Essentials
    List<Transform> barrels = new List<Transform>();
    public ListHash<int> shootingConditions;

    public bool isRifle;
    public Bullet bullet;
    public GunAnimator gunAnimator;
    public Targeter targeter;

    //Reload
    public float thisCycleReloadTime;
    public float reloadTime;
    public float currentReloadTime;

    //Reaction
    public float reactionTime;
    public float currentReactionTime;

    public SocketableNames origin;

    public RecoilPhase phase = RecoilPhase.Rest;

    //Recoil max
    public float recoilDuration;
    public float returnDuration;

    //Recoil current
    public float currentRecoilDuration = 0;
    public float currentReturnDuration = 0;

    public Gun(GunData data, SocketableNames sock) : this(
        data.itemName,
        data.itemDescription,
        Global.Resources[data.prefab],
        data.rarity,
        Global.Resources[data.sprite],
        Global.Resources[data.audio],
        EffectWhenDataToDictionary(data.effectsWhen),
        Global.Resources[data.bullet],
        data.reloadTime,
        PointOfInterestDataToDictionary(data.points),
        data.isRifle,
        data.diameter,
        SocketSlotDataToSocketTypeArray(data.sockets),
        Global.Resources[data.buffs],
        new Alignment(  data.position.x, data.position.y, data.position.z, 
                        data.rotation.x, data.rotation.y, data.rotation.z,
                        data.scale.x, data.scale.y, data.scale.z),
        sock
        )
    { }

    public Gun(
      //Itemname
      string itemn,
      string description,
      //The item prefab
      Transform item,
      //Rarity
      Rarity rarityVal,
      //The picture prefab
      Sprite picture,
      //Gun sounds
      AudioContainer soundsVal,
      //Muzzle effects
      //DictionaryList<PointOfInterest, Item> muzzleEffectVal,
      //
      DictionaryList<EffectWhen, Effect> effectLibrary,
      //Bullet
      Bullet b,
      //Reload time
      float reloadTimeVal,
      //Barrels
      DictionaryList<PointOfInterest, string> pointsOfInterestPreShowingVal,
      //IsRifle
      bool isRifle,
      //Diamter
      int diameter,
      //Sockets
      SocketType[] emptySockets = null,
      //Buffs applied by this item
      List<Buff> appliesBuff = null,
      //Alignment (not used for scaling)
      Alignment alig = null,
      SocketableNames orig = SocketableNames.NothingSocketed

    ) : base(itemn,item,rarityVal,description,emptySockets, picture,appliesBuff,alig,pointsOfInterestPreShowingVal,effectLibrary)
    {
        this.isRifle = isRifle;
        this.inventoryHeight = diameter;
        this.inventoryWidth = diameter;
        this.origin = orig;
        this.bullet = b;
        this.bullet.originator = this;
        this.sounds = soundsVal;
        this.thisCycleReloadTime = reloadTimeVal;
        this.reloadTime = reloadTimeVal;
        this.currentReloadTime = thisCycleReloadTime;

        if(bullet.type == BulletType.Gunshot)
        {
            reactionTime = MAX_REACTION_TIME * (float)Global.instance.rand.NextDouble();
        }else{
            reactionTime = 0;
        }

        SetConditions();
    }


    public override int GetReloadGroup()
    {
        if( bullet.type == BulletType.Array || bullet.type == BulletType.Gunshot)
        {
            return InventoryHandler.LEFT_MOUSE_RELOAD_GROUP;
        }
        else
        {
            return InventoryHandler.RIGHT_MOUSE_RELOAD_GROUP;
        }
    }
    public SocketableNames GetName()
    {
        return origin;
    }

    public Damage[] GetDamage()
    {
        return bullet.damage;
    }
    public DamageModifier[] GetDamageModifier()
    {
        return bullet.damageModifiers;
    }
    public OnHitWhen[] GetOnHit()
    {
        return bullet.onHit;
    }
    public ListHash<int> GetConditions()
    {
        return shootingConditions;
    }

    public override void Show(Transform parent, bool onMech = true)
    {
        base.Show(parent, onMech);

        //Find barrels
        barrels.Clear();
        if (pointsOfInterest.Contains(PointOfInterest.FirstBarrel))
        {
            barrels.Add(GetPointOfInterest(PointOfInterest.FirstBarrel));
        }
        if (pointsOfInterest.Contains(PointOfInterest.SecondBarrel))
        {
            barrels.Add(GetPointOfInterest(PointOfInterest.SecondBarrel));
        }

        audioSource = visualItem.gameObject.AddComponent<AudioSource>();

        if(visualItem.gameObject.GetComponent<GunAnimator>() == null)
        {
            this.gunAnimator = visualItem.gameObject.AddComponent<GunAnimator>();
            this.gunAnimator.shootable = this;
           // this.gunAnimator.EndShooting();
        }

        Reset();

    }
    public override void Detach()
    {
        base.Detach();

        gunAnimator.EndShooting();
    }


    public Sprite GetSprite()
    {
        return inventorySprite;
    }
    public SocketType GetSocketType()
    {
        if (isRifle)
        {
            return SocketType.Rifle;
        }
        return SocketType.Gun;
    }

    public void SetRecoilDurations()
    {
        recoilDuration = Mathf.Min(ANIMATION_RECOIL_DURATION_PERCENTAGE * thisCycleReloadTime, ANIMATION_RECOIL_DURATION_PERCENTAGE);
        returnDuration = Mathf.Min(ANIMATION_RETURN_DURATION_PERCENTAGE * thisCycleReloadTime, ANIMATION_RETURN_DURATION_PERCENTAGE);
    }

    public void AnimateRecoil(float actionspeed, bool shoot = false)
    {

        if (phase == RecoilPhase.Rest && shoot)
        {
            phase = RecoilPhase.Recoil;
            currentRecoilDuration = 0;
        }
        if (phase == RecoilPhase.Recoil)
        {
            if (currentRecoilDuration >= recoilDuration)
            {
                phase = RecoilPhase.Return;
                //Save the spill
                currentReturnDuration = currentRecoilDuration - recoilDuration;
                //Reset
                currentRecoilDuration = 0;
            }
            else
            {
                currentRecoilDuration += Time.deltaTime*actionspeed;

                float t = Mathf.Min(currentRecoilDuration / recoilDuration, 1);
                t = Mathf.Sin(t * Mathf.PI * 0.5f);
                //Smooth curve
                //Smoothstep =  t*t * (3f - 2f*t)
                //Sine: Mathf.Sin(t * Mathf.PI * 0.5f);
                // t = t * t * t * (t * (6f * t - 15f) + 10f);

                visualItem.localPosition = new Vector3(0, 0, -t*ANIMATION_RECOIL_LENGTH);


            }
        }
        else if (phase == RecoilPhase.Return)
        {
            if (currentReturnDuration >= returnDuration)
            {
                phase = RecoilPhase.Rest;
                //Reset
                currentReturnDuration = 0;
            }
            else
            {
                currentReturnDuration += Time.deltaTime*actionspeed;

                float prcntg = Mathf.Min(currentReturnDuration / returnDuration, 1);

                visualItem.localPosition = new Vector3(0, 0, -ANIMATION_RECOIL_LENGTH+prcntg*ANIMATION_RECOIL_LENGTH);
            }
        }
    }

    private void SetConditions()
    {
        Condition[] cnn = new Condition[2];

        cnn[0] = Condition.ShootingWithARangedWeapon;

        if (isRifle)
        {
            cnn[1] = Condition.ShootingWithARifle;

        }
        else if (bullet.type == BulletType.Mortar)
        {
            cnn[1] = Condition.ShootingWithAMortar;

        }
        else{
            cnn[1] = Condition.ShootingWithAMissileLauncher;

        }

        shootingConditions = GetConditions(cnn);
    }

    public void RegisterTargeter(Targeter targeter)
    {
        this.targeter = targeter;
    }

    public void Shoot(GameUnit target)//Targeter targeter)
    {

        if(targeter == null && socketedIn is GunArray) {

            targeter = ((GunArray)socketedIn).targeter;
        }

        //GameUnit target = targeter.GetTarget();

        //One shot per barrel
        foreach(Transform barrel in barrels)
        {
            if (bullet.type == BulletType.Gunshot)
            {
                GunShot(target, barrel);

            }else if (bullet.type == BulletType.Mortar || bullet.type == BulletType.MissileLauncher)
            {
                //Launch Projectile
                Global.instance.LaunchProjectileAtTarget(
                    GetOriginPoint().position,
                    targeter.GetJointPos(),
                    targeter.GetTargetTransform().position,
                    bullet);
                    

                   /*barrel,
                   targeter.GetTargetTransform(),
                   bullet,
                   TurretTargeter.PROJECTILE_GRAVITY,
                   targeter.GetLaunchAngle()*/
                   
                  // );
            }
            effects[EffectWhen.Shooting].Spawn(barrel.position).rotation = barrel.rotation;
        }
       
        //Play gun sound
        if (audioSource != null)
        {
            sounds.PlaySound(SoundWhen.Shooting, audioSource, false);
        }
    }

    private void GunShot(GameUnit target, Transform from)
    {
        if(target != null)
        {
            //Try to hit its shield first
            Senses.Hit shieldHit = owner.senses.TryToHit(from.position, target, RAYCASTS_WHEN_SHOOTING, true);

            if(shieldHit.unit != null && shieldHit.unit.stats.GetValuePercentage(Stat.Shield) > 0)
            {
                //Forge3D.Forcefield ffHit = shieldHit.hit.collider.transform.GetComponentInParent<Forge3D.Forcefield>();
                bullet.Hit(owner, shieldHit.unit, from, /*ffHit,*/ shieldHit.hit.point,false, 1);
                return;
            }

            //If there is no shield then hit health
            Senses.Hit healthHit = owner.senses.TryToHit(from.position, target, RAYCASTS_WHEN_SHOOTING, false);

            if (healthHit.unit != null )
            {
                bullet.Hit(owner, healthHit.unit, from, /*null,*/ healthHit.hit.point,false, 1);
                return;
            }

            if (owner.isPlayer)
            {
                Debug.Log("Missed shot");
            }
        }
    }

    //Aimable
    public BulletType GetBulletType()
    {
        return bullet.type;
    }
    public BulletTarget GetBulletTarget()
    {
        return bullet.target;
    }
    public Transform GetOriginPoint()
    {
        return GetPointOfInterest(PointOfInterest.FirstBarrel);
    }
    /*public bool CanDetect(GameUnit gu)
    {
        return owner.senses.CanHear(gu);
    }*/
    public Item Item()
    {
        return (Item)this;
    }
    public void Reload()
    {
        currentReloadTime = 0;
        thisCycleReloadTime = owner.stats.GetCurrentValue(Stat.ReloadSpeed) * reloadTime;
    }
    public void Reset()
    {
        currentReactionTime = 0;
        thisCycleReloadTime = owner.stats.GetCurrentValue(Stat.ReloadSpeed) * reloadTime;
        Reload();
    }
    public bool CanShoot()
    {
        return currentReloadTime >= thisCycleReloadTime;
    }
    public float GetTimeUntilCanShootPercentage()
    {
        if(currentReactionTime+ currentReloadTime < 0) { return 0; }
        return (currentReactionTime + currentReloadTime) / (reactionTime + thisCycleReloadTime);
    }

    public float GetTimeUntilCanShootTimer()
    {
        return (reactionTime + thisCycleReloadTime) * (1-GetTimeUntilCanShootPercentage());
    }

    public override bool HasReload()
    {
        return true;
    }

    public override float GetTimer(float actionspeed)
    {
        return GetTimeUntilCanShootTimer()/actionspeed;
    }

    public override bool IsReady()
    {
        return CanShoot();
    }

    public override float GetTimeUntilReloadFinished(float actionspeed)
    {
        return GetTimeUntilCanShootPercentage()/actionspeed;
    }

    public override void Execute(GameUnit target)
    {
        base.Execute(target);

        Shoot(target);
        Reload();

        if (isRifle)
        {
            AnimateRecoil(1, true); //Needed?
        }
        else
        {
            gunAnimator.ResumeShooting();
        }

    }

    public override void Tick(float actionspeed, Vector3 mousePosition, GameUnit target, Vector3 mousePositionOnZPlane)
    {
        base.Tick(actionspeed, mousePosition, target, mousePositionOnZPlane);

        if (currentReactionTime >= reactionTime || !(bullet.type == BulletType.Gunshot))
        {
            currentReactionTime = reactionTime;

            if (currentReloadTime >= thisCycleReloadTime)
            {
                currentReloadTime = thisCycleReloadTime;
                //Ready to shoot :)
            }
            else
            {
                currentReloadTime += Time.deltaTime*actionspeed;
            }
        }
        else
        {
            currentReactionTime += Time.deltaTime*actionspeed;
        }

        if (isRifle)
        {
            AnimateRecoil(actionspeed);
        }

    }

    public override Item CloneBaseClass()
    {
        return (Item) Clone();
    }
    public Socketable CloneSocketable()
    {
        return Clone();
    }
    public new Gun Clone()
    {
        return new Gun(
            itemName,
            description,
            prefab,
            rarity,
            inventorySprite,
            sounds.Clone(),
            //muzzles.CloneGameValues(),
            effects.CloneSimple(),
            bullet.Clone(),
            thisCycleReloadTime,
            pointsOfInterestPreShowing.CloneSimple(),
            isRifle,
            (int)inventoryHeight,
            socketTypes,
            buffs,
            alignment
            ,origin);


            /*itemName, 
            prefab, 
            alignment, 
            sounds, 
            muzzles.CloneGameValues(), 
            bullet.Clone(),
            reloadTime,
            muzzleName*/
           // shotsInSalvo,
           // timeBetweenSalvoShots
       //);
    }

}

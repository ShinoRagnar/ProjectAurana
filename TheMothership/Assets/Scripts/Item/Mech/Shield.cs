using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Shield : MechItem, Socketable, IGameClone<Shield>{

    public static readonly Vector3 SHIELD_SIZE = new Vector3(5, 5, 5);
    public static readonly float SHIELD_VISUAL_SCALE = 0.8f;
    public static readonly float BLOCK_FORCE = 5;

    public SocketableNames origin;
    public float rechargeTime;
    public float blockSize;
    private bool isBlocking = false;
    public float expectedTimeUntilNextBlock = 0;
    public float currentTimeUntilNextBlock = 0;

    public ListHash<int> blockingConditions;

    public Shield(ShieldData data, SocketableNames orig) : this
        (data.itemName,
        data.itemDescription,
        Global.Resources[data.prefab],
        data.rechargeTime,
        data.inventoryItemHeight,
        data.inventoryItemWidth,
        data.blockSize,
        data.rarity,
        Global.Resources[data.audio],
        Global.Resources[data.sprite],
        SocketSlotDataToSocketTypeArray(data.sockets),
        Global.Resources[data.buffs],
        null,
        PointOfInterestDataToDictionary(data.points),
        EffectWhenDataToDictionary(data.effectsWhen),
        orig
        )
    { }
    public Shield(
       //Itemname
       string itemn,
       string description,
       //The item prefab
       Transform item,
       //Rarity
       float rechargeTime,
       float itemHeight,
       float itemWidth,
       float blockSize,
       //Rarity
       Rarity rarity,
       AudioContainer audio,
       Sprite picture,
       SocketType[] emptySockets = null,
       //Buffs applied by this item
       List<Buff> appliesBuff = null,
       //Alignment (not used for scaling)
       Alignment alig = null,
       //Transform names on child items
       DictionaryList<PointOfInterest, string> pointsOfInterestPreShowingVal = null,
       DictionaryList<EffectWhen, Effect> effectLibrary = null,
       //Origin
       SocketableNames orig = SocketableNames.NothingSocketed

        //Buffs
        ) : base(itemn, item, rarity, description, null, picture, appliesBuff, alig, pointsOfInterestPreShowingVal, effectLibrary, audio)
    {
        this.rechargeTime = rechargeTime;
        this.blockSize = blockSize;
        this.inventoryWidth = itemWidth;
        this.inventoryHeight = itemHeight;
        origin = orig;
        SetConditions();
    }

    private void SetConditions()
    {
        blockingConditions = GetConditions(new Condition[] { Condition.Blocking });
    }
    /* public void Tick()
     {
         if (currentRechargeTime >= rechargeTime)
         {
             currentRechargeTime = rechargeTime;
             canBlock = true;
         }
         else
         {
             currentRechargeTime += Time.deltaTime;
         }
     }´*/

    public override bool HasReload()
    {
        return true;
    }

    public override float GetTimer(float actionspeed)
    {
        if (currentTimeUntilNextBlock == 0 || IsReady()) { return 0; };
        return ((currentTimeUntilNextBlock / expectedTimeUntilNextBlock) * expectedTimeUntilNextBlock) / actionspeed;
    }

    public override float GetTimeUntilReloadFinished(float actionspeed)
    {

        if (expectedTimeUntilNextBlock == 0 || IsReady()) { return 1; }
        return (currentTimeUntilNextBlock / expectedTimeUntilNextBlock) / actionspeed;
    }

    public override bool IsReady()
    {
        return socketedIn.IsReady();
    }

    public void SetBlockTime(float time)
    {
        currentTimeUntilNextBlock = 0;
        expectedTimeUntilNextBlock = time;
    }

    public override void Tick(float actionspeed, Vector3 mousePosition, GameUnit target, Vector3 mousePositionOnZPlane)
    {
        base.Tick(actionspeed, mousePosition, target, mousePositionOnZPlane);

        currentTimeUntilNextBlock += Time.deltaTime * actionspeed;

        if (currentTimeUntilNextBlock > expectedTimeUntilNextBlock)
        {
            currentTimeUntilNextBlock = expectedTimeUntilNextBlock;
        }
    }

    public override int GetReloadGroup()
    {
        return InventoryHandler.SHIELD_RELOAD_GROUP;
    }


    public void Block()
    {
        Animator anim = ((Core)socketedIn).meleeAnimator;

        if (!isBlocking)
        {
            anim.SetBool("Blocking", true);
            isBlocking = true;
        }
    }
    public void StopBlocking()
    {
        Animator anim = ((Core)socketedIn).meleeAnimator;

        if (isBlocking)
        {
            anim.SetBool("Blocking", false);
            isBlocking = false;
        }
    }

    public override void Execute(GameUnit target)
    {
        base.Execute(target);

        Block();

    }
    // GameObject display;

    public void BeginBlockingPhase()
    {

        bool playBlockSound = false;

        if (effects.Contains(EffectWhen.Shield))
        {
            effects[EffectWhen.Shield].Spawn(visualItem.position,null,0,0,0,SHIELD_SIZE.x* SHIELD_VISUAL_SCALE);
        }

        foreach(Global.InAirProjectile iap in Global.projectiles)
        {
            if (Vector3.Distance(iap.projectile.position, visualItem.position) < SHIELD_SIZE.x/2)
            {
                //Prevent from detonating
                iap.detonator.hasDetonated = true;

                //Block visual
                if (effects.Contains(EffectWhen.Block))
                {
                    effects[EffectWhen.Block].Spawn(iap.projectile.position);
                }

                //Mark for later deletion
                Global.projectiles.RemoveLater(iap);

                //Stop any particle systems of neutralized projectiles
                ParticleSystem[] ps = iap.projectile.GetComponentsInChildren<ParticleSystem>();
                if(ps != null)
                {
                    foreach(ParticleSystem p in ps)
                    {
                        p.Stop();
                    }
                }
                //Encapsule the projectile
                iap.projectile.parent = Global.Resources[EffectNames.Neutralized].Spawn(iap.projectile.position);
                
                //Push the projectile away
                iap.projectile.parent.gameObject.GetComponent<Rigidbody>().AddForce(new Vector3(0, 0, owner.movement.facing == Facing.Right ? BLOCK_FORCE : -BLOCK_FORCE), ForceMode.Impulse);

                playBlockSound = true;

                //Global.Resources[EffectNames.SuccessfullBlock].Spawn(iap.projectile.parent.position);
            }
        }
        if (owner.ai != null && owner.ai.target != null && owner.ai.target.mech != null)
        {
            foreach (Core c in owner.ai.target.mech.equippedCores)
            {
                if (c.weapon != null 
                    && c.weapon.isAttacking
                    && c.weapon.canAttack
                    && c.weapon.attackFrameBeforeDamageDealt 
                    && Vector3.Distance(c.weapon.visualItem.position, visualItem.position) < (SHIELD_SIZE.x / 2 + c.weapon.weaponLength / 2))
                {
                    c.weapon.Parry(c.meleeAnimator);
                    playBlockSound = true;

                    //Block visual
                    if (effects.Contains(EffectWhen.Block))
                    {
                        effects[EffectWhen.Block].Spawn(c.weapon.visualItem.position);
                    }
                }
            }
        }
        if (playBlockSound)
        {
            //Block sounds
            sounds.PlaySound(SoundWhen.Block, audioSource, true);

        }
        //Clean up remove laters
        Global.projectiles.Remove();
        

        /*display = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        display.transform.localScale = SHIELD_SIZE;
        display.transform.position = visualItem.position;
        Global.Destroy(display.GetComponent<SphereCollider>());*/

    }
    public void EndBlockingPhase()
    {
        StopBlocking();
       // Global.Destroy(display);

    }

    public SocketableNames GetName()
    {
        return origin;
    }

    public Socketable CloneSocketable()
    {
        return Clone();
    }

    public SocketType GetSocketType()
    {
        return SocketType.Shield;
    }
    public Sprite GetSprite()
    {
        return inventorySprite;
    }
    public override Item CloneBaseClass()
    {
        return (Item)Clone();
    }
    public new Shield Clone()
    {
        return new Shield(
            itemName,
            description,
            prefab,
            rechargeTime,
            inventoryHeight,
            inventoryWidth,
            blockSize,
            rarity,
            sounds.Clone(),
            inventorySprite,
            socketTypes,
            buffs,
            alignment,
            pointsOfInterestPreShowing,
            effects,
            origin);
    }
}

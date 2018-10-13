using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class GunArray : MechItem, Socketable, Shootable
{ //, Shootable, Socketable{ //, Shootable{

    public static readonly float ARRAY_MAX_DELAY = 1f;

    //public BulletType bulletType;
    //ListDictionary<Transform, Gun> mounted = new ListDictionary<Transform, Gun>();
    public List<Gun> socketedGuns = new List<Gun>();

    public SocketableNames origin;

    public GunAnimator gunAnimator;

    public Targeter targeter;

    public GunArray(GunArrayData data, SocketableNames orig) : this(
        data.itemName,
        data.itemDescription,
        Global.Resources[data.prefab],
        Global.Resources[data.sprite],
        data.rarity,
        SocketSlotDataToSocketTypeArray(data.sockets),
        data.inventoryHeight,
        data.inventoryWidth,
        Global.Resources[data.buffs],
        null,
        PointOfInterestDataToDictionary(data.points),
        EffectWhenDataToDictionary(data.effectsWhen),
        Global.Resources[data.audio],
        orig
    )
    {
        AddSocketsFromSocketData(data.sockets);
    }
    public GunArray(

       //Itemname
       string itemn,
       string description,
       //The item prefab
       Transform item,
       //The picture prefab
       Sprite picture,
       //Rarity
       Rarity rarityVal,
       //Bullet etc (Unique for this class)
       //BulletType bullet,
       //Sockets
       SocketType[] emptySockets,
       int inventoryHeight = 2,
       int inventoryWidth = 2,
       //Buffs applied by this item
       List<Buff> appliesBuff = null,
       //Alignment (not used for scaling)
       Alignment alig = null,

       DictionaryList<PointOfInterest, string> list = null,

       DictionaryList<EffectWhen, Effect> effectLibrary = null,

       AudioContainer audioContainer = null,

       SocketableNames orig = SocketableNames.NothingSocketed
        //Slots
        ) : base(itemn, item, rarityVal, description, emptySockets, picture, appliesBuff, alig, list,effectLibrary,audioContainer)
    {
        this.origin = orig;
        this.inventoryHeight = inventoryHeight;
        this.inventoryWidth = inventoryWidth;
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
        return SocketType.Gun;
    }
    public BulletTarget GetBulletTarget()
    {
        return BulletTarget.Target;
    }
    public Sprite GetSprite()
    {
        return inventorySprite;
    }
     public new GunArray Clone()
     {
         return new GunArray(itemName, description, prefab,inventorySprite, rarity, socketTypes, (int)inventoryHeight, (int)inventoryWidth, buffs, alignment,pointsOfInterestPreShowing.CloneSimple(),effects,sounds.Clone(),origin);

         // return new GunArray(itemName, bulletType, prefab, pointsOfInterestPreShowing);
     }

    public void RegisterTargeter(Targeter targeter)
    {
        this.targeter = targeter;
    }

    /*public override bool SocketItem(int slotNum, Socketable tosocket)
    {
        bool ret = base.SocketItem(slotNum, tosocket);

        if (tosocket is Gun)
        {
            gunAnimator.children.Add(((Gun)tosocket).gunAnimator);
        }

        return ret;
    }
    public override MechItem Unequip(int slot, bool keepVisible = false)
    {
        MechItem ret = base.Unequip(slot, keepVisible);

        if (ret is Gun)
        {
            gunAnimator.children.Remove(((Gun)ret).gunAnimator);
        }

        return ret;
    }*/

    /* public void MountAndShow(Gun g, PointOfInterest poi, TurretTargeter targeter)
     {
         //Get joint position
         Transform joint = GetPointOfInterest(poi);
         //Remove any character alignements
         g.alignment = Alignment.NO_ALIGNMENT;
         //Equip item
         ie.Equip(g);
         //Show gun
         g.Show(joint);
         //Add targeting
         mounted.Add(joint, g);
         //Add animator to each gun
         g.gunAnimator = g.visualItem.gameObject.AddComponent<GunAnimator>();
         g.gunAnimator.targeter = targeter;
         g.gunAnimator.shootable = g;
        // g.gunAnimator.EndShooting();
         //Add the animator as a sub animator
         targeter.gunAnimator.children.Add(g.gunAnimator);
         RecalculateGunReactionTimeOffset();
     }*/


    //Aimable
    public BulletType GetBulletType()
    {
        return BulletType.Array;
    }

    public Transform GetOriginPoint()
    {
        return visualItem;
    }
    /*public bool CanDetect(GameUnit gu)
    {
        return owner.senses.CanHear(gu);
    }*/
    public Item Item()
    {
        return (Item)this;
    }
    public void Reload(){}
    public void Reset(){}

    public bool CanShoot()
    {
        return true;
    }
    public float GetTimeUntilCanShootPercentage()
    {
        float max = 0;
        foreach (Gun g in socketedGuns)
        {
            max = g.GetTimeUntilCanShootPercentage() > max ? g.GetTimeUntilCanShootPercentage() : max;
        }
        return max;
    }
    public override void Tick(float actionspeed, Vector3 mousePosition, GameUnit target, Vector3 mousePositionOnZPlane)
    {
        base.Tick(actionspeed, mousePosition, target, mousePositionOnZPlane);

        gunAnimator.canShoot = true;

        /*foreach(Gun g in socketedGuns)
        {
            //Debug.Log("Iterating guns tick");
            g.Tick(actionspeed);
            g.AnimateRecoil(actionspeed);
        }*/
    }

    public void Shoot(GameUnit gameunit)
    {
        foreach (Gun g in socketedGuns)
        {
           // Debug.Log("Iterating guns shootat");
            if (g.CanShoot())
            {
                g.AnimateRecoil(1,true); //Needed?
                g.Shoot(gameunit);
                g.Reset();
            }
        }
    }
    public override void Show(Transform parent, bool onMech)
    {
        base.Show(parent, onMech);

        audioSource = visualItem.gameObject.AddComponent<AudioSource>();

        if (visualItem.gameObject.GetComponent<GunAnimator>() == null)
        {
            this.gunAnimator = visualItem.gameObject.AddComponent<GunAnimator>();
            this.gunAnimator.shootable = this;
            // this.gunAnimator.EndShooting();
        }

    }


    public override bool SocketItem(int socket, Socketable tosocket)
    {
        bool ret = base.SocketItem(socket, tosocket);

        if(tosocket is Gun)
        {
            Gun g = (Gun)tosocket;
            socketedGuns.Add(g);
            g.SetRecoilDurations();
            RecalculateGunReactionTimeOffset();
        }
        return ret;
    }

    public override MechItem Unequip(int slot, bool keepVisible = false)
    {
        MechItem ret =  base.Unequip(slot, keepVisible);

        if (ret is Gun)
        {
            socketedGuns.Remove((Gun)ret);
            RecalculateGunReactionTimeOffset();
        }
        return ret;
    }

    public void RecalculateGunReactionTimeOffset()
    {
        int count = socketedGuns.Count;
        float interval = ARRAY_MAX_DELAY / ((float) count);

        for(int i = 0; i < count; i++)
        {
            socketedGuns[i].reactionTime = i * interval;
        }
    }


}

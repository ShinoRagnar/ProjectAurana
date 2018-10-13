using System.Collections.Generic;
using UnityEngine;

public enum LegMovementType
{
    NoMovement,
    Walking,
    Rolling,
    Jetpack
}
public class Legs : MechItem, IGameClone<Legs> {

    public HashSet<int> movement = new HashSet<int>();

    public static readonly float MAX_LEG_INVENTORY_WIDTH = 10;
    public static readonly float MAX_LEG_INVENTORY_HEIGHT = 10;
    public static readonly float LEG_SCALE_MULTIPLIER = 0.2f;//0.075f;

    public int widthCapacity;
    public int heightCapacity;

    public MechJetpack jetpack;
    public Vector3 mountAlignment = new Vector3(0, 0, 0);

    public LegNames origin;

    public Vector2 GetInventorySize()
    {
        return new Vector2(Mathf.FloorToInt((widthCapacity+1) / 2), Mathf.FloorToInt((widthCapacity+1) / 2));
    }

    public Legs(LegData data, LegNames orig) : this
        (
        data.itemName,
        data.itemDescription,
        Global.Resources[data.prefab],
        data.rarity,
        Global.Resources[data.sprite],
        data.widthCapacity,
        data.heightCapacity,
        Global.Resources[data.audio],
        MovementTypeToHash(data.possibleMovements),
        SocketSlotDataToSocketTypeArray(data.sockets),
        EffectWhenDataToDictionary(data.effectsWhen),
        Global.Resources[data.buffs],
        new Alignment(data.position.x, data.position.y, data.position.z,
                        data.rotation.x, data.rotation.y, data.rotation.z,
                        data.scale.x, data.scale.y, data.scale.z),
        PointOfInterestDataToDictionary(data.points),
        orig)
    {
        AddSocketsFromSocketData(data.sockets);
    }
    public Legs(
        //Itemname
        string itemn,
        string description,
        //The item prefab
        Transform item,
        //Rarity
        Rarity rarityVal,
        //Picture of the item
        Sprite picture,
        //How many cores can be stacked in a row
        int widthCapacityVal,
        //How many cores can be stacked on top
        int heightCapacityVal,
        AudioContainer soundsVal,
        //What movement types that are possible with the legs
        HashSet<int> movementVal,
        //Sockets
        SocketType[] emptySockets = null,
        DictionaryList<EffectWhen, Effect> effectLibrary = null,
        //Buffs applied by this item
        List<Buff> appliesBuff = null,
        //Alignment (not used for scaling)
        Alignment alig = null,
        //Transform names on child items
        DictionaryList<PointOfInterest, string> pointsOfInterestPreShowingVal = null,
        LegNames legName = LegNames.NoName
        ) : base(itemn, item, rarityVal, description, emptySockets, picture, appliesBuff, alig, pointsOfInterestPreShowingVal)
    {
        origin = legName;

        //Adjust x-z scales of the model
        float scale = LEG_SCALE_MULTIPLIER * (widthCapacityVal-1);
        
        //Adjust Y-position on mount
        mountAlignment = new Vector3(0, scale == 0 ? 1 : (1 - (scale/0.5f)) * 0.35f);

        if (this.alignment != null)
        {
            this.alignment = new Alignment(alignment.x, alignment.y, alignment.z, alignment.rotX, alignment.rotY, alignment.rotZ, scale, scale, scale);
        }else{
            this.alignment = new Alignment(0, 0, 0, 0, 0, 0, scale, scale, scale);
        }

        this.effects = effectLibrary;
        this.widthCapacity = widthCapacityVal;
        this.heightCapacity = heightCapacityVal;
        this.sounds = soundsVal;
        this.movement = movementVal;
        this.inventoryWidth = GetInventorySize().x;
        this.inventoryHeight = GetInventorySize().y;

       // Debug.Log("Leg has size:"+GetInventorySize());
    }
    public static HashSet<int> MovementTypeToHash(LegMovementType[] types)
    {
        HashSet<int> ret = new HashSet<int>();
        foreach(LegMovementType type in types)
        {
            ret.Add((int)type);
        }
        return ret;
    }

    public override void DetachFromMech(ListHash<Rigidbody> detached)
    {
        base.DetachFromMech(detached);

        if(audioSource != null)
        {
            //Debug.Log("Playing explosion sounds");
            sounds.PlaySound(SoundWhen.Exploding, audioSource, false);
        }
        else
        {
            Debug.Log("Míssing explosion sound!!!");
        }

    }

    public override void ShowDetachVisuals()
    {
      //  Debug.Log("Showing leg degach visual");

        if (effects.Contains(EffectWhen.Detach))
        {
            effects[EffectWhen.Detach].Spawn(owner.GetCenterPos());
        }
        else
        {
            Debug.Log("no detach effect!!");
        }
    }

    public override Item CloneBaseClass()
    {
        return (Item) Clone();
    }

    public new Legs Clone()
    {
        return new Legs(itemName, description, prefab,rarity, inventorySprite, widthCapacity, heightCapacity, sounds.Clone(),movement, socketTypes,effects,buffs, alignment.Clone(), pointsOfInterestPreShowing.CloneSimple(),origin);
    }

    public Legs CloneWithNewSize(int wid, int hei)
    {
        return new Legs(itemName,description, prefab, rarity, inventorySprite, wid, hei, sounds.Clone(),movement, socketTypes, effects,buffs, alignment.Clone(), pointsOfInterestPreShowing.CloneSimple());
    }
}

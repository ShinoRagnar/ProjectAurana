using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Crystal : MechItem, Socketable, IGameClone<Crystal>{

    public SocketableNames origin;

    public Crystal(CrystalData data, SocketableNames orig) : this
        (data.itemName,
        data.itemDescription,
        Global.Resources[data.prefab],
        data.rarity,
        Global.Resources[data.audio],
        Global.Resources[data.sprite],
        Global.Resources[data.buffs],
        null,
        PointOfInterestDataToDictionary(data.points),
        EffectWhenDataToDictionary(data.effectsWhen),
        orig
        )
    { }
    public Crystal(
       //Itemname
       string itemn,
       string description,
       //The item prefab
       Transform item,
        //Rarity
        //Rarity rarityVal,
        //Sockets
        //SocketType[] emptySockets = null,
        //The picture prefab
        //Rarity
        Rarity rarity,
        AudioContainer audio,
       Sprite picture,
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
        ) : base(itemn, item, rarity,description, null,picture,appliesBuff,alig,pointsOfInterestPreShowingVal,effectLibrary,audio)
    {
        origin = orig;
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
        return SocketType.Crystal;
    }
    public Sprite GetSprite()
    {
        return inventorySprite;
    }
    public override Item CloneBaseClass()
    {
        return (Item) Clone();
    }
    public new Crystal Clone()
    {
        return new Crystal(itemName,description, prefab, rarity, sounds.Clone(), inventorySprite, buffs, alignment, pointsOfInterestPreShowing, effects, origin);
    }
    public override void ShowDetachVisuals()
    {

        if (effects.Contains(EffectWhen.Detach))
        {
            Transform t = effects[EffectWhen.Detach].Spawn(Global.References[SceneReferenceNames.NodeClone]);
            t.position = visualItem.position;
            t.rotation = visualItem.rotation;
            t.Rotate(0, 90, 0);


            Transform a = effects[EffectWhen.Detach].Spawn(Global.References[SceneReferenceNames.NodeClone]);
            a.position = visualItem.position;
            a.rotation = visualItem.rotation;
            a.Rotate(0, -90, 0);
            //boxcollider != null ? -boxcollider.size.z / 2 : 
        }
    }
}

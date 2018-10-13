using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/*
public class Attachment : MechItem, IGameClone<Attachment>{

    public static readonly float ATTACHMENT_ROTATION = 45;

    public int diameter;

    public Attachment(
       //Itemname
       string itemn,
       //The item prefab
       Transform item,
       //Rarity
       Rarity rarityVal,
       //Size in slots
       int diameterVal,
       //Sockets
       SocketType[] emptySockets = null,
       //The picture prefab
       Sprite picture = null,
       //Buffs applied by this item
       List<Buff> appliesBuff = null,
       //Alignment (not used for scaling)
       Alignment alig = null,
       //Transform names on child items
       DictionaryList<PointOfInterest, string> pointsOfInterestPreShowingVal = null
     ) : base(itemn,item,rarityVal,emptySockets,picture,appliesBuff,alig,pointsOfInterestPreShowingVal){
        this.inventoryWidth = diameterVal;
        this.inventoryHeight = diameterVal;
        this.rotation = ATTACHMENT_ROTATION;
        this.diameter = diameterVal;
    }
    public override Item CloneBaseClass()
    {
        return (Item) Clone();
    }

    public new Attachment Clone()
    {
        return new Attachment(itemName, prefab, rarity, (int)inventoryWidth,socketTypes, inventorySprite, buffs, alignment, pointsOfInterestPreShowing);
    }
    
}*/

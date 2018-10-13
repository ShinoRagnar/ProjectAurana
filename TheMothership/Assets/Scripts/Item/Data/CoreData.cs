using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewCore", menuName = "Game/Core", order = 3)]
public class CoreData : ScriptableObject
{
    [SerializeField]
    public string enumName;
    [SerializeField]
    public string itemName = "";
    [SerializeField]
    public string itemDescription = "";
    [SerializeField]
    public PrefabNames prefab;
    [SerializeField]
    public SpriteNames sprite;
    [SerializeField]
    public Rarity rarity = Rarity.Common;
    [SerializeField]
    public InventoryBlockTypeData[] rows = new InventoryBlockTypeData[]{
        new InventoryBlockTypeData(
            new InventoryBlockType[]{ InventoryBlockType.Vacant, InventoryBlockType.Connected, InventoryBlockType.Vacant}
        ),
        new InventoryBlockTypeData(
            new InventoryBlockType[]{ InventoryBlockType.Connected, InventoryBlockType.Occupied, InventoryBlockType.Connected}
        )
    };
    [SerializeField]
    public Vector2 connectionPos = new Vector2(1, 0);
    [SerializeField]
    public SocketSlotData[] sockets;
    [SerializeField]
    public BuffNames[] buffs;
    [SerializeField]
    public PointOfInterestData[] points;
    [SerializeField]
    public EffectsWhenData[] effectsWhen;
    [SerializeField]
    public AudioContainerNames audio;
    [SerializeField]
    public int armor = 1;

}

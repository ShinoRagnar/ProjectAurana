using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewGunArray", menuName = "Game/GunArray", order = 3)]
public class GunArrayData : ScriptableObject
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
    public int inventoryHeight = 2;
    [SerializeField]
    public int inventoryWidth = 2;

}

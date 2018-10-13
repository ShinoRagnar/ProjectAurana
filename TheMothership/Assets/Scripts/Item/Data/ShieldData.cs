using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewShield", menuName = "Game/Shield", order = 11)]
public class ShieldData : ScriptableObject
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
    public EffectsWhenData[] effectsWhen;
    [SerializeField]
    public SocketSlotData[] sockets;
    [SerializeField]
    public BuffNames[] buffs;
    [SerializeField]
    public PointOfInterestData[] points;
    [SerializeField]
    public AudioContainerNames audio;

    [SerializeField]
    public float rechargeTime = 4;
    [SerializeField]
    public int inventoryItemHeight = 2;
    [SerializeField]
    public int inventoryItemWidth = 2;
    [SerializeField]
    public float blockSize = 1;


    [SerializeField]
    public Vector3 position = Vector3.zero;
    [SerializeField]
    public Vector3 rotation = Vector3.zero;
    [SerializeField]
    public Vector3 scale = Vector3.zero;
}


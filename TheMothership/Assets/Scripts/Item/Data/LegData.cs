using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewLegs", menuName = "Game/Leg", order = 3)]
public class LegData : ScriptableObject
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
    public AudioContainerNames audio;
    [SerializeField]
    public SocketSlotData[] sockets;
    [SerializeField]
    public BuffNames[] buffs;
    [SerializeField]
    public PointOfInterestData[] points;
    [SerializeField]
    public int widthCapacity = 1;
    [SerializeField]
    public int heightCapacity = 1;
    [SerializeField]
    public LegMovementType[] possibleMovements;
    [SerializeField]
    public EffectsWhenData[] effectsWhen;

    [SerializeField]
    public Vector3 position = Vector3.zero;
    [SerializeField]
    public Vector3 rotation = Vector3.zero;
    [SerializeField]
    public Vector3 scale = Vector3.zero;

}

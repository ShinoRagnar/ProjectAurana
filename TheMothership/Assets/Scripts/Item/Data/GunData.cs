using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewGun", menuName = "Game/Gun", order = 11)]
public class GunData : ScriptableObject
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
    public BulletNames bullet;
    [SerializeField]
    public SocketSlotData[] sockets;
    [SerializeField]
    public BuffNames[] buffs;
    [SerializeField]
    public PointOfInterestData[] points;
    [SerializeField]
    public AudioContainerNames audio;

    [SerializeField]
    public float reloadTime = 1;

    [SerializeField]
    public bool isRifle = false;

    [SerializeField]
    public int diameter = 2;

    [SerializeField]
    public Vector3 position = Vector3.zero;
    [SerializeField]
    public Vector3 rotation = Vector3.zero;
    [SerializeField]
    public Vector3 scale = Vector3.zero;
}
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

[Serializable]
public struct OnHitWhenData
{
    public OnHitNames onHit; //onHit;
    public InteractionCondition[] conditions;
}
[CreateAssetMenu(fileName = "NewWeapon", menuName = "Game/Weapon", order = 11)]
public class WeaponData : ScriptableObject{

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
        public WeaponType type;
        [SerializeField]
        public OnHitWhenData[] onHit;
        [SerializeField]
        public DamageModifier[] damageModifiers;
        [SerializeField]
        public Damage[] damage = new Damage[] { new Damage(DamageType.Physical,1)};
        [SerializeField]
        public float impact = 10000;
        [SerializeField]
        public int inventoryItemHeight = 2;
        [SerializeField]
        public int inventoryItemWidth = 2;
        //[SerializeField]
       // public float damage;
        [SerializeField]
        public float shakeFactor = 1;


        [SerializeField]
        public Vector3 position = Vector3.zero;
        [SerializeField]
        public Vector3 rotation = Vector3.zero;
        [SerializeField]
        public Vector3 scale = Vector3.zero;
}

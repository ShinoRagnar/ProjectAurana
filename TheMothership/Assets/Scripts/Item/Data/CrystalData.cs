using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Scriptable item
[CreateAssetMenu(fileName = "NewCrystal", menuName = "Game/Crystal", order = 13)]
public class CrystalData : ScriptableObject {

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
    public BuffNames[] buffs;

    [SerializeField]
    public EffectsWhenData[] effectsWhen;
    [SerializeField]
    public PointOfInterestData[] points;
    [SerializeField]
    public AudioContainerNames audio;
}

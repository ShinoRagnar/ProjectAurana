using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

//A list of itemdata
[Serializable]
public struct PointOfInterestData
{
    public string prefabNode;
    public PointOfInterest point;
}
//Scriptable item
[CreateAssetMenu(fileName = "NewItem", menuName = "Game/Item", order = 2)]
public class ItemData : ScriptableObject
{
    [SerializeField]
    public string itemName = "";
    [SerializeField]
    public PrefabNames prefab;
    [SerializeField]
    public PointOfInterestData[] points;
    [SerializeField]
    public Vector3 position = Vector3.zero;
    [SerializeField]
    public Vector3 rotation = Vector3.zero;
    [SerializeField]
    public Vector3 scale = Vector3.zero;
    
}
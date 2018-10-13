using UnityEngine;
using System;

[CreateAssetMenu(fileName = "NewEffect", menuName = "Game/Effect", order = 20)]
public class EffectData : ScriptableObject
{
    [SerializeField]
    public string enumName;
    [SerializeField]
    public string effectName;
    [SerializeField]
    public PrefabNames prefab;
    [SerializeField]
    public EffectScaling scaling;
    [SerializeField]
    public float duration;

    [SerializeField]
    public bool handles = false;
    [SerializeField]
    public bool randomZRotation = false;
    [SerializeField]
    public bool hasYDistance = false;
    [SerializeField]
    public bool hasXDistance = false;
    [SerializeField]
    public bool hasZDistance = false;
    [SerializeField]
    public bool isStatic = false;
    [SerializeField]
    public bool cleanupInnerTransforms = false;
    [SerializeField]
    public float cleanupStartAt = 3;

    [SerializeField]
    public PrefabNames[] prefabAlterations;

    //Resources[PrefabNames.Footstep], 2,false,EffectScaling.SizeScaling,false
    //new StatsAffector("Standard Rifle Right Walk", Stat.WalkSpeed, Condition.MovingRight, Calculation.Additive, -0.02f, -0.5f);

    //Buff(string buffnameVal, float durati, Transform icn, bool isDebuffVal, BuffType buffTypeVal)
}
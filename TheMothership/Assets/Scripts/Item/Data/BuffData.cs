using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

//A list of itemdata
[Serializable]
public struct StatsAffectorData
{
    public string statAffectorName;
    public Stat stat;
    public Condition[] condition;
    public Calculation calculation;
    public float amount;
    public float threshold;
}
[CreateAssetMenu(fileName = "NewBuff", menuName = "Game/Buff", order = 20)]
public class BuffData : ScriptableObject
{
    [SerializeField]
    public string enumName;
    [SerializeField]
    public string buffName;
    [SerializeField]
    public SpriteNames buffPicture;
    [SerializeField]
    public BuffType type;
    [SerializeField]
    public float duration;
    [SerializeField]
    public bool isDebuff = true;
    [SerializeField]
    public StatsAffectorData[] affectors;

    //new StatsAffector("Standard Rifle Right Walk", Stat.WalkSpeed, Condition.MovingRight, Calculation.Additive, -0.02f, -0.5f);

    //Buff(string buffnameVal, float durati, Transform icn, bool isDebuffVal, BuffType buffTypeVal)
}
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewOnHit", menuName = "Game/OnHit", order = 11)]
public class OnHitData : ScriptableObject
{
    [SerializeField]
    public string enumName;
    [SerializeField]
    public string onHitName = "";
    [SerializeField]
    public string onHitDescription = "";
    [SerializeField]
    public OnHitType type = OnHitType.OnHitOnly;
    [SerializeField]
    public EndQualifier[] endQualifier = new EndQualifier[] { EndQualifier.BuffExpires, EndQualifier.Death };
    [SerializeField]
    public EffectsWhenData[] effectsWhen;
    [SerializeField]
    public BuffNames[] debuffs;
    [SerializeField]
    public OnHitDelayedDamage[] delayedDamage;


}

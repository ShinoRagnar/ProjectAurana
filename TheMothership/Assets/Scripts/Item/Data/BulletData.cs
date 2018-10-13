using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

[Serializable]
public struct EffectsWhenData
{
    public EffectWhen when;
    public EffectNames effect;
}
[Serializable]
public struct ProjectileRoleData
{
    public ProjectileRole role;
    public PrefabNames projectile;
}
/*[Serializable]
public struct BuffTransferData
{
    public BuffNames buff;
    public float direction;
}*/
[CreateAssetMenu(fileName = "NewBullet", menuName = "Game/Bullet", order = 15)]
public class BulletData : ScriptableObject
{
    [SerializeField]
    public string enumName = "";
    [SerializeField]
    public string bulletName = "";
    [SerializeField]
    public BulletType type;
    [SerializeField]
    public BulletTarget target = BulletTarget.Target;
    [SerializeField]
    public AudioContainerNames audio;
    //[SerializeField]
    //public BuffTransferData[] buffsThatTransfer;
    [SerializeField]
    public EffectsWhenData[] effectsWhen;
    [SerializeField]
    public ProjectileRoleData[] projectiles;
    [SerializeField]
    public OnHitWhenData[] onHit;
    [SerializeField]
    public DamageModifier[] damageModifiers;
    [SerializeField]
    public Damage[] damage = new Damage[] { new Damage(DamageType.Physical, 1) };
    //[SerializeField]
    //public float damage = 0;
    [SerializeField]
    public float impact = 0;
    [SerializeField]
    public float explosionRadius = 0;
    [SerializeField]
    public float waitForParticlesTime = 5;

}

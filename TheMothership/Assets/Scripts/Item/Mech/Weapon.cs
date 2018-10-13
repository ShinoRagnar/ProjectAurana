using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum WeaponType
{
    NoType = -1,
    Sword = 10,
    Hammer = 20,
    Spear = 30,
    Axe = 40
}
public interface Damager
{
    Damage[] GetDamage();
    DamageModifier[] GetDamageModifier();
    OnHitWhen[] GetOnHit();
    ListHash<int> GetConditions();
}
public class Weapon : MechItem, Socketable, Damager,  IGameClone<Weapon> {



    public static readonly float WEAPON_PUSH_RANGE_MULTIPLIER = 3;
    public static readonly float SHOW_DETACH_VISUAL_ON_HIT_CHANCE = 0.3f;
    public static readonly int RAYCASTS_WHEN_ATTACKING = 3;

    public SocketableNames origin;
   // public float damage;
    public float impact;
    public float shakeFactor;
    public WeaponType type;
    public bool isAttacking = false;
    public bool canAttack = true;
    public bool isVulnerableToExecutionAttack = false;
    public bool attackFrameBeforeDamageDealt = false;
    public float weaponLength = 1;
    public bool canCombo = true;
    private bool comboLastFrame = false;
    public float expectedTimeUntilNextSwing = 0;
    public float currentTimeUntilNextSwing = 0;

    //  public DictionaryList<Buff, float> transferringBuffs;
    public ListHash<GameUnit> unitsHitThisPhase = new ListHash<GameUnit>();
    public ListHash<Rigidbody> rigidBodiesHitThisPhase = new ListHash<Rigidbody>();
    public OnHitWhen[] onHit;
    public ListHash<int> meleeConditions;



    // public float currentRechargeTime = 0;
    //  public bool canAttack = false;

    public Weapon(WeaponData data, SocketableNames orig) : this
        (data.itemName,
        data.itemDescription,
        Global.Resources[data.prefab],
        data.impact,
        data.inventoryItemHeight,
        data.inventoryItemWidth,
        data.damage,
        data.shakeFactor,
        data.type,
        data.rarity,
        Global.Resources[data.audio],
        Global.Resources[data.sprite],
        data.damageModifiers,
       //Interaction.BuffTransferDataToDictionary(data.buffsThatTransfer),
        OnHitWhenDataToOnHitWhen(data.onHit),
        SocketSlotDataToSocketTypeArray(data.sockets),
        Global.Resources[data.buffs],
        null,
        PointOfInterestDataToDictionary(data.points),
        EffectWhenDataToDictionary(data.effectsWhen),
        orig
        )
    { }
    public Weapon(
       //Itemname
       string itemn,
       string description,
       //The item prefab
       Transform item,
       //Rarity
       float impactVal,
       float itemHeight,
       float itemWidth,
       Damage[] damage,
       float shakeFactor,
       WeaponType type,
       //Rarity
       Rarity rarity,
       AudioContainer audio,
       Sprite picture,
       DamageModifier[] modifiers,
       //  DictionaryList<Buff, float> transferringBuffsVal,
       OnHitWhen[] onHits = null,
       SocketType[] emptySockets = null,
       //Buffs applied by this item
       List<Buff> givesBuff = null,
       //Alignment (not used for scaling)
       Alignment alig = null,
       //Transform names on child items
       DictionaryList<PointOfInterest, string> pointsOfInterestPreShowingVal = null,
       DictionaryList<EffectWhen, Effect> effectLibrary = null,
       //Origin
       SocketableNames orig = SocketableNames.NothingSocketed

        //Buffs
        ) : base(itemn, item, rarity, description, null, picture, givesBuff, alig, pointsOfInterestPreShowingVal, effectLibrary, audio)
    {
        this.modifiers = modifiers;
        this.onHit = onHits;
        this.shakeFactor = shakeFactor;
       // this.transferringBuffs = transferringBuffsVal;
        this.impact = impactVal;
        this.damage = damage;
        this.inventoryWidth = itemWidth;
        this.inventoryHeight = itemHeight;
        this.type = type;
        origin = orig;
        SetConditions();

    }

    private void SetConditions()
    {
        Condition[] cnn = new Condition[2];

        cnn[0] = Condition.AttackingWithAMeleeWeapon;

        if (type == WeaponType.Sword)
        {
            cnn[1] = Condition.AttackingWithASword;

        }
        else if (type == WeaponType.Spear)
        {
            cnn[1] = Condition.AttackingWithASpear;

        }
        else if (type == WeaponType.Hammer)
        {
            cnn[1] = Condition.AttackingWithAHammer;

        }
        else
        {

            cnn[1] = Condition.AttackingWithAAxe;
        }

        meleeConditions = GetConditions(cnn);
    }

    public Damage[] GetDamage()
    {
        return damage;
    }
    public DamageModifier[] GetDamageModifier()
    {
        return modifiers;
    }
    public OnHitWhen[] GetOnHit()
    {
        return onHit;
    }
    public ListHash<int> GetConditions()
    {
        return meleeConditions;
    }
    /*public void Tick()
    {
        if(currentRechargeTime >= rechargeTime)
        {
            currentRechargeTime = rechargeTime;
            canAttack = true;
        }
        else
        {
            currentRechargeTime += Time.deltaTime;
        }
    }*/
    public override void Show(Transform parent, bool onMech = true)
    {
        base.Show(parent, onMech);

        weaponLength = Mathf.Max(Mathf.Max(boxcollider.size.x * (worldScale.x + 1), boxcollider.size.y * (worldScale.y + 1)), boxcollider.size.z * (worldScale.z + 1));
    }


    public void Attack()
    {
        Animator anim = ((Core)socketedIn).meleeAnimator;

        if (canAttack)
        {
            if (!isAttacking)
            {
                if (type == WeaponType.Sword || type == WeaponType.Axe)
                {
                    anim.SetBool("SwordAttack", true);
                }
                else
                {
                    Debug.Log("Type: " + type + " is not yet implemented");
                }
                isAttacking = true;
            }
        }
        else
        {
            StopAttacking();
        }

        if(canCombo && !comboLastFrame)
        {
            anim.SetBool("Combo", true);
           // Debug.Log("Comboing!");
        }

        comboLastFrame = canCombo;
    }

    public void StopAttacking()
    {
        Animator anim = ((Core)socketedIn).meleeAnimator;

        if (isAttacking)
        {
            if (type == WeaponType.Sword || type == WeaponType.Axe)
            {
                anim.SetBool("SwordAttack", false);
            }
            else
            {
                Debug.Log("Type: " + type + " is not yet implemented");
            }
            isAttacking = false;
            if (canCombo)
            {
                canCombo = false;
                anim.SetBool("Combo", false);
              //  Debug.Log("stopping combo!");
            }
        }
    }

    public void Parry(Animator anim)
    {
        if (canAttack)
        {
            anim.SetTrigger("Parry");
            isVulnerableToExecutionAttack = true;
            //anim.SetBool("CanAttack", false);
            //canAttack = false;
        }
    }
    //GameObject display;

    public override void Execute(GameUnit target)
    {
        base.Execute(target);

        Attack();
    }

    public void FirstAttackFrame()
    {
        attackFrameBeforeDamageDealt = true;
    }

    public void SetSwingTime(float time)
    {
        currentTimeUntilNextSwing = 0;
        expectedTimeUntilNextSwing = time;
        //Debug.Log(time);
    }

    public override void Tick(float actionspeed, Vector3 mousePosition, GameUnit target, Vector3 mousePositionOnZPlane)
    {
        base.Tick(actionspeed, mousePosition, target, mousePositionOnZPlane);

        currentTimeUntilNextSwing += Time.deltaTime * actionspeed;

        if(currentTimeUntilNextSwing > expectedTimeUntilNextSwing)
        {
            currentTimeUntilNextSwing = expectedTimeUntilNextSwing;
        }
    }

    public override bool HasReload()
    {
        return true;
    }

    public override float GetTimer(float actionspeed)
    {
        if(expectedTimeUntilNextSwing == 0 || IsReady()) { return 0; };
        return ((currentTimeUntilNextSwing / expectedTimeUntilNextSwing) * expectedTimeUntilNextSwing ) / actionspeed;
    }

    public override bool IsReady()
    {
        return socketedIn.IsReady();
    }

    public override int GetReloadGroup()
    {
        return InventoryHandler.ATTACK_RELOAD_GROUP;
    }

    public override float GetTimeUntilReloadFinished(float actionspeed)
    {

        if(expectedTimeUntilNextSwing == 0 || IsReady()) { return 1; }
        return (currentTimeUntilNextSwing/ expectedTimeUntilNextSwing)/actionspeed;
    }

    public void BeginAttackPhase()
    {
        if (canAttack)
        {
             HitWithWeapon();

            if (unitsHitThisPhase.Count == 0)
            {
                sounds.PlaySound(SoundWhen.Swing, audioSource, false);
            }

            if (effects.Contains(EffectWhen.SwingTrail))
            {
                if (owner.movement.facing == Facing.Right)
                {
                    effects[EffectWhen.SwingTrail].Spawn(visualItem.position + new Vector3(weaponLength / 2, 0));
                    effects[EffectWhen.SwingMarker].Spawn(new Vector3(visualItem.position.x, owner.body.position.y - owner.body.localScale.y / 2 + 1, visualItem.position.z) + new Vector3(weaponLength / 2, 0), null, 0, 0, 0, 0.8f);
                    effects[EffectWhen.SwingMarker].Spawn(new Vector3(visualItem.position.x, owner.body.position.y + owner.body.localScale.y / 2, visualItem.position.z) + new Vector3(weaponLength / 2, 0), null, 0, 0, 0, 0.8f);
                }
                else if (owner.movement.facing == Facing.Left)
                {
                    effects[EffectWhen.SwingTrail].Spawn(visualItem.position - new Vector3(weaponLength / 2, 0), null, 0, 180);
                    effects[EffectWhen.SwingMarker].Spawn(new Vector3(visualItem.position.x, owner.body.position.y - owner.body.localScale.y / 2 + 1, visualItem.position.z) - new Vector3(weaponLength / 2, 0), null, 0, 180, 0, 0.8f);
                    effects[EffectWhen.SwingMarker].Spawn(new Vector3(visualItem.position.x, owner.body.position.y + owner.body.localScale.y / 2, visualItem.position.z) - new Vector3(weaponLength / 2, 0), null, 0, 180, 0, 0.8f);
                }

            }
            // Debug.Log("Beginning attack phase");

            unitsHitThisPhase.Clear();
            rigidBodiesHitThisPhase.Clear();

 
        }
        else
        {

        }

        attackFrameBeforeDamageDealt = false;
    }
    public void EndAttackPhase()
    {
        canCombo = true;
        StopAttacking();
        //Debug.Log("Ending attack phase");


        /*if (DevelopmentSettings.SHOW_MARKERS)
        {
            Global.Destroy(display);
        }*/

        //boxcollider.enabled = false;
        //boxcollider.isTrigger = false;
    }

    public void HitWithWeapon()
    {
        Vector3 pos = new Vector3(visualItem.transform.position.x, owner.body.position.y, visualItem.transform.position.z);
        Vector3 scale = new Vector3(weaponLength, owner.body.localScale.y, weaponLength);
        float zDiff = owner.movement.facing == Facing.Right || owner.movement.facing == Facing.TurningRight ? 1: -1;
        bool mechWasHit = false;
        /*if (DevelopmentSettings.SHOW_MARKERS)
        {
            display = GameObject.CreatePrimitive(PrimitiveType.Cube);
            display.transform.localScale = scale;
            display.transform.position = pos;
            Global.Destroy(display.GetComponent<BoxCollider>());
        }*/

        Collider[] hitColliders = Physics.OverlapBox(pos, scale / 2);
        if (hitColliders != null)
        {
            foreach (Collider col in hitColliders)
            {
                GameUnitBodyComponent gbc = col.gameObject.GetComponent<GameUnitBodyComponent>();

                if (gbc != null && gbc.owner != owner &&  gbc.owner.isActive && owner.belongsToFaction.IsHostileTo(gbc.owner.belongsToFaction))
                {
                    unitsHitThisPhase.AddIfNotContains(gbc.owner);
                }

                Rigidbody rb = col.gameObject.GetComponent<Rigidbody>();

                if (rb != null && !rb.isKinematic)
                {
                    rigidBodiesHitThisPhase.AddIfNotContains(rb);
                }
            }
        }

        foreach (GameUnit gu in unitsHitThisPhase)
        {
            Vector3 impactPos = new Vector3(0, 0, 0);

            if (gu.mech != null)
            {
                mechWasHit = true;
                bool firstCore = true;

                //gu.mech.InitShake();

                foreach (Core c in gu.mech.equippedCores)
                {
                    if(Vector3.Distance(c.visualItem.position,visualItem.position) < weaponLength)
                    {
                        if (firstCore)
                        {
                            //Shield hit effects
                            if (effects.Contains(EffectWhen.ShieldHit) && gu.stats.GetValuePercentage(Stat.Shield) > 0)
                            {
                                effects[EffectWhen.ShieldHit].Spawn(c.visualItem.position, null, 0, 180);
                                sounds.PlaySound(SoundWhen.HitEffect, audioSource, false);
                                // Health hit effects
                            }
                            else if (effects.Contains(EffectWhen.HealthHit) && gu.stats.GetValuePercentage(Stat.Shield) == 0)
                            {
                                effects[EffectWhen.HealthHit].Spawn(c.visualItem.position, null, 0, 180);
                                sounds.PlaySound(SoundWhen.HitEffect, audioSource, false);
                            }
                        }

                        //Detach visuals
                        if (firstCore || Global.instance.rand.NextDouble() < SHOW_DETACH_VISUAL_ON_HIT_CHANCE)
                        {
                            c.ShowDetachVisuals();
                            firstCore = false;
                        }

                        //Extra hit visuals
                        if (c.effects.Contains(EffectWhen.HealthHit) && c.meleeAnimator != null)
                        {
                            c.effects[EffectWhen.HealthHit].Spawn(c.meleeAnimator.transform.position, null, 0, 0, 0, 3);
                        }

                        //Interrupt
                        //c.Interrupt();

                    }
                }

                
                sounds.PlaySound(SoundWhen.HullHit, audioSource, false);
                sounds.PlaySound(SoundWhen.Vibration, audioSource, false);

                impactPos = owner.GetCenterPos().x < gu.GetCenterPos().x ?
                    new Vector3(gu.body.position.x - gu.body.localScale.x / 2, visualItem.position.y, visualItem.position.z + weaponLength * zDiff) :
                    new Vector3(gu.body.position.x + gu.body.localScale.x / 2, visualItem.position.y, visualItem.position.z + weaponLength * zDiff);
            }
            else
            {
                sounds.PlaySound(SoundWhen.FleshHit,audioSource,false);
                impactPos = gu.GetCenterPos();
            }
            if (gu.stats.GetValuePercentage(Stat.Shield) > 0)
            {
                sounds.PlaySound(SoundWhen.ShieldHit, owner.mech.source, false);
            }

            Senses.Hit hit = owner.senses.TryToHit(visualItem.position, gu, RAYCASTS_WHEN_ATTACKING, true);
            Interaction.Hit(this,owner, gu, meleeConditions, visualItem, 
                impactPos, hit.didHit ? hit.pos : impactPos, 1,false, damage, 
                impact, weaponLength * WEAPON_PUSH_RANGE_MULTIPLIER, onHit, 
                modifiers, null,shakeFactor);// transferringBuffs);

        }

        if(unitsHitThisPhase.Count == 0)
        {
            foreach (Rigidbody rb in rigidBodiesHitThisPhase)
            {
                Interaction.TransferImpact(rb, 1, rb.transform.position + new Vector3(0, 0, zDiff * weaponLength), impact, weaponLength * WEAPON_PUSH_RANGE_MULTIPLIER);
            }
        }
    }

    /*public void HitUnit(GameUnit gu, Collider col)
    {
        if (!unitsHitThisPhase.Contains(gu))
        {
            Debug.Log("Game unit slashed: " + gu.uniqueName);
            unitsHitThisPhase.Add(gu);
        }
    }*/

    /*public override void Show(Transform parent)
    {
        base.Show(parent);

       // visualItem.gameObject.AddComponent<FightingAttackHitReceiver>().weapon = this;
        //visualItem.gameObject.layer = LayerMask.NameToLayer(Global.LAYER_PROJECTILES);

        //boxcollider.enabled = true;
        //boxcollider.isTrigger = true;

        //Global.Destroy(visualItem.gameObject.GetComponent<Rigidbody>());
    }*/

    public SocketableNames GetName()
    {
        return origin;
    }

    public Socketable CloneSocketable()
    {
        return Clone();
    }

    public SocketType GetSocketType()
    {
        return SocketType.Melee;
    }
    public Sprite GetSprite()
    {
        return inventorySprite;
    }
    public override Item CloneBaseClass()
    {
        return (Item)Clone();
    }
    public new Weapon Clone()
    {
        return new Weapon(
            itemName,
            description,
            prefab,
            impact,
            inventoryHeight,
            inventoryWidth,
            damage,
            shakeFactor,
            type,
            rarity, 
            sounds.Clone(), 
            inventorySprite,
            modifiers,
            onHit,
           // transferringBuffs,
            socketTypes,
            buffs, 
            alignment, 
            pointsOfInterestPreShowing, 
            effects, 
            origin);
    }
}

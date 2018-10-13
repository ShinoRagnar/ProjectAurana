using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System;
using System.Text;

public class Tooltip {

    public static readonly int NUMBER_OF_TWO_PART_ROWS = 30;
    public static readonly int NUMBER_OF_BUFFS = 10;
    public static readonly int NUMBER_OF_MODIFIERS = 10;
    public static readonly int NUMBER_OF_ONHITS = 10;
    //public static string TTStr = "0.0";

    private struct Margin
    {
        Item topMargin;
        Item dividor;
        Item bottomMargin;

        public Margin(Item top, Item divid, Item bot)
        {
            this.topMargin = top;
            this.dividor = divid;
            this.bottomMargin = bot;
        }
        public void ShowHide(bool show)
        {
            if (show)
            {
                topMargin.Enable();
                dividor.Enable();
                bottomMargin.Enable();
            }
            else
            {
                topMargin.Disable();
                dividor.Disable();
                bottomMargin.Disable();
            }
        }
    }

    public DictionaryList<Transform, TextMeshProUGUI> textmesh = new DictionaryList<Transform, TextMeshProUGUI>();
    public DictionaryList<Transform, Image> image = new DictionaryList<Transform, Image>();


    public Item tooltip;
    Item itemName;
    Item damageTitles;
    Item fluff;
    Item topMargin;
    Item onHitExplanation;
    Image pic;
    TextMeshProUGUI nameText;

    public bool IsShowing
    {
        get { return tooltip.isEnabled; }
    }

    GameUnit owner;

    List<Item> marginsUnderName = new List<Item>();
    //List<Item> dividor = new List<Item>();
    List<Item> damage = new List<Item>();
    List<Item> twoPart = new List<Item>();
    List<Item> modifiers = new List<Item>();
    List<Item> onHit = new List<Item>();
    List<Item> onHitDamage = new List<Item>();
    List<Item> buffs = new List<Item>();

    List<Item> all = new List<Item>();

    Margin underDamageTitles;
    Margin underDamageBeforeOnHit;
    Margin underDamages;
    Margin underTwoPartFields;
    Margin underModifiers;
    Margin underBuffs;
    Margin underOnHits;

    bool addedDamageTitles;
    bool addedDamageBeforeOnHit;
    bool addedDamages;
    bool addedTwoPartFields;
    bool addedModifiers;
    bool addedBuffs;
    bool addedOnHits;

    VerticalLayoutGroup[] verticalLayoutGroups;
    HorizontalLayoutGroup[] horizontalLayoutGroups;
    ContentSizeFitter[] contentSizeFitters;



    public Tooltip(GameUnit player)
    {
        this.owner = player;

        tooltip = GetShowDisable(ItemNames.Tooltip, Global.References[SceneReferenceNames.PanelOnMouseItem]);
        Transform t = tooltip.GetPointOfInterest(PointOfInterest.Add);

        //Item name
        topMargin = GetShowDisable(ItemNames.TooltipMargin, t);
        itemName = GetShowDisable(ItemNames.TooltipItemNameBorder, t);
        nameText = itemName.GetPointOfInterest(PointOfInterest.Title).GetComponent<TextMeshProUGUI>();
        marginsUnderName.Add(GetShowDisable(ItemNames.TooltipMargin, t));
        marginsUnderName.Add(GetShowDisable(ItemNames.TooltipMargin, t));

        //Damage
        damageTitles = GetShowDisable(ItemNames.TooltipDamageTitles, t);
        underDamageTitles = AddMargin(t);

        //DPS and damage
        damage.Add(GetShowDisable(ItemNames.TooltipDamageTitles, t));
        damage.Add(GetShowDisable(ItemNames.TooltipDamageTitles, t));
        underDamageBeforeOnHit = AddMargin(t);

        //Onhit DPS and damage
        damage.Add(GetShowDisable(ItemNames.TooltipDamageTitles, t));
        damage.Add(GetShowDisable(ItemNames.TooltipDamageTitles, t));
        underDamages = AddMargin(t);

        //Descriptors
        GetShowDisable(ItemNames.TooltipTwoPartRow, t,NUMBER_OF_TWO_PART_ROWS,twoPart);
        underTwoPartFields = AddMargin(t);

        //Modifiers
        GetShowDisable(ItemNames.TooltipModifier, t, NUMBER_OF_MODIFIERS, modifiers);
        underModifiers = AddMargin(t);

        //Buffs
        GetShowDisable(ItemNames.TooltipBuff, t, NUMBER_OF_BUFFS, buffs);
        underBuffs = AddMargin(t);

        //OnHit
        onHitExplanation = GetShowDisable(ItemNames.TooltipModifier, t);
        GetShowDisable(ItemNames.TooltipOnHit, t, NUMBER_OF_BUFFS, onHit);

        //OnHitDamage
        GetShowDisable(ItemNames.TooltipModifier, t, NUMBER_OF_BUFFS, onHitDamage);
        underOnHits = AddMargin(t);

        //Fluff
        fluff = GetShowDisable(ItemNames.TooltipFluff, t);

        pic = tooltip.GetPointOfInterest(PointOfInterest.Sprite).GetComponent<Image>();


        verticalLayoutGroups = tooltip.visualItem.GetComponentsInChildren<VerticalLayoutGroup>();
        horizontalLayoutGroups = tooltip.visualItem.GetComponentsInChildren<HorizontalLayoutGroup>();
        contentSizeFitters = tooltip.visualItem.GetComponentsInChildren<ContentSizeFitter>();
    }

    private Margin AddMargin(Transform t)
    {
       return new Margin(
            GetShowDisable(ItemNames.TooltipMargin, t),
            GetShowDisable(ItemNames.TooltipDividor, t),
            GetShowDisable(ItemNames.TooltipMargin, t)

            );
    }

    private void GetShowDisable(ItemNames name, Transform pos, int amount, List<Item> ret)
    {
        for(int i = 0; i < amount; i++)
        {
            ret.Add(GetShowDisable(name, pos));
        }
    }

    private Item GetShowDisable(ItemNames name, Transform pos)
    {
        Item ret = Global.Resources[name]; //Global.instance.TOOLTIP.Clone(); //
        owner.itemEquiper.Equip(ret);
        ret.Show(pos); //Global.instance.PANEL_ON_MOUSE);
        ret.Disable();
        all.Add(ret);
        return ret;
    }

    public void HideAllInnerElements()
    {
        addedDamageTitles = false;
        addedDamageBeforeOnHit = false;
        addedDamages = false;
        addedTwoPartFields = false;
        addedModifiers = false;
        addedBuffs = false;
        addedOnHits = false;

        foreach (Item i in all)
        {
            i.Disable();
        }
    }

    public void Hide()
    {
        tooltip.Disable();
    }


    public void Show(MechItem mi)
    {
        HideAllInnerElements();

        int twoParts = 1;

        //margins[0].Enable();
        itemName.Enable();
        nameText.text = AddSpacesToSentence(mi.itemName);

        //Make space under name
        foreach(Item i in marginsUnderName)
        {
            i.Enable();
        }

        if(mi is Damager)
        {
            damageTitles.Enable();
            Damager damager = (Damager)mi;

            addedDamageTitles = true;

            SetText(damage[0], PointOfInterest.Name, "Damage");

            Damage[] dmm = damager.GetDamage();

            if(dmm != null)
            {
                float physicalDamage = 0;
                float EMPDamage = 0;
                float corruptionDamage = 0;

                foreach (Damage dam in dmm)
                {
                    if (dam.type == DamageType.Physical)
                    {
                        physicalDamage += dam.GetDamage(owner, damager.GetConditions());
                    }
                    else if (dam.type == DamageType.EMP)
                    {
                        EMPDamage += dam.GetDamage(owner, damager.GetConditions());
                    }
                    else if (dam.type == DamageType.Corruption)
                    {
                        corruptionDamage += dam.GetDamage(owner, damager.GetConditions());
                    }
                }
                SetText(damage[0], PointOfInterest.Physical, TTStr(physicalDamage));
                SetText(damage[0], PointOfInterest.EMP, TTStr(EMPDamage));
                SetText(damage[0], PointOfInterest.Corruption, TTStr(corruptionDamage));

                if (mi is Gun)
                {
                    Gun g = (Gun)mi;
                    SetText(damage[1], PointOfInterest.Name, "DPS");
                    SetText(damage[1], PointOfInterest.Physical, TTStr(physicalDamage / g.reloadTime));
                    SetText(damage[1], PointOfInterest.EMP, TTStr(EMPDamage / g.reloadTime));
                    SetText(damage[1], PointOfInterest.Corruption, TTStr(corruptionDamage / g.reloadTime));

                }

                addedDamages = true;

                if (mi is Gun)
                {

                    Gun g = (Gun)mi;

                    SetText(twoPart[1], PointOfInterest.Name, "Reload Time");
                    SetText(twoPart[1], PointOfInterest.Value, TTStr(g.reloadTime));

                    SetText(twoPart[2], PointOfInterest.Name, "Radius");
                    SetText(twoPart[2], PointOfInterest.Value, TTStr(g.bullet.explosionRadius));

                    SetText(twoPart[3], PointOfInterest.Name, "Type");
                    SetText(twoPart[3], PointOfInterest.Value, AddSpacesToSentence(g.bullet.type.ToString()));

                    SetText(twoPart[4], PointOfInterest.Name, "Aim");

                    if (g.bullet.target == BulletTarget.GroundUnderTarget)
                    {
                        SetText(twoPart[4], PointOfInterest.Value, "Ground");
                    }
                    else if (g.bullet.target == BulletTarget.GroundInFrontOfTarget)
                    {
                        SetText(twoPart[4], PointOfInterest.Value, "Front of target");
                    }
                    else
                    {
                        SetText(twoPart[4], PointOfInterest.Value, "Target");
                    }

                    twoParts = 5;

                }
                else if(mi is Weapon)
                {
                    Weapon w = (Weapon)mi;

                    SetText(twoPart[1], PointOfInterest.Name, "Type");
                    SetText(twoPart[1], PointOfInterest.Value, "Weapon - "+AddSpacesToSentence(w.type.ToString()));

                    twoParts = 2;
                    //Hangle weapon strings
                }

                DamageModifier[] dm = damager.GetDamageModifier();

                if(dm != null)
                {
                    

                    int i = 0;

                    foreach (DamageModifier d in dm)
                    {
                        //float mod = (d.damageModifier-1) * 100;

                        SetText(modifiers[i], PointOfInterest.Value,
                           DamageModifierToText(d) +
                           InteractionConditionToText(d.conditions));
                        addedModifiers = true;
                        i++;
                    }

                }
            }


        }
        else if(mi is Core)
        {
            Core c = (Core)mi;

            string type = "";
            if(c.pointsOfInterestPreShowing.Contains(PointOfInterest.AttackAnimator)) { type = " - Avatar"; }

            SetText(twoPart[1], PointOfInterest.Name, "Type");
            SetText(twoPart[1], PointOfInterest.Value, "Core"+type);

            twoParts = 2;

            if(c.armor > 0)
            {
                SetText(twoPart[twoParts], PointOfInterest.Name, "Armor");
                SetText(twoPart[twoParts], PointOfInterest.Value, c.armor.ToString());
                twoParts++;
            }

            if (c.GetInventoryTypeAmount(InventoryBlockType.Blocked) > 0)
            {
                SetText(twoPart[twoParts], PointOfInterest.Name, "Blocked Sq.");
                SetText(twoPart[twoParts], PointOfInterest.Value, c.GetInventoryTypeAmount(InventoryBlockType.Blocked).ToString());
                twoParts++;
            }

            if (c.GetInventoryTypeAmount(InventoryBlockType.Connected) > 0)
            {
                SetText(twoPart[twoParts], PointOfInterest.Name, "Connected Sq.");
                SetText(twoPart[twoParts], PointOfInterest.Value, c.GetInventoryTypeAmount(InventoryBlockType.Connected).ToString());
                twoParts++;
            }

            if (c.GetInventoryTypeAmount(InventoryBlockType.Occupied) > 0)
            {
                SetText(twoPart[twoParts], PointOfInterest.Name, "Occupied Sq.");
                SetText(twoPart[twoParts], PointOfInterest.Value, c.GetInventoryTypeAmount(InventoryBlockType.Occupied).ToString());
                twoParts++;
            }

        }else if(mi is Shield)
        {
            Shield s = (Shield)mi;

            SetText(twoPart[1], PointOfInterest.Name, "Type");
            SetText(twoPart[1], PointOfInterest.Value, "Shield");

            SetText(twoPart[2], PointOfInterest.Name, "Block Radius");
            SetText(twoPart[2], PointOfInterest.Value, TTStr(s.blockSize));

            SetText(twoPart[3], PointOfInterest.Name, "Can Block");
            SetText(twoPart[3], PointOfInterest.Value, "Attacks, Projectiles");

            twoParts = 4;
        }

        addedTwoPartFields = true;

        SetText(twoPart[0], PointOfInterest.Name, "Rarity");
        SetText(twoPart[0], PointOfInterest.Value, AddSpacesToSentence(mi.rarity.ToString()));

        //Sockets
        foreach(SocketType ss in new SocketType[] { SocketType.Crystal, SocketType.Gun, SocketType.Melee, SocketType.Rifle, SocketType.Shield })
        {
            if(mi.GetSocketAmountOfType(ss) > 0)
            {
                SetText(twoPart[twoParts], PointOfInterest.Name, AddSpacesToSentence(ss.ToString()) + " Sockets");
                SetText(twoPart[twoParts], PointOfInterest.Value, mi.GetSocketAmountOfType(ss).ToString());
                twoParts++;
            }
        }


        if (mi.buffs != null && mi.buffs.Count > 0)
        {
            int i = 0;

            foreach (Buff b in mi.buffs)
            {
                SetImage(buffs[i], PointOfInterest.Sprite, b.sprite);
                SetText(buffs[i], PointOfInterest.Title, AddSpacesToSentence(b.buffName));
                SetText(buffs[i], PointOfInterest.Value, AffectorsToText(b.affectors));

                addedBuffs = true;
                i++;
            }
        }

        if(mi is Damager)
        {
            Damager damager = (Damager)mi;
            OnHitWhen[] onHits = damager.GetOnHit();
            if(onHits != null && onHit.Count > 0)
            {
                int o = 0;
                foreach (OnHitWhen oh in onHits)
                {
                    addedOnHits = true;

                    if (oh.onHit.transferringBuffs != null && oh.onHit.transferringBuffs.Count > 0)
                    {
                        SetText(onHitExplanation, PointOfInterest.Value, "Applies the following On Hit:");

                        foreach (Buff b in oh.onHit.transferringBuffs)
                        {
                            SetImage(onHit[o], PointOfInterest.Sprite, b.sprite);
                            SetText(onHit[o], PointOfInterest.Name,
                                AddSpacesToSentence(b.buffName) + ": " +
                                InteractionConditionToText(oh.conditions));
                            SetText(onHit[o], PointOfInterest.Value,
                                "*" + AddSpacesToSentence(b.buffName) + ": " + AffectorsToText(b.affectors)+ " for "+TTStr(b.duration)+" seconds.");
                            o++;
                        }
                    }
                    if(oh.onHit.delayedDamages != null && oh.onHit.delayedDamages.Length > 0)
                    {
                        int i = 0;
                        SetText(onHitDamage[i], PointOfInterest.Value, "On Hit: "+oh.onHit.description);
                        i++;

                        float corruption = 0;
                        float phys = 0;
                        float emp = 0;

                        foreach (OnHitDelayedDamage ohdd in oh.onHit.delayedDamages)
                        {
                            string damagemods = DamageModifiersToText(ohdd.modifiers);
                            SetText(onHitDamage[i], PointOfInterest.Value,
                                DamageToString(ohdd.damage) + " after " + TTStr(ohdd.delay)
                                + " Seconds, in a " + TTStr(ohdd.radius) + " radius"+
                                (damagemods.Length > 1 ? " with "+damagemods : ".")
                                );

                            corruption += GetDamage(ohdd.damage,DamageType.Corruption);
                            phys += GetDamage(ohdd.damage, DamageType.Physical);
                            emp += GetDamage(ohdd.damage, DamageType.EMP);

                            i++;
                        }

                        if(corruption+phys+emp > 0)
                        {
                            addedDamageBeforeOnHit = true;

                            SetText(damage[2], PointOfInterest.Name, "On Hit");
                            SetText(damage[2], PointOfInterest.Physical, TTStr(phys));
                            SetText(damage[2], PointOfInterest.EMP, TTStr(emp));
                            SetText(damage[2], PointOfInterest.Corruption, TTStr(corruption));

                            if(mi is Gun)
                            {
                                Gun g = (Gun)mi;
                                SetText(damage[3], PointOfInterest.Name, "OH DPS");
                                SetText(damage[3], PointOfInterest.Physical, TTStr(phys / g.reloadTime));
                                SetText(damage[3], PointOfInterest.EMP, TTStr(emp / g.reloadTime));
                                SetText(damage[3], PointOfInterest.Corruption, TTStr(corruption / g.reloadTime));
                            }
                        }
                    }
                }
            }
        }

        //Rarity
        if(mi.rarity == Rarity.Legendary)
        {
            tooltip.GetPointOfInterest(PointOfInterest.Legendary).gameObject.SetActive(true);
        }
        else
        {
            tooltip.GetPointOfInterest(PointOfInterest.Legendary).gameObject.SetActive(false);
        }
        if (mi.rarity == Rarity.Rare)
        {
            tooltip.GetPointOfInterest(PointOfInterest.Rare).gameObject.SetActive(true);
        }
        else
        {
            tooltip.GetPointOfInterest(PointOfInterest.Rare).gameObject.SetActive(false);
        }
        if(mi.rarity == Rarity.Uncommon)
        {
            tooltip.GetPointOfInterest(PointOfInterest.Uncommon).gameObject.SetActive(true);
        }
        else
        {
            tooltip.GetPointOfInterest(PointOfInterest.Uncommon).gameObject.SetActive(false);
        }
        if (mi.rarity == Rarity.Common)
        {
            tooltip.GetPointOfInterest(PointOfInterest.Common).gameObject.SetActive(true);
        }
        else
        {
            tooltip.GetPointOfInterest(PointOfInterest.Common).gameObject.SetActive(false);
        }

        //Sprite
        pic.sprite = mi.inventorySprite;

        ShowMargins();

       SetText(fluff, PointOfInterest.Value, mi.description);

       tooltip.Enable();



        //Unity bug-fix
        Canvas.ForceUpdateCanvases();

        if (verticalLayoutGroups != null)
        {
            foreach(VerticalLayoutGroup v in verticalLayoutGroups)
            {
                v.enabled = false;
                v.enabled = true;
            }
        }

        if (horizontalLayoutGroups != null)
        {
            foreach (HorizontalLayoutGroup v in horizontalLayoutGroups)
            {
                v.enabled = false;
                v.enabled = true;
            }
        }

        if (contentSizeFitters != null)
        {
            foreach (ContentSizeFitter v in contentSizeFitters)
            {
                v.enabled = false;
                v.enabled = true;
            }
        }
    }

    public float GetDamage(Damage[] dam, DamageType damageType)
    {
        float ret = 0;

        if(dam == null)
        {
            return 0;
        }
        foreach(Damage d in dam)
        {
            if(d.type == damageType)
            {
                ret += d.amount;
            }
        }
        return ret;
    }

    public void ShowMargins()
    {
        underDamageTitles.ShowHide(addedDamageTitles);
        underDamageBeforeOnHit.ShowHide(addedDamageBeforeOnHit);
        underDamages.ShowHide(addedDamages);
        underTwoPartFields.ShowHide(addedTwoPartFields);
        underModifiers.ShowHide(addedModifiers);
        underBuffs.ShowHide(addedBuffs);
        underOnHits.ShowHide(addedOnHits);
    }

    public string DamageModifiersToText(DamageModifier[] d)
    {
        if(d == null) { return ""; }
        string ret = "";

        foreach(DamageModifier dm in d)
        {
            ret += DamageModifierToText(dm) + InteractionConditionToText(dm.conditions)+ ".";
        }
        return ret;
    }

    public string DamageModifierToText(DamageModifier d)
    {
        float mod = (d.damageModifier - 1) * 100;
        return (mod > 0 ? "+" : "-") + TTStr(Math.Abs(mod)) + "% damage ";
    }

    public string DamageToString(Damage[] damages)
    {
        if(damages == null) { return "no damage"; }

        float phys = 0;
        float emp = 0;
        float corr = 0;

        foreach(Damage d in damages)
        {
            if(d.type == DamageType.EMP)
            {
                emp += d.amount;
            }
            if (d.type == DamageType.Corruption)
            {
                corr += d.amount;
            }
            else
            {
                phys += d.amount;
            }
        }

        if(phys+emp+corr == 0) { return "no damage";  }

        return (phys > 0 ? TTStr(phys) + " (Physical), " : "") +
            (emp > 0 ? TTStr(emp) + " (EMP), " : "") +
            (corr > 0 ? TTStr(corr) + " (Corruption), " : "") + " damage";
    }

    public string AffectorsToText(ListDictionary<string,StatsAffector> affectors)
    {
        if(affectors == null ) { return "Does nothing by itself"; }
        if(affectors.Count == 0) { return "Does nothing by itself"; }

            string ret = "";

        foreach(StatsAffector sa in affectors)
        {
            ret += AddSpacesToSentence(sa.affecting.ToString()) + " is "
                    + (
                        sa.calculation == Calculation.UniqueModifier ? "set to " :
                            (   
                            sa.calculation == Calculation.Additive ?
                                (sa.magnitude < 0 ? "decreased by " : "increased by ")
                                : (sa.magnitude > 1 ? "increased by " : "deacreased by ")
                            )
                    ) + 
                    (sa.calculation == Calculation.Additive ? 
                        TTStr(sa.magnitude * 100)
                        : TTStr((sa.magnitude * 100)-100)
                        )
                    + "%"+ (sa.calculation == Calculation.Multiplicative ? " (multiplicative)" : "")+
                    //(sa.calculation == Calculation.Additive ? " " : "") +
                    ((sa.cap == 0 || sa.calculation == Calculation.UniqueModifier) ? "" : (" to a maximum of: " + TTStr(sa.cap * 100) +"%"))+
                    " and ";
        }
        return ret.Substring(0, Mathf.Max(ret.Length - " and ".Length,0)) + ".";
    }

    public string InteractionConditionToText(InteractionCondition[] conditions)
    {
        if(conditions == null) { return "always."; }

        string ret = "when ";
        foreach(InteractionCondition ic in conditions)
        {
            if (ic.selector == Selector.IsGrounded)
            {
                ret += ic.target == InteractionComparatorTarget.Self ? "you are on the ground " : "the enemy is on the ground ";

            } else if (ic.selector == Selector.NoCondition)
            {
                ret += " always";
            }
            else if (ic.selector == Selector.DealtDamage)
            {
                ret += " you deal "+TTStr(ic.threshold)+(ic.calculation == InteractionCalculation.Percent ? "% of enemy max health":"damage to the enemy");
            }
            else if (ic.selector == Selector.ToTheLeftOf)
            {
                ret += " you are to the left of the target";
            }
            else if (ic.selector == Selector.ToTheRightOf)
            {
                ret += " you are to the right of the target";
            }
            else if (ic.selector == Selector.HasBuff || ic.selector == Selector.DoesNotHaveBuff)
            {
                ret += (ic.target == InteractionComparatorTarget.Self ? " you " +
                            (ic.selector == Selector.HasBuff ? "have " : "do not have ") : " the enemy " +
                            (ic.selector == Selector.HasBuff ? "has " : "does not have ")
                            ) + AddSpacesToSentence(ic.buff.ToString());

            }
            else if (ic.selector == Selector.Stat)
            {
                string rr = " is ";
                if(ic.comparator == InteractionComparator.Above)
                {
                    rr = " is more than ";
                }
                if (ic.comparator == InteractionComparator.Below)
                {
                    rr = " is less than ";
                }
                ret += (ic.target == InteractionComparatorTarget.Self ? " your " : " the enemy's "
                            ) + AddSpacesToSentence(ic.stat.ToString())
                            + rr + TTStr(ic.threshold) + (ic.calculation == InteractionCalculation.Percent ? "%" : "");

            }
            else if (ic.selector == Selector.HitGround)
            {
                ret+=" the projectile hit the ground";
            }
            ret += " and ";
        }
        return ret.Substring(0,ret.Length- " and ".Length)+".";
    }

    string AddSpacesToSentence(string text, bool preserveAcronyms = true)
    {
        if (string.IsNullOrEmpty(text))
            return string.Empty;

        System.Text.StringBuilder newText = new StringBuilder(text.Length * 2);
        newText.Append(text[0]);
        for (int i = 1; i < text.Length; i++)
        {
            if (char.IsUpper(text[i]))
                if ((text[i - 1] != ' ' && !char.IsUpper(text[i - 1])) ||
                    (preserveAcronyms && char.IsUpper(text[i - 1]) &&
                     i < text.Length - 1 && !char.IsUpper(text[i + 1])))
                    newText.Append(' ');
            newText.Append(text[i]);
        }
        return newText.ToString();
    }


    public static string TTStr(float t)
    {
        if(t >= 10) { return ((int)t).ToString(); }
        return (((float)((int)(t * 10))) / 10).ToString();
    }


    public void SetText(Item i, PointOfInterest poi, string text )
    {
        Transform t = i.GetPointOfInterest(poi);
        if (!textmesh.Contains(t))
        {
            textmesh.Add(t, t.GetComponent<TextMeshProUGUI>());
        }
        textmesh[t].text = text;
        t.gameObject.SetActive(true);
        i.Enable();
    }

    public void SetImage(Item i, PointOfInterest poi, Sprite s)
    {
        Transform t = i.GetPointOfInterest(poi);
        if (!image.Contains(t))
        {
            image.Add(t, t.GetComponent<Image>());
        }
        image[t].sprite = s;
        t.gameObject.SetActive(true);
        i.Enable();
    }

    public void HideImage(Item i, PointOfInterest poi)
    {
        Transform t = i.GetPointOfInterest(poi);
        if (!image.Contains(t))
        {
            image.Add(t, t.GetComponent<Image>());
        }
        t.gameObject.SetActive(false);
    }

}

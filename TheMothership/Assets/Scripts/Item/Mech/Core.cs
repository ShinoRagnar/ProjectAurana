using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

/*public enum Direction
{
    None,
    Right,
    Left,
    Up,
    Down
}*/
[Serializable]
public struct InventoryBlockTypeData
{
    public InventoryBlockType[] columns;

    public InventoryBlockTypeData(InventoryBlockType[] col)
    {
        columns = col;
    }
} 

public class Core : MechItem, IGameClone<Core>{

    //DictionaryList<Transform, TurretTargeter> mountedTargeters = new DictionaryList<Transform, TurretTargeter>();
    // DictionaryList<Attachment, Transform> mountedAttachments = new DictionaryList<Attachment, Transform>();

   // Stack<Attachment> clones = new Stack<Attachment>();

    public InventoryBlockType[,] inventorySpace;
    public Vector2 connection;

    public CoreNames origin;

    public Animator meleeAnimator = null;
    public FightingEvents events = null;

    public float actionSpeedLastFrame = 1;
    public Weapon weapon;
    public Shield shield;

    public int armor = 0;


    public Core(CoreData data, CoreNames orig) : this(
            data.itemName,
            data.itemDescription,
            data.armor,
            Global.Resources[data.prefab],
            data.rarity,
            Global.Resources[data.sprite],
            InventoryBlock.RotateMatrixClockwise(BlockDataToInventoryBlockTypeArray(data.rows)),
            data.connectionPos,
            SocketSlotDataToSocketTypeArray(data.sockets),
            Global.Resources[data.buffs],
            null,
            PointOfInterestDataToDictionary(data.points),
            EffectWhenDataToDictionary(data.effectsWhen),
            Global.Resources[data.audio],
            orig
        )
    {
        AddSocketsFromSocketData(data.sockets);
    }
    public Core(
        string corename,
        string description,
        int armor,
        Transform item,
        Rarity rarityVal,
        Sprite picture,
        InventoryBlockType[,] inventorySpaceVal,
        Vector2 connectionPos,

       //Sockets
        SocketType[] emptySockets = null,
        List<Buff> givesBuff = null,

        Alignment align = null,
        DictionaryList<PointOfInterest, string> list = null,

        DictionaryList<EffectWhen, Effect> effectLibrary = null,

        AudioContainer audioContainer = null,

        CoreNames coreName = CoreNames.NoName
        ) : base(corename, item, rarityVal, description, emptySockets, picture, givesBuff, align, list, effectLibrary, audioContainer)
    {
        this.origin = coreName;
        this.inventorySpace = inventorySpaceVal;
        this.connection = connectionPos;
        this.armor = armor;
        CalculateSize();


      //  this.hasMirrorSockets = true;
    }

    public static InventoryBlockType[,] BlockDataToInventoryBlockTypeArray(InventoryBlockTypeData[] ibt)
    {
        int width = ibt[0].columns.Length;
        int height = ibt.Length;
        InventoryBlockType[,] ret = new InventoryBlockType[height, width];

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                ret[y, x] = ibt[y].columns[x];
            }
        }
        return ret;
    }

    public int GetInventoryTypeAmount(InventoryBlockType ibt)
    {
        int ret = 0;

        for (int i = 0; i < inventorySpace.GetLength(0); i++)
        {
            for (int j = 0; j < inventorySpace.GetLength(1); j++)
            {
                if(inventorySpace[i,j] == ibt)
                {
                    ret++;
                }
            }
        }
        return ret;
    }

    public override void Tick(float actionspeed, Vector3 mousePosition, GameUnit target, Vector3 mousePositionOnZPlane)
    {
        base.Tick(actionspeed, mousePosition, target, mousePositionOnZPlane);

        if (actionspeed != actionSpeedLastFrame && meleeAnimator != null)
        {
            meleeAnimator.speed = actionspeed;
            actionSpeedLastFrame = actionspeed;
        }
    }

    /*public override void DetachFromMech(ListHash<Rigidbody> detached)
    {
        base.DetachFromMech(detached);



    }*/

    public override void Show(Transform parent, bool onMech = true)
    {
        base.Show(parent, onMech);

        if (pointsOfInterest != null && pointsOfInterest.Contains(PointOfInterest.AttackAnimator))
        {
            meleeAnimator = GetPointOfInterest(PointOfInterest.AttackAnimator).GetComponent<Animator>();

            if(meleeAnimator != null)
            {
                events = GetPointOfInterest(PointOfInterest.AttackAnimator).GetComponent<FightingEvents>();
                if(events == null)
                {
                    events = GetPointOfInterest(PointOfInterest.AttackAnimator).gameObject.AddComponent<FightingEvents>();
                }
                events.core = this;
            }
        }
    }

    public override void Detach()
    {
        base.Detach();

        if (meleeAnimator != null)
        {
            meleeAnimator.SetBool("Dead",true);
        }
    }


    public void Interrupt()
    {
        if(meleeAnimator != null)
        {
            meleeAnimator.SetTrigger("Interrupt");
        }
    }

    /*public void Tick()
    {
        if(weapon != null)
        {
            weapon.Tick();
        }
        if (shield != null)
        {
            shield.Tick();
        }
    }*/

    /*public void AttackWithWeapon(bool attack)
    {

        if(meleeAnimator != null && weapon != null)
        {
            if (attack)
            {
                weapon.Attack(meleeAnimator);
            }
            else
            {
                weapon.StopAttacking(meleeAnimator);
            }
        }
        else
        {
           // Debug.Log("Missing: "+(meleeAnimator == null) + " - weapon: "+ (weapon == null));
        }
    }*/

    /*public void BlockWithShield(bool blocking)
    {
        if (meleeAnimator != null && shield != null)
        {
            if (blocking)
            {
                shield.Block(meleeAnimator);
            }
            else
            {
                shield.StopBlocking(meleeAnimator);
            }
        }
        else
        {
          //  Debug.Log("Missing: " + (meleeAnimator == null) + " - shield: " + (shield == null));
        }
    }*/

    public override MechItem Unequip(int slot, bool keepVisible = false)
    {
        MechItem mi = base.Unequip(slot, keepVisible);

        if (mi is Weapon)
        {
            weapon = null;
        }
        if (mi is Shield)
        {
            shield = null;
        }
        return mi;
    }

    public override bool SocketItem(int slotNum, Socketable tosocket)
    {
        if(base.SocketItem(slotNum, tosocket)){
            if (tosocket is Weapon)
            {
                weapon = (Weapon)tosocket;
            }
            if (tosocket is Shield)
            {
                shield = (Shield)tosocket;
            }
            return true;
        }
        else
        {
            return false;
        }
    }

    public override bool IsReady()
    {
        return meleeAnimator == null ? false : meleeAnimator.GetCurrentAnimatorStateInfo(0).IsName("Idle");
    }

    /*

    public override float GetTimer()
    {
        return GetTimeUntilCanShootTimer();
    }

    public override float GetTimeUntilReloadFinished()
    {
        if(meleeAnimator != null)
        {
            AnimatorClipInfo[] aci = meleeAnimator.GetCurrentAnimatorClipInfo(0);
            foreach(AnimatorClipInfo a in aci)
            {
                a.clip.
            }

        }
        return GetTimeUntilCanShootPercentage();
    }*7
    /*public void Unmount(Attachment att)
    {
        //Remove all animators for our guns
        foreach (Transform t in mountedTargeters)
        {
            Global.Destroy(mountedTargeters[t].gunAnimator);
        }

        //Hide this attachment
        att.Hide();

        //Hide clones
        while(clones.Count > 0)
        {
            Attachment clone = clones.Pop();
            ie.Unequip(clone);
            clone.Hide();
        }

        //Clear list of targeters
        mountedTargeters.Clear();

    }*/

    //Mounts and shows the attachment, also returns a turret targeter if one was created
    /*public TurretTargeter MountAndShow(Attachment att)
    {
        //Get joint positions
        Transform jointFront = GetPointOfInterest(PointOfInterest.AttachmentSlot1);
        Transform jointBack = GetPointOfInterest(PointOfInterest.AttachmentSlot2);

        //Show front (already equipped)
        att.Show(jointFront);

        //Add targeting
        if (att is Gun)
        {
            //Create a clone for the back slot
            Gun back = ((Gun)att).Clone();
            ie.Equip(back);
            back.Show(jointBack);
            clones.Push(back);

            TurretTargeter targ = new TurretTargeter((Gun)att, jointFront);
            mountedTargeters.Add(jointFront, targ);

            TurretTargeter backTarg = new TurretTargeter(back, jointBack);
            mountedTargeters.Add(jointBack, backTarg);

            return targ;
        }
        else
        {
            Attachment back = att.Clone();
            ie.Equip(back);
            back.Show(jointBack);
            clones.Push(back);

        }
        return null;
        // Debug.Log(g.reactionTime);
    }*/



    private void CalculateSize()
    {
        int width = this.inventorySpace.GetLength(0);
        int height = this.inventorySpace.GetLength(1);
        int maxX = -1;
        int maxY = -1;
        int minX = -1;
        int minY = -1;
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                if (inventorySpace[x, y] == InventoryBlockType.Occupied || inventorySpace[x, y] == InventoryBlockType.Blocked)
                {
                    if (x > maxX || maxX == -1)
                    {
                        maxX = x;
                    }
                    if (y > maxY || maxY == -1)
                    {
                        maxY = y;
                    }
                    if (y < minY || minY == -1)
                    {
                        minY = y;
                    }
                    if(x < minX || minX == -1)
                    {
                        minX = x;
                    }
                }
            }
        }
        //Debug.Log(itemName + " maxX: " + maxX + " maxY:" + maxY + " minX:" + minX + " minY:" + minY);
        this.inventoryHeight = (maxY - minY)+1;
        this.inventoryWidth = (maxX - minX)+1;
        this.equipSlotOffset = connection - new Vector2(minX, maxY);
    }

    public new Core Clone()
    {
        return new Core(itemName, description, armor, prefab,rarity, inventorySprite, inventorySpace, connection,socketTypes, buffs, alignment, pointsOfInterestPreShowing, effects.CloneSimple(),sounds, origin);
    }
    public override Item CloneBaseClass()
    {
        return (Item) Clone();
    }
}

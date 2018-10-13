using System.Collections;
using System.Collections.Generic;
using UnityEngine;


//Points of interest on the prefab
public enum PointOfInterest
{
    FirstBarrel = 1,
    SecondBarrel = 2,

    //Mounts
    AttachmentSlot1 = -8,
    AttachmentSlot2 = -7,
    AttachmentSlot3 = -6,
    AttachmentSlot4 = -5,
    AttachmentSlot5 = -4,
    AttachmentSlot6 = -3,
    AttachmentSlot7 = -2,
    AttachmentSlot8 = -1,

    //Socket slots
    SocketSlot1 = 5,
    SocketSlot2 = 6,
    SocketSlot3 = 7,
    SocketSlot4 = 8,
    SocketSlot5 = 9,
    SocketSlot6 = 10,
    SocketSlot7 = 11,
    SocketSlot8 = 12,

    //Mirror slots
    SocketMirrorSlot1 = 13,
    SocketMirrorSlot2 = 14,
    SocketMirrorSlot3 = 16,
    SocketMirrorSlot4 = 17,
    SocketMirrorSlot5 = 18,
    SocketMirrorSlot6 = 19,
    SocketMirrorSlot7 = 20,
    SocketMirrorSlot8 = 21,

    //Mount point on legs
    LegMountSlot = 38,
    Grab = 39,

    //Inventory items
    Common = 40,
    Uncommon = 41,
    Legendary = 42,
    Rare = 43,

    //Picture
    Sprite = 44,
    Selection = 45,
    Add = 46,
    Remove = 47,
    Blocked = 48,
    Positive = 49,
    Negative = 50,
    Border = 51,
    Origin = 52,
    LeftCorner = 53,
    Background = 54,
    InnerBorder = 55,

    //Tooltip
    Title = 56,

    Corner = 70,

    //Attack
    AttackAnimator = 80,

    Name = 100,
    Physical = 110,
    EMP = 120,
    Corruption = 130,
    Value = 140,

    Glow = 500,
    GlowSmall = 510,

    Progress = 520,
    Dividor = 530,
    Mask = 540,
    Top = 550,
    Bottom = 560,
    Rotate = 570,
    Shield = 580,
    Health = 590,
//Special points

   //Foot points on mechs
    BackRightFoot1 = 99922,
    BackRightFoot2 = 99923,
    BackRightFoot3 = 99924,
    BackLeftFoot1 = 99925,
    BackLeftFoot2 = 99926,
    BackLeftFoot3 = 99927,
    FrontLeftFoot1 = 99928,
    FrontLeftFoot2 = 99929,
    FrontLeftFoot3 = 99930,
    FrontRightFoot1 = 99931,
    FrontRightFoot2 = 99932,
    FrontRightFoot3 = 99933,

    //Exhaust pipes
    ExhaustFront1 = 99934,
    ExhaustFront2 = 99935,
    ExhaustBack1 = 99936,
    ExhaustBack2 = 99937

}


//The item
public class Item : IGameClone<Item>{


    //Counter for all items
    private static readonly string ITEM_NAME_NR_TEXT = " - Nr: ";
    private static Dictionary<string, int> ALL_ITEMS_COUNTER = new Dictionary<string, int>();

    //private static int ITEM_COUNT = 0;

    //Subitems
    public ArrayList subItems = new ArrayList();

    //Points of interest are positions within the item itself, joints etc

        //TODO: Should be protected, private
    public DictionaryList<PointOfInterest, string> pointsOfInterestPreShowing;
    public ListDictionary<PointOfInterest, Transform> pointsOfInterest;

    //The instantiated item
    public Transform visualItem;
    //The original item
    public Transform prefab;
    //Alignement on the joint
    public Alignment alignment;

    //Sounds
    public AudioContainer sounds;
    public AudioSource audioSource;

    //Is instantiated
    public bool showing;

    //Every item has a name
    public string itemName;
    //The unique instance name
    public string uniqueItemName;

    //The equiper of this item, contains owner
    protected Equipper ie;

    //Animation
    public Animator animator;

    //Every item has a number
    //public int itemNumber;

    //Owner
    public GameUnit owner
    {
        get { if (ie == null) { Debug.Log("No item equipper for: " + itemName); } return ie.GetOwner(); }
        set { }
    }
    //Is enabled or nor?
    public bool isEnabled
    {
        get
        {
            if (showing)
            {
                return visualItem.gameObject.activeSelf;
            }
            else { return false; }
        }
    }


    //Never use this
    //public Item() { }
    public Item(ItemData data) : this(
        data.itemName.Length > 0 ? data.itemName : data.prefab.ToString(),
        Global.Resources[data.prefab],
        new Alignment(
            data.position.x,data.position.y,data.position.z
            ,data.rotation.x,data.rotation.y,data.rotation.z
            ,data.scale.x,data.scale.y,data.scale.z),
        PointOfInterestDataToDictionary(data.points)
        ){}

    public Item(
        //Itemname
        string itemn,
        //The item prefab
        Transform item,
        //Alignment when equipped on parent joint
        Alignment alig = null, 
        //Transform names on child items
        DictionaryList<PointOfInterest,string> pointsOfInterestPreShowingVal = null)
    {
        // InstantiateItem(itemn, item, alig, pointsOfInterestPreShowingVal);
        //this.itemNumber = ITEM_COUNT;
        this.itemName = itemn;
        if (!ALL_ITEMS_COUNTER.ContainsKey(itemName))
        {
            ALL_ITEMS_COUNTER.Add(itemName, 0);
        }
        ALL_ITEMS_COUNTER[itemName]++;
        this.uniqueItemName = itemName + ITEM_NAME_NR_TEXT + ALL_ITEMS_COUNTER[itemName];

        this.prefab = item;
        if (alig == null)
        {
            alig = new Alignment(0, 0, 0, 0, 0, 0, 0, 0, 0);
        }
        this.alignment = alig;
        this.pointsOfInterestPreShowing = pointsOfInterestPreShowingVal;
    }
    /*protected void InstantiateItem(string itemn, Transform item, Alignment alig, DictionaryList<PointOfInterest, string> pointsOfInterestPreShowingVal)
    {

    }*/
    public void AddEquipper(Equipper iteme)
    {
        //Debug.Log("Adding equipper to:" + itemName);
        ie = iteme;
        //owner = ie.owner;
        foreach(Item child in subItems)
        {
            child.ie = iteme;
        }
    }
    public void RemoveEquipper()
    {
        ie = null;
        foreach (Item child in subItems)
        {
            child.ie = null;
        }
    }

    protected void AddPointOfInterest(PointOfInterest poi, string s)
    {
        if(pointsOfInterestPreShowing == null)
        {
            pointsOfInterestPreShowing = new DictionaryList<PointOfInterest, string>();
        }
        pointsOfInterestPreShowing.AddIfNotContains(poi, s);
    }

    public Transform GetPointOfInterest(PointOfInterest pois)
    {
        if(pointsOfInterest != null)
        {
            if (pointsOfInterest.Contains(pois)) { 
                return pointsOfInterest[pois];
            }
        }
        Debug.Log("Couldnt find: " + pois + " for item: " + uniqueItemName);
        return null;
    }

    public virtual void Show(   Transform parent,
                                bool onMech = true //Used by mech items
        )
    {
        if (ie != null)
        {
            if (!showing)
            {
                //Materialize item
                ie.Materialize(this, parent);

                //Add joints etc
                PopulatePointsOfInterestAfterShowing();

                //Add animator
                animator = visualItem.GetComponent<Animator>();
            }
            showing = true;
        }
        else
        {
            Debug.Log("No item equiper for: " + itemName);
        }
    }
    public virtual void Hide()
    {
        animator = null;
        if(visualItem != null)
        {
            Global.Remove(visualItem.gameObject);
            visualItem = null;
        }
        if(pointsOfInterest != null)
        {
            pointsOfInterest.Clear();
        }
        showing = false;

    }

    private void PopulatePointsOfInterestAfterShowing()
    {
        //Add any points of interest from our list
        if (pointsOfInterestPreShowing != null)
        {
            foreach (PointOfInterest pos in pointsOfInterestPreShowing)
            {
                Transform t = Global.FindDeepChild(visualItem, pointsOfInterestPreShowing[pos]);
                if (t != null)
                {
                    if (pointsOfInterest == null)
                    {
                        pointsOfInterest = new ListDictionary<PointOfInterest, Transform>(new PointOfInterestComparer());
                    }
                    pointsOfInterest.Add(pos, t);
                }
                else
                {
                    Debug.Log("Unable to find: " + pointsOfInterestPreShowing[pos] + " for " + itemName+ ", Listing all children: ");
                    //Global.FindDeepChild(visualItem, pointsOfInterestPreShowing[pos],true);
                }

            }
        }
    }

    public Alignment GetAlignment()
    {
        return alignment;
    }
    public Item Clone()
    {
        return CloneBaseClass();
    }
    public virtual Item CloneBaseClass()
    {
        return new Item(itemName, prefab, alignment, pointsOfInterestPreShowing == null? null : pointsOfInterestPreShowing.CloneSimple());
    }

    public Item CloneNewAlignment(Alignment aligo)
    {
        if (pointsOfInterestPreShowing != null)
        {
            return new Item(itemName, prefab, aligo, pointsOfInterestPreShowing.CloneSimple());
        }
        return new Item(itemName, prefab, aligo);
    }
    


    public void Enable()
    {
        if (showing)
        {
            visualItem.gameObject.SetActive(true);
        }
        foreach (Item i in subItems)
        {
            i.Enable();
        }
    }
    public void Disable()
    {
        if (showing)
        {
            visualItem.gameObject.SetActive(false);
        }
        foreach (Item i in subItems)
        {
            i.Disable();
        }
    }
    public void ReEnable()
    {
        if(showing)
        {
            visualItem.gameObject.SetActive(false);
            visualItem.gameObject.SetActive(true);
        }
        foreach (Item i in subItems)
        {
            i.ReEnable();
        }
    }

    public static DictionaryList<PointOfInterest, string> PointOfInterestDataToDictionary(PointOfInterestData[] data)
    {
        DictionaryList<PointOfInterest, string> ret = new DictionaryList<PointOfInterest, string>();
        foreach (PointOfInterestData poid in data)
        {
            ret.Add(poid.point, poid.prefabNode);
        }
        return ret;
    }
    /*public static void Position(Transform visualItem, Alignment alignment)
    {
        visualItem.Rotate(new Vector3(alignment.rotX, alignment.rotY, alignment.rotZ));
        //visualItem.localScale += new Vector3(alignment.scaleX, alignment.scaleY, alignment.scaleZ);
        visualItem.position += new Vector3(alignment.x, alignment.y, alignment.z);

    }*/
}

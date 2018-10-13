using System.Collections;
using System.Collections.Generic;
using UnityEngine.EventSystems;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public enum InventoryType
{
    Inventory,
    Equipment,
    //Attachment,
    Socketable
}
public enum CursorHoverMode
{
    Default,
    HoverItem,
    PickUp,
    Target,
    Socket
}
public class InventoryHandler : MonoBehaviour {

    private class ReloadFrame
    {
        public MechItem mi;
        public Vector3 position;
        public Item i;
        public RectTransform self;
        public RectTransform progress;
        public RectTransform dividor;
        public TextMeshProUGUI text;
        public float currentExpandTime;
        public float currentDetractTime;
        public bool hasExpanded = false;

        public ReloadFrame(MechItem mi)
        {
            this.mi = mi;
            this.i = mi.reloadItem;
            this.self = i.visualItem.GetComponent<RectTransform>();
            this.progress = i.GetPointOfInterest(PointOfInterest.Progress).GetComponent<RectTransform>();
            this.dividor = i.GetPointOfInterest(PointOfInterest.Dividor).GetComponent<RectTransform>();
            this.text = i.GetPointOfInterest(PointOfInterest.Title).GetComponent<TextMeshProUGUI>();
            this.currentExpandTime = 0;
            this.currentDetractTime = 0;
            this.position = Vector3.zero;
        }
    }

    private class MouseReloadFrame
    {
        public static readonly float MOUSE_RELOAD_FRAME_MOVE_TIME = 0.25f;

        public Item i;
        public RectTransform self;
        public RectTransform progress;
        public RectTransform dividor;
        public RectTransform topDividor;
        public RectTransform bottomDividor;
        public RectTransform rotate;

        //public RectTransform innerBorder;
        //public RectTransform border;
        //public RectTransform mask;
        //public RectTransform glow;

        public Image image;
        public Image backgroundImage;

        public Image dividorImage;
        public Image bottomDividorImage;
        public Image topDividorImage;
        public Image progressImage;

        //public Image innerBorderImage;
        //public Image outerBorderImage;
        // public Image progressImage;

        // public RawImage maskImage;

        public MoveDirection moveDirection;
        public Vector3 offset;

        public float dividorLength = 1.3f;
        public float innerBorderExtraWidth = 1.03f;
        public float diameter;

        //Used by actual frame
        public MechItem inhabitedBy = null;
        public float timeUntilReady = 0;

        //Used by copies
        public Vector3 targetOffset = Vector3.zero;
        public float currentMoveTime = MOUSE_RELOAD_FRAME_MOVE_TIME;
        public bool reverseDirection = false;


        public MouseReloadFrame(GameUnit owner, float diameter, MoveDirection moveDirection, Vector3 offset)
        {
            this.moveDirection = moveDirection;
            this.offset = offset;
            this.i = Global.Resources[ItemNames.MouseReload];
            owner.itemEquiper.Equip(i);
            this.i.Show(Global.References[SceneReferenceNames.PanelOnMouseItem]);
            this.self = i.visualItem.GetComponent<RectTransform>();
            this.progress = i.GetPointOfInterest(PointOfInterest.Progress).GetComponent<RectTransform>();
            this.dividor = i.GetPointOfInterest(PointOfInterest.Dividor).GetComponent<RectTransform>();
            this.topDividor = i.GetPointOfInterest(PointOfInterest.Top).GetComponent<RectTransform>();
            this.bottomDividor = i.GetPointOfInterest(PointOfInterest.Bottom).GetComponent<RectTransform>();
            this.rotate = i.GetPointOfInterest(PointOfInterest.Rotate).GetComponent<RectTransform>();
            this.image = i.GetPointOfInterest(PointOfInterest.Sprite).GetComponent<Image>();
            this.backgroundImage = i.GetPointOfInterest(PointOfInterest.Background).GetComponent<Image>();
            this.dividorImage = i.GetPointOfInterest(PointOfInterest.Dividor).GetComponent<Image>();
            this.bottomDividorImage = i.GetPointOfInterest(PointOfInterest.Bottom).GetComponent<Image>();
            this.topDividorImage = i.GetPointOfInterest(PointOfInterest.Top).GetComponent<Image>();
            this.progressImage = i.GetPointOfInterest(PointOfInterest.Progress).GetComponent<Image>();
            this.diameter = diameter;

            self.localPosition = offset;
            SetSize(diameter, moveDirection);

        }

        public void SetSize(float diameter, MoveDirection md)
        {
            self.sizeDelta = new Vector2(diameter, diameter);
            rotate.sizeDelta = new Vector2(diameter, diameter);
            dividor.sizeDelta = new Vector2(diameter * dividorLength, dividor.sizeDelta.y);
            topDividor.sizeDelta = new Vector2(diameter * dividorLength, dividor.sizeDelta.y);
            bottomDividor.sizeDelta = new Vector2(diameter * dividorLength, dividor.sizeDelta.y);

            bottomDividor.localPosition = new Vector3(bottomDividor.localPosition.x, -diameter / 2);

            if (md == MoveDirection.Right)
            {
                rotate.localEulerAngles = new Vector3(0, 0, 270);

            } else if (md == MoveDirection.Left)
            {
                rotate.localEulerAngles = new Vector3(0, 0, 90);
            }
            else if (md == MoveDirection.Up)
            {
                rotate.localEulerAngles = new Vector3(0, 0, 0);
            }
            else if (md == MoveDirection.Down)
            {
                rotate.localEulerAngles = new Vector3(0, 0, 180);
            }
            //mask.sizeDelta = new Vector2(diameter, diameter);
            //innerBorder.sizeDelta = new Vector2(diameter, diameter);
            //  border.sizeDelta = new Vector2(diameter, diameter);
            // glow.sizeDelta = new Vector2(diameter, diameter);
            //progress.sizeDelta = new Vector2(diameter, progress.sizeDelta.y);


            //progress.localPosition = new Vector3(progress.localPosition.x, -diameter / 2);
            //innerBorder.localPosition = new Vector3(innerBorder.localPosition.x, diameter / 2);

        }

        public void SetProgress(float percentage)
        {

            if (percentage > 1) { percentage = 1; }
            float y = self.sizeDelta.x * (percentage); //(progress.sizeDelta.x+Mathf.Abs(progress.localPosition.y)*2) * (1-percentage);
            float divY = -(self.sizeDelta.x * percentage) + self.sizeDelta.x / 2;

            if (progress.offsetMax.y != -y)
            {
                progress.offsetMax = new Vector3(progress.offsetMax.x, -y);
                //progress.sizeDelta = new Vector2(progress.sizeDelta.x,y);

                if (percentage == 1)
                {
                    dividor.gameObject.SetActive(false);
                }
                else
                {
                    if (dividor.gameObject.activeSelf == false)
                    {
                        dividor.gameObject.SetActive(true);
                    }
                    dividor.localPosition = new Vector3(dividor.localPosition.x, divY);
                }

                //Debug.Log(percentage);
            }

        }

        public bool Move(Vector3 mousePosition)
        {
            if (currentMoveTime >= MOUSE_RELOAD_FRAME_MOVE_TIME)
            {
                i.visualItem.position = mousePosition;
                i.visualItem.localPosition += offset;
                return true;
            }
            else
            {
                currentMoveTime += Time.deltaTime;
                float t = currentMoveTime / MOUSE_RELOAD_FRAME_MOVE_TIME;
                t = Mathf.Sin(t * Mathf.PI * 0.5f);
                i.visualItem.position = mousePosition;
                i.visualItem.localPosition += Vector3.Lerp(offset, targetOffset, reverseDirection ? 1 - t : t);
            }
            return false;
        }
    }

    private struct EnemyMouseOverHealthBar
    {
        public RectTransform self;
        public RectTransform health;
        public RectTransform shield;
        public RectTransform dividor;
        public float healthLastFrame;
        public float shieldLastFrame;

        public EnemyMouseOverHealthBar(
             RectTransform self,
             RectTransform dividor,
             RectTransform health,
             RectTransform shield)
        {
            this.self = self;
            this.dividor = dividor;
            this.health = health;
            this.shield = shield;
            this.healthLastFrame = -1;
            this.shieldLastFrame = -1;
        }

    }

    private static readonly string BLOCK = "-";
    private static readonly string ADD = "+";
    private static readonly string VACANT = "";

    //Onmouse
    private static readonly int ON_MOUSE_MARKER_BUFFER = 50;
    private static readonly float TOOLTIP_DELAY = 1;

    //Colors used in inventory
    private static readonly Color COLOR_HOLLOW = new Color(1, 1, 1, 0.1f);
    private static readonly Color COLOR_HOVER_SLOT_INVENTORY = new Color(0.8113f, 0.6204f, 0.2028f, 0.05f);
    private static readonly Color COLOR_HOVER_ITEM = new Color(0.8113f, 0.6204f, 0.2028f, 0.3f);
    private static readonly Color COLOR_HOVER_SLOT_EQUIPMENT = new Color(0.8113f, 0.6204f, 0.2028f, 0.3f); //new Color(0.69f, 0.54f, 0.31f, 0.3f);
    private static readonly Color COLOR_CAN_EQUIP = new Color(0f, 0.5804f, 0.1328f, 0.3f);
    private static readonly Color COLOR_DID_BLOCK = new Color(0.58f, 0.02f, 0, 0.3f);
    private static readonly Color COLOR_ATTACHMENT_BORDER = new Color(0.6f, 0.6f, 0.6f, 1);
    private static readonly Color COLOR_ATTACHMENT_TEXT = new Color(1f, 1f, 1f, 0.6f);

    private static readonly Color SELECTION_COLOR = new Color(1, 1, 1, 0.33f);
    private static readonly Color SOCKET_SELECTION_COLOR = new Color(1, 1, 1, 0.8f);
    private static readonly Color BLOCKED_COLOR = new Color(0.3f, 0, 0.04f, 1f);
    private static readonly Color TRANSPARENT = new Color(0, 0, 0, 0);

    //Size variabbles
    public static readonly int INVENTORY_ROWS = 6;
    public static readonly int INVENTORY_COLUMNS = 11;
    public static readonly float SLOT_WIDTH = 120;
    public static readonly float SLOT_HEIGHT = 120;
    public static readonly float TOOLTIP_MOVE_UP_DISTANCE = 100;
    public static readonly float TOOLTIP_MOVE_UP_DURATION = 0.35f;

    //Reload frame variables
    public static readonly int MAX_RELOAD_FRAMES_PER_ROW = 10;
    public static readonly int RELOAD_FRAME_SIZE = 120;
    public static readonly int RELOAD_FRAME_DISTANCE = 120;
    public static readonly int RELOAD_FRAME_BOTTOM_DISTANCE = 30;
    public static readonly int RELOAD_FRAME_PROGRESS_MIN = 8;
    public static readonly int RELOAD_FRAME_PROGRESS_MAX = 114;
    public static readonly float RELOAD_EXPAND_TIME = 0.1f;
    public static readonly float RELOAD_DETRACT_TIME = 0.2f;
    public static readonly float RELOAD_EXPAND_SIZE = 15;
    public static readonly float RELOAD_GROUP_X = -15;
    public static readonly float RELOAD_GROUP_Y = 30;

    public static readonly int LEFT_MOUSE_RELOAD_GROUP = 0;
    public static readonly int RIGHT_MOUSE_RELOAD_GROUP = 1;
    public static readonly int SHIELD_RELOAD_GROUP = 2;
    public static readonly int ATTACK_RELOAD_GROUP = 3;


    //Mouse reload variables
    public static readonly float[] MOUSE_RELOAD_FRAME_MEMBERS_SIZE = new float[] { 100, 90, 80 };
    public static readonly float MOUSE_RELOAD_FRAME_INIT_DISTANCE = MOUSE_RELOAD_FRAME_MEMBERS_SIZE[0];
    public static readonly Vector3 MOUSE_POINTER_RELOAD_FRAME_OFFSET = new Vector3(
        -MOUSE_RELOAD_FRAME_INIT_DISTANCE / 2+ MOUSE_RELOAD_FRAME_INIT_DISTANCE / 6,
        -MOUSE_RELOAD_FRAME_INIT_DISTANCE / 6);

    public static readonly float MOUSE_RELOAD_FRAME_FADE_TIME = 0.4f;
    public static readonly Color MOUSE_RELOAD_FADE_OUT_COLOR = new Color(0, 0, 0, 0.2f);
    public static readonly float[] MOUSE_RELOAD_FRAME_SHORT_FADE_TIME = new float[] { 0.2f, 0.2f };

    //Healthbbar over selected enemy
    public static readonly Vector3 HEALTHBAR_SELECTED_ENEMY_SIZE = new Vector3(30, 400);


    //Initial positions
    public static readonly Vector3 INVENTORY_INIT_POS = new Vector3(50, -680, 0);
    public static readonly Vector3 EQUIPMENT_CENTER_POS = new Vector3(0, 0, 0);

    //Special slots
    private static int SLOT_LEG_X = -1;
    private static int SLOT_LEG_Y = -1;

    //The positions in our inventory
    public MechItem[,] inventoryMatrix;

    //Lookup tables
    private ListDictionary<Item, Vector3> itemToSlot = new ListDictionary<Item, Vector3>();
    private ListDictionary<Vector3, Item> slotToItem = new ListDictionary<Vector3, Item>(new Vector3Comparer());
    private ListDictionary<Item, Fader> itemToFader = new ListDictionary<Item, Fader>();

    //Current active faders
    private ListHash<Fader> activeFaders = new ListHash<Fader>();

    //List of the locations that we can equip something in
    private ListHash<Item> currentPossibleEquipSlots = new ListHash<Item>();

    //List of all reload items
    private DictionaryList<int,ListHash<ReloadFrame>> reloading = new DictionaryList<int, ListHash<ReloadFrame>>();
    private DictionaryList<int, Item> groups = new DictionaryList<int, Item>();
    private DictionaryList<int, ListHash<MouseReloadFrame>> mouseGroups = new DictionaryList<int, ListHash<MouseReloadFrame>>();
    private ListHash<MouseReloadFrame> animatingReloadFrames = new ListHash<MouseReloadFrame>();
    private Stack<MouseReloadFrame> reloadFramePool = new Stack<MouseReloadFrame>();

    //OnMouse markers
    public Stack<Item> availableMarkers = new Stack<Item>();
    public DictionaryList<Item, Vector3> onMouseMarkers = new DictionaryList<Item, Vector3>();

    //Selected game units
    public ListHash<GameUnit> selectedGameUnits = new ListHash<GameUnit>();
    public ListHash<MechItem> selectedMechItems = new ListHash<MechItem>();

    //The item currently on the mouse pointer
    private MechItem onMouseItem;
    //private MechItem mechItemTooltipLastFrame;

    private Vector3 onMouseEquipOffset = new Vector3(0, 0, 0);
    //The player
    public GameUnit owner;
    //Audiosource for inventory sounds
    public AudioSource source;
    //The slot which the leg goes in
    private Item legslot;

    //State tracking
    private bool showingEquipmentMenu = false;
    private bool showingInventoryMenu = false;
    private MechItem lastFrameMouseOverItem;
    private Item lastFrameMouseOverSlot;
    private Item lastFrameMouseOverSocket;
    private Vector3 lastFrameMousePosition;
    private float timeInSameMousePosition = 0;
    private bool isHovering = false;

    //Tooltip
    //private Item currentTooltip;

    private Tooltip tooltip;
    private float currentTooltipMoveDuration = TOOLTIP_MOVE_UP_DURATION;
    private Vector3 originalTooltipPosition = Vector3.zero;
    private Vector3 moveToTooltipPosition = Vector3.zero;

    //Camera
    private CameraMovement cameraMovement;

    public AudioContainer inventoryAudio;

    public CursorHoverMode currentCursorMode = CursorHoverMode.Default;


    private DictionaryList<Rarity, Transform> itemLabels;

    //Health meters
    public Item selectedEnemyHealthBar;
    private EnemyMouseOverHealthBar enemyHealth;
    // public RectTransform onMousePointerCanvas;


    // Monobehaviour
    void Start () {


        cameraMovement = GameObject.FindObjectOfType<CameraMovement>();
        inventoryAudio = Global.Resources[AudioContainerNames.Inventory];

        PopulateInventory();
        PopulateEquipment();
        source = owner.body.gameObject.AddComponent<AudioSource>();

        //Create a buffer of markers (So we don't have to create them over and over again)
        for(int i = 0; i < ON_MOUSE_MARKER_BUFFER; i++)
        {
            Item marker =Global.Resources[ItemNames.EquipmentSlot]; // Global.instance.EQUIPMENT_OPEN_SLOT.Clone(); //
            owner.itemEquiper.Equip(marker);
            marker.Show(Global.References[SceneReferenceNames.PanelOnMouseMarker]); //Global.instance.PANEL_ON_MOUSE_MARKERS);
            marker.Disable();
            marker.GetPointOfInterest(PointOfInterest.Add).gameObject.SetActive(false);
            marker.GetPointOfInterest(PointOfInterest.Border).gameObject.SetActive(false);
            marker.visualItem.GetComponent<RectTransform>().sizeDelta = new Vector2(SLOT_WIDTH, SLOT_HEIGHT);
            availableMarkers.Push(marker);
        }

        tooltip = new Tooltip(owner);

        
        //Create mouse reload groups
        for(int group = 0; group < 4; group++)
        {
            float width = 0;
            ListHash<MouseReloadFrame> ms = new ListHash<MouseReloadFrame>();
            for (int i = 0; i < MOUSE_RELOAD_FRAME_MEMBERS_SIZE.Length; i++)
            {
                float diameter = MOUSE_RELOAD_FRAME_MEMBERS_SIZE[i];
                MoveDirection md;
                Vector3 pos = MOUSE_POINTER_RELOAD_FRAME_OFFSET;
                
                if (group == LEFT_MOUSE_RELOAD_GROUP)
                {
                    md = MoveDirection.Left;
                    pos += new Vector3(-MOUSE_RELOAD_FRAME_INIT_DISTANCE - width, diameter/2);
                }
                else if(group == RIGHT_MOUSE_RELOAD_GROUP)
                {
                    md = MoveDirection.Right;
                    pos += new Vector3(MOUSE_RELOAD_FRAME_INIT_DISTANCE + width, diameter/2);
                }
                else if(group == SHIELD_RELOAD_GROUP)
                {
                    md = MoveDirection.Up;
                    pos += new Vector3(0, MOUSE_RELOAD_FRAME_INIT_DISTANCE + width+diameter/2);
                }
                else
                {
                    md = MoveDirection.Down;
                    pos += new Vector3(0, - MOUSE_RELOAD_FRAME_INIT_DISTANCE - width+diameter/2);
                    
                }
                MouseReloadFrame mrf = new MouseReloadFrame(owner, diameter, md, pos);
                ms.Add(mrf);
                AddFaderToMouseReloadFrame(mrf);
                width += MOUSE_RELOAD_FRAME_MEMBERS_SIZE[i];
            }
            mouseGroups.Add(group, ms);
        }

            itemLabels = new DictionaryList<Rarity, Transform>
        {
            {Rarity.None,Global.Create(Global.Resources[PrefabNames.ItemName],Global.References[SceneReferenceNames.NodeDroppedItems])},
            {Rarity.Common,Global.Create(Global.Resources[PrefabNames.ItemNameCommon],Global.References[SceneReferenceNames.NodeDroppedItems])},
            {Rarity.Uncommon,Global.Create(Global.Resources[PrefabNames.ItemNameUncommon],Global.References[SceneReferenceNames.NodeDroppedItems])},
            {Rarity.Rare,Global.Create(Global.Resources[PrefabNames.ItemNameRare],Global.References[SceneReferenceNames.NodeDroppedItems])},
            {Rarity.Legendary,Global.Create(Global.Resources[PrefabNames.ItemNameLegendary],Global.References[SceneReferenceNames.NodeDroppedItems])}
        };
        foreach(Rarity r in itemLabels)
        {
            itemLabels[r].gameObject.SetActive(false);
        }

        //Cursors
        /*cursors = new DictionaryList<int, Texture2D>
        {
            { (int)MouseCursorMode.Default, Global.Resources[SpriteNames.CursorDefault].texture},
            { (int)MouseCursorMode.HoverEnemy, Global.Resources[SpriteNames.CursorTarget].texture},
            { (int)MouseCursorMode.HoverItem, Global.Resources[SpriteNames.CursorItem].texture},
            { (int)MouseCursorMode.HoverPickUp, Global.Resources[SpriteNames.CursorPickUp].texture},
            { (int)MouseCursorMode.HoverSocket, Global.Resources[SpriteNames.CursorSocket].texture},
        };*/

        selectedEnemyHealthBar = Global.Resources[ItemNames.EnemyHealthBar];
        owner.itemEquiper.Equip(selectedEnemyHealthBar);
        selectedEnemyHealthBar.Show(Global.References[SceneReferenceNames.PanelOnMouseItem]);
        enemyHealth = new EnemyMouseOverHealthBar(
            selectedEnemyHealthBar.visualItem.GetComponent<RectTransform>(),
            selectedEnemyHealthBar.GetPointOfInterest(PointOfInterest.Dividor).GetComponent<RectTransform>(),
            selectedEnemyHealthBar.GetPointOfInterest(PointOfInterest.Health).GetComponent<RectTransform>(),
            selectedEnemyHealthBar.GetPointOfInterest(PointOfInterest.Shield).GetComponent<RectTransform>()
        );
        selectedEnemyHealthBar.Disable();


        //onMousePointerCanvas = Global.References[SceneReferenceNames.PanelOnMouseItem].GetComponent<RectTransform>();

        //Subscribe to menu changes
        Global.instance.menues.menuChange += OnMenuChange;

        //Set the camera pos depending on what we have equipped
        UpdateCameraPosition();


    }
    void Update()
    {
        //Check each frame for mouseovers
        bool thisFrameClickZero = Input.GetMouseButtonUp(0);
        bool thisFrameClickOne = Input.GetMouseButtonUp(1);
        bool interactedWithEnvironmentThisFrame = false;

        Vector3 thisFrameMousePosition = Input.mousePosition;
        Item thisFrameMouseOverSlot = null;
        MechItem thisFrameMouseOverItem = null;
        Item thisFrameMouseOverSocket = null;
        InventoryType type = InventoryType.Inventory;
        float mouseX = Input.mousePosition.x;
        bool isLeftSide = mouseX < Screen.width / 2;
        bool isRightSide = !isLeftSide;
        bool mouseIsOverInventory = showingInventoryMenu && isRightSide;
        bool mouseIsOverEquipment = showingEquipmentMenu && isLeftSide;
        float actionspeed = owner.stats.GetCurrentValue(Stat.ActionSpeed);

        bool attackGroup = Input.GetKeyDown(KeyCode.S);
        bool shieldGroup = Input.GetKeyDown(KeyCode.W);

        //Do mouseover checks
        if (!thisFrameClickZero && !showingEquipmentMenu && !showingInventoryMenu && lastFrameMouseOverItem == null && lastFrameMouseOverSlot == null)
        {
            //Ignore when hidden!
        }
        else
        {
            //Check for deleted items
            if (lastFrameMouseOverSlot != null && !itemToSlot.Contains(lastFrameMouseOverSlot))
            {
                lastFrameMouseOverSlot = null;
            }
            if (lastFrameMouseOverItem != null && !lastFrameMouseOverItem.inventoryItem.showing)
            {
                lastFrameMouseOverItem = null;
            }

            //Get mouse over slot
            if (mouseIsOverInventory)
            {
                thisFrameMouseOverSlot = GetItemSlotInInventoryThatMouseIsOn();
            }
            if (thisFrameMouseOverSlot == null && mouseIsOverEquipment)
            {
                thisFrameMouseOverSlot = GetEquipmentSlotThatMouseIsOn();
            }

            //Get mouse over item
            if (thisFrameMouseOverSlot != null)
            {
                Vector3 pos = itemToSlot[thisFrameMouseOverSlot];
                type = (InventoryType)pos.z;

                if (type == InventoryType.Inventory)
                {
                    if (inventoryMatrix[(int)pos.x, (int)pos.y] != null)
                    {
                        thisFrameMouseOverItem = inventoryMatrix[(int)pos.x, (int)pos.y];
                    }
                }
                else if (type == InventoryType.Equipment)
                {
                    if (pos.x == SLOT_LEG_X && pos.y == SLOT_LEG_Y)
                    {
                        thisFrameMouseOverItem = owner.mech.legs;
                    }
                    else
                    {
                        thisFrameMouseOverItem =
                            owner.mech.equippedCoreMatrix[
                                (int)pos.x, 
                                (int)(owner.mech.equippedCoreMatrix.GetLength(1) - 1 - pos.y)].occupant;
                    }
                }
                /*else if(type == InventoryType.Attachment){
                    thisFrameMouseOverItem = owner.mech.attachmentSlots[new Vector2(pos.x,pos.y)].occupant;
                }*/
            }

            //Socket
            if (thisFrameMouseOverItem != null)
            {
                thisFrameMouseOverSocket = GetHoveredSocket(thisFrameMouseOverItem);
            }

            //Check for changes of slot
            if (thisFrameMouseOverSlot != lastFrameMouseOverSlot){
                if (thisFrameMouseOverSlot != null){
                    OnPointerEnter(thisFrameMouseOverSlot, type, MechItemClass.NotMechItem);
                }
                if (lastFrameMouseOverSlot != null){
                    OnPointerExit(lastFrameMouseOverSlot, type, MechItemClass.NotMechItem);
                }
            }

            //Check for changes of item
            if (thisFrameMouseOverItem != lastFrameMouseOverItem){
                if (thisFrameMouseOverItem != null){
                    OnPointerEnter(thisFrameMouseOverItem, type, MechItem.GetClass(thisFrameMouseOverItem));
                }
                if (lastFrameMouseOverItem != null){
                    OnPointerExit(lastFrameMouseOverItem, type, MechItem.GetClass(lastFrameMouseOverItem));
                }
            }

            //Check for changes of socket
            if (thisFrameMouseOverSocket != lastFrameMouseOverSocket)
            {
                if (thisFrameMouseOverSocket != null)
                {
                    OnPointerEnter(thisFrameMouseOverSocket, InventoryType.Socketable, MechItemClass.NotMechItem);
                }
                if (lastFrameMouseOverSocket != null)
                {
                    OnPointerExit(lastFrameMouseOverSocket, InventoryType.Socketable, MechItemClass.NotMechItem);
                }
            }

            //Same mouse position
            if (thisFrameMousePosition == lastFrameMousePosition
                && !thisFrameClickZero
                && thisFrameMouseOverItem == lastFrameMouseOverItem
                && thisFrameMouseOverItem != null
                && onMouseItem == null)
            {
                timeInSameMousePosition += Time.deltaTime;
            }
            else
            {
                timeInSameMousePosition = 0;
            }

            //Check if we clicked
            if (thisFrameClickZero)
            {
                interactedWithEnvironmentThisFrame = OnPointerClick(
                    mouseIsOverEquipment || mouseIsOverInventory,
                    thisFrameMouseOverItem, 
                    thisFrameMouseOverSlot,
                    thisFrameMouseOverSocket,
                    type,
                    thisFrameMouseOverItem != null ? MechItem.GetClass(thisFrameMouseOverItem)
                    : MechItemClass.NotMechItem);

            //Hovering
            }else if(thisFrameMouseOverItem != null 
                && timeInSameMousePosition > TOOLTIP_DELAY
                && !isHovering){

                OnPointerHoverStart(
                    thisFrameMouseOverItem,
                    thisFrameMouseOverSlot,
                    thisFrameMouseOverSocket,
                    type);
                isHovering = true;
            }

            if (isHovering && timeInSameMousePosition == 0)
            {
                OnPointerHoverEnd();
                isHovering = false;

            }
        }

        //Tooltip
        if(currentTooltipMoveDuration  < TOOLTIP_MOVE_UP_DURATION)
        {
            currentTooltipMoveDuration += Time.unscaledDeltaTime;
            float t = currentTooltipMoveDuration / TOOLTIP_MOVE_UP_DURATION;
            t = Mathf.Sin(t * Mathf.PI * 0.5f);
            tooltip.tooltip.visualItem.localPosition = Vector3.Lerp(originalTooltipPosition, moveToTooltipPosition, t);
        }
        


        //Fade items
        foreach (Fader fad in activeFaders)
        {
            if (!fad.hasFaded)
            {
                fad.FadeColor();
            }
            else
            {
                fad.EndFade();
                activeFaders.RemoveLater(fad);
            }
        }
        activeFaders.Remove();

        //Keeps item on cursor
        if (onMouseItem != null)
        {
            onMouseItem.inventoryItem.visualItem.position = thisFrameMousePosition + 
                ((type == InventoryType.Inventory )? Vector3.zero: onMouseEquipOffset);

            //Show markers (+/-)
            if(onMouseMarkers.Count > 0)
            {
                foreach(Item i in onMouseMarkers)
                {
                    Vector3 pos = onMouseMarkers[i];
                    i.visualItem.position = onMouseItem.inventoryItem.visualItem.position + pos;
                }
            }
        }

        //Set mouse reload frame
        if(onMouseItem == null && !mouseIsOverInventory && !mouseIsOverEquipment)
        {
            ShowMouseReloadFrame(thisFrameMousePosition,actionspeed);
        }
        else
        {
            HideMouseReloadFrame();
        }

        //Set correct cursor
        if (!mouseIsOverEquipment && !mouseIsOverInventory && onMouseItem == null
            && selectedMechItems.Count > 0)
        {
            if(currentCursorMode != CursorHoverMode.PickUp)
            {
                currentCursorMode = CursorHoverMode.PickUp;
                Cursor.SetCursor(Global.instance.pickUpItemCursor, Vector2.zero, CursorMode.Auto);

                FadeReloadFrame(Fade.FadeIn);
            }
        }else if (!mouseIsOverEquipment && !mouseIsOverInventory && onMouseItem == null)
        {
            if (currentCursorMode != CursorHoverMode.Target)
            {
                currentCursorMode = CursorHoverMode.Target;
                Cursor.SetCursor(Global.instance.targetCursor, Vector2.zero, CursorMode.Auto);

                FadeReloadFrame(Fade.FadeOut);
            }
        }else if(thisFrameMouseOverSocket != null && onMouseItem != null)
        {
            if(currentCursorMode != CursorHoverMode.Socket)
            {
                currentCursorMode = CursorHoverMode.Socket;
                Cursor.SetCursor(Global.instance.socketCursor, Vector2.zero, CursorMode.Auto);
            }
        }else if (thisFrameMouseOverItem != null)
        {
            if (currentCursorMode != CursorHoverMode.HoverItem)
            {
                currentCursorMode = CursorHoverMode.HoverItem;
                Cursor.SetCursor(Global.instance.hoverItemCursor, Vector2.zero, CursorMode.Auto);
            }
        }
        else if(currentCursorMode != CursorHoverMode.Default)
        {
            currentCursorMode = CursorHoverMode.Default;
            Cursor.SetCursor(Global.instance.defaultCursor, Vector2.zero, CursorMode.Auto);

            FadeReloadFrame(Fade.FadeIn);
        }

        //Move mouseframes
        foreach(MouseReloadFrame mrf in animatingReloadFrames)
        {
            if (mrf.Move(thisFrameMousePosition))
            {
                animatingReloadFrames.RemoveLater(mrf);
                reloadFramePool.Push(mrf);
                mrf.i.Disable();
            }
        }
        animatingReloadFrames.Remove();

        //Move item titles
        if(selectedMechItems.Count > 0)
        {
            MechItem mi = selectedMechItems.Get(0);
            itemLabels[mi.rarity].position = mi.visualItem.position + mi.GetTextOffset();
        }

        //Move health bar over enemy
       if(selectedGameUnits.Count > 0)
        {
            ShowHealthFor(selectedGameUnits.Get(0));
        }
        else if (selectedEnemyHealthBar.isEnabled)
        {
            selectedEnemyHealthBar.Disable();
        }
        /*Vector2 ViewportPosition = Camera.main.WorldToViewportPoint(objectTransformPosition);
        Vector2 proportionalPosition = new Vector2(
            ViewportPosition.x * Canvas.sizeDelta.x, ViewportPosition.y * Canvas.sizeDelta.y);
        this.rectTransform.localPosition = proportionalPosition - uiOffset;*/



        //Detect actions
        if (!mouseIsOverEquipment && !mouseIsOverInventory)
        {
            if (thisFrameClickZero && !interactedWithEnvironmentThisFrame)
            {
                ExecuteFirstMemberOfGroup(LEFT_MOUSE_RELOAD_GROUP,actionspeed);
            }
            else if (thisFrameClickOne)
            {
                ExecuteFirstMemberOfGroup(RIGHT_MOUSE_RELOAD_GROUP,actionspeed);
            }

            if (attackGroup)
            {
                ExecuteFirstMemberOfGroup(ATTACK_RELOAD_GROUP, actionspeed);

            }else if (shieldGroup)
            {
                ExecuteFirstMemberOfGroup(SHIELD_RELOAD_GROUP, actionspeed);
            }
        }

        if (onMouseItem == null && !mouseIsOverInventory && !mouseIsOverEquipment)
        {
            UpdateReloadFrames(actionspeed);
        }

        //Set last frame values at the every end
        lastFrameMouseOverItem = thisFrameMouseOverItem;
        lastFrameMouseOverSlot = thisFrameMouseOverSlot;
        lastFrameMouseOverSocket = thisFrameMouseOverSocket;
        lastFrameMousePosition = thisFrameMousePosition;
    }

    public void ShowHealthFor(GameUnit gu)
    {
        if (!selectedEnemyHealthBar.isEnabled)
        {
            selectedEnemyHealthBar.Enable();

        }
        Vector2 ViewportPosition = Camera.main.WorldToViewportPoint(gu.GetHeadPos());
        Vector3 to = Camera.main.WorldToScreenPoint(gu.GetHeadPos()+ new Vector3(0, 4));

        if (selectedEnemyHealthBar.visualItem.position != to)
        {
            selectedEnemyHealthBar.visualItem.position = to;
        }

        float maxHealth = gu.stats.GetStat(Stat.Health);
        float maxShield = gu.stats.GetStat(Stat.Shield);
        float total = maxHealth + maxShield;
        float maxPrcntHealth = maxHealth / total;
        float maxPrcntShield = maxShield / total;

        float dividorPosY = (maxPrcntHealth * HEALTHBAR_SELECTED_ENEMY_SIZE.y)- HEALTHBAR_SELECTED_ENEMY_SIZE.y/2;

        if(enemyHealth.dividor.localPosition.y != dividorPosY)
        {
            enemyHealth.dividor.localPosition = new Vector3(enemyHealth.dividor.localPosition.x, dividorPosY);
            enemyHealth.health.sizeDelta = new Vector3(
                enemyHealth.health.sizeDelta.x, maxPrcntHealth * HEALTHBAR_SELECTED_ENEMY_SIZE.y);
            enemyHealth.health.localPosition = new Vector3(
                enemyHealth.health.localPosition.x, -(maxPrcntShield* HEALTHBAR_SELECTED_ENEMY_SIZE.y)/2);
            enemyHealth.shield.sizeDelta = new Vector3(
                enemyHealth.shield.sizeDelta.x, maxPrcntShield* HEALTHBAR_SELECTED_ENEMY_SIZE.y);
            enemyHealth.shield.localPosition = new Vector3(
                enemyHealth.shield.localPosition.x,
                (maxPrcntShield * HEALTHBAR_SELECTED_ENEMY_SIZE.y )/ 2
                - (maxPrcntHealth * HEALTHBAR_SELECTED_ENEMY_SIZE.y) / 2

                );

        }

        float healthPercent = gu.stats.GetValuePercentage(Stat.Health);
        float shieldPercent = gu.stats.GetValuePercentage(Stat.Shield);

        if(enemyHealth.healthLastFrame != healthPercent)
        {
            Global.Resources[MaterialNames.HealthSelectedEnemy].SetFloat(OrbVariable.FILL, healthPercent);
            enemyHealth.healthLastFrame = healthPercent;

        }
        if (enemyHealth.shieldLastFrame != shieldPercent)
        {
            Global.Resources[MaterialNames.ShieldSelectedEnemy].SetFloat(OrbVariable.FILL, shieldPercent);
            enemyHealth.shieldLastFrame = shieldPercent;
        }
        //HEALTHBAR_SELECTED_ENEMY_SIZE

    }

    public void ShowMouseReloadFrame(Vector3 mousePos, float actionspeed)
    {
        foreach (int i in mouseGroups)
        {
            foreach (MouseReloadFrame mrf in mouseGroups[i])
            {
                if (!mrf.i.isEnabled && mrf.inhabitedBy != null)
                {
                    mrf.i.Enable();

                }else if (mrf.i.isEnabled && mrf.inhabitedBy == null)
                {
                    mrf.i.Disable();
                }
                if(mrf.inhabitedBy != null)
                {
                    mrf.Move(mousePos);
                    mrf.timeUntilReady = mrf.inhabitedBy.GetTimer(actionspeed);

                    /*if (mrf.inhabitedBy.IsReady())
                    {
                        if (!mrf.i.GetPointOfInterest(PointOfInterest.Glow).gameObject.activeSelf)
                        {
                            mrf.i.GetPointOfInterest(PointOfInterest.Glow).gameObject.SetActive(true);
                        }
                    }
                    else
                    {
                        if (mrf.i.GetPointOfInterest(PointOfInterest.Glow).gameObject.activeSelf)
                        {
                            mrf.i.GetPointOfInterest(PointOfInterest.Glow).gameObject.SetActive(false);
                        }
                    }*/
                }
            }
        }
    }

    public void ExecuteFirstMemberOfGroup(int group, float actionspeed)
    {
        MouseReloadFrame mrf = mouseGroups[group].Get(0);

        if(mrf.inhabitedBy == null)
        {
            inventoryAudio.PlaySound(SoundWhen.Blocked, source, false);

        }else if (!mrf.inhabitedBy.IsReady())
        {
            FadeItem(mrf.i, FadePriority.Marking, Fade.FadeIn, FadePattern.FadeInAndOut, MOUSE_RELOAD_FRAME_SHORT_FADE_TIME, COLOR_DID_BLOCK);
        }
        else
        {
            MechItem toExecute = mrf.inhabitedBy;
            //Fade out the current frame
            GameUnit target = null;
            if (selectedGameUnits.Count > 0)
            {
                target = selectedGameUnits.Get(0);
            }
            //Debug.Log("Fading out: " + mrf.moveDirection);
            FadeOutMouseReloadFrame(mrf, actionspeed, true);


            toExecute.Execute(target);

            //If the next items counter is less than this after executing move everything towards the center
            if(mouseGroups[group].Get(1).inhabitedBy != null 
                && mouseGroups[group].Get(1).inhabitedBy.GetTimer(actionspeed) < toExecute.GetTimer(actionspeed))
            {
                for (int i = 0; i < mouseGroups[group].Count; i++)
                {
                    //Fade all frames except the last
                    MouseReloadFrame curr = mouseGroups[group].Get(i);
                    if (i + 1 != mouseGroups[group].Count && mouseGroups[group].Get(i + 1).inhabitedBy != null)
                    {
                        MouseReloadFrame next = mouseGroups[group].Get(i + 1);
                        FadeInMouseReloadFrame(next.inhabitedBy, curr, next.offset, false);
                    }
                    else
                    {
                        //Fade in the last frame
                        MechItem found = null;
                        float currentTimer = Mathf.Infinity;
                        foreach (ReloadFrame rf in reloading[group])
                        {
                            float rfTimer = rf.mi.GetTimer(actionspeed);
                            bool alreadyExists = false;
                            if (found == null || rfTimer <= 0 || rfTimer < currentTimer)
                            {
                                foreach (MouseReloadFrame contains in mouseGroups[group])
                                {
                                    if (contains.inhabitedBy == rf.mi) { alreadyExists = true; break; }
                                }
                                if (!alreadyExists)
                                {
                                    found = rf.mi;
                                    currentTimer = rfTimer;

                                    if (currentTimer <= 0)
                                    {
                                        break;
                                    }
                                }
                            }
                        }
                        if (found != null)
                        {
                            FadeInMouseReloadFrame(found, curr, curr.offset * 2f, true);
                        }
                    }

                }
            }
           

        }

    }


    public void ShowTextOver(MechItem mi)
    {
        foreach (Rarity r in itemLabels)
        {
            itemLabels[r].gameObject.SetActive(r == mi.rarity);
        }
        itemLabels[mi.rarity].GetComponent<TextMeshPro>().text = mi.itemName;
    }

    public void HideTextOver(MechItem mi)
    {
        foreach (Rarity r in itemLabels)
        {
            itemLabels[r].gameObject.SetActive(false);
        }
    }

    private void SwapIn(int group, int groupPos, MechItem mi, MouseReloadFrame mrf, float actionspeed)
    {

        for (int pos = groupPos+1; pos < MOUSE_RELOAD_FRAME_MEMBERS_SIZE.Length; pos++)
        {
            if(mouseGroups[group].Get(pos).inhabitedBy == mi)
            {
                //Fade out the old version
                FadeOutMouseReloadFrame(mouseGroups[group].Get(pos),actionspeed);
                mouseGroups[group].Get(pos).inhabitedBy = null;
            }
        }
        if (mrf.inhabitedBy != null)
        {
            //Fade out the previous inhabitant
            FadeOutMouseReloadFrame(mrf,actionspeed);
        }
        //Fade in self
        FadeInMouseReloadFrame(mi,mrf, GetFadeInMouseReloadFrameOffset(mrf));
    }

    private void FadeOutMouseReloadFrame(MouseReloadFrame mrf, float actionspeed, bool noOffset = false)
    {
        
        MouseReloadFrame dissolvePrevious = CopyMouseReloadFrame(mrf, actionspeed);

        if (noOffset)
        {
            Vector3 off = new Vector3(0,0);

            if(mrf.moveDirection == MoveDirection.Up || mrf.moveDirection == MoveDirection.Down){

                off = new Vector3(mrf.diameter/2, 0);
            }else if(mrf.moveDirection == MoveDirection.Left)
            {
                off = new Vector3(-mrf.diameter / 2, mrf.diameter / 2 );
            }
            else if(mrf.moveDirection == MoveDirection.Right)
            {
                off = new Vector3(mrf.diameter/2, mrf.diameter/2);
            }
            dissolvePrevious.targetOffset = MOUSE_POINTER_RELOAD_FRAME_OFFSET + off;

        }
        else{
            dissolvePrevious.targetOffset =
            mrf.offset + new Vector3(
                mrf.moveDirection == MoveDirection.Up || mrf.moveDirection == MoveDirection.Down ? mrf.diameter * 2 : 0,
                mrf.moveDirection == MoveDirection.Left || mrf.moveDirection == MoveDirection.Right ? mrf.diameter * 2 : 0);
        }
        dissolvePrevious.currentMoveTime = 0;
        dissolvePrevious.reverseDirection = false;
        animatingReloadFrames.Add(dissolvePrevious);

        FadeItem(dissolvePrevious.i, FadePriority.Selection, Fade.FadeIn, FadePattern.FadeInAndOut, new float[] { MouseReloadFrame.MOUSE_RELOAD_FRAME_MOVE_TIME, 0 }, TRANSPARENT);

    }

    private void FadeInMouseReloadFrame(MechItem mi, MouseReloadFrame mrf, Vector3 offset, bool fade = true)
    {
        mrf.inhabitedBy = mi;
        mrf.image.sprite = mi.inventorySprite;
        mrf.targetOffset = offset;
        mrf.currentMoveTime = 0;
        mrf.reverseDirection = true;

        if (fade)
        {
            FadeItem(mrf.i, FadePriority.Selection, Fade.FadeIn, FadePattern.FadeInAndOut, new float[] { 0, MouseReloadFrame.MOUSE_RELOAD_FRAME_MOVE_TIME }, TRANSPARENT);
        }
    }

    private Vector3 GetFadeInMouseReloadFrameOffset(MouseReloadFrame mrf)
    {
        return mrf.offset + new Vector3(
                    mrf.moveDirection == MoveDirection.Up || mrf.moveDirection == MoveDirection.Down ? -mrf.diameter * 2 : 0,
                    mrf.moveDirection == MoveDirection.Left || mrf.moveDirection == MoveDirection.Right ? -mrf.diameter * 2 : 0);
    }


    private void AddFaderToMouseReloadFrame(MouseReloadFrame mrf)
    {
        itemToFader.Add(mrf.i, new Fader(
                    new Image[]{
                    mrf.backgroundImage,
                    mrf.bottomDividorImage,
                    mrf.topDividorImage,
                    mrf.dividorImage,
                    mrf.image,
                    mrf.progressImage
                    }
                    ));
    }

    public void HideMouseReloadFrame()
    {
        foreach(int i in mouseGroups)
        {
            foreach(MouseReloadFrame mrf in mouseGroups[i])
            {
                if (mrf.i.isEnabled)
                {
                    mrf.i.Disable();
                }
            }
        }
    }

    public void FadeReloadFrame(Fade fad)
    {
        foreach (int i in mouseGroups)
        {
            foreach (MouseReloadFrame mrf in mouseGroups[i])
            {
                FadeItem(mrf.i, FadePriority.MarkSelection, fad, FadePattern.FadeAndStay, MOUSE_RELOAD_FRAME_FADE_TIME, MOUSE_RELOAD_FADE_OUT_COLOR);
            }
        }
    }

    public void UpdateReloadFrames(float actionspeed)
    {
        foreach(int group in reloading)
        {
            ListHash<ReloadFrame> members = reloading[group];

            foreach(ReloadFrame member in members)
            {

                float dist = RELOAD_FRAME_PROGRESS_MAX - RELOAD_FRAME_PROGRESS_MIN;
                float timer = member.mi.GetTimer(actionspeed);
                timer = timer < 0 ? 0 : timer;
                float percentage = member.mi.GetTimeUntilReloadFinished(actionspeed);
                float y = RELOAD_FRAME_PROGRESS_MIN + percentage * dist;

                if (DevelopmentSettings.SHOW_RELOAD_FRAMES)
                {
                    Vector2 offset = new Vector2(member.progress.offsetMax.x, -y);
                    Vector3 pos = new Vector3(member.dividor.localPosition.x, -y + RELOAD_FRAME_SIZE / 2);

                    if (member.progress.offsetMax != offset)
                    {
                        member.progress.offsetMax = offset;
                        member.dividor.localPosition = pos;
                        member.text.text = Tooltip.TTStr(timer);
                        member.hasExpanded = false;

                        if (member.i.GetPointOfInterest(PointOfInterest.Dividor).gameObject.activeSelf == false)
                        {
                            member.i.GetPointOfInterest(PointOfInterest.Glow).gameObject.SetActive(false);
                            member.i.GetPointOfInterest(PointOfInterest.Dividor).gameObject.SetActive(true);
                            member.i.GetPointOfInterest(PointOfInterest.Progress).gameObject.SetActive(true);
                            member.text.gameObject.SetActive(true);
                        }
                    }

                    if (!member.hasExpanded && member.mi.IsReady())
                    {
                        member.currentExpandTime = 0;
                        member.currentDetractTime = 0;
                        member.i.GetPointOfInterest(PointOfInterest.Glow).gameObject.SetActive(true);
                        member.i.GetPointOfInterest(PointOfInterest.Dividor).gameObject.SetActive(false);
                        member.i.GetPointOfInterest(PointOfInterest.Progress).gameObject.SetActive(false);
                        member.text.gameObject.SetActive(false);
                        member.hasExpanded = true;
                    }
                }

                //Find frames to swap in
                int groupPos = 0;
                foreach (MouseReloadFrame mrf in mouseGroups[group])
                {
                    if(mrf.inhabitedBy == member.mi){
                        mrf.SetProgress(percentage);
                        break;
                    }else if(mrf.inhabitedBy == null || mrf.timeUntilReady > timer){
                        SwapIn(group, groupPos, member.mi, mrf,actionspeed);
                        break;
                    }
                    groupPos++;
                }

                if (DevelopmentSettings.SHOW_RELOAD_FRAMES)
                {
                    if (member.currentExpandTime < RELOAD_EXPAND_TIME)
                    {
                        member.currentExpandTime += Time.unscaledDeltaTime;
                        float t = member.currentExpandTime / RELOAD_EXPAND_TIME;
                        t = Mathf.Sin(t * Mathf.PI * 0.5f); //Ease out

                        member.self.localPosition = Vector3.Lerp(
                            member.position, member.position + new Vector3(-RELOAD_EXPAND_SIZE / 2, -RELOAD_EXPAND_SIZE / 2), t);
                        member.self.sizeDelta = Vector2.Lerp(
                            new Vector2(RELOAD_FRAME_SIZE, RELOAD_FRAME_SIZE),
                            new Vector2(RELOAD_FRAME_SIZE + RELOAD_EXPAND_SIZE, RELOAD_FRAME_SIZE + RELOAD_EXPAND_SIZE),
                            t);
                    }
                    else if (member.currentDetractTime < RELOAD_DETRACT_TIME)
                    {
                        member.currentDetractTime += Time.unscaledDeltaTime;
                        float t = member.currentDetractTime / RELOAD_EXPAND_TIME;
                        t = 1f - Mathf.Cos(t * Mathf.PI * 0.5f); // Ease in
                        member.self.localPosition = Vector3.Lerp(
                            member.position + new Vector3(-RELOAD_EXPAND_SIZE / 2, -RELOAD_EXPAND_SIZE / 2), member.position, t);

                        member.self.sizeDelta = Vector2.Lerp(
                            new Vector2(RELOAD_FRAME_SIZE + RELOAD_EXPAND_SIZE, RELOAD_FRAME_SIZE + RELOAD_EXPAND_SIZE),
                            new Vector2(RELOAD_FRAME_SIZE, RELOAD_FRAME_SIZE),
                            t);
                    }
                }


            }
            
        }
    }

    public void AddReloadFrame(MechItem mi, int group)
    {

        mi.ShowReloadVisual(Global.References[SceneReferenceNames.PanelReload]);

        ReloadFrame rf = new ReloadFrame(mi);
        if (reloading.Contains(group))
        {
            reloading[group].Add(rf);
        }
        else
        {
            ListHash<ReloadFrame> dd = new ListHash<ReloadFrame>();
            dd.Add(rf);
            reloading.Add(group, dd);
        }
        RepositionReloadFrames();
    }

    public void RemoveReloadFrame(MechItem mi)
    {
        foreach (int group in reloading)
        {
            ListHash<ReloadFrame> members = reloading[group];
            foreach(ReloadFrame member in members)
            {
                if(member.mi == mi)
                {
                    members.RemoveLater(member);
                    mi.reloadItem.Hide();
                }
            }
            members.Remove();

            foreach (MouseReloadFrame mrf in mouseGroups[group])
            {
                if (mrf.inhabitedBy == mi)
                {
                    mrf.inhabitedBy = null;
                }
            }
        }

        RepositionReloadFrames();
    }

    public void RepositionReloadFrames()
    {
        int numberOfGroups = reloading.Count;
        int maxHeight = 0;
        int maxGroup = 0;

        //Hide groups
        foreach(int group in groups)
        {
            groups[group].Hide();
        }

        //Count max height 
        foreach(int group in reloading)
        {
            ListHash<ReloadFrame> members = reloading[group];
            float height = ((float)members.Count) / ((float)MAX_RELOAD_FRAMES_PER_ROW);
            maxHeight = height > maxHeight ? Mathf.FloorToInt(height) : maxHeight;
            maxGroup = group > maxGroup ? group : maxGroup;
        }

        int maxWidth = 0;
        //Count max width
        foreach (int group in reloading)
        {
            ListHash<ReloadFrame> members = reloading[group];
            if(members.Count >= MAX_RELOAD_FRAMES_PER_ROW)
            {
                maxWidth += MAX_RELOAD_FRAMES_PER_ROW;
            }
            else
            {
                maxWidth +=(int) (((float)members.Count) / ((float)maxHeight+1));
            }
        }
        //Position each member in each group
        int widthPos = 0;
        float startPosX = -((maxWidth+ (numberOfGroups-1)) * RELOAD_FRAME_SIZE  ) / 2;
        float startPosY = (maxHeight+1) * RELOAD_FRAME_SIZE + RELOAD_FRAME_BOTTOM_DISTANCE;

       
        for (int group = 0; group <= maxGroup; group++)
        {
            if (reloading.Contains(group))
            {
                ListHash<ReloadFrame> members = reloading[group];
                int memberNum = 0;
                int currentRow = 0;
                int membersPerRow = members.Count / (maxHeight + 1);

                if (!groups.Contains(group))
                {
                    groups.Add(group, Global.Resources[ItemNames.Group]);
                    owner.itemEquiper.Equip(groups[group]);
                }

                if (!groups[group].showing && members.Count > 0)
                {
                    string text = "Group: " + group;

                    groups[group].Show(Global.References[SceneReferenceNames.PanelReload]);
                    groups[group].GetPointOfInterest(PointOfInterest.Title).GetComponent<TextMeshProUGUI>().text = text;
                    groups[group].GetPointOfInterest(PointOfInterest.Name).GetComponent<TextMeshProUGUI>().text = text;

                    if (group == LEFT_MOUSE_RELOAD_GROUP)
                    {
                        groups[group].GetPointOfInterest(PointOfInterest.Add).gameObject.SetActive(true);
                        groups[group].GetPointOfInterest(PointOfInterest.Sprite).localEulerAngles = new Vector3(0, 180);
                    }
                    else if (group == RIGHT_MOUSE_RELOAD_GROUP)
                    {
                        groups[group].GetPointOfInterest(PointOfInterest.Add).gameObject.SetActive(true);
                        groups[group].GetPointOfInterest(PointOfInterest.Sprite).localEulerAngles = new Vector3(0, 0);
                    }
                    else
                    {
                        groups[group].GetPointOfInterest(PointOfInterest.Add).gameObject.SetActive(false);
                    }

                    groups[group].visualItem.localPosition = new Vector3(
                        startPosX + (widthPos + group) * RELOAD_FRAME_SIZE + RELOAD_GROUP_X,
                        startPosY - currentRow * RELOAD_FRAME_SIZE + RELOAD_GROUP_Y);

                    if (!DevelopmentSettings.SHOW_RELOAD_FRAMES)
                    {
                        groups[group].Disable();
                    }
                }

                foreach (ReloadFrame member in members)
                {

                    member.self.localPosition = new Vector3(
                        startPosX + ((memberNum % membersPerRow) + widthPos + group) * RELOAD_FRAME_SIZE,
                        startPosY - currentRow * RELOAD_FRAME_SIZE);

                    member.position = member.self.localPosition;

                    if (memberNum + 1 % membersPerRow == 0)
                    {
                        currentRow++;
                    }
                    memberNum++;

                    if (!DevelopmentSettings.SHOW_RELOAD_FRAMES)
                    {
                        member.i.Disable();
                    }

                }

                if (members.Count >= MAX_RELOAD_FRAMES_PER_ROW)
                {
                    widthPos += MAX_RELOAD_FRAMES_PER_ROW;
                }
                else
                {
                    widthPos += (int)(((float)members.Count) / ((float)maxHeight + 1));
                }

                //Debug.Log("group: " + group + " has :" + widthPos);
            }
        }
    }

    private MouseReloadFrame CopyMouseReloadFrame(MouseReloadFrame mrf, float actionspeed)
    {
        if(reloadFramePool.Count == 0)
        {
            MouseReloadFrame m = new MouseReloadFrame(owner, mrf.diameter, mrf.moveDirection, mrf.offset);
            AddFaderToMouseReloadFrame(m);
            reloadFramePool.Push(m);
        }
        MouseReloadFrame ret = reloadFramePool.Pop();
        ret.i.Enable();
        ret.diameter = mrf.diameter;
        ret.SetSize(mrf.diameter, mrf.moveDirection);
        ret.moveDirection = mrf.moveDirection;
        ret.offset = mrf.offset;
        ret.image.sprite = mrf.inhabitedBy.inventorySprite;
        ret.SetProgress(mrf.inhabitedBy.GetTimeUntilReloadFinished(actionspeed));

        return ret;
    }

    //Updates the camera positon when we equip or unequip
    public void UpdateCameraPosition()
    {
        float maxLegFactor = Legs.MAX_LEG_INVENTORY_WIDTH + Legs.MAX_LEG_INVENTORY_HEIGHT;
        float legFactor = maxLegFactor+(owner.mech.legs.heightCapacity + owner.mech.legs.widthCapacity) / 2;
        Vector2 coreBounds = owner.mech.GetCoresOccupiedSize();
        float widthFactor = coreBounds.x + coreBounds.y;

        cameraMovement.SetTarget(
            new Vector3(
                Global.CAMERA_DISTANCE.x,
                (legFactor + widthFactor)/3, 
                -(legFactor + widthFactor))
                );
    }
    //Register to lookup list
    private void RegisterSlot(Item slot, InventoryType type, float x, float y)
    {
        itemToSlot.Add(slot, new Vector3(x, y, (int)type));
        slotToItem.Add(new Vector3(x, y, (int)type), slot);
    }
    //Unregister from lookup list
    private void UnregisterSlot(Item slot, InventoryType type, float x, float y)
    {
        itemToSlot.Remove(slot);
        slotToItem.Remove(new Vector3(x, y, (int)type));
    }
    //Creates a slot
    private void RemoveSlot(Item slot, InventoryType type, int x, int y)
    {
        owner.itemEquiper.Unequip(slot);
        if (activeFaders.Contains(itemToFader[slot]))
        {
            activeFaders.Remove(itemToFader[slot]);
        }
        itemToFader.Remove(slot);
        UnregisterSlot(slot, type, x, y);
        slot.Hide();
    }
    private void RemoveFaderFor(MechItem mi)
    {
        if (activeFaders.Contains(itemToFader[mi]))
        {
            activeFaders.Remove(itemToFader[mi]);
        }
        itemToFader.Remove(mi);
    }
    //Creates a equipment slot and returns it
    private Item CreateEquipmentSlot(Vector3 pos, float slotWidth, float slotHeight)
    {
        Item slot =Global.Resources[ItemNames.EquipmentSlot]; // Global.instance.EQUIPMENT_OPEN_SLOT.Clone(); //
        slot.itemName = slot.itemName + "<" + pos.x + "," + pos.y + ">";
        owner.itemEquiper.Equip(slot);
        slot.Show(Global.References[SceneReferenceNames.PanelEquipmentSlot]); //GGlobal.instance.PANEL_EQUIPMENT_SLOT); //
        slot.visualItem.localPosition = pos;
        slot.visualItem.GetComponent<RectTransform>().sizeDelta = new Vector2(slotWidth, slotHeight);
        return slot;
    }
    //Method for equipping legs
    private bool EquipLegs(MechItem legs)
    {
        //Save the old legs since these will be unequipped
        Legs oldLegs = owner.mech.legs;

        //Get old leg stats
        int width = owner.mech.legs.widthCapacity + 2;
        int height = owner.mech.legs.heightCapacity + 1;
        float legSlotWidth = owner.mech.legs.GetInventorySize().x * SLOT_WIDTH;
        float legSlotHeight = owner.mech.legs.GetInventorySize().y * SLOT_HEIGHT;
        Vector3 legPos = EQUIPMENT_CENTER_POS + new Vector3(-legSlotWidth / 2, -SLOT_HEIGHT * height / 2) + new Vector3(0, legSlotHeight / 2);

        //Equip legs on model
        ListHash<Core> blockers = owner.mech.EquipLegs(legs);

        if(blockers.Count == 0)
        {
            //Remove current slots
            HideEquipmentSlots(width, height, legSlotWidth, legSlotHeight, legPos);

            //Get new leg stats
            width = owner.mech.legs.widthCapacity + 2;
            height = owner.mech.legs.heightCapacity + 1;
            legSlotWidth = owner.mech.legs.GetInventorySize().x * SLOT_WIDTH;
            legSlotHeight = owner.mech.legs.GetInventorySize().y * SLOT_HEIGHT;
            legPos = EQUIPMENT_CENTER_POS + new Vector3(-legSlotWidth / 2, -SLOT_HEIGHT * height / 2) + new Vector3(0, legSlotHeight / 2);

            //Show new slots
            ShowEquipmentSlots(width, height, legSlotWidth, legSlotHeight, legPos);

            //Set pos
            legs.inventoryItem.visualItem.position = legslot.visualItem.position;
            legs.inventoryItem.visualItem.transform.SetParent(Global.References[SceneReferenceNames.PanelEquipmentItem]); //Global.instance.PANEL_EQUIPMENT_ITEM);
            itemToFader[legs].EndFade();

            //Clear onmouse
            onMouseItem = null;

            //Pickup old legs
            PickUpItem(oldLegs,null, InventoryType.Equipment);

            //Update blocks
            UpdateBlockTypes(owner.mech.legs.widthCapacity + 2, owner.mech.legs.heightCapacity + 1);

            //Update positions of items
            //Debug.Log("Recalc position");
            RecalculatePositionOfEquipmentItems();
            //Debug.Log("Done recalcing");

            //Clear attachments and recreate
           // UpdateAttachmentSpots(true);
        }
        else
        {
            //Show why we can't switch legs
            foreach (Core c in blockers)
            {
                SignalBlock(c, FadePriority.MarkSelection, false);
            }
            SignalBlock(legs, FadePriority.MarkSelection, true, false);

            return false;
        }
        return true;
    }
    // Equips a core if possible
    private bool EquipCore(MechItem mi, Item slot)
    {
        //Get pos
        Vector3 slotPos = itemToSlot[slot];

        //Converts equipment visuals to actual backend location (y-reversed in backend since legs are attached to bottom)
        Vector2 pos = EquipmentSlotToMechEquippedMatrixSlot(slotPos);

        //Get what is currently equipped on this square
        InventoryBlock currentlyEquipped = owner.mech.equippedCoreMatrix[(int)pos.x,(int)pos.y];

        //Save old equipment spots
        InventoryBlockType[,] oldEquipped = owner.mech.SnapShotEquippedMatrixTypes();

        //Convert to core
        Core core = (Core)mi;
        if (owner.mech.EquipCore(core, currentlyEquipped))
        {
            //Set position
            Vector3 newSlotPos = new Vector3(
                        slotPos.x - core.equipSlotOffset.x,
                        slotPos.y + core.equipSlotOffset.y,
                        (int)InventoryType.Equipment);

            //Update position and also put on equipment panel from mouse panel
            mi.inventoryItem.visualItem.position = slotToItem[newSlotPos].visualItem.position;
            mi.inventoryItem.visualItem.transform.SetParent(Global.References[SceneReferenceNames.PanelEquipmentItem]); //Global.instance.PANEL_EQUIPMENT_ITEM);

            //Clear mouse
            onMouseItem = null;

            //Update blocks visual
            UpdateBlockTypes(owner.mech.legs.widthCapacity + 2, owner.mech.legs.heightCapacity + 1);

            //Mark differences visually
            MarkEquippedDifferences(oldEquipped);

            //Create attachment spots
            //UpdateAttachmentSpots();

            return true;
        }
        else
        {
            SignalBlock(mi, FadePriority.MarkSelection);
            return false;
        }

        //owner.mech.EquipCore(mi);

    }
    // Equips a attachment
    /*private bool EquipAttachment(Attachment att, Item slot)
    {
        Vector3 pos = itemToSlot[slot];
        if (pos.z == (int)InventoryType.Attachment)
        {
            AttachmentSlot attSlot = owner.mech.attachmentSlots[new Vector2(pos.x,pos.y)];
            if(owner.mech.EquipAttachment(att,attSlot))
            {
                //Position item
                att.inventoryItem.visualItem.SetParent(Global.References[SceneReferenceNames.PanelAttachmentItem], false);//Global.instance.PANEL_EQUIPMENT_ATTACHMENT_ITEM, false);
                att.inventoryItem.visualItem.position = slot.visualItem.position;
                
                onMouseItem = null;

                //Create attachment spots
                UpdateAttachmentSpots();

                return true;
            }
            else
            {
                SignalBlock(att, FadePriority.MarkSelection);
            }
        }
        return false;
    }
    // Synchronizes attachment spots with the ones on the mech
    private void UpdateAttachmentSpots(bool clearAll = false)
    {
        //Check for slots that are no longer in use
        List<Item> itemsToRemove = new List<Item>();
        foreach (Vector3 pos in itemToSlot)
        {
            //Filter on only attachments
            if ((InventoryType)((int)pos.z) == InventoryType.Attachment)
            {
                Vector2 p = new Vector2(pos.x, pos.y);
                if (!owner.mech.attachmentSlots.Contains(p) || clearAll)
                {
                    itemsToRemove.Add(slotToItem[pos]);
                }
            }
        }
        foreach (Item i in itemsToRemove)
        {
            Vector3 unregPos = itemToSlot[i];
            RemoveSlot(i, InventoryType.Attachment, (int)unregPos.x, (int)unregPos.y);
        }
        itemsToRemove.Clear();

        //Check for new slots
        foreach (Vector2 p in owner.mech.attachmentSlots)
        {
            
            Vector3 pos = new Vector3(p.x, p.y, (int)InventoryType.Attachment);

            if (!slotToItem.Contains(pos))
            {
                //Debug.Log("Creating attachment: " + pos + " from "+p);

                AttachmentSlot slot = owner.mech.attachmentSlots[p];

                //Show item
                Item i = Global.Resources[ItemNames.EquipmentSlot]; //Global.instance.EQUIPMENT_OPEN_SLOT.Clone(); // 
                owner.itemEquiper.Equip(i);
                i.Show(Global.References[SceneReferenceNames.PanelEquipmentSlot]); //Global.instance.PANEL_EQUIPMENT_ATTACHMENT_SLOT);

                //Size and rotate to become slanted
                i.visualItem.GetComponent<RectTransform>().sizeDelta = new Vector2(
                    SLOT_WIDTH*slot.diameter*MechItem.ROTATION_SCALING, 
                    SLOT_HEIGHT*slot.diameter*MechItem.ROTATION_SCALING);
                i.GetPointOfInterest(PointOfInterest.Add).Rotate(0, 0, -MechItem.DEFAULT_ROTATION);
                i.GetPointOfInterest(PointOfInterest.LeftCorner).localPosition += new Vector3(
                    slot.diameter*0.5f*SLOT_WIDTH * (1 - MechItem.ROTATION_SCALING), 
                    -slot.diameter * 0.5f * SLOT_HEIGHT * (1 - MechItem.ROTATION_SCALING));
                i.GetPointOfInterest(PointOfInterest.LeftCorner).Rotate(0, 0, MechItem.DEFAULT_ROTATION);

                //Fix colors etc
                i.GetPointOfInterest(PointOfInterest.Add).GetComponentInChildren<TextMeshProUGUI>().color = COLOR_ATTACHMENT_TEXT;
                i.GetPointOfInterest(PointOfInterest.Border).GetComponent<Image>().color = COLOR_ATTACHMENT_BORDER;
                i.GetPointOfInterest(PointOfInterest.Background).gameObject.SetActive(true);
                i.GetPointOfInterest(PointOfInterest.InnerBorder).gameObject.SetActive(true);

                Vector2 scale = GetEquipmentPanelScaleFactor();

                //Position
                MoveAttachmentSlot(i, p, scale, slot);

                //Register the new slot
                RegisterSlot(i, InventoryType.Attachment, p.x, p.y);

                //Add fader possibility
                itemToFader.Add(i, new Fader(
                    i.GetPointOfInterest(
                        PointOfInterest.Selection).GetComponent<Image>(), 
                        i.GetPointOfInterest(PointOfInterest.Add).GetComponentInChildren<TextMeshProUGUI>()));

                //Show the new possibility
                FadeItem(i, FadePriority.MarkSelection, Fade.FadeIn, FadePattern.FadeInAndOut, new float[] { 0.5f, 1f }, COLOR_CAN_EQUIP);

                //Move item
                if(slot.occupant != null)
                {
                    slot.occupant.inventoryItem.visualItem.position = i.visualItem.position;
                }
            }
        }

        
    }
    // Moves attachment slot (inner method)
    private void MoveAttachmentSlot(Item i,Vector2 p, Vector2 scale, AttachmentSlot slot){
        i.visualItem.position =
                    slotToItem[MechEquippedMatrixSlotToEquipmentSlot(p)].visualItem.position
                    + new Vector3(
                        SLOT_WIDTH * scale.x - slot.diameter * SLOT_WIDTH * scale.x / 2,
                        -SLOT_HEIGHT * scale.y + slot.diameter * SLOT_HEIGHT * scale.y / 2);
    }*/
    //Marks differences after a change
    private void MarkEquippedDifferences(InventoryBlockType[,] comparison)
    {
        InventoryBlock[,] eq = owner.mech.equippedCoreMatrix;

        if (comparison.GetLength(0) == eq.GetLength(0) && comparison.GetLength(1) == eq.GetLength(1))
        {
            int width = eq.GetLength(0);
            int height = eq.GetLength(1);
            for(int x = 0; x < width; x++){
                for(int y = 0; y < height; y++){
                    if(eq[x,y].type != comparison[x, y])
                    {
                        Item slot = slotToItem[MechEquippedMatrixSlotToEquipmentSlot(new Vector2(x, y))];
                        float[] fadeTime = new float[] { 0.1f + x * 0.1f + y * 0.1f, 0.1f + (width-x) * 0.1f + (height-y) * 0.1f };

                        if (eq[x,y].type == InventoryBlockType.Blocked)
                        {
                            FadeItem(slot, FadePriority.StickyMarking, Fade.FadeIn, FadePattern.FadeInAndOut, fadeTime, COLOR_DID_BLOCK);
                        }
                        else if (eq[x, y].type == InventoryBlockType.Connected)
                        {
                            FadeItem(slot, FadePriority.StickyMarking, Fade.FadeIn, FadePattern.FadeInAndOut, fadeTime, COLOR_CAN_EQUIP);
                        }
                    }
                }
            }
        }
        else
        {
            Debug.Log("[MarkEquippedDifferences] Invalid comparison");
        }
    }
    //Unequips a core if possible
    private bool UnequipCore(Core core)
    {
        if(onMouseItem == null)
        {
            List<MechItem> blockers = owner.mech.UnequipCore(core);
           // InventoryBlockType[,] oldMatrix = owner.mech.SnapShotEquippedMatrixTypes();
            if (blockers == null)
            {
                int width = owner.mech.legs.widthCapacity + 2;
                int height = owner.mech.legs.heightCapacity + 1;
                
                //Update new blocktypes
                UpdateBlockTypes(width, height);

                //Update attachments
                //UpdateAttachmentSpots();

             //   MarkEquippedDifferences(oldMatrix);
                return true;
            }
            else
            {
                //Show why we can't take it off
                foreach(MechItem c in blockers)
                {
                    SignalBlock(c, FadePriority.MarkSelection,false);
                }
                SignalBlock(core, FadePriority.MarkSelection, true, false);
            }
        }
        else
        {
            SignalBlock(onMouseItem, FadePriority.MarkSelection);
        }
        return false;
    }
    //Unequips an attachment, this should always be possible
   /* private bool UnequipAttachment(Attachment att)
    {
        return owner.mech.UnequipAttachment(att);
    }*/
    //Converts inventory handlers equipment slot to mech equipment slot
    private Vector2 EquipmentSlotToMechEquippedMatrixSlot(Vector3 pos)
    {
        return new Vector2(pos.x, owner.mech.equippedCoreMatrix.GetLength(1) - 1 - pos.y);
    }
    //Converts mech equipment slot to the one used by inventory handler
    private Vector3 MechEquippedMatrixSlotToEquipmentSlot(Vector2 pos)
    {
        return new Vector3(pos.x, owner.mech.equippedCoreMatrix.GetLength(1) - 1 - pos.y,(int)InventoryType.Equipment);
    }
    //Moves all items that have been moved due to changing legs
    private void RecalculatePositionOfEquipmentItems() { 

        foreach(Core core in owner.mech.equippedCores)
        {
            Vector2 pos = owner.mech.equippedCores[core];

            Vector3 slotPos = MechEquippedMatrixSlotToEquipmentSlot(pos);

            Vector3 newSlotPos = new Vector3(
                     slotPos.x - core.equipSlotOffset.x,
                     slotPos.y + core.equipSlotOffset.y,
                     (int)InventoryType.Equipment);

            core.inventoryItem.visualItem.position = slotToItem[newSlotPos].visualItem.position;
        }
    }
    //Hides all equipment slots (inverse of creating)
    private void HideEquipmentSlots(int width, int height, float legSlotWidth, float legSlotHeight, Vector3 legPos)
    {
        RemoveSlot(legslot, InventoryType.Equipment, -1, -1);
        for (int y = height - 1; y >= 0; y--)
        {
            for (int x = 0; x < width; x++)
            {
                RemoveSlot(slotToItem[new Vector3(x, y, (int)InventoryType.Equipment)], InventoryType.Equipment,x,y);
            }
        }
    }
    //Shows all equipment slots
    private void ShowEquipmentSlots(int width, int height, float legSlotWidth, float legSlotHeight, Vector3 legPos)
    {

        
        legslot = CreateEquipmentSlot(
            legPos,
            legSlotWidth,
            legSlotHeight);

        legslot.GetPointOfInterest(PointOfInterest.Add).gameObject.SetActive(true);
        RegisterSlot(legslot, InventoryType.Equipment, SLOT_LEG_X, SLOT_LEG_Y);
        itemToFader.Add(legslot, 
            new Fader(legslot.GetPointOfInterest(PointOfInterest.Selection).GetComponent<Image>()));

        //Create normal item matrix
        for (int y = height - 1; y >= 0; y--)
        {
            for (int x = 0; x < width; x++)
            {
                Item slot = CreateEquipmentSlot(
                    EQUIPMENT_CENTER_POS
                    //Size dependant
                    + new Vector3(-SLOT_WIDTH * width / 2, SLOT_HEIGHT * height / 2)
                    //Position
                    + new Vector3(x * SLOT_WIDTH, -y * SLOT_HEIGHT)
                    //Move up depeneding on leg size
                    + new Vector3(0, legSlotHeight / 2)
                    , SLOT_WIDTH
                    , SLOT_HEIGHT);

                /*InventoryBlockType ibt = owner.mech.equippedMatrix[owner.mech.equippedMatrix.GetLength(0)-1-x, owner.mech.equippedMatrix.GetLength(1)-1-y].type;
                if (ibt == InventoryBlockType.Blocked)
                {
                    slot.GetPointOfInterest(PointOfInterest.Remove).gameObject.SetActive(true);
                }else if(ibt == InventoryBlockType.Connected)
                {
                    slot.GetPointOfInterest(PointOfInterest.Add).gameObject.SetActive(true);
                }*/

                //Debug.Log(slot.GetPointOfInterest(PointOfInterest.Add).GetComponentInChildren<TextMeshProUGUI>() == null);


                itemToFader.Add(slot, new Fader(
                    slot.GetPointOfInterest(PointOfInterest.Selection).GetComponent<Image>()
                    ,slot.GetPointOfInterest(PointOfInterest.Add).GetComponentInChildren<TextMeshProUGUI>()));
                //slot.visualItem.gameObject.AddComponent<InventoryItemOver>().SetItem(this, slot, InventoryType.Equipment);
                RegisterSlot(slot, InventoryType.Equipment, x, y);
            }
        }
    }
    //Updates +/- when an item has changed
    private void UpdateBlockTypes(int width, int height)
    {
        //Create normal item matrix
        for (int y = height - 1; y >= 0; y--)
        {
            for (int x = 0; x < width; x++)
            {
                Vector3 pos = new Vector3(x, y, (int)InventoryType.Equipment);
                Item slot = slotToItem[pos];
                Vector2 eqSlot = EquipmentSlotToMechEquippedMatrixSlot(pos);

                InventoryBlockType ibt = owner.mech.equippedCoreMatrix[(int)eqSlot.x,(int)eqSlot.y].type;

                if (ibt == InventoryBlockType.Blocked)
                {
                    slot.GetPointOfInterest(PointOfInterest.Add).gameObject.SetActive(true);
                    slot.GetPointOfInterest(PointOfInterest.Add).GetComponentInChildren<TextMeshProUGUI>().text = BLOCK;
                }
                else if (ibt == InventoryBlockType.Connected)
                {
                    slot.GetPointOfInterest(PointOfInterest.Add).gameObject.SetActive(true);
                    slot.GetPointOfInterest(PointOfInterest.Add).GetComponentInChildren<TextMeshProUGUI>().text = ADD;
                }
                else
                {
                    slot.GetPointOfInterest(PointOfInterest.Add).gameObject.SetActive(false);
                    slot.GetPointOfInterest(PointOfInterest.Add).GetComponentInChildren<TextMeshProUGUI>().text = VACANT;
                }
            }
        }
    }
    //Creates the equipment panel, called by constructor
    private void PopulateEquipment()
    {
        int width = owner.mech.legs.widthCapacity + 2; //owner.mech.legs.widthCapacity + 2; //2 padded left and right for -
        int height = owner.mech.legs.heightCapacity + 1; //owner.mech.legs.heightCapacity + 1;

        //Add legs
        float legSlotWidth = owner.mech.legs.GetInventorySize().x* SLOT_WIDTH;
        float legSlotHeight = owner.mech.legs.GetInventorySize().y * SLOT_HEIGHT;

        Vector3 legPos = EQUIPMENT_CENTER_POS + new Vector3(-legSlotWidth / 2, -SLOT_HEIGHT * height / 2) + new Vector3(0, legSlotHeight / 2);

        //Show slots
        ShowEquipmentSlots(width, height, legSlotWidth, legSlotHeight, legPos);
        UpdateBlockTypes(width, height);

        //Show leg
        owner.mech.legs.ShowInventoryVisual(Global.References[SceneReferenceNames.PanelEquipmentItem], SLOT_WIDTH, SLOT_HEIGHT);//Global.instance.PANEL_EQUIPMENT_ITEM
        owner.mech.legs.inventoryItem.visualItem.localPosition = legPos;
        itemToFader.Add(owner.mech.legs, new Fader(owner.mech.legs.inventoryItem.GetPointOfInterest(PointOfInterest.Selection).GetComponent<Image>()));

        //Show equipped items
        foreach(Core core in owner.mech.equippedCores)
        {
            Vector3 pos = MechEquippedMatrixSlotToEquipmentSlot(owner.mech.equippedCores[core]);

            Vector3 newSlotPos = new Vector3(
              pos.x - core.equipSlotOffset.x,
              pos.y + core.equipSlotOffset.y,
              (int)InventoryType.Equipment);

            core.ShowInventoryVisual(Global.References[SceneReferenceNames.PanelEquipmentItem], SLOT_WIDTH, SLOT_HEIGHT);
            core.inventoryItem.visualItem.position = slotToItem[newSlotPos].visualItem.position;
            itemToFader.Add(core, new Fader(core.inventoryItem.GetPointOfInterest(PointOfInterest.Selection).GetComponent<Image>()));

            //core.inventoryItem.visualItem.transform.SetParent(Global.References[SceneReferenceNames.PanelEquipmentItem]); //Global.instance.PANEL_EQUIPMENT_ITEM);


        }


    }
    //Create the inventory panel, called by constructor
    private void PopulateInventory()
    {

        inventoryMatrix = new MechItem[INVENTORY_COLUMNS, INVENTORY_ROWS];

        //Create empty slots
        for (int y = INVENTORY_ROWS-1; y >= 0; y--)
        {
            for (int x = 0; x < INVENTORY_COLUMNS; x++)
            {
                //Show empty slot
                Item slot =  Global.Resources[ItemNames.InventorySlot]; //Global.instance.INVENTORY_OPEN_SLOT.Clone(); // 
                owner.itemEquiper.Equip(slot);
                slot.Show(Global.References[SceneReferenceNames.PanelInventorySlot]); //Global.instance.PANEL_INVENTORY_SLOT);
                slot.visualItem.localPosition = INVENTORY_INIT_POS + new Vector3(x*SLOT_WIDTH, -y*SLOT_HEIGHT);
                slot.visualItem.GetComponent<RectTransform>().sizeDelta = new Vector2(SLOT_WIDTH, SLOT_HEIGHT);

                //Checks for mouse over
                //slot.visualItem.gameObject.AddComponent<InventoryItemOver>().SetItem(this, slot, InventoryType.Inventory);

                //Save for easy lookup
                RegisterSlot(slot, InventoryType.Inventory, x, y);

                //Create faders
                itemToFader.Add(slot, new Fader(slot.GetPointOfInterest(PointOfInterest.Selection).GetComponent<Image>()));

            }
        }

        //Add items on top
        foreach (MechItem mi in owner.inventory)
        {
            //Create visual element
            mi.ShowInventoryVisual(Global.References[SceneReferenceNames.PanelInventoryItem], SLOT_WIDTH, SLOT_HEIGHT); //Global.instance.PANEL_INVENTORY_ITEM

            //Adds to matrix for easy overlap check
            AddToInventoryMatrix(mi, owner.inventory[mi]);
            mi.inventoryItem.visualItem.position = slotToItem[owner.inventory[mi]].visualItem.position;

            //Add item dragger enabling us to drag items
           // mi.inventoryItem.visualItem.gameObject.AddComponent<InventoryItemDragger>().SetItem(this, mi);

            //Add selection fader
            itemToFader.Add(mi, new Fader(mi.inventoryItem.GetPointOfInterest(PointOfInterest.Selection).GetComponent<Image>()));
        }
    }
    //Used by update 
    private Item GetItemSlotInInventoryThatMouseIsOn()
    {
        return GetSlot(INVENTORY_ROWS, INVENTORY_COLUMNS, InventoryType.Inventory);
    }
    //Used by update
    private Item GetEquipmentSlotThatMouseIsOn()
    {
        //Try and find attachment
        Item i = null; //GetSlot(owner.mech.legs.heightCapacity + 1, owner.mech.legs.widthCapacity + 2, InventoryType.Attachment);
        if(i == null){
            //Try and find equipment
            i = GetSlot(owner.mech.legs.heightCapacity + 1, owner.mech.legs.widthCapacity + 2, InventoryType.Equipment);
            if (i == null)
            {
                //See if its the leg spot
                float legSlotWidth = owner.mech.legs.GetInventorySize().x * SLOT_WIDTH;
                float legSlotHeight = owner.mech.legs.GetInventorySize().y * SLOT_HEIGHT;
                Vector3 diff = Input.mousePosition - legslot.visualItem.position;

                if (diff.x > 0 && diff.y < 0)
                {
                    Vector2 scale = GetEquipmentPanelScaleFactor();
                    if (diff.x < legSlotWidth * scale.x && diff.y > -legSlotHeight * scale.y)
                    {
                        return legslot;
                    }
                }
            }
        }

        return i;
    }
    //Gets the local scale factor. Temporary solution
    private Vector2 GetEquipmentPanelScaleFactor()
    {
        Vector3 initpos = slotToItem[new Vector3(0, 0, (int)InventoryType.Equipment)].visualItem.position;
        Vector3 lastpos = slotToItem[new Vector3(1, 1, (int)InventoryType.Equipment)].visualItem.position;

        float width = (lastpos.x - initpos.x);
        float height = -(lastpos.y - initpos.y);

        return new Vector2(width / SLOT_WIDTH, height /SLOT_HEIGHT);

    }
    //Returns the slot that the item is over
    private Item GetItemSlotThatMouseIsOn()
    {
        Item i = GetItemSlotInInventoryThatMouseIsOn();
        if(i == null)
        {
            i = GetEquipmentSlotThatMouseIsOn();
        }
        return i;
    }
    //Inner method of GetItemSlotThatMouseIsOn
    private Item GetSlot(int rows, int columns, InventoryType thisType)
    {
        InventoryType type = /*thisType == InventoryType.Attachment ? InventoryType.Equipment :*/ thisType;

        if(type == InventoryType.Socketable) { return null;  }

        Vector3 initpos = slotToItem[new Vector3(0, 0, (int)type)].visualItem.position;
        Vector3 lastpos = slotToItem[new Vector3(columns - 1, rows - 1, (int)type)].visualItem.position;

        float width = (lastpos.x - initpos.x) / (columns - 1);
        float height = -(lastpos.y - initpos.y) / (rows - 1);

       // Debug.Log(width/SLOT_WIDTH);

        Vector3 diff = Input.mousePosition - initpos;
        Vector3 pos = new Vector3(Mathf.FloorToInt(diff.x / width), Mathf.FloorToInt(-diff.y / height),(int)type);
       
        if (slotToItem.Contains(pos))
        {
            //Check for attachments
            /*if (thisType == InventoryType.Attachment)
            {
                foreach(Vector2 attPos in owner.mech.attachmentSlots)
                {
                    //Get slot
                    AttachmentSlot slot = owner.mech.attachmentSlots[attPos];
                    //Diameter to item if possible
                    int diameter = slot.occupant == null ? slot.diameter : slot.occupant.diameter;
                    //Position amongst items
                    Vector3 thisPos = new Vector3(attPos.x, attPos.y);//MechEquippedMatrixSlotToEquipmentSlot(attPos);
                    thisPos.z = (int)InventoryType.Attachment;
                    //Actual position of item
                    Item attSlot = slotToItem[thisPos];
                    Vector3 visPos = attSlot.visualItem.position;
                    //Get difference
                    Vector3 diffAtt = Input.mousePosition - visPos;

                    //Check if within square
                    if(diffAtt.x > 0 && diffAtt.x < width * diameter
                        && diffAtt.y < 0 && diffAtt.y > -height * diameter)
                    {
                        if (CheckIsWithinSlantedSquare(diffAtt, diameter, width, height))
                        {
                            return attSlot;
                        }

                    }

                }
            }
            else
            { */
                return slotToItem[pos];
           // }

        }
        return null;
    }
    //Checks attachments slots
    private bool CheckIsWithinSlantedSquare(Vector3 P, int diameter, float width, float height)
    {

        //Check top left square
        float Ax = 0;
        float Ay = -height*diameter*0.5f;

        float Bx = width*diameter*0.5f;
        float By = 0;

        if((P.x - Ax) * (By - Ay) - (P.y - Ay) * (Bx - Ax) < 0)
        {
            return false;
        }

        //Check top right square
        Ax = Bx;
        Ay = By;

        Bx = width * diameter;
        By = -height*diameter*0.5f;

        if ((P.x - Ax) * (By - Ay) - (P.y - Ay) * (Bx - Ax) < 0)
        {
            return false;
        }

        //Check bottom right square
        Ax = Bx;
        Ay = By;

        Bx = width * diameter*0.5f;
        By = -height * diameter;

        if ((P.x - Ax) * (By - Ay) - (P.y - Ay) * (Bx - Ax) < 0)
        {
            return false;
        }

        //Check bottom left square
        Ax = Bx;
        Ay = By;

        Bx = 0;
        By = -height * diameter * 0.5f;

        if ((P.x - Ax) * (By - Ay) - (P.y - Ay) * (Bx - Ax) < 0)
        {
            return false;
        }

        //Vector2 A = new Vector2(width * 0.5f, 0);
        //Vector2 B = new Vector2(0, -height * 0.5f);

        return true;
    }
    //Removes item from the inventory matrix
    private void RemoveFromInventoryMatrix(MechItem mi)
    {
        for (int x = 0; x < INVENTORY_COLUMNS ; x++)
        {
            for (int y = 0; y < INVENTORY_ROWS ; y++)
            {
                if(inventoryMatrix[x,y] == mi)
                {
                    inventoryMatrix[x, y] = null;
                }
            }
        }
    }
    //Adds mech item to the inventory matrix
    private void AddToInventoryMatrix(MechItem mi, Vector2 pos)
    {
        for (int x = (int)pos.x; x < pos.x + mi.inventoryWidth; x++)
        {
            for (int y = (int)pos.y; y < pos.y + mi.inventoryHeight; y++)
            {
                inventoryMatrix[x, y] = mi;
            }
        }
    }
    //Returns yes if we can place an item within bounds of the inventory matrix
    private bool CanPlaceInInventoryMatrix(MechItem mi, Vector2 position)
    {
        if(position.x+mi.inventoryWidth > INVENTORY_COLUMNS
           ||
           position.y+mi.inventoryHeight > INVENTORY_ROWS)
        {
           // Debug.Log("Can't place at, out of bounds: " + position);
            return false;
        }
        return true;
    }
    //Returns all mech items in the inventory matrix that collides with the mech item in the argument. Detects collision
    private List<MechItem> GetAllItemsUnder(MechItem mi, Vector2 pos)
    {
        List<MechItem> ret = new List<MechItem>();

        for (int x = (int)pos.x; x < pos.x + mi.inventoryWidth; x++)
        {
            for (int y = (int)pos.y; y < pos.y + mi.inventoryHeight; y++)
            {
                if (inventoryMatrix[x, y] != null)
                {
                    if (!ret.Contains(inventoryMatrix[x, y]) && inventoryMatrix[x,y] != mi)
                    {
                        ret.Add(inventoryMatrix[x, y]);
                    }
                }
            }
        }
        return ret;
    }
    //Puts a non-showing item into the inventory
    public bool PutIntoInventory(MechItem mi)
    {
        for (int y = INVENTORY_ROWS - 1; y >= 0; y--)
        {
            for (int x = 0; x < INVENTORY_COLUMNS; x++)
            {
                Vector2 pos = new Vector2(x, y);
                if (CanPlaceInInventoryMatrix(mi, pos))
                {
                    if(GetAllItemsUnder(mi,pos).Count == 0)
                    {
                        owner.itemEquiper.Equip(mi);
                        mi.ShowInventoryVisual(Global.References[SceneReferenceNames.PanelInventoryItem], SLOT_WIDTH, SLOT_HEIGHT);
                        itemToFader.Add(mi, new Fader(mi.inventoryItem.GetPointOfInterest(PointOfInterest.Selection).GetComponent<Image>()));

                        MechItem temp = onMouseItem;
                        onMouseItem = mi;
                        PlaceItem(null, slotToItem[new Vector3(x, y, (int)InventoryType.Inventory)], null, MechItemClass.NotMechItem);
                        onMouseItem = temp;
                        return true;
                    }
                }
            }
        }
        return false;
    }
    //Debug method to visualize matrix
    private void DebugPrintMatrix()
    {
        for (int y = 0; y < INVENTORY_ROWS; y++)
        {
            string row = "";
            for (int x = 0; x < INVENTORY_COLUMNS; x++)
            {
                string s = "<X>";
                if(inventoryMatrix[x,y] != null)
                {
                    s = "<"+inventoryMatrix[x, y].itemName.Substring(0, 1)+">";
                }
                row += s;
            }
            Debug.Log(row);
        }
    }
    //Used to indicate invalid movement within inventory and equipment panels
    private void SignalBlock(MechItem mi, FadePriority prio, bool sound = true, bool fade = true)
    {
        if (sound)
        {
            //Global.instance.AUDIO_INVENTORY
            inventoryAudio.PlaySound(SoundWhen.Blocked, source, false);
        }
        if (fade)
        {
            FadeItem(mi, prio, Fade.FadeIn, FadePattern.FadeInAndOut, new float[] { 0.5f, 1 }, BLOCKED_COLOR);
        }
    }
    //Tries to place an item. Picks up items if there is only 1 item blocking
    private bool TryPlace(MechItem clickedMechItem, Item clickedSlot, Item clickedSocket, MechItemClass mechClass)
    {
        if(clickedSlot != null)
        {
            if (!PlaceItem(clickedMechItem, clickedSlot, clickedSocket, mechClass))
            {
                MechItem mi = onMouseItem;
                if (mi != null)
                {
                    Vector3 pos = itemToSlot[clickedSlot];

                    if (clickedSocket == null && (InventoryType)pos.z == InventoryType.Inventory && CanPlaceInInventoryMatrix(mi, pos))
                    {
                        List<MechItem> list = GetAllItemsUnder(mi, pos);

                        if (list.Count == 1)
                        {
                            onMouseItem = null;
                            PickUpItem(list[0],null, InventoryType.Inventory);
                            MechItem temp = onMouseItem;

                            onMouseItem = mi;
                            PlaceItem(clickedMechItem, clickedSlot, clickedSocket, mechClass);

                            PickUpItem(temp,null, InventoryType.Inventory);
                            return true;
                        }
                        else
                        {
                            foreach (MechItem blocked in list)
                            {
                                SignalBlock(blocked, FadePriority.Hover);
                                //Global.instance.AUDIO_INVENTORY.PlaySound(SoundWhen.Blocked, source, false);
                                //FadeItem(blocked, FadePriority.Marking, Fade.FadeIn, FadePattern.FadeInAndOut, new float[] { 0.5f, 1 }, BLOCKED_COLOR);
                            }
                        }
                    }
                }
            }
            else
            {
                return true;
            }
        }
        return false;
    }
    //Places an item if possible. Cannot pick up items if something is blocking
    private bool PlaceItem(MechItem clickedItem, Item clickedSlot, Item clickedSocket, MechItemClass mechClass)
    {
        Vector3 pos = itemToSlot[clickedSlot];
        MechItem mi = onMouseItem;
        bool didPlace = false;

        if(mi != null)
        {
            InventoryType type = (InventoryType)pos.z;
            MechItemClass onMouseClass = MechItem.GetClass(mi);
            bool socketed = false;

            //Equipping legs
            if (type == InventoryType.Equipment && mechClass == MechItemClass.Leg){
                if (onMouseClass == MechItemClass.Leg){
                    didPlace = EquipLegs(mi);
                }
                else
                {
                    Debug.Log("Trying to equip something other than legs in a leg slot");
                    SignalBlock(mi, FadePriority.MarkSelection);
                }
            }
            //Socket
            else if (clickedSocket != null && clickedItem != null && onMouseClass == MechItemClass.Socketable)
            {
                
                didPlace = clickedItem.SocketItem(clickedSocket, (Socketable)onMouseItem);
                if (didPlace)
                {
                    //Debug.Log("Socketing: " + onMouseItem.uniqueItemName + " into: " + clickedSlot.uniqueItemName);
                    onMouseItem.inventoryItem.Hide();
                    //Remove faders while socketed
                    activeFaders.Remove(itemToFader[onMouseItem]);
                    itemToFader.Remove(onMouseItem);

                    socketed = true;
                    onMouseItem = null;
                }
                else
                {
                    SignalBlock(mi, FadePriority.MarkSelection);
                }
            }
            //Equipping core
            else if (type == InventoryType.Equipment)
            {
                if(mechClass == MechItemClass.NotMechItem && onMouseClass == MechItemClass.Core)
                {
                    Debug.Log("Placing core in equipment: " + mi.itemName);
                    didPlace = EquipCore(mi, slotToItem[pos]);
                }
                else
                {
                    SignalBlock(mi, FadePriority.MarkSelection);
                }
            }//Equipping Attachment
            /*else if (type == InventoryType.Attachment) 
            {
                if(mechClass == MechItemClass.NotMechItem && onMouseClass == MechItemClass.Attachment)
                {
                    Debug.Log("Placing attachment in equipment: " + mi.itemName);
                    didPlace = EquipAttachment((Attachment)mi, slotToItem[pos]);
                }else{
                    SignalBlock(mi, FadePriority.MarkSelection);
                }
            }*/
            else if (type == InventoryType.Inventory && CanPlaceInInventoryMatrix(mi, pos)){
                List<MechItem> list = GetAllItemsUnder(mi, pos);
                if (list.Count == 0){
                    owner.inventory.Add(mi, pos);
                    AddToInventoryMatrix(mi, pos);
                    mi.inventoryItem.visualItem.position = slotToItem[pos].visualItem.position;
                    mi.inventoryItem.visualItem.transform.SetParent(Global.References[SceneReferenceNames.PanelInventoryItem]); //Global.instance.PANEL_INVENTORY_ITEM);
                    onMouseItem = null;
                    didPlace = true;
                }
            }
            if (didPlace)
            {
                PlacedItem(mi,socketed);
            }
        }
        return didPlace;
    }

    private void PlacedItem(MechItem mi, bool socketed)
    {
        //Update camera positon
        UpdateCameraPosition();

        //Fade out selection
        if (!socketed)
        {
            FadeItem(mi, FadePriority.Selection, Fade.FadeOut, FadePattern.FadeAndStay, 1, SELECTION_COLOR);

            mi.inventoryItem.GetPointOfInterest(PointOfInterest.Glow).gameObject.SetActive(false);
            mi.inventoryItem.GetPointOfInterest(PointOfInterest.GlowSmall).gameObject.SetActive(false);
        }

        //Try to play specific sound first
        if (!(mi.sounds != null && mi.sounds.PlaySound(SoundWhen.PutDown, source, false)))
        {
            //Default sound
            inventoryAudio.PlaySound(SoundWhen.PutDown, source, false);
        }

        //Hide suggestions
        HidePossibleEquipLocations();

        //Remove markers
        ReturnMarkers();
    }

    public void DropItem()
    {
        onMouseItem.Drop();
        PlacedItem(onMouseItem, false);
        RemoveFaderFor(onMouseItem);
        onMouseItem.inventoryItem.Hide();
        onMouseItem = null;
    }
    //Picks up items and puts it on mousepointer
    private bool PickUpItem(MechItem clickedItem, Item clickedSocket, InventoryType it)
    {
        if (onMouseItem == null && clickedItem != null)
        {
            bool pickedUp = false;
            bool unsocketed = false;

            //Pick up socketables
            if(clickedSocket != null && clickedItem != null)
            {
                MechItem socketed = clickedItem.Unequip(clickedSocket);
                if(socketed != null)
                {
                    clickedItem = socketed;

                    //If the inventoryitem has not been created yet
                    clickedItem.ShowInventoryVisual(Global.References[SceneReferenceNames.PanelOnMouseItem], SLOT_WIDTH, SLOT_HEIGHT); // Global.instance.PANEL_ON_MOUSE

                    //Add fader
                    itemToFader.Add(clickedItem, new Fader(clickedItem.inventoryItem.GetPointOfInterest(PointOfInterest.Selection).GetComponent<Image>()));
                    //clickedItem.inventoryItem.Show(Global.instance.PANEL_ON_MOUSE);

                    //If the item was disabled
                    clickedItem.inventoryItem.Enable();

                    pickedUp = true;
                    unsocketed = true;
                }
            }

            //Pick up cores
            if(!pickedUp && it == InventoryType.Equipment && MechItem.GetClass(clickedItem) == MechItemClass.Core){

                pickedUp = UnequipCore((Core)clickedItem);
            //Pick up attachments
            }
            /*else if(!pickedUp && it == InventoryType.Attachment && MechItem.GetClass(clickedItem) == MechItemClass.Attachment)
            {
                pickedUp = UnequipAttachment((Attachment)clickedItem);
            //Pick up other
            }*/else{
                pickedUp = true;
            }

            if (pickedUp)
            {
                //Update camera positon
                UpdateCameraPosition();

                //Remove from inventory and put on mousepointer
                clickedItem.inventoryItem.visualItem.transform.SetParent(Global.References[SceneReferenceNames.PanelOnMouseItem]); //Global.instance.PANEL_ON_MOUSE);

                //Remove from inventory
                if (it == InventoryType.Inventory && !unsocketed)
                {
                    RemoveFromInventoryMatrix(clickedItem);
                    owner.inventory.Remove(clickedItem);
                }

                //Set on pointer
                onMouseItem = clickedItem;

                //Fade in selection
                FadeItem(clickedItem, FadePriority.Selection, Fade.FadeIn, FadePattern.Pulse, new float[] { 1, 2 }, SELECTION_COLOR);

                //Play sound
                //Global.instance.AUDIO_INVENTORY

                //Try to play specific sound first
                if (!(onMouseItem.sounds != null && onMouseItem.sounds.PlaySound(SoundWhen.PickUp, source, false)))
                {
                    //Default sound
                    inventoryAudio.PlaySound(SoundWhen.PickUp, source, false);
                }

                if(clickedItem.inventoryWidth == 1 || clickedItem.inventoryHeight == 1)
                {
                    clickedItem.inventoryItem.GetPointOfInterest(PointOfInterest.GlowSmall).gameObject.SetActive(true);
                }
                else
                {
                    clickedItem.inventoryItem.GetPointOfInterest(PointOfInterest.Glow).gameObject.SetActive(true);
                }

                //Make equipping easier for cores
                if (MechItem.GetClass(clickedItem) == MechItemClass.Core)
                {
                    //Set mouse offset relative to mousepointer
                    Vector2 scale = GetEquipmentPanelScaleFactor();
                    onMouseEquipOffset =
                        new Vector3(
                          -(clickedItem.equipSlotOffset.x + 0.5f) * SLOT_WIDTH * scale.x
                        , -(clickedItem.equipSlotOffset.y - 0.5f) * SLOT_HEIGHT * scale.y);

                    //Create marker
                    CreateMarkers((Core)clickedItem, scale);

                    //Show equip locations
                    ShowPossibleEquipLocations((Core)clickedItem);
                }
                else
                {
                    onMouseEquipOffset = Vector3.zero;
                }

                return true;
            }
           
        }
        return false;
    }
    //Returns mouse on markers to our pool of markers
    public void ReturnMarkers()
    {
        foreach(Item i in onMouseMarkers)
        {
            i.Disable();
            availableMarkers.Push(i);
        }
        onMouseMarkers.Clear();
    }
    //Pools markers and displays +/- next around the item
    public void CreateMarkers(Core core, Vector2 scale)
    {
        InventoryBlockType[,] ibt = core.inventorySpace;
        Vector2 originPoint = core.connection;

        for(int x = 0; x < ibt.GetLength(0); x++)
        {
            for(int y = 0; y < ibt.GetLength(1); y++)
            {
                CreateMarker(core, scale, new Vector2(x, y), ibt[x, y], originPoint);
            }
        }
    }
    //Creates marker that is shown when holding an item on mousepointer
    private void CreateMarker(Core core, Vector2 scale, Vector2 offset, InventoryBlockType ibt, Vector2 originPoint)
    {
        Item marker = availableMarkers.Pop();
        marker.Enable();

        if (offset.x == originPoint.x && offset.y == originPoint.y)
        {
            marker.GetPointOfInterest(PointOfInterest.Origin).gameObject.SetActive(true);
            marker.GetPointOfInterest(PointOfInterest.Positive).gameObject.SetActive(false);
            marker.GetPointOfInterest(PointOfInterest.Negative).gameObject.SetActive(false);
        }
        else if (ibt == InventoryBlockType.Connected)
        {
            marker.GetPointOfInterest(PointOfInterest.Origin).gameObject.SetActive(false);
            marker.GetPointOfInterest(PointOfInterest.Positive).gameObject.SetActive(true);
            marker.GetPointOfInterest(PointOfInterest.Negative).gameObject.SetActive(false);
        }
        else if (ibt == InventoryBlockType.Blocked)
        {
            marker.GetPointOfInterest(PointOfInterest.Origin).gameObject.SetActive(false);
            marker.GetPointOfInterest(PointOfInterest.Positive).gameObject.SetActive(false);
            marker.GetPointOfInterest(PointOfInterest.Negative).gameObject.SetActive(true);
        }
        else if (ibt == InventoryBlockType.Vacant || ibt == InventoryBlockType.Occupied)
        {
            marker.Disable();
        }

        onMouseMarkers.Add(marker, new Vector3(
                  (offset.x- originPoint.x+ core.equipSlotOffset.x) * SLOT_WIDTH  * scale.x
                  
                , (offset.y- originPoint.y+ core.equipSlotOffset.y) * SLOT_HEIGHT * scale.y));

    }
    //Hides equip locations when a core is put down
    private void HidePossibleEquipLocations()
    {
        foreach(Item i in currentPossibleEquipSlots)
        {
            FadeItem(i, FadePriority.Marking, Fade.FadeOut, FadePattern.FadeAndStay, 0.5f, COLOR_CAN_EQUIP);
        }
        currentPossibleEquipSlots.Clear();
    }
    //Shows equip locations when a core is picked up
    private void ShowPossibleEquipLocations(Core core)
    {
        int width = owner.mech.legs.widthCapacity + 2; //owner.mech.legs.widthCapacity + 2; //2 padded left and right for -
        int height = owner.mech.legs.heightCapacity + 1; //owner.mech.legs.heightCapacity + 1;


        for (int y = height - 1; y >= 0; y--)
        {
            for (int x = 0; x < width; x++)
            {
                Vector3 pos = new Vector3(x, y, (int)InventoryType.Equipment);
                Item slot = slotToItem[pos];
                Vector2 eqSlot = EquipmentSlotToMechEquippedMatrixSlot(pos);
                if (owner.mech.CanEquipCore(core, owner.mech.equippedCoreMatrix[(int)eqSlot.x, (int)eqSlot.y]))
                {
                    FadeItem(slot, FadePriority.Marking, Fade.FadeIn, FadePattern.Pulse, new float[] { 1, 2 }, COLOR_CAN_EQUIP);
                    currentPossibleEquipSlots.AddIfNotContains(slot);
                }
            }
        }
    }
    // Used to set the fade of an item
    private void FadeItem(Item item, FadePriority prio, Fade fade, FadePattern pattern, float time, Color c)
    {
        FadeItem(item, prio, fade, pattern, new float[] { time }, c);
    }
    //Als used for setting the fade of an item
    private void FadeItem(Item item, FadePriority prio, Fade fade, FadePattern pattern, float[] time, Color c)
    {
        if (!itemToFader.Contains(item))
        {
            Debug.Log("Missing: " + item.itemName);
        }
        itemToFader[item].SetFade(prio, fade, pattern, time, c);
        if (!activeFaders.Contains(itemToFader[item]) && fade != Fade.FadeOut)
        {
            activeFaders.Add(itemToFader[item]);
        }
    }
    //Shows or hides sockets
    public void ShowHideSockets(MechItem mi, bool show = true) {
        if (mi.sockets != null)
        {
            foreach (int i in mi.sockets)
            {
                Item socket = mi.sockets[i].slot;

                if (!itemToFader.Contains(socket))
                {
                    itemToFader.Add(socket, new Fader(socket.visualItem.GetComponent<Image>()));
                }

                if (show)
                {
                   FadeItem(socket, FadePriority.Visibility, Fade.FadeIn, FadePattern.FadeAndStay, 0.3f, 
                       MechItem.SOCKET_COLORS[mi.sockets[i].type]
                       );
                }
                else
                {
                   FadeItem(socket, FadePriority.Visibility, Fade.FadeOut, FadePattern.FadeAndStay, 1, 
                       MechItem.SOCKET_COLORS[mi.sockets[i].type]);
                }
            }
        }
    }
    //Detects if we are hovering a socket in a item
    public Item GetHoveredSocket(MechItem mi)
    {
        if(mi.sockets != null)
        {
            Vector3 mPos = Input.mousePosition;
            Vector3 scale = GetEquipmentPanelScaleFactor();

            foreach (int k in mi.sockets)
            {
                Item i = mi.sockets[k].slot;
                float size = (mi.sockets[k].type == SocketType.Crystal || mi.sockets[k].type == SocketType.Rifle) ? MechItem.SOCKET_CRYSTAL_SIZE : MechItem.SOCKET_OTHER_SIZE;
                Vector3 offSet = mPos - i.visualItem.position;

                if (offSet.x > -scale.x *  size / 2
                    && offSet.x < scale.x * size / 2
                    && offSet.y < scale.y * size / 2
                    && offSet.y > -scale.y * size / 2)
                {
                    return i;
                }
            }
        }
        return null;
    }
    //Method for showing tooltip
    public void ShowTooltip(MechItem clickedItem, Item clickedSlot, Item clickedSocket, InventoryType type)
    {
        int width = 500;

        currentTooltipMoveDuration = 0;
        tooltip.Show(clickedItem);

        tooltip.tooltip.visualItem.position = clickedItem.inventoryItem.visualItem.position;

        if(type == InventoryType.Inventory)
        {
            tooltip.tooltip.visualItem.localPosition += new Vector3(-width / 2, 0);
        }
        else
        {
            tooltip.tooltip.visualItem.localPosition += new Vector3(clickedItem.inventoryWidth * SLOT_WIDTH+width / 2, 0);
        }
        originalTooltipPosition = tooltip.tooltip.visualItem.localPosition;
        moveToTooltipPosition = originalTooltipPosition + new Vector3(0, TOOLTIP_MOVE_UP_DISTANCE);

        //Global.instance.AUDIO_INVENTORY
        inventoryAudio.PlaySound(SoundWhen.Tooltip, source, false);
    }

    //MENU TRIGGERS
    //Plays sound when opening and closing menues. Called from menusystem (delegate, event)
    public void OnMenuChange(MenuGroup m, bool show)
    {
        if(m == MenuGroup.Inventory)
        {
            if (show)
            {
                //Global.instance.AUDIO_INVENTORY
                inventoryAudio.PlaySound(SoundWhen.InventoryOpen, source, false);
                showingInventoryMenu = true;
            }
            else
            {
                //Global.instance.AUDIO_INVENTORY
                inventoryAudio.PlaySound(SoundWhen.InventoryClose, source, false);
                showingInventoryMenu = false;
            }
        }

        if (m == MenuGroup.Equipment)
        {
            if (show)
            {
                //Global.instance.AUDIO_INVENTORY
                inventoryAudio.PlaySound(SoundWhen.EquipmentOpen, source, false);
                showingEquipmentMenu = true;
            }
            else
            {
                //Global.instance.AUDIO_INVENTORY
                inventoryAudio.PlaySound(SoundWhen.EquipmentClose, source, false);
                showingEquipmentMenu = false;
            }
        }
    }
//ITEM TRIGGERS
    //Called when item is dragged within inventory
    /*public void OnBeginDrag(MechItem mi, InventoryType type, MechItemClass mechClass)
    {
        if(type == InventoryType.Equipment && mechClass == MechItemClass.Leg)
        {
            //Can't pick up legs from equipment
            Debug.Log("Can't pick up legs from equipment");
            SignalBlock(mi,FadePriority.Selection);
        }
        else
        {
            PickUpItem(mi,type);
        }
    }*/
    //Called on drag in inventory
    /*public void OnDrag(MechItem mi, InventoryType type, MechItemClass mechClass)
    {

    }*/
    //Called on click returns true if something was interacted with in the environment
    public bool OnPointerClick(bool clickedOnMenu, MechItem clickedItem, Item clickedSlot, Item clickedSocket, InventoryType type, MechItemClass mechClass)
    {
        
        if (!clickedOnMenu)
        {
            //Pick up item
            if (onMouseItem == null)
            {
                Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                RaycastHit hit;
                if (Physics.Raycast(ray, out hit, LayerMask.GetMask(Global.LAYER_DROPPED_ITEM)))
                {
                    if (hit.collider.GetComponent<MechItemDetached>() != null)
                    {
                        PutIntoInventory(hit.collider.GetComponent<MechItemDetached>().mechitem.PickUp());
                        return true;
                    }
                }
            }
            //Drop item
            else
            {
                DropItem();
                return true;
            }
            
        }
        else
        {
            if (onMouseItem == null && type == InventoryType.Equipment && mechClass == MechItemClass.Leg)
            {
                SignalBlock(clickedItem, FadePriority.Selection);
            }
            else if (!TryPlace(clickedItem, clickedSlot, clickedSocket, mechClass))
            {
                PickUpItem(clickedItem, clickedSocket, type);
            }
        }
        return false;


    }
    public void OnPointerHoverStart(MechItem clickedItem, Item clickedSlot, Item clickedSocket, InventoryType type)
    {
        ShowTooltip(clickedItem, clickedSlot, clickedSocket, type);
    }
    public void OnPointerHoverEnd()
    {
        if (tooltip.IsShowing)//currentTooltip.isEnabled)
        {
            //Global.instance.AUDIO_INVENTORY.PlaySound(SoundWhen.Tooltip, source, false);
            tooltip.Hide();
        }
    }
    //Called on end drag of mech item
    /*public void OnEndDrag(MechItem mi, InventoryType type, MechItemClass mechClass)
    {
        if (onMouseItem != null)
        {
            Item i = GetItemSlotThatMouseIsOn();
            if(i != null)
            {
                TryPlace(itemToSlot[i],mechClass);
            }
        }
    }*/
    // SLOT TRIGGERS
    //Called on slot click
    /*public void OnPointerClick(Item i,  InventoryType type, MechItemClass mechClass)
    {
        if (onMouseItem != null)
        {
            TryPlace(i, mechClass);
        }
    }*/
    //Called on mouse enter slot
    public void OnPointerEnter(Item item, InventoryType type, MechItemClass mechClass){

        //Set fade color
        Color c = mechClass == MechItemClass.NotMechItem ? COLOR_HOVER_SLOT_INVENTORY : COLOR_HOVER_ITEM;
        c = (type == InventoryType.Equipment /*|| type == InventoryType.Attachment*/) && mechClass == MechItemClass.NotMechItem ? COLOR_HOVER_SLOT_EQUIPMENT : c;

        //Add faders to sockets
        if (type == InventoryType.Socketable && mechClass == MechItemClass.NotMechItem)
        {
            if (!itemToFader.Contains(item))
            {
                itemToFader.AddIfNotContains(item, new Fader(item.visualItem.GetComponent<Image>()));
            }
            c = SOCKET_SELECTION_COLOR;
        }

        //Show sockets
        if (mechClass != MechItemClass.NotMechItem) //&& mechClass != MechItemClass.Socketable)
        {
            ShowHideSockets((MechItem)item);
        }

        //Fade item
        FadeItem(item, FadePriority.Hover, Fade.FadeIn, FadePattern.FadeAndStay, 0.1f, c);
    }
    //Called on mouse exit slot
    public void OnPointerExit(Item item, InventoryType type, MechItemClass mechClass)
    {
        //Set color
        Color c = mechClass == MechItemClass.NotMechItem ? COLOR_HOVER_SLOT_INVENTORY : COLOR_HOVER_ITEM;
        c = (type == InventoryType.Equipment /*|| type == InventoryType.Attachment*/) && mechClass == MechItemClass.NotMechItem ? COLOR_HOVER_SLOT_EQUIPMENT : c;

        float fadeoutTime = 0.5f;

        //Add faders to sockets
        if (type == InventoryType.Socketable && mechClass == MechItemClass.NotMechItem)
        {
            if (!itemToFader.Contains(item))
            {
                itemToFader.AddIfNotContains(item, new Fader(item.visualItem.GetComponent<Image>()));
            }
            c = SOCKET_SELECTION_COLOR;
            fadeoutTime = 1;
        }

        //Show sockets
        if (mechClass != MechItemClass.NotMechItem) //&& mechClass != MechItemClass.Socketable)
        {
            ShowHideSockets((MechItem)item, false);
        }

        //Fade out hover
        FadeItem(item, FadePriority.Hover, Fade.FadeOut, FadePattern.FadeAndStay, fadeoutTime, c);

    }


}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class PlayerSpawner : Spawner, Initiates{

    private Transform playerNode;
    private Global o;
    private GameUnit player;
    private Transform playerBody;
    private Transform playerShield;
    private Transform focusTransform;
    private FocusMovement cameraFocus;
    // private Camera cam;

    public void Start()
    {
        Initiate();
    }

    public void Initiate()
    {
        if (Global.IsAwake)
        {
            Spawn();
        }
        else
        {
            Global.initiates.AddIfNotContains(this);
        }
    }


    // Use this for initialization
    void Spawn () {

        //Debug.Log("Player spawner init");

        o = Global.instance;
        player = Global.PLAYER_STANDARD_SETUP.Clone();
        Global.AddInventoryHandler(player);

        SpawnMech(player, MechNames.MortarGunarrayMelee, Global.NAME_PLAYER_GAMEOBJECT, Global.LAYER_PLAYER);
       // player.AddAI<AIMech>();

        //Focus
        GameObject focus = GameObject.CreatePrimitive(PrimitiveType.Cube);
        focus.GetComponent<BoxCollider>().enabled = false;
        focus.name = Global.NAME_MAIN_FOCUS;
        focus.transform.position = this.transform.position;
        focus.transform.localScale += new Vector3(10, 10, -0.9f);
        focusTransform = focus.transform;
        cameraFocus = focus.AddComponent<FocusMovement>();
        cameraFocus.owner = player;
        //cameraFocus.playerBody = this.playerBody;
        focusTransform.parent = playerNode;

        //Camera
        GameObject.FindObjectOfType<CameraMovement>().target = focus.transform;


        //Health
        UIContainer uc = new UIContainer(HealthBarHandler.NAME_SELF,
            Global.References[SceneReferenceNames.PanelHealth],
            Global.Resources[PrefabNames.HealthBar]
            , FadeDirection.FadeLeft);
        uc.Show();
        HealthBarHandler healthHandler = uc[0].gameObject.AddComponent<HealthBarHandler>();
        healthHandler.owner = player;
        healthHandler.ui = uc;

        //player.buffHandler.AddBuff(player.mech.legs, Global.Resources[BuffNames.Conflux]); //Global.instance.BUFF_CONFLUX.Clone());

        //Player specific components

        //InventoryHandler i = Global.References[SceneReferenceNames.Main].gameObject.AddComponent<InventoryHandler>(); //Global.instance.MAIN
        //i.owner = player;

        //
        //player.AddToInventory(Global.Resources[CoreNames.Core], new Vector2(0, 5));
        //player.AddToInventory(Global.Resources[LegNames.TurretBase], new Vector2(1, 5));
        //player.AddToInventory(Global.Resources[CoreNames.TurretHead], new Vector2(3, 2)); //Global.Resources[LegNames.SpiderLegs].CloneWithNewSize(8, 8), new Vector2(3, 2));

        // player.AddToInventory(Global.Resources[CoreNames.MeleeBot], new Vector2(1, 1));
        //player.AddToInventory(Global.Resources[WeaponNames.EnergySword], new Vector2(5, 2));
        /*player.AddToInventory(Global.Resources[GunNames.TheDemonflame], new Vector2(0, 0));
        player.AddToInventory(Global.Resources[GunNames.TheLastFlame], new Vector2(2, 0));
        player.AddToInventory(Global.Resources[GunNames.TheThorProject], new Vector2(4, 0));
        player.AddToInventory(Global.Resources[GunNames.DoubleMissileLauncher], new Vector2(6, 0));

        player.AddToInventory(Global.Resources[WeaponNames.EnergySword], new Vector2(0, 2));
        player.AddToInventory(Global.Resources[WeaponNames.TheHive], new Vector2(2, 2));
        player.AddToInventory(Global.Resources[WeaponNames.TheSpineOfWinter], new Vector2(4, 2));
        player.AddToInventory(Global.Resources[CoreNames.MeleeBot], new Vector2(6, 2));

        player.AddToInventory(Global.Resources[GunArrayNames.GunArrayEightSlot], new Vector2(0, 4));
        player.AddToInventory(Global.Resources[GunNames.StandardRifle], new Vector2(2, 4));*/

        player.AddToInventory(Global.Resources[CoreNames.RearTurret], new Vector2(1, 1));
        player.AddToInventory(Global.Resources[CrystalNames.Crystal], new Vector2(3, 4));
        player.AddToInventory(Global.Resources[CoreNames.Core], new Vector2(4, 4));

        //Global.Inventory.PutIntoInventory(Global.Resources[GunArrayNames.GunArrayEightSlot]);


        //Add player controller
        player.AddAI<PlayerKeypresses>();

        /*//GameObject
        playerNode = new GameObject(Global.NAME_PLAYER_GAMEOBJECT).transform;
        playerNode.parent = this.transform;
        playerNode.position = this.transform.position;


        //Player Placeholder
        GameObject body = GameObject.CreatePrimitive(PrimitiveType.Cube);
        if (DevelopmentSettings.SHOW_PLAYER_BOUNDS)
        {
            body.GetComponent<Renderer>().material = Global.Resources[MaterialNames.Glass]; //Global.instance.MAT_GLASS;
        }
        else
        {
            body.GetComponent<Renderer>().enabled = false;
        }


        body.name = Global.NAME_PLAYER_GAMEOBJECT+Global.NAME_BODY;
        playerBody = body.transform;
        playerBody.parent = playerNode;
        playerBody.position = this.transform.position;
       // playerBody.Rotate(0, 90, 0);

        //Movement controller
        LegMovement mov = body.AddComponent<LegMovement>();
        body.AddComponent<CharacterController>();
        body.AddComponent<NavMeshObstacle>();
        mov.owner = player;
        player.body = playerBody;
        player.AddCommonComponents();

        //Mech creation
        player.mech = Global.Resources[MechNames.LegsWithCore, player, "Player mech"];

       






        //Focus
        GameObject focus = GameObject.CreatePrimitive(PrimitiveType.Cube);
        focus.GetComponent<BoxCollider>().enabled = false;
        focus.name = Global.NAME_MAIN_FOCUS;
        focus.transform.position = this.transform.position;
        focus.transform.localScale += new Vector3(10, 10, -0.9f);
        focusTransform = focus.transform;
        cameraFocus = focus.AddComponent<FocusMovement>();
        cameraFocus.owner = player;
        //cameraFocus.playerBody = this.playerBody;
        focusTransform.parent = playerNode;

        //Camera
        GameObject.FindObjectOfType<CameraMovement>().target = focus.transform;

        //cam = GameObject.Find(Global.NAME_MAIN_CAMERA).GetComponent<Camera>();
        //cam.GetComponent<CameraMovement>().target = focus.transform;

        //Shield
        playerShield = Instantiate(//o.P_FORCE_SHIELD
            Global.Resources[PrefabNames.ForceShield]
            , playerBody);
        playerShield.name = Global.NAME_PLAYER_GAMEOBJECT + Global.NAME_SHIELD;
        playerShield.localScale += new Vector3(0.2f, 0.2f, 0.2f); //Add some extra radius to be safe
        playerShield.parent = playerBody;
        ColliderOwner coShield = playerShield.gameObject.AddComponent<ColliderOwner>();
        coShield.owner = player;

        //Layer
        Global.SetLayerOfThisAndChildren(Global.LAYER_PLAYER, playerNode.gameObject);
        Global.SetLayerOfThisAndChildren(Global.LAYER_SHIELDS, playerShield.gameObject);


        //UI
        //Health
        UIContainer uc = new UIContainer(HealthBarHandler.NAME_SELF,
            Global.References[SceneReferenceNames.PanelHealth],
            //Global.instance.PANEL_HEALTH, 
            Global.Resources[PrefabNames.HealthBar]
            //Global.instance.UI_HEALTH_BAR
            ,FadeDirection.FadeLeft);
        uc.Show();
        HealthBarHandler healthHandler = uc[0].gameObject.AddComponent<HealthBarHandler>();
        healthHandler.owner = player;
        healthHandler.ui = uc;

        player.buffHandler.AddBuff("Test", Global.Resources[BuffNames.Conflux]); //Global.instance.BUFF_CONFLUX.Clone());

        //Stats
        player.stats.AddStat(Stat.JumpForce, 1);

        //Player specific components
        Global.AddInventoryHandler(player);
        //InventoryHandler i = Global.References[SceneReferenceNames.Main].gameObject.AddComponent<InventoryHandler>(); //Global.instance.MAIN
        //i.owner = player;

        //
        player.AddToInventory(Global.Resources[CoreNames.Core], new Vector2(0, 5));
        player.AddToInventory(Global.Resources[LegNames.TurretBase], new Vector2(1, 5));
        player.AddToInventory(Global.Resources[CoreNames.TurretHead], new Vector2(3, 2)); //Global.Resources[LegNames.SpiderLegs].CloneWithNewSize(8, 8), new Vector2(3, 2));
        */

        //Shows threat levels (For debug purposes)
        if (DevelopmentSettings.SHOW_THREAT_LEVELS) { 
            GameObject threatPreferred = GameObject.CreatePrimitive(PrimitiveType.Cube);
            threatPreferred.GetComponent<BoxCollider>().enabled = false;
            threatPreferred.transform.localScale = new Vector3(AISquad.DISTANCE_PREFERRED * 2, 0.5f, 0.5f);
            threatPreferred.transform.position = new Vector3(this.transform.position.x, this.transform.position.y, 3);
            threatPreferred.transform.parent = player.body.transform;
            Renderer rend = threatPreferred.GetComponent<Renderer>();
            rend.material.shader = Shader.Find("_Color");
            rend.material.SetColor("_Color", Color.green);
            rend.material.shader = Shader.Find("Specular");
            rend.material.SetColor("_SpecColor", Color.green);

            GameObject threatTooClose = GameObject.CreatePrimitive(PrimitiveType.Cube);
            threatTooClose.GetComponent<BoxCollider>().enabled = false;
            threatTooClose.transform.localScale = new Vector3(AISquad.DISTANCE_TOO_CLOSE * 2, 0.5f, 0.5f);
            threatTooClose.transform.position = new Vector3(this.transform.position.x, this.transform.position.y+0.1f, 2.9f);
            threatTooClose.transform.parent = player.body.transform;
            Renderer rendToo = threatTooClose.GetComponent<Renderer>();
            rendToo.material.shader = Shader.Find("_Color");
            rendToo.material.SetColor("_Color", Color.red);
            rendToo.material.shader = Shader.Find("Specular");
            rendToo.material.SetColor("_SpecColor", Color.red);

            GameObject threatLastStand = GameObject.CreatePrimitive(PrimitiveType.Cube);
            threatLastStand.GetComponent<BoxCollider>().enabled = false;
            threatLastStand.transform.localScale = new Vector3(AISquad.DISTANCE_LAST_STAND * 2, 0.5f, 0.5f);
            threatLastStand.transform.position = new Vector3(this.transform.position.x, this.transform.position.y + 0.2f, 2.8f);
            threatLastStand.transform.parent = player.body.transform;
            Renderer rendLast = threatLastStand.GetComponent<Renderer>();
            rendLast.material.shader = Shader.Find("_Color");
            rendLast.material.SetColor("_Color", Color.black);
            rendLast.material.shader = Shader.Find("Specular");
            rendLast.material.SetColor("_SpecColor", Color.black);
        }

       // Debug.Log("Player spawner end");
    }

  
}

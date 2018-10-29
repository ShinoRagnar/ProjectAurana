using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;


public enum GameEvent
{
    CaveEnter = 0,
    CaveExit = 1
}
public interface Initiates
{
    void Initiate();
}
public class Global : MonoBehaviour {

    public struct DelayedOnHitDamage
    {
        public GameUnit owner;
        public OnHitDelayedDamage delayed;
        public Vector3 position;
        public Vector3 forceOriginposition;


        public DelayedOnHitDamage(GameUnit owner, OnHitDelayedDamage delayed, Vector3 position, Vector3 forceOriginPosition)
        {
            this.forceOriginposition = forceOriginPosition;
            this.owner = owner;
            this.delayed = delayed;
            this.position = position;
        }
    }
    public struct InAirProjectile
    {
        public Vector3 muzzlePos;
        public Vector3 jointPos;
        public Vector3 targetPos;
   
        public Transform projectile;
        public Detonator detonator;
        public Bullet bullet;

        public InAirProjectile(
            Vector3 muzzlePos,
            Vector3 jointPos,
            Vector3 targetPos,
            Transform projectileVal,
            Detonator detonatorVal,
            Bullet bulletVal
            )
        {
            this.muzzlePos = muzzlePos;
            this.jointPos = jointPos;
            this.targetPos = targetPos;
            projectile = projectileVal;
            detonator = detonatorVal;
            bullet = bulletVal;
        }
    }

    //IDS
    //public static readonly int ABOVE_GROUND = 0;
    //public static readonly int UNDER_GROUND = 1;
    //public static int SHOWING = ABOVE_GROUND;
    //public static DictionaryList<int, SceneReferenceNames> GroundNodes = new DictionaryList<int, SceneReferenceNames>() { { ABOVE_GROUND, Sce} }
    public static SceneReferenceNames[] ALL_GROUNDS = new SceneReferenceNames[] { SceneReferenceNames.NodeAboveGround, SceneReferenceNames.NodeUnderground };
    public static SceneReferenceNames SHOWING = SceneReferenceNames.NodeAboveGround;


    //------- SELF

    public static Global instance;

    //Used fore sequential initilization
    public static ListHash<Initiates> initiates = new ListHash<Initiates>();
    public static ListHash<GameObject> removeMe = new ListHash<GameObject>();
    public static ListHash<InAirProjectile> projectiles = new ListHash<InAirProjectile>();
    public static DictionaryList<DelayedOnHitDamage,float> delayedDamage = new DictionaryList<DelayedOnHitDamage,float>();


   // public static DictionaryList<Transform, Ground> Grounds = new DictionaryList<Transform, Ground>();

    public static DictionaryList<SceneReferenceNames, DictionaryList<Transform, Ground>> Grounds = new DictionaryList<SceneReferenceNames, DictionaryList<Transform, Ground>>();
    public static DictionaryList<SceneReferenceNames, DictionaryList<Transform, Ground>> NonNavigateableGrounds = new DictionaryList<SceneReferenceNames, DictionaryList<Transform, Ground>>();



    public static bool IsAwake = false;

    public static List<Transform> BirdWaypoints = new List<Transform>();

    //Cursors
    public Texture2D defaultCursor;
    public Texture2D pickUpItemCursor;
    public Texture2D hoverItemCursor;
    public Texture2D targetCursor;
    public Texture2D socketCursor;

    //------- Mono Behaviours
    //References within the scene
    private SceneReferences sceneRefs;
    private InventoryHandler handler;
    //References to prefabs etc
    public Resources resources;
    public static DictionaryList<SceneReferenceNames, TerrainGenerator> Terrain = new DictionaryList<SceneReferenceNames, TerrainGenerator>();

    //Static version
    public static Resources Resources
    {
        get { return instance.resources;  }
    }
    public static SceneReferences References
    {
        get { return instance.sceneRefs; }
    }
    public static Console Console
    {
        get { return instance.console; }
    }
    public static InventoryHandler Inventory
    {
        get { return instance.handler;  }
    }

    //Menues
    public MenuSystem menues;
    private Console console;
    public static void AddInventoryHandler(GameUnit own)
    {
        instance.handler = References[SceneReferenceNames.Main].gameObject.AddComponent<InventoryHandler>();
        instance.handler.owner = own;
    }

    //Effects
    private ListHash<Effect> activeEffects = new ListHash<Effect>();
    public static void AddEffect(Effect e) { instance.activeEffects.AddIfNotContains(e); }
    //------- Random
    public System.Random rand = new System.Random();

    //------- Time
    public bool gameIsPaused;

    //------- Camera
    public static Vector3 CAMERA_DISTANCE = new Vector3(0, 5, -20);

    //Int and floats
    public static readonly float UI_DURATION_TIME_LENGTH = 85f;
    public static readonly float UI_DURATION_BIG_TIME_LENGTH = 265f;

    public static readonly float FLASH_TIME = 0.1f;


    // ---------------------- STRINGS
    //GameObjects
    public static readonly string NAME_MAIN_CAMERA = "Main Camera";
    public static readonly string NAME_TIME_BAR = "TimeDuration";
    public static readonly string NAME_MAIN_FOCUS = "CameraFocus";
    public static readonly string NAME_PLAYER_GAMEOBJECT = "Player";
    //String parts
    public static readonly string NAME_SHIELD = " Shield";
    public static readonly string NAME_BODY = " Body";
    //Layers

    public static readonly string LAYER_PROJECTILES = "Projectiles";
    public static readonly string LAYER_SHIELDS = "Shields";
    public static readonly string LAYER_GROUND = "Ground";
    public static readonly string LAYER_PLAYER = "Player";
    public static readonly string LAYER_ENEMY = "Enemy";
    public static readonly string LAYER_AVOIDANCE = "Avoidance";
    public static readonly string LAYER_DROPPED_ITEM = "DroppedItems";
    public static readonly string LAYER_DROPPED_ITEM_CORNER = "DroppedItemsCorners";
    public static readonly string LAYER_NO_INTERACTION = "No Interaction";
    public static readonly string LAYER_ONLY_GROUND_INTERACTION = "OnlyGroundInteraction";

    public static readonly string[] LAYERS_GAME_OBJECTS_NON_GROUND = new string[] {LAYER_ENEMY, LAYER_SHIELDS, LAYER_PLAYER };
    public static readonly string[] LAYERS_GAME_OBJECTS = new string[] { LAYER_GROUND, LAYER_ENEMY, LAYER_SHIELDS, LAYER_PLAYER };
    public static readonly string[] LAYERS_NONE_SHIELD_GAME_OBJECTS = new string[] { LAYER_GROUND, LAYER_ENEMY, LAYER_PLAYER };

    public JetPack JETPACK_STANDARD;


    //------------- STATICS --------------------
    //Stats
    public static readonly Stats PLAYER_STANDARD_STATS = new Stats(500,1000,1000,1,7,4,20,10);
    public static readonly Stats ENEMY_STANDARD_STATS = new Stats(300,100,100,2,7,10);
    public static readonly Stats ENEMY_MECH_STANDARD_STATS = new Stats(100,200, 1000, 1, 6, 4, 20, 10);

    //Senses
    public static readonly Senses STANDARD_HUMANOID_SENSES = new Senses(30, 20, 15, 15, 0.5f);
    public static readonly Senses STANDARD_ROBOT_SENSES = new Senses(30, 30, 30, 30, 0.25f);

    //Factions
    public static readonly Faction FACTION_PLAYER = new Faction("Player Faction");
    public static readonly Faction FACTION_ENEMY = new Faction("Enemy Faction", FACTION_PLAYER);

    //Enemies
    //public static Health ENEMY_SOLDIER_STANDARD_HEALTH = new Health(100, 200, 0, 1);
    public static readonly GameUnit ENEMY_SOLDIER_STANDARD = new GameUnit("Standard Enemy Soldier ",SceneReferenceNames.NodeAboveGround, FACTION_ENEMY, STANDARD_HUMANOID_SENSES,ENEMY_STANDARD_STATS, false);
    public static readonly GameUnit ENEMY_MECH = new GameUnit("Mortar Turret", SceneReferenceNames.NodeAboveGround, FACTION_ENEMY, STANDARD_ROBOT_SENSES, ENEMY_MECH_STANDARD_STATS, false);

    //Player
    //public static Health PLAYER_STANDARD_HEALTH = new Health(500, 1000, 1, 10);
    public static readonly GameUnit PLAYER_STANDARD_SETUP = new GameUnit("Player", SceneReferenceNames.NodeAboveGround, FACTION_PLAYER,/* PLAYER_STANDARD_HEALTH,*/ STANDARD_ROBOT_SENSES,PLAYER_STANDARD_STATS,/*, 1000, 1,*/true);

    //Conditions
    public static readonly ListHash<int> ALWAYS = new ListHash<int> { };
    public static readonly ListHash<int> CONDITION_MOVING_LEFT = new ListHash<int> { (int)Condition.MovingLeft };
    public static readonly ListHash<int> CONDITION_MOVING_RIGHT = new ListHash<int> { (int)Condition.MovingRight };


    public static Vector3 GetBirdWaypoint()
    {
        Vector3 ret = Vector3.zero;

        if(BirdWaypoints.Count > 0)
        {
            Transform t = BirdWaypoints[Random.Range(0, BirdWaypoints.Count)];
            ret.x = Random.Range(-t.localScale.x/2, t.localScale.x / 2) + t.position.x;
            ret.z = Random.Range(-t.localScale.y / 2, t.localScale.y / 2) + t.position.z;
            ret.y = Random.Range(-t.localScale.z / 2, t.localScale.z / 2) + t.position.y;
        }

        return ret;
    }

    
    //Dictionary
    //
    // private System.Collections.Generic.Dictionary<string, Transform> prefabs;



    //Startup
    public void Awake()
    {
        //Debug.Log("[Global] Awake started");

        instance = this;
        //Load console
        console = GetComponent<Console>();

        //Then load references
        this.sceneRefs = this.gameObject.GetComponent<SceneReferences>();
        this.sceneRefs.Init();
        //Only when this is done can we load resources
        this.resources.Init();
        //Menues can now pick up resources
        this.menues = this.gameObject.AddComponent<MenuSystem>();

       // SpawnEffects();

        CreateMenues();
        CreateItems();

        //Considered awake at this state
        IsAwake = true;

        //Now we can start anything that was waiting for this process to finish
        foreach(Initiates initiate in initiates)
        {
            initiate.Initiate();
        }

        SHOWING = SceneReferenceNames.NodeAboveGround;
        RestoreScene();

        Cursor.SetCursor(defaultCursor, Vector2.zero, CursorMode.Auto);

        //Debug.Log("[Global] Finished awaking with "+initiates.Count+" waiting objects");
    }

    ListHash<GameUnit> alreadyCollidedWith = new ListHash<GameUnit>();
    ListHash<Rigidbody> alreadyCollidedRigid = new ListHash<Rigidbody>();

    public void Update()
    {
        bool removed = false;

        foreach(Effect e in activeEffects)
        {
            e.Update();
        }

        foreach(DelayedOnHitDamage d in delayedDamage)
        {
            if(delayedDamage[d] > d.delayed.delay)
            {
                alreadyCollidedWith.Clear();
                alreadyCollidedRigid.Clear();

                if (DevelopmentSettings.SHOW_MARKERS)
                {
                    GameObject sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                    Destroy(sphere.GetComponent<SphereCollider>());
                    sphere.transform.position = d.position;
                    sphere.transform.localScale = new Vector3(d.delayed.radius, d.delayed.radius, d.delayed.radius);
                }

                Interaction.UnitsHit unitshit = Interaction.GetUnitsHitInRadius(d.position, d.delayed.radius, alreadyCollidedWith, alreadyCollidedRigid);

                foreach (GameUnit gu in unitshit.unitsHit)
                {
                    Interaction.Damage(d.owner, gu, ALWAYS, d.position, 1, d.delayed.damage, d.delayed.modifiers, d.delayed.impact, d.delayed.radius,true);
                    Interaction.TransferImpact(d.forceOriginposition, gu, d.owner.uniqueName, d.delayed.impact, d.delayed.radius);
                }
                foreach (Rigidbody r in unitshit.rigidBodiesHit)
                {
                    Interaction.TransferImpact(r, 1, d.position, d.delayed.impact, d.delayed.radius);
                }

                delayedDamage.RemoveLater(d);

            }
            else
            {
                delayedDamage[d] += Time.deltaTime;
            }
        }

        delayedDamage.Remove();

        foreach (GameObject go in removeMe)
        {
            Destroy(go);
            removed = true;
        }

        if (removed)
        {
            removeMe.Clear();
        }
        
    }


    public void Pause()
    {
        gameIsPaused = true;
        Time.timeScale = 0;
    }
    public void UnPause()
    {
        gameIsPaused = false;
        Time.timeScale = 1;
    }
    private void CreateMenues()
    {
        Create(
            //instance.MENU_EQUIPMENT, 
            Global.Resources[PrefabNames.MenuEquipment],
            Global.References[SceneReferenceNames.PanelStaticEquipment]); //instance.PANEL_EQUIPMENT_STATIC);
        Create(
            //instance.MENU_INVENTORY,
            Global.Resources[PrefabNames.MenuInventory],
            Global.References[SceneReferenceNames.PanelStaticInventory]); //instance.PANEL_INVENTORY_STATIC);
    }

    private void CreateItems()
    {
       
        
        instance.JETPACK_STANDARD = new JetPack("Jetpack Soldier Standard"
            , Resources[ItemNames.JetBeamStandardSoldierLeft] //JET_BEAMER_STANDARD_LEFT
            , Resources[ItemNames.JetBeamStandardSoldierRight] //JET_BEAMER_STANDARD_RIGHT//
            );
       
    }

    //Either returns an object from the object pool or instantiates the object in quesiton
    public static Transform PoolOrCreate(Transform prefab, Transform parent)
    {
        //TODO: Object pool
        return instance.InstantiateInner(prefab, parent);
    }

    public static Transform Create(Transform prefab, Transform parent)
    {
        return instance.InstantiateInner(prefab, parent);
    }

    public static void Remove(GameObject go)
    {
        Destroy(go);
    }

    public static void ShowOnly(SceneReferenceNames slot, bool hideTerrain)
    {
        Debug.Log("SHOWONLY Count: "+Terrain.Count);

        foreach(SceneReferenceNames nam in Terrain)
        {
            if(slot == nam)
            {
                Debug.Log("Showing only: " + nam + " hideTerrain: " + hideTerrain);

                Terrain[nam].Show(hideTerrain);
            }
            else
            {
                Debug.Log("Hiding: " + nam + " hideTerrain: " + hideTerrain);
                Terrain[nam].Hide(hideTerrain);
            }
        }
    }
    public static void RestoreScene(bool restoreTerrain = true)
    {
        ShowOnly(SHOWING,restoreTerrain);
    }

    public static void Enter(GameUnit u, SceneReferenceNames toEnter)
    {
        SHOWING = toEnter;
        RestoreScene();
        u.slot = SHOWING;
    }

    public static void TriggerEnter(GameUnit gu, GameObject from, GameEvent gameEvent)
    {
        if (gu.isPlayer && gameEvent == GameEvent.CaveEnter && SHOWING == SceneReferenceNames.NodeAboveGround)
        {
            Enter(gu, SceneReferenceNames.NodeUnderground);
        }
        else if(gu.isPlayer && gameEvent == GameEvent.CaveExit && SHOWING == SceneReferenceNames.NodeUnderground)
        {
            Enter(gu, SceneReferenceNames.NodeAboveGround);
        }

        

        Debug.Log("ENTER: "+gu.uniqueName + " caused event: " + gameEvent.ToString());
    }
    public static void TriggerExit(GameUnit gu, GameObject from, GameEvent gameEvent)
    {
        Debug.Log("EXIT: " + gu.uniqueName + " caused event: " + gameEvent.ToString());
    }

    public void Materialize(Item i, Transform parent)
    {
        //Debug.Log(i.itemName);
        if (i.prefab != null && !i.showing)
        {
            i.visualItem = Create(i.prefab, parent);
            i.visualItem.Rotate(new Vector3(i.alignment.rotX, i.alignment.rotY, i.alignment.rotZ));

            i.visualItem.localScale += new Vector3(i.alignment.scaleX, i.alignment.scaleY, i.alignment.scaleZ);
            i.visualItem.localPosition += new Vector3(i.alignment.x, i.alignment.y, i.alignment.z);
            i.showing = true;
        }
    }
    //Private class
    private Transform InstantiateInner(Transform prefab, Transform parent)
    {
        return Instantiate(prefab, parent);
    }

    public static void SetLayerOfThisAndChildren(string layer, GameObject go)
    {
        go.layer = LayerMask.NameToLayer(layer);
        foreach (Transform child in go.transform)
        {
            SetLayerOfThisAndChildren(layer, child.gameObject);
        }
    }
    public static Transform FindDeepChild(Transform aParent, string aName, bool debug = false)
    {
        if (debug)
        {
            Debug.Log(aParent.gameObject.name);
        }

        Transform result = null;

        try
        {
            result = aParent.Find(aName);
        }catch(System.Exception e)
        {
            Debug.Log(aName);
            
        }
        
        if (result != null)
            return result;
        foreach (Transform child in aParent)
        {
            result = FindDeepChild(child, aName, debug);
            if (result != null)
                return result;
        }
        return null;
    }

    public static void FindAllNames(ListHash<Transform> names, Transform aParent, bool findInnerNames = true)
    {
        foreach (Transform child in aParent)
        {
            names.AddIfNotContains(child);

            if (findInnerNames)
            {
                FindAllNames(names, aParent);
            }
        }
    }


    //Projectiles need to be global. They can't die with their creator
    public void LaunchProjectileAtTarget(Vector3 muzzlePos, Vector3 jointPos, Vector3 targetPos, Bullet bullet) //Transform from, Transform target, Bullet bullet, float gravity, float angle)
    {
        Transform projectile = bullet.GetAvailableProjectile(Global.References[SceneReferenceNames.NodeProjectiles]);
        Detonator detonator = projectile.GetComponent<Detonator>();

        InAirProjectile iap = new InAirProjectile(muzzlePos, jointPos, targetPos, projectile, detonator, bullet); //from, target, projectile, detonator, bullet, gravity, angle);
        projectiles.Add(iap);

        StartCoroutine(SimulateProjectile(iap)); //angle, gravity,bullet, from, target));
    }

    IEnumerator SimulateProjectile(InAirProjectile iap) //float firingAngle, float gravity, Bullet bullet, Transform myTransform, Transform target)
    {

        float maxFlightDuration = 10;
        float intendedFlightDuration = 1.5f; //distance/200;
        float elapsedTime = 0;
        float missileLauncherFactor = 10;

        if (iap.bullet.type == BulletType.MissileLauncher)
        {
            //float distance = Vector3.Distance(iap.muzzlePos, iap.targetPos);
            iap.projectile.position = iap.muzzlePos;
            iap.projectile.LookAt(iap.targetPos);

            while (elapsedTime < maxFlightDuration && !iap.detonator.hasDetonated)
            {
                elapsedTime += Time.deltaTime;
                iap.projectile.position += missileLauncherFactor * iap.projectile.forward * Time.deltaTime * intendedFlightDuration / 1;
                yield return null;
            }

        }
        else
        {
            Vector3 p0 = TurretTargeter.GetBezierPoint(0, iap.muzzlePos, iap.targetPos, iap.jointPos); //muzzle.position;
            Vector3 p1 = TurretTargeter.GetBezierPoint(1, iap.muzzlePos, iap.targetPos, iap.jointPos);//muzzle.position + (muzzle.position - joint.position).normalized * AIM_HIGH_POINT;
            Vector3 p2 = TurretTargeter.GetBezierPoint(2, iap.muzzlePos, iap.targetPos, iap.jointPos);//new Vector3(p1.x + (p3.x - p1.x) / 2, p1.y, p3.z);
            Vector3 p3 = TurretTargeter.GetBezierPoint(3, iap.muzzlePos, iap.targetPos, iap.jointPos);//target.position;

            iap.projectile.position = p0;

            float sampleRate = 0.02f;
            float gravity = 10f;
            Vector3 pointBefore = TurretTargeter.CalculateCubicBezierPoint(1 - sampleRate, p0, p1, p2, p3);
            Vector3 pointLast = TurretTargeter.CalculateCubicBezierPoint(1, p0, p1, p2, p3);
            float terminalSpeed = Vector3.Distance(pointBefore, pointLast) / (sampleRate * intendedFlightDuration);
            Vector3 diff = pointLast - pointBefore;
            float xSpeed = (diff.normalized * terminalSpeed).x;
            float ySpeed = (diff.normalized * terminalSpeed).y;
            float zSpeed = (diff.normalized * terminalSpeed).z;
            float minYSpeed = -22;

            while (elapsedTime < maxFlightDuration && !iap.detonator.hasDetonated)
            {
                elapsedTime += Time.deltaTime;

                if (elapsedTime < intendedFlightDuration)
                {
                    float t = elapsedTime / intendedFlightDuration;
                    iap.projectile.position = TurretTargeter.CalculateCubicBezierPoint(t, p0, p1, p2, p3);
                    iap.projectile.LookAt(
                        TurretTargeter.CalculateCubicBezierPoint(
                            (elapsedTime + Time.deltaTime) / intendedFlightDuration, p0, p1, p2, p3));
                }
                else
                {

                    if (ySpeed > minYSpeed)
                    {
                        ySpeed -= gravity * Time.deltaTime;
                    }
                    Vector3 move = new Vector3(xSpeed, ySpeed, zSpeed) * Time.deltaTime;
                    iap.projectile.LookAt(iap.projectile.position + move);
                    iap.projectile.position += move;
                }

                yield return null;
            }
        }

        if (!iap.detonator.hasDetonated)
        {
            iap.detonator.Detonate(null);
        }

        //If not taken by someone else
        if (projectiles.Contains(iap))
        {
            ReturnProjectile(iap.projectile,iap.bullet);
            projectiles.Remove(iap);
        }
    }

    public void ReturnProjectile(Transform proj, Bullet b)
    {
        StartCoroutine(WaitForParticleSystem(proj, b));
    }

    IEnumerator WaitForParticleSystem(Transform proj, Bullet b)
    {
        ParticleSystem[] ps = proj.GetComponentsInChildren<ParticleSystem>();

        if(ps != null)
        {
            foreach(ParticleSystem p in ps)
            {
                p.Stop();
            }
        }

        MeshRenderer[] mr = proj.GetComponentsInChildren<MeshRenderer>();

        if(mr != null)
        {
            foreach(MeshRenderer m in mr)
            {
                m.enabled = false;
            }
        }

        Light[] li = proj.GetComponentsInChildren<Light>();

        if (li != null)
        {
            foreach (Light l in li)
            {
                l.enabled = false;
            }
        }

        yield return new WaitForSeconds(b.waitBeforeParticleSystemFinishTime);

        if (ps != null)
        {
            foreach (ParticleSystem p in ps)
            {
                p.Play();
            }
        }
        if (mr != null)
        {
            foreach (MeshRenderer m in mr)
            {
                m.enabled = true;
            }
        }

        if (li != null)
        {
            foreach (Light l in li)
            {
                l.enabled = true;
            }
        }

        b.ReturnProjectile(proj);
    }

}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;
using TMPro;

public enum Rarity
{
    None = -1,
    Common = 10,
    Uncommon = 20,
    Rare = 30,
    Legendary = 40
   
}
public enum MechItemClass
{
    NotMechItem = -1,
    Core = 10,
    Leg = 20,
    Attachment = 30,
    Socketable = 40
}
public enum SocketType
{
    NoType = -1,
    Crystal = 10,
    Gun = 20,
    Rifle = 25,
    Melee = 30,
    Shield = 40
}
public enum DamageType
{
    Physical = 0,
    EMP = 5,
    Corruption = 10
}
public struct OnHitWhen
{
    public OnHit onHit; //onHit;
    public InteractionCondition[] conditions;

    public OnHitWhen(OnHit onHit, InteractionCondition[] conditions)
    {
        this.onHit = onHit;
        this.conditions = conditions;
    }

    public void HitIfConditions(MechItem source, GameUnit from, GameUnit target, Vector3 hitPos, Vector3 rotation,  Vector3 delayedDamageOffset, float damageDealt, bool groundHit)
    {
        if (Check(from, target, damageDealt, groundHit)){
            onHit.Hit(source, from, target, hitPos, rotation, delayedDamageOffset);
        }
    }

    public bool Check(GameUnit from, GameUnit target, float damageDealt, bool groundHit)
    {
        if(conditions == null) { return true;  }

        foreach (InteractionCondition condition in conditions)
        {
            if (!condition.IsSatisified(from, target, damageDealt,groundHit)) { return false; }
        }
        return true;
    }
}
//Used for corner collisions when detached
public interface CornerCollidable
{
    void CornerCollided(Vector3 pos, bool groundCollision);
}
public interface Socketable
{
    SocketType GetSocketType();
    Sprite GetSprite();
    Socketable CloneSocketable();
    SocketableNames GetName();
}
[Serializable]
public struct SocketSlotData
{
    public SocketType type;
    public SocketableNames occupant;
}
[Serializable]
public struct DamageModifier
{
    public InteractionCondition[] conditions;
    public float damageModifier;
    public float impactAddition;
    public float shakeAddition;
}
[Serializable]
public struct Damage
{
    public DamageType type;
    public float amount;

    public Damage(DamageType type, float amount) { this.type = type; this.amount = amount; }

    public float GetDamage(GameUnit owner, ListHash<int> conditions)
    {
        if(type == DamageType.Corruption)
        {
            return amount*owner.stats.GetCurrentValue(Stat.DamageDealt, conditions) + (1 - owner.stats.GetCurrentValue(Stat.CorruptionDamageDealt, conditions));
        }
        if (type == DamageType.EMP)
        {
            return amount*owner.stats.GetCurrentValue(Stat.DamageDealt, conditions) + (1 - owner.stats.GetCurrentValue(Stat.EMPDamageDealt, conditions));

        }
        return amount*owner.stats.GetCurrentValue(Stat.DamageDealt, conditions);
    }

}
public class MechItemDetached : MonoBehaviour
{
    public MechItem mechitem;
}
public class MechItem : Item, CornerCollidable {

    public Vector3 worldScale;

    public static readonly Vector3 ITEM_DROP_OFFSET = new Vector3(0, 4, -6);

    public class SocketSlot
    {
        public SocketType type;
        public Socketable occupant;
        public MechItem mirror;

        public Item slot;

        public SocketSlot(Item slotVal, SocketType typeVal)
        {
            this.type = typeVal;
            this.slot = slotVal;
        }
    }

    //Statics
    public static readonly DictionaryList<SocketType, Color> SOCKET_COLORS = new DictionaryList<SocketType, Color>(){
        {SocketType.Crystal, new Color(0.86f, 0.66f, 0.33f, 0.8f)},  //new Color(0.3f,0.3f,0.3f,0.8f) }, //new Color(0.86f, 0.66f, 0.33f, 0.5f)},
        {SocketType.Melee, new Color(0.8f, 0, 0, 0.8f)},
        {SocketType.Shield, new Color(1f, 1f, 1f, 0.8f)},
        {SocketType.Gun, new Color(0.58f, 0.52f, 0.62f, 0.8f)},
        {SocketType.Rifle, new Color(0.58f, 0.52f, 0.62f, 0.8f)}
    };

    public static readonly Color OUTLINE_DEFAULT_COLOR = new Color(0.86f, 0.66f, 0.33f, 0.8f);
    public static readonly Color SOCKET_DEFAULT_COLOR = new Color(0, 0, 0, 0);
    public static readonly float ROTATION_SCALING = 0.75f;
    public static readonly float DEFAULT_ROTATION = 45f;
    public static readonly float SOCKET_CRYSTAL_SIZE = 50;
    public static readonly float SOCKET_OTHER_SIZE = 100;
    public static readonly float CORNER_COLLISON_MIN_INTERVAL = 0.2f;
    public static readonly float OUTLINE_WIDTH = 3;
    public static readonly float TEXT_OVER_ITEM_EXTRA_DISTANCE = 1.5f;

    public static List<ListHash<int>> conditions = new List<ListHash<int>>();

    //Buff given by this item
    public List<Buff> buffs;
    public DamageModifier[] modifiers;
    public Damage[] damage;

    //Sockets
    public DictionaryList<int, SocketSlot> sockets;

    //Meshes
    public DictionaryList<Material, ListHash<MeshFilter>> combinedMeshes = new DictionaryList<Material, ListHash<MeshFilter>>();
    public DictionaryList<Material, MeshFilter> combinedNodes = new DictionaryList<Material, MeshFilter>();
    public DictionaryList<Material, MeshRenderer> combinedRenderers = new DictionaryList<Material, MeshRenderer>();

    //Targeters
    public DictionaryList<Transform, TurretTargeter> mountedTargeters = new DictionaryList<Transform, TurretTargeter>();
    public DictionaryList<Transform, Quaternion> originalRotation = new DictionaryList<Transform, Quaternion>();

    public DictionaryList<EffectWhen, Effect> effects;

    public DictionaryList<Renderer, Material> meshMaterials = new DictionaryList<Renderer, Material>();

    /*public DictionaryList<Item, int> socketsNumber;
    public DictionaryList<Item, SocketType> sockets;
    public DictionaryList<Item, Socketable> socketed;
    public DictionaryList<Item, Item> mirrorItems;*/
    public SocketType[] socketTypes;

    //Picture of the item
    public Sprite inventorySprite;
    //The inventory item, border and picture
    public Item inventoryItem;
    public Item reloadItem;
    public MechItem socketedIn;
    // Rarity of the item
    public Rarity rarity;

    //Width in units (not pixels)
    public float inventoryWidth = 1;
    //Height in units (not pixels)
    public float inventoryHeight = 1;
    //The offset to the connection point of the item (orange tinted +)
    public Vector2 equipSlotOffset = new Vector2(0, 0);
    //Rotation of the item frame
    public float rotation = 0;
    //If socketed items should be mirrored on the backside
    // public bool hasMirrorSockets = false;

    //Used when killed
    public Rigidbody rigidbody;
    public BoxCollider boxcollider;
    public CapsuleCollider capsulecollider;

    //public Transform textOverItem = null;

    public bool hasLandedOnce = false;

    public float lastCornerCollisonTime = 0;

    public string description = "No fluff";


    public MechItem(
       //Itemname
       string itemn,
       //The item prefab
       Transform item,
       //Rarity
       Rarity rarityVal,
       //Fluff
       string fluff,
       //Sockets
       SocketType[] emptySockets = null,
       //The picture prefab
       Sprite picture = null,
       //Buffs applied by this item
       List<Buff> appliesBuff = null,
       //Alignment (not used for scaling)
       Alignment alig = null,
       //Transform names on child items
       DictionaryList<PointOfInterest, string> pointsOfInterestPreShowingVal = null,
        //effecgts
        DictionaryList<EffectWhen, Effect> effectLibrary = null,
        //Sounds
        AudioContainer soundsContainer = null
        ) : base(itemn,item,alig,pointsOfInterestPreShowingVal)
    {
        //Sockets
        if (emptySockets != null)
        {
            sockets = new DictionaryList<int, SocketSlot>(); //new DictionaryList<Item, SocketType>();
            for (int i = 0; i < emptySockets.Length; i++)
            {
                this.sockets.Add(i, new SocketSlot(
                    null
                    , emptySockets[i])
                    
                    );
            }
            this.socketTypes = emptySockets;
        }
        this.buffs = appliesBuff ?? new List<Buff>();
        this.inventorySprite = picture;
        this.rarity = rarityVal;
        this.effects = effectLibrary ?? new DictionaryList<EffectWhen, Effect>();
        this.sounds = soundsContainer;
        this.description = fluff;
    }

    public static ListHash<int> GetConditions(Condition[] c)
    {
        foreach(ListHash<int> list in conditions)
        {
            if(c.Length == list.Count)
            {
                bool match = true;

                foreach (Condition con in c)
                {
                    if (!list.Contains((int)con))
                    {
                        match = false;
                        break;
                    }
                }
                if (match)
                {
                    return list;
                }
            }
        }
        ListHash<int> ret = new ListHash<int>();
        foreach(Condition con in c)
        {
            ret.Add((int)con);
        }
        conditions.Add(ret);
        return ret;
    }

    public static MechItemClass GetClass(MechItem i)
    {
        if(i is Socketable){
            return MechItemClass.Socketable;
        }else if (i is Legs){
            return  MechItemClass.Leg;
        }else if (i is Core){
            return MechItemClass.Core;
        }
        /*else if(i is Attachment){
            return MechItemClass.Attachment;
        }*/
        return MechItemClass.NotMechItem;
    }

    public void AddSocketsFromSocketData(SocketSlotData[] sockets)
    {
        if (sockets != null)
        {
            for (int i = 0; i < sockets.Length; i++)
            {
                if (sockets[i].occupant != SocketableNames.NothingSocketed)
                {
                    SocketItem(i, Global.Resources[sockets[i].occupant]);
                }
            }
        }
    }

    public int GetSocketAmountOfType(SocketType st)
    {
        if(sockets == null) { return 0; }

        int ret = 0;

        foreach(int s in sockets){
            if(sockets[s].type == st)
            {
                ret++;
            }
        }
        return ret;
    }

    public static SocketType[] SocketSlotDataToSocketTypeArray(SocketSlotData[] ssd)
    {
        if(ssd == null || ssd.Length == 0)
        {
            return null;
        }

        SocketType[] ret = new SocketType[ssd.Length];
        for(int i = 0; i < ret.Length; i++)
        {
            ret[i] = ssd[i].type;
        }
        return ret;
    }

    public void DetachAllSockets(ListHash<Rigidbody> detached)
    {
        if(sockets != null)
        {
            foreach (int i in sockets)
            {
                if (sockets[i].occupant != null)
                {
                    Unequip(i, true).DetachFromMech(detached);
                }
            }
        }
    }

    public virtual MechItem Unequip(Item socket, bool keepVisible = false)
    {
        foreach (int i in sockets)
        {
            if (sockets[i].slot == socket)
            {
                return Unequip(i, keepVisible);
            }
        }
        return null;
    }

    public virtual MechItem Unequip(int slot, bool keepVisible = false)
    {
        if (sockets[slot].occupant != null)
        {
            MechItem ret = HideSocket(slot, keepVisible);

            if(sockets[slot].slot != null && sockets[slot].slot.showing)
            {
                //Clear sprite
                sockets[slot].slot.GetPointOfInterest(PointOfInterest.Sprite).gameObject.SetActive(false);
                sockets[slot].slot.GetPointOfInterest(PointOfInterest.Sprite).GetComponent<Image>().sprite = null;
            }

            //Remove parent
            ret.socketedIn = null;

            sockets[slot].occupant = null;


            return ret;
        }
        
        return null;
    }

    public virtual bool SocketItem(Item socket, Socketable tosocket)
    {
        int slotNum = -1;

        foreach (int i in sockets)
        {
            if (sockets[i].slot == socket)
            {
                if (sockets[i].occupant != null)
                {
                    return false;
                }
                slotNum = i;
            }
        }
        if(SocketItem(slotNum, tosocket))
        {
           ShowSocket(slotNum);
            return true;
        }
        return false;
    }

    public void ShowSockets()
    {
        if(sockets != null)
        {
            foreach (int a in sockets)
            {
                if (sockets[a].occupant != null)
                {
                    ShowSocket(a);
                }
            }
        }
    }

   /* public void AimAndShootAt(GameUnit target, bool shoot, float actionspeed)
    {
       
    }*/

    private void Aim(Transform t, float actionspeed, Vector3 mousePosition, GameUnit target, Vector3 mousePositionOnZPlane)
    {
        //Get gun and targeting system
        TurretTargeter targ = mountedTargeters[t];
        Shootable g = targ.shootable;
      //  GameUnit target = targ.gameUnitTarget;
        
        //Try to shoot from muzzle
        Transform from = g.GetOriginPoint();//g.GetPointOfInterest(PointOfInterest.Muzzle);
        if (from == null)
        {
            from = t;
        }

        //Move joint, tick reload time etc
        //g.Tick(actionspeed);
        targ.AcquireTarget(target, mousePosition, mousePositionOnZPlane);
        targ.AdjustTarget(actionspeed, mousePosition, mousePositionOnZPlane);
        targ.AdjustJointTowardsTarget(actionspeed);
        //BulletType type = g.GetBulletType();

        if (targ.gunAnimator.canShoot)
        {
            if (targ.gunAnimator.skipOneFrame)
            {
                targ.gunAnimator.skipOneFrame = false;
            }
            /*else if (shoot && g.CanShoot())
            {
                g.Shoot();
                g.Reload();
                targ.gunAnimator.ResumeShooting();
            }*/
            else
            {
                targ.gunAnimator.EndShooting();
            }
        }
        else
        {
            targ.gunAnimator.ResumeShooting();
        }

        targ.ShowArc(from);

    }

    public void ShowSocket(int slotNum)
    {

        sockets[slotNum].slot.GetPointOfInterest(PointOfInterest.Sprite).gameObject.SetActive(true);
        sockets[slotNum].slot.GetPointOfInterest(PointOfInterest.Sprite).GetComponent<Image>().sprite = sockets[slotNum].occupant.GetSprite();
    }

    public virtual bool SocketItem(int slotNum, Socketable tosocket)
    {
        //Check for correct socket type
        if (sockets != null && sockets[slotNum].type == tosocket.GetSocketType() && sockets[slotNum].occupant == null)
        {
            //Add socketed
            sockets[slotNum].occupant = tosocket;
            //Get socket position
            PointOfInterest sockPos = GetSocketPointOfInterest(slotNum); //(PointOfInterest)(((int)PointOfInterest.SocketSlot1) + socketsNumber[socket]);
            //Cast to mechitem
            MechItem toSockItem = (MechItem)tosocket;
            //Equip
            owner.itemEquiper.Equip(toSockItem);
            //Show if self is showing
            if (showing)
            {
                bool isRifle = false;
                //Remove any character specific alignements from rifles
                if (tosocket is Gun && ((Gun)tosocket).isRifle)
                {
                    ((Gun)tosocket).alignment = Alignment.NO_ALIGNMENT;
                    isRifle = true;
                }

                toSockItem.Show(GetPointOfInterest(sockPos));

                //Targeter
                if (tosocket is Shootable && !isRifle)
                {
                    AddTargeter(slotNum, (Shootable)tosocket);
                }
            }

            //Save parent
            toSockItem.socketedIn = this;

            return true;
        }
        return false;
    }

    public void AddTargeter(int slotNum, Shootable tosocket)
    {
        Transform joint = GetPointOfInterest(GetSocketPointOfInterest(slotNum));
        originalRotation.AddIfNotContains(joint, joint.localRotation);
        joint.localRotation = originalRotation[joint];
        TurretTargeter targ = new TurretTargeter(owner, (Shootable)tosocket, joint);
        mountedTargeters.AddIfNotContains(joint, targ);
    }

    public PointOfInterest GetSocketPointOfInterest(int num)
    {
        return (PointOfInterest)(((int)PointOfInterest.SocketSlot1) + num);
    }

    public PointOfInterest GetSocketMirrorPointOfInterest(int num)
    {
        return (PointOfInterest)(((int)PointOfInterest.SocketMirrorSlot1) + num);
    }

    public MechItem PickUp()
    {

        Highlight hl = visualItem.gameObject.GetComponent<Highlight>();

        if(hl != null)
        {
            hl.Exit();
        }

        Global.Resources[EffectNames.CloudPickup].Spawn(visualItem.position, null, 0, 0, 0, (inventoryHeight + inventoryWidth) / 2);

        if (rarity == Rarity.Rare)
        {
            Global.Resources[EffectNames.PickupRare].Spawn(visualItem.position, null, 0, 0, 0, (inventoryHeight + inventoryWidth) / 2);
        }
        else if (rarity == Rarity.Legendary)
        {
            Global.Resources[EffectNames.PickupLegendary].Spawn(visualItem.position, null, 0, 0, 0, (inventoryHeight + inventoryWidth) / 2);
        }
        else{

            Global.Resources[EffectNames.PickupAny].Spawn(visualItem.position,null,0,0,0,(inventoryHeight+inventoryWidth)/2);
        }

        Hide();



        

        return this;
    }

    public void CornerCollided(Vector3 pos, bool isGround)
    {
        if (Time.time - lastCornerCollisonTime > CORNER_COLLISON_MIN_INTERVAL)
        {
            //Sound
            if(audioSource != null && sounds != null)
            {
                sounds.PlaySound(SoundWhen.CornerHit, audioSource, false);
            }
            else
            {
                Debug.Log("Corner of: " + uniqueItemName + " collided but missing audio!"+ (audioSource == null)+" - "+(sounds == null));
            }
            //Craters
            if (effects.Contains(EffectWhen.CraterCreation))
            {
                effects[EffectWhen.CraterCreation].Spawn(pos, null, 0, 0, 0, (inventoryWidth + inventoryHeight) / 2);
            }

            if (isGround && effects.Contains(EffectWhen.Crater))
            {
                effects[EffectWhen.Crater].Spawn(pos, null, -90, 0, 0, (inventoryWidth + inventoryHeight) / 4);
            }
        }

        //Show drop-ray
        if(isGround && !hasLandedOnce)
        {
            if (rarity == Rarity.Rare)
            {
                Global.Resources[EffectNames.RayRareDrop].Spawn(visualItem.position, null, -90);
            }
            else if (rarity == Rarity.Legendary)
            {
                Global.Resources[EffectNames.RayLegendaryDrop].Spawn(visualItem.position, null, -90);
            }
            hasLandedOnce = true;
        }

        lastCornerCollisonTime = Time.time;
    }

    
    public void Drop()
    {
        Show(Global.References[SceneReferenceNames.NodeDroppedItems], false);
        Detach();
        visualItem.position = owner.GetHeadPos();
        visualItem.localPosition += ITEM_DROP_OFFSET + new Vector3(0,worldScale.y*visualItem.localScale.y);
        visualItem.localRotation = UnityEngine.Random.rotation;
    }

    public override void Show(Transform parent, bool onMech = true)
    {
        //Base showing of item
        base.Show(parent,onMech);

        //Set default materials
        meshMaterials.Clear();

        MeshRenderer[] renderer = visualItem.GetComponentsInChildren<MeshRenderer>();
        foreach (MeshRenderer render in renderer)
        {
            if (render.sharedMaterial != null) { meshMaterials.AddIfNotContains(render, render.sharedMaterial); }
            if(DevelopmentSettings.HIDE_SHADOWS_ON_NONSHADONLY_OBJECTS && render.shadowCastingMode != UnityEngine.Rendering.ShadowCastingMode.ShadowsOnly)
            {
                render.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            }
        }
        SkinnedMeshRenderer[] skinnedRenderer = visualItem.GetComponentsInChildren<SkinnedMeshRenderer>();
        foreach (SkinnedMeshRenderer render in skinnedRenderer)
        {
            if (render.sharedMaterial != null) { meshMaterials.AddIfNotContains(render, render.sharedMaterial); }
            /*if (DevelopmentSettings.HIDE_SHADOWS_ON_NONSHADONLY_OBJECTS && render.shadowCastingMode != UnityEngine.Rendering.ShadowCastingMode.ShadowsOnly)
            {
                render.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            }*/
        }
        MeshRenderer selfRender = visualItem.GetComponent<MeshRenderer>();
        if (selfRender != null)
        {
            if (selfRender.sharedMaterial != null) { meshMaterials.AddIfNotContains(selfRender, selfRender.sharedMaterial); }
            if (DevelopmentSettings.HIDE_SHADOWS_ON_NONSHADONLY_OBJECTS && selfRender.shadowCastingMode != UnityEngine.Rendering.ShadowCastingMode.ShadowsOnly)
            {
                selfRender.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            }
        }
        SkinnedMeshRenderer selfSkinRender = visualItem.GetComponent<SkinnedMeshRenderer>();
        if (selfSkinRender != null)
        {
            if (selfSkinRender.sharedMaterial != null) { meshMaterials.AddIfNotContains(selfSkinRender, selfSkinRender.sharedMaterial); }

            /*if (DevelopmentSettings.HIDE_SHADOWS_ON_NONSHADONLY_OBJECTS && selfSkinRender.shadowCastingMode != UnityEngine.Rendering.ShadowCastingMode.ShadowsOnly)
            {
                selfSkinRender.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            }*/
        }

        //Combine gunarrays
        if(this is GunArray)
        {
            CombineMeshes();
        }

        //Add buffs
        if (onMech)
        {
            Interaction.TransferBuffs(this, owner, owner, buffs);
        }

        //World scale
        worldScale = GetWorldScale(visualItem);

        //Show sockets
        if (sockets != null)
        {
            foreach (int slotPos in sockets)
            {
                if (sockets[slotPos].occupant != null)
                {
                    MechItem i = (MechItem)sockets[slotPos].occupant;

                    bool isRifle = false;
                    //Remove any character specific alignements from rifles
                    if (i is Gun && ((Gun)i).isRifle)
                    {
                        ((Gun)i).alignment = Alignment.NO_ALIGNMENT;
                        isRifle = true;
                    }

                    if (!i.showing)
                    {
                        i.Show(GetPointOfInterest(GetSocketPointOfInterest(slotPos)), onMech);
                    }

                    //Targeter
                    if (onMech)
                    {
                        if (i is Shootable && !isRifle)
                        {
                            AddTargeter(slotPos, (Shootable)i);
                        }
                    }
                }
            }
        }

        //Turn off rigid bodies when attached to a mech
        rigidbody = visualItem.GetComponent<Rigidbody>();
        if (rigidbody != null)
        {
            rigidbody.detectCollisions = false;
        }
        else
        {
            Debug.Log("Missing rigid body: " + uniqueItemName);
        }

        boxcollider = visualItem.GetComponent<BoxCollider>();

        if (boxcollider != null)
        {
            boxcollider.enabled = false;
        }

        capsulecollider = visualItem.GetComponent<CapsuleCollider>();

        if (capsulecollider != null)
        {
            capsulecollider.enabled = false;
        }

        //Audio
        audioSource = visualItem.GetComponent<AudioSource>();

        if (audioSource == null)
        {
            audioSource = visualItem.gameObject.AddComponent<AudioSource>();
        }

        //Layer
        if (onMech)
        {
            if (owner.isPlayer)
            {
                Global.SetLayerOfThisAndChildren(Global.LAYER_PLAYER, visualItem.gameObject);
            }
            else
            {
                Global.SetLayerOfThisAndChildren(Global.LAYER_ENEMY, visualItem.gameObject);
            }
        }


        //Add reload frames
        if (owner.isPlayer && HasReload() && onMech)
        {
            //Debug.Log("Added reload frame for: " + itemName);

            Global.Inventory.AddReloadFrame(this, GetReloadGroup());
        }

    }

    public virtual void UncombineMeshes()
    {
        //Destroy the old nodes of combined meshes
        foreach (Material m in combinedNodes)
        {
            Global.Destroy(combinedNodes[m].gameObject);
        }
        combinedNodes.Clear();

        foreach (Material m in combinedMeshes)
        {
            foreach (MeshFilter f in combinedMeshes[m])
            {
                f.gameObject.SetActive(true);
            }
        }
        combinedMeshes.Clear();

        combinedRenderers.Clear();
    }

    public virtual void CombineMeshes()
    {

        UncombineMeshes();

        MeshRenderer[] renderer = visualItem.GetComponentsInChildren<MeshRenderer>();
        foreach (MeshRenderer r in renderer)
        {
            if (r.gameObject.activeSelf)
            {
                Material mat = r.sharedMaterial;

                if(r.shadowCastingMode != UnityEngine.Rendering.ShadowCastingMode.ShadowsOnly)
                {
                    if (!combinedMeshes.Contains(mat))
                    {
                        combinedMeshes.Add(mat, new ListHash<MeshFilter>());
                    }
                    combinedMeshes[mat].AddIfNotContains(r.transform.GetComponent<MeshFilter>());
                }
            }
        }

        //Create a transform for each material
        foreach (Material m in combinedMeshes)
        {
            GameObject go = new GameObject("Combined Material ("+itemName+"): " + m.name);
            MeshRenderer mr = go.AddComponent<MeshRenderer>();
            mr.sharedMaterial = m;
            mr.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            MeshFilter mf = go.AddComponent<MeshFilter>();
            mf.mesh = new Mesh();

            go.transform.parent = visualItem;

            combinedNodes.Add(m, mf);
            combinedRenderers.Add(m, mr);
        }

        foreach (Material m in combinedMeshes)
        {
            ListHash<MeshFilter> combined = combinedMeshes[m];
            CombineInstance[] combine = new CombineInstance[combined.Count];

            int i = 0;
            while (i < combined.Count)
            {
                combine[i].mesh = combined.Get(i).sharedMesh;
                combine[i].transform = combined.Get(i).transform.localToWorldMatrix;
                //combined.Get(i).gameObject.SetActive(false);
                i++;
            }

            MeshFilter toCombine = combinedNodes[m];
            toCombine.mesh.CombineMeshes(combine);
            toCombine.gameObject.SetActive(true);
        }

        //Hide previous
        foreach (Material m in combinedMeshes)
        {
            foreach (MeshFilter f in combinedMeshes[m])
            {
                f.gameObject.SetActive(false);
            }
        }

    }

    public virtual int GetReloadGroup()
    {
        return 0;
    }

    public void SetMaterialForAllRenderers(Material m)
    {
        if(meshMaterials != null)
        {
            foreach (Renderer mr in meshMaterials)
            {
                if (mr is MeshRenderer){
                    ((MeshRenderer)mr).sharedMaterial = m;
                }
                else if(mr is SkinnedMeshRenderer){
                    ((SkinnedMeshRenderer)mr).sharedMaterial = m;
                }
            }
        }
        if(sockets != null)
        {
            foreach(int i in sockets)
            {
                if(sockets[i].occupant != null)
                {
                    ((MechItem)sockets[i].occupant).SetMaterialForAllRenderers(m);
                }
            }
        }
        foreach(Material mat in combinedRenderers)
        {
            combinedRenderers[mat].sharedMaterial = m;
        }
    }

    public void ReturnAllRenderersToDefaultMaterial()
    {
        if (meshMaterials != null)
        {
            foreach (Renderer mr in meshMaterials)
            {
                if (mr is MeshRenderer)
                {
                    ((MeshRenderer)mr).sharedMaterial = meshMaterials[mr];
                }
                else if (mr is SkinnedMeshRenderer)
                {
                    ((SkinnedMeshRenderer)mr).sharedMaterial = meshMaterials[mr];
                }
            }
        }
        if (sockets != null)
        {
            foreach (int i in sockets)
            {
                if (sockets[i].occupant != null)
                {
                    ((MechItem)sockets[i].occupant).ReturnAllRenderersToDefaultMaterial();
                }
            }
        }
        foreach (Material mat in combinedRenderers)
        {
            combinedRenderers[mat].sharedMaterial = mat;
        }
    }

    public void Flash(Material m, float time)
    {
        Global.instance.StartCoroutine(FlashSelf(m, time));
    }
    

    public IEnumerator FlashSelf(Material m, float time)
    {
        SetMaterialForAllRenderers(m);
        yield return new WaitForSeconds(time);
        ReturnAllRenderersToDefaultMaterial();
    }

    public virtual void DetachFromMech(ListHash<Rigidbody> detached)
    {
        DetachAllSockets(detached);

        Detach();

        if (rigidbody != null)
        {
            detached.AddIfNotContains(rigidbody);
        }

        ShowDetachVisuals();

        hasLandedOnce = false;

    }

    public virtual void Detach()
    {
        visualItem.parent = Global.References[SceneReferenceNames.NodeDroppedItems];
        visualItem.gameObject.AddComponent<MechItemDetached>().mechitem = this;

        if(!(this is Legs) && combinedMeshes.Count == 0)
        {
            CombineMeshes();
        }

        Outline ol = visualItem.gameObject.GetComponent<Outline>();
        if(ol == null)
        {
            ol = visualItem.gameObject.AddComponent<Outline>();
        }
        else
        {
            ol.Reload();
        }
        
        ol.outlineColor = OUTLINE_DEFAULT_COLOR;
        ol.outlineWidth = OUTLINE_WIDTH;
        ol.enabled = false;
        Highlight hl = visualItem.gameObject.AddComponent<Highlight>();
        hl.outline = ol;
        hl.item = this;

        Global.SetLayerOfThisAndChildren(Global.LAYER_DROPPED_ITEM, visualItem.gameObject);


        if (rigidbody != null)
        {
            rigidbody.detectCollisions = true;
            rigidbody.isKinematic = false;
            rigidbody.useGravity = true;
        }
        else
        {
            Debug.Log("Missing rigid body: " + uniqueItemName);
        }

        CreateCorners();

       //AddTextOverItem();

        //Return materials
        ReturnAllRenderersToDefaultMaterial();

        hasLandedOnce = false;
    }

    public virtual void ShowDetachVisuals()
    {
        if (effects.Contains(EffectWhen.Detach))
        {
            Transform t = effects[EffectWhen.Detach].Spawn(Global.References[SceneReferenceNames.NodeClone]);
            t.position = visualItem.position;
            t.rotation = visualItem.rotation;
            t.localPosition += new Vector3(0, 0, boxcollider != null ? boxcollider.size.z : 1);

            Transform a = effects[EffectWhen.Detach].Spawn(Global.References[SceneReferenceNames.NodeClone]);
            a.position = visualItem.position;
            a.rotation = visualItem.rotation;
            a.localPosition += new Vector3(0, 0, boxcollider != null ? -boxcollider.size.z : -1);
            a.Rotate(0, 180, 0);
            //boxcollider != null ? -boxcollider.size.z / 2 : 
        }
    }

   /* public void AddTextOverItem()
    {
        //Add text
        textOverItem = Global.Create(GetTextPrefab(), Global.References[SceneReferenceNames.NodeDroppedItems]);
        textOverItem.GetComponent<AlwaysStraight>().follow = visualItem;
        textOverItem.GetComponent<TextMeshPro>().text = itemName;

        if (boxcollider != null)
        {
            textOverItem.GetComponent<AlwaysStraight>().distance = new Vector3(0, boxcollider.size.y * visualItem.localScale.y* TEXT_OVER_ITEM_EXTRA_DISTANCE);
        }
        else if (capsulecollider != null)
        {
            textOverItem.GetComponent<AlwaysStraight>().distance = new Vector3(0, capsulecollider.height * visualItem.localScale.y* TEXT_OVER_ITEM_EXTRA_DISTANCE);
        }
        textOverItem.gameObject.SetActive(false);
    }*/

    public Vector3 GetTextOffset()
    {
        if (boxcollider != null)
        {
            return new Vector3(0, boxcollider.size.y * visualItem.localScale.y * TEXT_OVER_ITEM_EXTRA_DISTANCE);
        }
        else if (capsulecollider != null)
        {
            return new Vector3(0, capsulecollider.height * visualItem.localScale.y * TEXT_OVER_ITEM_EXTRA_DISTANCE);
        }
        return Vector3.zero;
    }

    public void CreateCorners()
    {
        if (boxcollider != null || capsulecollider != null)
        {

            Transform corners = Global.Create(Global.Resources[PrefabNames.CornerCollisons], visualItem);
            corners.position = visualItem.position;

            if (boxcollider != null)
            {
                boxcollider.enabled = true;
                corners.localScale = new Vector3(boxcollider.size.x, boxcollider.size.y, boxcollider.size.z);
                corners.localPosition = new Vector3(
                    (boxcollider.size.x - 1) / 2,
                    (boxcollider.size.y - 1) / 2,
                    (boxcollider.size.z - 1) / 2
                    );

            }
            else if (capsulecollider != null)
            {
                capsulecollider.enabled = true;
                corners.localScale = new Vector3(capsulecollider.radius * 2, capsulecollider.height, capsulecollider.radius);
            }


            CornerCollisionDetector[] corn = corners.GetComponentsInChildren<CornerCollisionDetector>();

            foreach (CornerCollisionDetector c in corn)
            {
                c.receiver = this;
            }

            audioSource.spatialBlend = 1;
            audioSource.maxDistance = 30;
            audioSource.minDistance = 1;

            //Layer
            Global.SetLayerOfThisAndChildren(Global.LAYER_DROPPED_ITEM_CORNER, corners.gameObject);

        }
    }

    /*public Transform GetTextPrefab()
    {
        Transform prefab = Global.Resources[PrefabNames.ItemName];
        if (rarity == Rarity.Common)
        {
            prefab = Global.Resources[PrefabNames.ItemNameCommon];
        }else if (rarity == Rarity.Uncommon)
        {
            prefab = Global.Resources[PrefabNames.ItemNameUncommon];
        }else if (rarity == Rarity.Rare)
        {
            prefab = Global.Resources[PrefabNames.ItemNameRare];

        }else if (rarity == Rarity.Legendary)
        {
            prefab = Global.Resources[PrefabNames.ItemNameLegendary];
        }
        return prefab;
    }*/

    public override void Hide()
    {
        //Remove reload frames
        if (owner.isPlayer && HasReload())
        {
            Global.Inventory.RemoveReloadFrame(this);
        }

        Interaction.RetractStacks(this, owner, buffs);

        UncombineMeshes();

        //Hide sockets
        if (sockets != null)
        {
            foreach (int slotPos in sockets)
            {
                if(sockets[slotPos].occupant != null)
                {
                    HideSocket(slotPos);
                }
            }
        }

        //Clear targeters
       /* foreach(Transform t in mountedTargeters)
        {
            mountedTargeters[t].Hide();
        }
        mountedTargeters.Clear();*/

        //Hide the rest
        base.Hide();

        //No more meshes
        meshMaterials.Clear();
    }

    public MechItem HideSocket(int slotPos, bool keepVisible = false)
    {
        RemoveTargeter(slotPos);

        MechItem i = (MechItem)sockets[slotPos].occupant;

        if (!keepVisible)
        {
            i.Hide();
        }

        return i;
    }

    public void RemoveTargeter(int slotPos)
    {
        if (sockets[slotPos].occupant is Shootable)
        {
            if(sockets[slotPos].occupant is Gun && ((Gun)sockets[slotPos].occupant).isRifle)
            {

            }
            else
            {
                if (showing)
                {
                    Transform joint = GetPointOfInterest(GetSocketPointOfInterest(slotPos));

                    // mountedTargeters[joint].gunAnimator.EndShooting();
                    mountedTargeters[joint].Hide();
                    mountedTargeters.Remove(joint);
                }

                //Debug.Log("Removed targeter. Remaining targeters: " + mountedTargeters.Count);
            }
        }
    }
    // public void Show

    public Item ShowReloadVisual(Transform parent)
    {
        if(reloadItem == null)
        {
            reloadItem = Global.Resources[ItemNames.ReloadFrame];

            owner.itemEquiper.Equip(reloadItem);
            reloadItem.Show(parent);

            reloadItem.GetPointOfInterest(PointOfInterest.Sprite).GetComponent<Image>().sprite = inventorySprite;
        }
        else if(!reloadItem.showing)
        {
            reloadItem.Show(parent);

            reloadItem.GetPointOfInterest(PointOfInterest.Sprite).GetComponent<Image>().sprite = inventorySprite;
        }
        return reloadItem;
    }
    public Item ShowInventoryVisual(Transform parent, float width, float height)
    {
        if (inventoryItem == null)
        {
            //Create border
            if (GetClass(this) == MechItemClass.Socketable)
            {
                inventoryItem = Global.Resources[ItemNames.InventorySocketable]; //Global.instance.SOCKETABLE.Clone(); //
            }
            else
            {
                inventoryItem =  Global.Resources[ItemNames.InventoryItemBorder]; //Global.instance.INVENTORY_ITEM.Clone(); //
            }
        }

        if (!inventoryItem.showing)
        {
            float calculatedWidth = inventoryWidth * width;
            float calculatedHeight = inventoryHeight * height;

            owner.itemEquiper.Equip(inventoryItem);
            inventoryItem.Show(parent);
            //Add pic
            inventoryItem.GetPointOfInterest(PointOfInterest.Sprite).GetComponent<Image>().sprite = inventorySprite;
            //Rotation
            if (rotation != 0)
            {
                //Set size and rotation
                calculatedWidth *= ROTATION_SCALING;
                calculatedHeight *= ROTATION_SCALING;
                inventoryItem.visualItem.GetComponent<RectTransform>().sizeDelta = new Vector2(calculatedWidth, calculatedHeight);
                inventoryItem.GetPointOfInterest(PointOfInterest.Sprite).Rotate(new Vector3(0, 0, -rotation));
                inventoryItem.GetPointOfInterest(PointOfInterest.LeftCorner).localPosition += new Vector3(
                        0.5f * inventoryWidth * width * (1 - ROTATION_SCALING),
                        -0.5f * inventoryHeight * height * (1 - ROTATION_SCALING));
                inventoryItem.GetPointOfInterest(PointOfInterest.LeftCorner).Rotate(new Vector3(0, 0, rotation));
            }
            else
            {
                //Set size
                inventoryItem.visualItem.GetComponent<RectTransform>().sizeDelta = new Vector2(calculatedWidth, calculatedHeight);
            }

            //
           //if (GetClass(this) != MechItemClass.Socketable)
            //{
                //Show rarity
                if (rarity == Rarity.Common)
                {
                    inventoryItem.GetPointOfInterest(PointOfInterest.Common).gameObject.SetActive(true);

                }
                else if (rarity == Rarity.Uncommon)
                {
                    inventoryItem.GetPointOfInterest(PointOfInterest.Uncommon).gameObject.SetActive(true);

                }
                else if (rarity == Rarity.Rare)
                {
                    inventoryItem.GetPointOfInterest(PointOfInterest.Rare).gameObject.SetActive(true);

                }
                else if (rarity == Rarity.Legendary)
                {
                    inventoryItem.GetPointOfInterest(PointOfInterest.Legendary).gameObject.SetActive(true);
                }
            //}
            //else
            if(GetClass(this) == MechItemClass.Socketable)
            {
                inventoryItem.GetPointOfInterest(PointOfInterest.Border).GetComponent<Image>().color =
                    SOCKET_COLORS[((Socketable)this).GetSocketType()];
            }
            
            //Show sockets
            if (sockets != null && sockets.Count > 0)
            {
                //Store sockets in a matrix to calculate height and width of the socket array
               // int socketsInARow = Mathf.FloorToInt(calculatedWidth / SOCKET_SIZE);

                List<List<Item>> matrix = new List<List<Item>>();
                List<Item> currentRow = new List<Item>();
                DictionaryList<List<Item>, float> rowHeight = new DictionaryList<List<Item>, float>();
                DictionaryList<List<Item>, float> rowWidth = new DictionaryList<List<Item>, float>();
                DictionaryList<Item, SocketType> sockettype = new DictionaryList<Item, SocketType>();
                float currentRowWidth = 0;

                calculatedWidth -=  this is Socketable ? SOCKET_OTHER_SIZE : 0;

                //Add sockets into rows before placement
                foreach (int a in sockets)
                {
                    sockets[a].slot = Global.Resources[ItemNames.InventorySocket];

                    if (!rowHeight.Contains(currentRow)){
                        rowHeight.Add(currentRow, SOCKET_CRYSTAL_SIZE);
                    }
                    if (!rowWidth.Contains(currentRow))
                    {
                        rowWidth.Add(currentRow, 0);
                    }
                    Item socket = sockets[a].slot;
        
                    owner.itemEquiper.Equip(socket);
                    socket.Show(inventoryItem.GetPointOfInterest(PointOfInterest.LeftCorner));
                    sockettype.Add(socket, sockets[a].type);

                    

                    if (sockets[a].type == SocketType.Crystal || sockets[a].type == SocketType.Rifle)
                    {
                        socket.visualItem.GetComponent<RectTransform>().sizeDelta  = new Vector2(SOCKET_CRYSTAL_SIZE, SOCKET_CRYSTAL_SIZE);
                        rowWidth[currentRow] += SOCKET_CRYSTAL_SIZE;
                        currentRowWidth += SOCKET_CRYSTAL_SIZE;
                    }
                    else
                    {
                        socket.visualItem.GetComponent<RectTransform>().sizeDelta = new Vector2(SOCKET_OTHER_SIZE, SOCKET_OTHER_SIZE);
                        currentRowWidth += SOCKET_OTHER_SIZE;
                        rowWidth[currentRow] += SOCKET_OTHER_SIZE;
                        rowHeight[currentRow] = SOCKET_OTHER_SIZE;
                    }

                    socket.visualItem.GetComponent<Image>().color = SOCKET_DEFAULT_COLOR; //SOCKET_COLORS[sockets[socket]];

                    if (currentRowWidth >= calculatedWidth)
                    {
                        matrix.Add(currentRow);
                        currentRow = new List<Item>();
                        currentRowWidth = 0;
                    }

                    currentRow.Add(socket);
                }
                if (currentRow.Count > 0)
                {
                    matrix.Add(currentRow);
                    if (!rowHeight.Contains(currentRow))
                    {
                        rowHeight.Add(currentRow, SOCKET_CRYSTAL_SIZE);
                    }
                    if (!rowWidth.Contains(currentRow))
                    {
                        rowWidth.Add(currentRow, 0);
                    }
                }

                float y = rowHeight[rowHeight.Get(0)]/2;
                foreach (List<Item> currRow in rowHeight)
                {
                    y -= rowHeight[currRow] / 2;
                }

                    //Keep in mind this is relative to the center position
                   // float y = -((float)(matrix.Count - 1)) * SOCKET_SIZE / 2;

                //Place the sockets as close to center as possible
                for (int row = 0; row < matrix.Count; row++)
                {
                    List<Item> currRow = matrix[row];
                    float x = -rowWidth[currentRow] / 2 + ((currRow.Count % 2 == 0) ? rowHeight[currentRow] / 2 : 0);
                    //float x = -(currRow.Count - 1) * SOCKET_SIZE / 2;
                    foreach (Item i in currRow)
                    {
                        i.visualItem.localPosition = new Vector3(x, y);
                        x += sockettype[i] == SocketType.Crystal || sockettype[i] == SocketType.Rifle ? SOCKET_CRYSTAL_SIZE : SOCKET_OTHER_SIZE; //SOCKET_SIZE;
                    }
                    y += rowHeight[currentRow]; //SOCKET_SIZE;
                }
            }

            ShowSockets();
        }
        return inventoryItem;
    }

    public static DictionaryList<EffectWhen, Effect> EffectWhenDataToDictionary(EffectsWhenData[] data)
    {
        DictionaryList<EffectWhen, Effect> ret = new DictionaryList<EffectWhen, Effect>();
        foreach (EffectsWhenData dat in data)
        {
                ret.Add(dat.when, Global.Resources[dat.effect]);

        }
        return ret;
    }


    public static Vector3 GetWorldScale(Transform transform)
    {
        Vector3 worldScale = transform.localScale;
        Transform parent = transform.parent;

        while (parent != null)
        {
            worldScale = Vector3.Scale(worldScale, parent.localScale);
            parent = parent.parent;
        }

        return worldScale;
    }


    public static OnHitWhen[] OnHitWhenDataToOnHitWhen(OnHitWhenData[] orig)
    {
        if (orig == null)
        {
            return null;
        }

        OnHitWhen[] ret = new OnHitWhen[orig.Length];

        for (int i = 0; i < orig.Length; i++)
        {
            ret[i] = new OnHitWhen(Global.Resources[orig[i].onHit], orig[i].conditions);
        }
        return ret;
    }
    //List<Buff> GetBuffs();

    public virtual bool HasReload()
    {
        return false;
    }

    public virtual float GetTimer(float actionspeed)
    {
        return 1/actionspeed;
    }

    public virtual bool IsReady()
    {
        return false;
    }

    public virtual float GetTimeUntilReloadFinished(float actionspeed)
    {
        return 1/actionspeed;
    }

    public virtual void Execute(GameUnit target){}

    public virtual void Tick(float actionspeed, Vector3 mousePosition, GameUnit target, Vector3 mousePositionOnZPlane)
    {
        if(sockets != null)
        {
            foreach (int i in sockets)
            {
                if (sockets[i].occupant != null)
                {
                    ((MechItem)sockets[i].occupant).Tick(actionspeed,mousePosition, target, mousePositionOnZPlane);
                }
            }
        }
        foreach (Transform t in mountedTargeters)
        {
            Aim(t, actionspeed, mousePosition, target, mousePositionOnZPlane);
        }
    }
}

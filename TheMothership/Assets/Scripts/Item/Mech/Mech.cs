using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;


public enum Orientation
{
    Above,
    Below,
    ToTheRightOf,
    ToTheLeftOf
}
public class Mech : ItemEquipper{

    public static readonly float SHIELD_SIZE = 1.2f;
    public static readonly float MOUNT_IMPACT_DAMPENING = 1000;
    public static readonly float MOUNT_SHAKE_END = 0.01f;
    public static readonly float MOUNT_SHAKE_DAMPENING = 0.65f;
    public static readonly float MOUNT_SHAKE_DURATION = 0.2f;
    public static readonly float MOUNT_SHAKE_THRESHOLD_MIN = 4f;
    public static readonly float MOUNT_SHAKE_THRESHOLD_MAX = 12f;

    public static readonly Color SELECTION_COLOR_ENEMY = new Color(0.8f, 0, 0, 0.8f);
    public static readonly float SELECTION_WIDTH = 5;

    //private static readonly string MOUNT_NAME = "Mount for: ";

    //Equipped mechitems
    public ListHash<MechItem> mechItems = new ListHash<MechItem>();

    //---------------------- INVENTORY
    public InventoryBlock[,] equippedCoreMatrix;

    //List of equipped cores
    public DictionaryList<Core,Vector2> equippedCores = new DictionaryList<Core, Vector2>();

    public DictionaryList<Material, ListHash<MeshFilter>> combinedMeshes = new DictionaryList<Material, ListHash<MeshFilter>>();
    public DictionaryList<Material, MeshFilter> combinedNodes = new DictionaryList<Material, MeshFilter>();
    public DictionaryList<Material, MeshRenderer> combinedRenderers = new DictionaryList<Material, MeshRenderer>();

    public ListHash<MeshFilter> combinedShadow = new ListHash<MeshFilter>();
    public MeshFilter coreShadows;
    //List of all equipped slots
    //  public DictionaryList<Vector2, AttachmentSlot> attachmentSlots = new DictionaryList<Vector2, AttachmentSlot>(new Vector2Comparer());


    //Name of this mech
    public string name;

    //Mechitem legs
    public Legs legs;
    public SoundEngine soundEngine;

    //Jetpack (not mechitem)
    public MechJetpack jetpack;
    public Item jetpackBeam;
    public Item exhaustBeam;

    //Default audiosource (for play once)
    public AudioSource source;

    private Transform mount;

    //Mount shake
    private float initPos;
    private float shakePos;
    private float currentShakeDuration = 0;
    private float shakeDuration = 2;
    private bool shakeEnded = false;

    //Loaded data mechs must be showed
    public Mech(MechData data, GameUnit owner, string name) : 
        this(name, owner, Global.Resources[data.legs.leg], 
            Global.Resources[ItemNames.JetBeamLegs], 
            Global.Resources[ItemNames.JetBeamRolling])
    {
        Show();
        Equip(legs);

        foreach (SocketMount ss in data.legs.sockets)
        {
            SocketData(legs, ss);
        }

        foreach (CoreMount ms in data.mounts)
        {
            Core c = Global.Resources[ms.core];
            Equip(c);
            EquipCore(c, equippedCoreMatrix[(int)ms.pos.x, (int)ms.pos.y], true);
            foreach (SocketMount ss in ms.sockets)
            {
                SocketData(c, ss);
            }
            
        }

        CombineMeshesOfCores();

        UpdateHighlights();
    }
    //Constructor
    public Mech(
        string nameVal,
        GameUnit owner,
        Legs legsVal,
        Item jetpackBeamval,
        Item exhaustBeamval) : base(owner)
    { 
        this.jetpackBeam = jetpackBeamval;
        this.exhaustBeam = exhaustBeamval;
        this.name = nameVal;
        this.legs = legsVal;
        //this.owner = gu;
        this.jetpack = new MechJetpack(owner, jetpackBeam,exhaustBeam);

        this.owner.itemEquiper = this;

        CreateOrUpdateEquippedMatrix();


    }

    //Resize body to fit new size
    public void ResizeBodyToFitEquippedGear()
    {
        //Calculate sizes
        Vector2 size = GetCoresOccupiedSize();
        float minX = GetLeastOccupiedWidth();
        //float maxX = GetMaxOccupiedWidth();
        float mid = GetLegAttachmentInventorySpot().x;

        //Move parent to main node
        legs.visualItem.parent = Global.References[SceneReferenceNames.Main]; //Global.instance.MAIN;

        //Calculate height of the mech
        float y = Mathf.Max(1,legs.visualItem.GetComponentInChildren<MeshRenderer>().bounds.size.y*1.5f + size.y);
        float x = equippedCores.Count == 0 ? 0 : mid - minX-size.x/2;
        float xScale = size.x + 1;

        //Set the body size to this new size
        float footPos = owner.body.position.y - owner.body.localScale.y/2;
        owner.body.localScale = new Vector3(xScale, y , xScale);
        float newFootPos = owner.body.position.y - owner.body.localScale.y/2;

        owner.body.position += new Vector3(0, -(newFootPos - footPos),0);

        float colliderOffset = (xScale - y)/(y*2);

        //Copy rotation but turn 90 degrees (facing right)
        legs.visualItem.rotation = owner.body.rotation;
        legs.visualItem.Rotate(0, 90, 0);

        //Set position within the body
        legs.visualItem.position = owner.body.position;
        legs.visualItem.position += new Vector3(x, -y / 2, 0);

        //Set the parent back to the body
        legs.visualItem.parent = owner.body;

        //Shield
        float part =  owner.body.localScale.y /owner.body.localScale.x;
        owner.shield.transform.localScale = new Vector3(SHIELD_SIZE * part, SHIELD_SIZE, SHIELD_SIZE * part);

        //Adjust controller
        owner.controller.height = 1f;
        owner.controller.center = new Vector3(0, colliderOffset > 0 ? colliderOffset : 0);
        
    }
    public ListHash<Rigidbody> Kill()
    {
        UncombineMeshesOfCores();

        //Clear highlight
        if (!owner.isPlayer)
        {
            Highlight hl = owner.shield.gameObject.GetComponent<Highlight>();
            if(hl != null)
            {
                hl.Exit();
                Global.Destroy(hl);
            }
        }

        ListHash<Rigidbody> detached = new ListHash<Rigidbody>();

        foreach (Core c in equippedCores)
        {
            c.DetachFromMech(detached);
        }
        equippedCores.Clear();

        legs.DetachFromMech(detached);

        return detached;
    }

    public void Flash(Material m, float time)
    {
        foreach(Core c in equippedCores)
        {
            c.Flash(m, time);
        }
        legs.Flash(m, time);
        Global.instance.StartCoroutine(FlashSelf(m, time));
    }

    public IEnumerator FlashSelf(Material m, float time)
    {
        SetMaterialForAllCombinedCoreRenderers(m);
        yield return new WaitForSeconds(time);
        ReturnAllRenderersOfCombinedCoresToDefaultMaterial();
    }

    public void SetMaterialForAllCombinedCoreRenderers(Material m)
    {
        foreach (Material mat in combinedRenderers)
        {
            combinedRenderers[mat].sharedMaterial = m;
        }
    }

    public void ReturnAllRenderersOfCombinedCoresToDefaultMaterial()
    {
        foreach (Material mat in combinedRenderers)
        {
            combinedRenderers[mat].sharedMaterial = mat;
        }
    }

    public void MountShakeInit(float impact)
    {
        float magnitude = Mathf.Min(impact / MOUNT_IMPACT_DAMPENING, MOUNT_SHAKE_THRESHOLD_MAX);
        //Debug.Log(magnitude);

        if (magnitude > MOUNT_SHAKE_THRESHOLD_MIN && Mathf.Abs(shakePos) < magnitude)
        {
            initPos = 0;
            currentShakeDuration = 0;
            shakePos = magnitude;
            shakeDuration = MOUNT_SHAKE_DURATION;
            shakeEnded = false;
        }
    }

    public void UncombineMeshesOfCores()
    {
        //Destroy the old nodes of combined meshes
        foreach (Material m in combinedNodes)
        {
            Global.Destroy(combinedNodes[m].gameObject);
        }
        combinedNodes.Clear();

        foreach(Material m in combinedMeshes)
        {
            foreach(MeshFilter f in combinedMeshes[m])
            {
                f.gameObject.SetActive(true);
            }
        }
        combinedMeshes.Clear();

        if (coreShadows != null)
        {
            Global.Destroy(coreShadows.gameObject);
        }

        foreach (MeshFilter f in combinedShadow)
        {
            f.gameObject.SetActive(true);
        }

        combinedShadow.Clear();

        combinedRenderers.Clear();
    }

    void CombineMeshesOfCores()
    {

        UncombineMeshesOfCores();
        // Find all materials

        foreach (Core c in equippedCores)
        {
            foreach(Renderer r in c.meshMaterials)
            {
                if(!(r is SkinnedMeshRenderer) 
                    && r.gameObject.activeSelf)
                {
                    if(r.shadowCastingMode != UnityEngine.Rendering.ShadowCastingMode.ShadowsOnly)
                    {
                        bool add = true;

                        //Ignore stuff that animates
                        if (c.meleeAnimator != null)
                        {
                            if (r.transform.IsChildOf(c.meleeAnimator.transform))
                            {
                                add = false;
                            }
                        }

                        if (add)
                        {
                            Material mat = r.sharedMaterial;

                            if (!combinedMeshes.Contains(mat))
                            {
                                combinedMeshes.Add(mat, new ListHash<MeshFilter>());
                            }

                            combinedMeshes[mat].AddIfNotContains(r.transform.GetComponent<MeshFilter>());
                        }
                    }
                    else
                    {
                        combinedShadow.Add(r.transform.GetComponent<MeshFilter>());
                    }
                }
            }
        }

        //Create a transform for each material
        foreach(Material m in combinedMeshes)
        {
            GameObject go = new GameObject("Combined Material: "+m.name);
            MeshRenderer mr = go.AddComponent<MeshRenderer>();
            mr.sharedMaterial = m;
            mr.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            MeshFilter mf = go.AddComponent<MeshFilter>();
            mf.mesh = new Mesh();

            go.transform.parent = mount;

            combinedNodes.Add(m, mf);
            combinedRenderers.Add(m, mr);
        }

        //And for the shadows
        if(combinedShadow.Count > 0)
        {
            GameObject shadowGo = new GameObject("Combined Shadow: " + name);
            MeshRenderer shadowRenderer = shadowGo.AddComponent<MeshRenderer>();
            shadowRenderer.sharedMaterial = Global.Resources[MaterialNames.Default];
            shadowRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.ShadowsOnly;
            MeshFilter coreShadows = shadowGo.AddComponent<MeshFilter>();
            coreShadows.mesh = new Mesh();
            shadowGo.transform.parent = mount;

            //Combine the shadow obbjects
            CombineInstance[] combine = new CombineInstance[combinedShadow.Count];

            int i = 0;
            while (i < combinedShadow.Count)
            {
                combine[i].mesh = combinedShadow.Get(i).sharedMesh;
                combine[i].transform = combinedShadow.Get(i).transform.localToWorldMatrix;
                i++;
            }

            coreShadows.mesh.CombineMeshes(combine);
            coreShadows.gameObject.SetActive(true);

            foreach (MeshFilter f in combinedShadow)
            {
                f.gameObject.SetActive(false);
            }
        }
        else
        {
            coreShadows = null;
        }


        //Combine material
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

        /*MeshFilter[] meshFilters = GetComponentsInChildren<MeshFilter>();
        CombineInstance[] combine = new CombineInstance[meshFilters.Length];
        int i = 0;
        while (i < meshFilters.Length)
        {
            combine[i].mesh = meshFilters[i].sharedMesh;
            combine[i].transform = meshFilters[i].transform.localToWorldMatrix;
            meshFilters[i].gameObject.active = false;
            i++;
        }
        transform.GetComponent<MeshFilter>().mesh = new Mesh();
        transform.GetComponent<MeshFilter>().mesh.CombineMeshes(combine);
        transform.gameObject.active = true;*/
    }


    public void MountShake(float actionspeed)
    {

        if (!shakeEnded)
        {

            if (actionspeed == 0)
            {
                mount.eulerAngles = new Vector3(0, mount.eulerAngles.y, 0);
                shakeEnded = true;
                return;
            }

            if (initPos != shakePos && mount != null && currentShakeDuration < shakeDuration)
            {
                currentShakeDuration += Time.deltaTime;
                float t = currentShakeDuration / shakeDuration;
                t = Mathf.Sin(t * Mathf.PI * 0.5f);
                mount.eulerAngles = new Vector3(Mathf.Lerp(initPos, shakePos, t), mount.eulerAngles.y, Mathf.Lerp(initPos, shakePos, t));
            }
            else if (mount != null && currentShakeDuration >= shakeDuration)
            {
                initPos = shakePos;
                shakePos = initPos * -MOUNT_SHAKE_DAMPENING;
                shakeDuration = shakeDuration * MOUNT_SHAKE_DAMPENING;
                currentShakeDuration = 0;

                if (shakeDuration < MOUNT_SHAKE_END)
                {
                    mount.eulerAngles = new Vector3(0, mount.eulerAngles.y, 0);
                    shakeEnded = true;
                }
            }
        }
    }

    public Vector3 GetRandomSurfacePointFrom(Orientation ori)
    {
        int width = equippedCoreMatrix.GetLength(0);
        int height = equippedCoreMatrix.GetLength(1);
        int foundX = -1;
        int foundY = -1;

        if (ori == Orientation.ToTheRightOf)
        {
            for(int x = width-1; x > 0; x--)
            {
                for (int y = 0; y < height; y++)
                {
                    if(equippedCoreMatrix[x,y].occupant != null)
                    {
                        if(foundX == -1 || UnityEngine.Random.Range(0, 1) < 0.5f)
                        {
                            foundX = x;
                            foundY = y;
                        }
                    }
                }
                if (foundX != -1) {
                    return new Vector3(
                        equippedCoreMatrix[foundX, foundY].occupant.visualItem.position.x + 0.5f,
                        equippedCoreMatrix[foundX, foundY].occupant.visualItem.position.y + UnityEngine.Random.Range(-0.5f, 0.5f),
                        UnityEngine.Random.Range(-0.5f, 0.5f));
                }
            }
        }
        else if (ori == Orientation.ToTheLeftOf)
        {
            for (int x = 0; x > width-1; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    if (equippedCoreMatrix[x, y].occupant != null)
                    {
                        if (foundX == -1 || UnityEngine.Random.Range(0, 1) < 0.5f)
                        {
                            foundX = x;
                            foundY = y;
                        }
                    }
                }
                if (foundX != -1) {
                    return new Vector3(
                        equippedCoreMatrix[foundX, foundY].occupant.visualItem.position.x - 0.5f,
                        equippedCoreMatrix[foundX, foundY].occupant.visualItem.position.y + UnityEngine.Random.Range(-0.5f, 0.5f),
                        UnityEngine.Random.Range(-0.5f, 0.5f));
                }
            }
        }
        else if (ori == Orientation.Above)
        {
            for (int y = height-1; y > 0; y--)
            {
                for (int x = 0; x < width; x++)
                {
                    if (equippedCoreMatrix[x, y].occupant != null)
                    {
                        if (foundX == -1 || UnityEngine.Random.Range(0, 1) < 0.5f)
                        {
                            foundX = x;
                            foundY = y;
                        }
                    }
                }
                if (foundX != -1) {
                    return new Vector3(
                        equippedCoreMatrix[foundX, foundY].occupant.visualItem.position.x + UnityEngine.Random.Range(-0.5f, 0.5f),
                        equippedCoreMatrix[foundX, foundY].occupant.visualItem.position.y + 0.5f,
                        UnityEngine.Random.Range(-0.5f, 0.5f));
                }
            }
        }
        else if (ori == Orientation.Below)
        {
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    if (equippedCoreMatrix[x, y].occupant != null)
                    {
                        if (foundX == -1 || UnityEngine.Random.Range(0, 1) < 0.5f)
                        {
                            foundX = x;
                            foundY = y;
                        }
                    }
                }
                if (foundX != -1)
                {
                    return new Vector3(
                        equippedCoreMatrix[foundX, foundY].occupant.visualItem.position.x + UnityEngine.Random.Range(-0.5f, 0.5f),
                        equippedCoreMatrix[foundX, foundY].occupant.visualItem.position.y - 0.5f,
                        UnityEngine.Random.Range(-0.5f, 0.5f));
                }
            }
        }

        return legs.visualItem.position;
    }

    public void SocketData(MechItem mi, SocketMount mount, int innerPointer = 0)
    {
        if (innerPointer < mount.pos.Length && mi != null && mi.sockets != null)
        {
            Socketable skk = null;

            if (innerPointer == mount.pos.Length - 1)
            {
                skk = (Socketable)Equip((Item)Global.Resources[mount.socket]);
                mi.SocketItem(mount.pos[innerPointer], skk);
            }
            else if (mi.sockets[mount.pos[innerPointer]].occupant != null)
            {
                skk = mi.sockets[mount.pos[innerPointer]].occupant;
                SocketData((MechItem)skk, mount, innerPointer + 1);
            }
        }
    }
    //Returns true if item is equipped
    public bool IsEquipped(MechItem mi)
    {
        return mechItems.Contains(mi) || mi == legs;

    }
    //Returns a list of blockers or unequips a core
    public List<MechItem> UnequipCore(Core core)
    {
        int width = equippedCoreMatrix.GetLength(0);
        int height = equippedCoreMatrix.GetLength(1);

        List<MechItem> blockers = new List<MechItem>();

        //Quick check if we are trying to detach the root with other things equipped
        if (core == GetCoreAttachedToLegs() && equippedCores.Count != 1)
        {
            return new List<MechItem> { core };
        }

        //Check simple connection dependencies (not needed but gives better blocking visuals)
        foreach (Core check in equippedCores)
        {
            if(check != core)
            {
                bool foundOther = false;
                //Check if its only connection is this
                for (int x = 0; x < width; x++)
                {
                    for (int y = 0; y < height; y++)
                    {
                        if (equippedCoreMatrix[x, y].occupant == check && equippedCoreMatrix[x,y].type == InventoryBlockType.Occupied)
                        {
                            if (
                                   (
                                   //If its connected but has other connectors
                                   equippedCoreMatrix[x, y].connectors.Contains(core) &&
                                   equippedCoreMatrix[x, y].connectors.Count > 1
                                   )
                                   ||
                                   //If it has other connectors
                                   (!equippedCoreMatrix[x, y].connectors.Contains(core)
                                   && equippedCoreMatrix[x, y].connectors.Count > 0
                                   )
                                   ||
                                   //If its the midpoint (which is always connected)
                                   (x == GetLegAttachmentInventorySpot().x 
                                   && y == GetLegAttachmentInventorySpot().y)
                              )

                            {
                                foundOther = true;
                            }

                        }
                    }
                }
                //We found a core that is depandant on only this
                if (!foundOther)
                {
                    blockers.Add(check);
                    //Debug.Log(check.uniqueItemName + ", depends on: " + core.uniqueItemName);
                    //return null;
                }
            }
        }

        if (blockers.Count > 0)
        {
            return blockers;
        }

        //Check if there is a connection from the initial block to each of the other block if the core was removed
        foreach (Core destinationCheck in equippedCores)
        {
            if(destinationCheck != core)
            {
                if(!CanTravelToWithout(destinationCheck, core))
                {
                    return new List<MechItem> { core };
                }
            }
        }
        //Check attachments
        /*foreach(Vector2 slotPos in attachmentSlots)
        {
            AttachmentSlot slot = attachmentSlots[slotPos];

            if (slot.ContainsCore(core) && slot.occupant != null)
            {
                return new List<MechItem> { slot.occupant };
            }
        }*/



        //Clear now invalid attachment slots
        //RemoveAttachmentSlotsContaining(core);

        //Remove from equipped list
        equippedCores.Remove(core);
        mechItems.Remove(core);

        //Remove from matrix
        for(int x = 0; x < width; x++)
        {
            for(int y = 0; y < height; y++)
            {
                //Remove connections and blockings caused
                equippedCoreMatrix[x, y].connectors.Remove(core);
                equippedCoreMatrix[x, y].blockers.Remove(core);

                //Count for debug purposes
                /*connectorsRemaining += equippedMatrix[x, y].connectors.Count;
                blockersRemaining += equippedMatrix[x, y].blockers.Count;*/

                //Remove occupied
                if (equippedCoreMatrix[x, y].occupant == core)
                {
                    equippedCoreMatrix[x, y].occupant = null;

                    //Remove occupied status
                    if(equippedCoreMatrix[x, y].type == InventoryBlockType.Occupied)
                    {
                        if (equippedCoreMatrix[x, y].blockers.Count > 0)
                        {
                            equippedCoreMatrix[x, y].type = InventoryBlockType.Blocked;
                        }else if (equippedCoreMatrix[x, y].connectors.Count > 0 ||
                           (x == GetLegAttachmentInventorySpot().x &&
                           y == GetLegAttachmentInventorySpot().y)){
                            equippedCoreMatrix[x, y].type = InventoryBlockType.Connected;
                        }else{
                            equippedCoreMatrix[x, y].type = InventoryBlockType.Vacant;
                        }
                    }
                }
                //If there are no longer any blockers
                if (equippedCoreMatrix[x, y].type == InventoryBlockType.Blocked
                    && equippedCoreMatrix[x, y].blockers.Count == 0)
                {
                    if (equippedCoreMatrix[x, y].connectors.Count > 0 ||
                        (x == GetLegAttachmentInventorySpot().x &&
                        y == GetLegAttachmentInventorySpot().y))
                    {
                        equippedCoreMatrix[x, y].type = InventoryBlockType.Connected;
                    }
                    else
                    {
                        equippedCoreMatrix[x, y].type = InventoryBlockType.Vacant;
                    }
                //If there are no connectors
                }else if(equippedCoreMatrix[x, y].type == InventoryBlockType.Connected &&
                        equippedCoreMatrix[x, y].connectors.Count == 0 &&
                        !(x == GetLegAttachmentInventorySpot().x &&
                        y == GetLegAttachmentInventorySpot().y)){
                        equippedCoreMatrix[x, y].type = InventoryBlockType.Vacant;
                }

                //Mark borders
                if (x == 0 || x == width - 1 || y == height - 1 || y == 0)
                {
                    equippedCoreMatrix[x, y].type = InventoryBlockType.Blocked;
                }
            }
        }
        //Hide visuals
        core.Hide();
        UpdateBlockRays();

        if (DevelopmentSettings.DEBUG_INVENTORY)
        {
            DebugPrintInventory();
        }
        

        ResizeBodyToFitEquippedGear();

        CombineMeshesOfCores();

        // Debug.Log("Connectors remaining: " + connectorsRemaining);
        // Debug.Log("Blockers remaining: " + blockersRemaining);

        return null;
    }
    //Returns an array copy of the blocktypes of the current matrix
    public InventoryBlockType[,] SnapShotEquippedMatrixTypes()
    {
        int width = equippedCoreMatrix.GetLength(0);
        int height = equippedCoreMatrix.GetLength(1);
        InventoryBlockType[,] returnMatrix = new InventoryBlockType[width, height];

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                returnMatrix[x, y] = equippedCoreMatrix[x, y].type;
            }
        }
        return returnMatrix;
    }
    //Returns the core attached to legs if there is any
    private Core GetCoreAttachedToLegs()
    {
        return equippedCoreMatrix[(int)GetLegAttachmentInventorySpot().x, (int)GetLegAttachmentInventorySpot().y].occupant;
    }
    //True if you can travel to destination from legs without the core "without"
    public bool CanTravelToWithout(Core destination, Core without)
    {
        ListHash<Vector2> connections = new ListHash<Vector2>();
        ListHash<Core> alreadyTried = new ListHash<Core>();
        alreadyTried.Add(without);

        //Get the core connected to the legs
        Core init = GetCoreAttachedToLegs();

        if (destination == without)
        {
            return false;
        }

        //Check scenarios
        if(init == destination)
        {
            return true;
        }

        GetConnectionsOfCore(init, connections);

        //Tries to go to destination without the "without-core"
        while (connections.Count > 0)
        {
            Core con = equippedCoreMatrix[(int)connections[0].x, (int)connections[0].y].occupant;
            if(con == destination)
            {
                return true;
            }else if(con != null && !alreadyTried.Contains(con))
            {
                alreadyTried.Add(con);
                GetConnectionsOfCore(con, connections);
            }
            connections.Remove(connections[0]);
        }
        return false;

    }
    //Unequips an attachment
   /* public bool UnequipAttachment(Attachment att)
    {
        foreach(Vector2 pos in attachmentSlots)
        {
            AttachmentSlot slot = attachmentSlots[pos];
            if(slot.occupant == att)
            {
                slot.topLeft.Unmount(att);
                slot.occupant = null;
            }
        }
        return true;

    }
    //Equip Attacment
    public bool EquipAttachment(Attachment att, AttachmentSlot slot)
    {
        if(att.diameter <= slot.diameter && slot.occupant == null)
        {
            slot.occupant = att;
            slot.topLeft.MountAndShow(att);
            return true;
        }
        return false;
    }*/
    //Equip a core or return false if not possible
    public bool EquipCore(Core core, InventoryBlock ib, bool ignoreConnectionRestrictions = false)
    {
        return InnerquipCore(core, ib, false, ignoreConnectionRestrictions);
    }
    //Returns if equippins is possible (not blocker or outside of bounds)
    public bool CanEquipCore(Core core, InventoryBlock ib)
    {
        return InnerquipCore(core, ib, true);
    }
    //Inner method for equipping
    private bool InnerquipCore(Core core, InventoryBlock ib, bool checkIfCanEquipOnly = false, bool ignoreConnectionRestrictions = false)
    {
        //Only attach to connected tiles
        if(ib.type == InventoryBlockType.Connected)
        {
            //Get connectionpoint in inventory
            Vector2 initPos = GetConnections()[ib];
            
            //Get connectionpoint in item
            Vector2 itemInitPos = core.connection;
            InventoryBlockType[,] ibt = core.inventorySpace;
            int itemWidth = ibt.GetLength(0);
            int itemHeight = ibt.GetLength(1);
            int width = equippedCoreMatrix.GetLength(0);
            int height = equippedCoreMatrix.GetLength(1);

            //Get bounds
            Vector2 topLeftStart = initPos + (new Vector2(0, itemHeight-1) - itemInitPos);
            Vector2 topRightStart = new Vector2(topLeftStart.x + (itemWidth - 1), topLeftStart.y);
            Vector2 bottomRightEnd = new Vector2(topRightStart.x, topRightStart.y-(itemHeight-1));

            /*Debug.Log("Top left start: " + topLeftStart);
            Debug.Log("Top right start: " + topRightStart);
            Debug.Log("Bottom right end: " + bottomRightEnd);*/

            //Check if item can be put within bounds
            if(WithinBounds(new Vector2[] { topLeftStart, topRightStart, bottomRightEnd}, new Vector2(width-1,height-1))){
                //Debug.Log("Within bounds, checking blocks...");

                //Check if the item fits without being blocked or blocking other things
                if (!ignoreConnectionRestrictions)
                {
                    for (int y = (int)topLeftStart.y, yItem = itemHeight - 1; y >= bottomRightEnd.y; y--, yItem--)
                    {
                        //string row = "";
                        for (int x = (int)topLeftStart.x, xItem = 0; x <= topRightStart.x; x++, xItem++)
                        {
                            // row += equippedMatrix[x, y].DebugString(x, y);
                            if (CheckSquareBlocked(ibt[xItem, yItem], equippedCoreMatrix[x, y].type))
                            {
                                // Debug.Log("Blocked: Item: <" + xItem + "," + yItem + "> Inventory <" + x + "," + y + ">");
                                return false;
                            }
                            //Check block rays
                            if (ibt[xItem, yItem] == InventoryBlockType.Blocked)
                            {
                                if (y == topLeftStart.y)
                                {
                                    //Block Ray upwards
                                    for (int yBlock = (int)topLeftStart.y; yBlock < height; yBlock++)
                                    {
                                        if (equippedCoreMatrix[x, yBlock].type == InventoryBlockType.Occupied)
                                        {
                                            return false;
                                        }
                                    }
                                }
                                if (x == topLeftStart.x)
                                {
                                    //Block Ray left
                                    for (int xBlock = (int)topLeftStart.x; xBlock > 0; xBlock--)
                                    {
                                        if (equippedCoreMatrix[xBlock, y].type == InventoryBlockType.Occupied)
                                        {
                                            return false;
                                        }
                                    }
                                }
                                if (x == topRightStart.x)
                                {
                                    //Block Ray right
                                    for (int xBlock = (int)topRightStart.x; xBlock < width; xBlock++)
                                    {
                                        if (equippedCoreMatrix[xBlock, y].type == InventoryBlockType.Occupied)
                                        {
                                            return false;
                                        }
                                    }
                                }
                            }
                        }
                        //Debug.Log(row);
                    }
                }
                //Stop here if we are only checking
                if (checkIfCanEquipOnly)
                {
                    return true;
                }
                //Debug.Log("Nothing blocking. Equipping:");
                //Equip item
                for (int y = (int)topLeftStart.y, yItem = itemHeight - 1; y >= bottomRightEnd.y; y--, yItem--){
                    //string row = "";
                    for (int x = (int)topLeftStart.x, xItem = 0; x <= topRightStart.x; x++, xItem++)
                    {
                        //row+= InventoryBlock.DebugString(ibt[xItem, yItem],xItem,yItem);

                        equippedCoreMatrix[x, y].type = MergeSquareTypes(ibt[xItem, yItem], equippedCoreMatrix[x, y].type);
                        MergeSquares(equippedCoreMatrix[x, y], ibt[xItem, yItem], core);

                        //Block rays
                        if(ibt[xItem, yItem] == InventoryBlockType.Blocked)
                        {
                            if(y == topLeftStart.y)
                            {
                               // Debug.Log("Adding block upwards");
                                //Block Ray upwards
                                for(int yBlock = (int)topLeftStart.y; yBlock < height; yBlock++) {
                                    equippedCoreMatrix[x, yBlock].type = InventoryBlockType.Blocked;
                                   // equippedMatrix[x, yBlock].blockers.AddIfNotContains(core);
                                }
                            }
                            if(x == topLeftStart.x)
                            {
                                //Debug.Log("Adding block left");
                                //Block Ray left
                                for (int xBlock = (int)topLeftStart.x; xBlock > 0; xBlock--)
                                {
                                    equippedCoreMatrix[xBlock, y].type = InventoryBlockType.Blocked;
                                   // equippedMatrix[xBlock, y].blockers.AddIfNotContains(core);
                                }
                            }
                            if (x == topRightStart.x)
                            {
                                //Debug.Log("Adding block right");
                                //Block Ray right
                                for (int xBlock = (int)topRightStart.x; xBlock < width; xBlock++)
                                {
                                    equippedCoreMatrix[xBlock, y].type = InventoryBlockType.Blocked;
                                   // equippedMatrix[xBlock, y].blockers.AddIfNotContains(core);
                                }
                            }

                        }


                        //ibt[xItem, yItem], inventory[x, y].type)
                    }
                   // Debug.Log(row);
                }
                //SpawnAttachmentSlots();

                //Debug.Log("Item equipped, displaying inventory: ");
                equippedCores.Add(core, initPos);
                mechItems.AddIfNotContains(core);

                if (DevelopmentSettings.DEBUG_INVENTORY)
                {
                    DebugPrintInventory();
                }



                Vector2 offset = (GetLegAttachmentInventorySpot() - initPos)*-1;
                Vector3 v3Offset = new Vector3(offset.x, offset.y);

                /*Debug.Log("Attachment slot:" + GetLegAttachmentInventorySpot());
                Debug.Log("Init slot: " + initPos);
                Debug.Log("Offset: " + offset);*/
                //Show item

                Equip(core);

                //Attach to main
                mount.parent = Global.References[SceneReferenceNames.Main];//Global.instance.MAIN;
                //Keep scale
                mount.localScale = new Vector3(1, 1, 1);
                //Mount the core
                core.Show(mount);
                //Mount at offset
                core.visualItem.localPosition = v3Offset;
                //Remount the mount
                mount.parent = legs.GetPointOfInterest(PointOfInterest.LegMountSlot);
                //Reset the position
                mount.localPosition = legs.mountAlignment;

                //core.Show(Global.instance.X_CLONES);
                //core.visualItem.parent = mount;
                //legs.GetPointOfInterest(PointOfInterest.LegMountSlot).position+ v3Offset;
                //legs.GetPointOfInterest(PointOfInterest.LegMountSlot);


                ResizeBodyToFitEquippedGear();

                UpdateHighlights();


                if (!ignoreConnectionRestrictions)
                {
                    CombineMeshesOfCores();
                }

                return true;

            }
            //Vector2 topRightEnd = //topLeftStart + new Vector2(itemWidth - 1,0);
            //Vector2 bottomRightEnd = //topRightEnd + new Vector2(0, itemHeight-1);

            //for(int x = (int)topLeftStart.x; x <= topRightEnd.x; x++)
            //{
            //  for(int y = topLeftStart.y; y < )
            //}



        }
        return false;
    }
    //Removes all attachment slots containing the core in question
   /* private void RemoveAttachmentSlotsContaining(Core core)
    {
        int width = equippedCoreMatrix.GetLength(0);
        int height = equippedCoreMatrix.GetLength(1);

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                Vector2 pos = new Vector2(x, y);
                if (attachmentSlots.Contains(pos))
                {
                    Core one = CoreAtPos(pos);
                    Core two = CoreAtPos(new Vector2(x + 1, y));
                    Core three = CoreAtPos(new Vector2(x + 1, y - 1));
                    Core four = CoreAtPos(new Vector2(x, y - 1));

                    if (one == core || two == core || three == core || four == core)
                    {
                        attachmentSlots.Remove(pos);
                    }
                }
            }
        }
    }*/
    //Spawns new attachment slots (After a core has been added)
   /* private void SpawnAttachmentSlots()
    {
        //Debug.Log("Spawn?");

        int width = equippedCoreMatrix.GetLength(0);
        int height = equippedCoreMatrix.GetLength(1);

        for(int y = 0; y < height; y++)
        {
            for(int x = 0; x < width; x++)
            {
                Vector2 pos = new Vector2(x, y);
                if (!attachmentSlots.Contains(pos))
                {
                    //Debug.Log("Did not contain: " + pos);

                    Core one = CoreAtPos(pos);
                    Core two = CoreAtPos(new Vector2(x + 1, y));
                    Core three = CoreAtPos(new Vector2(x + 1, y-1));
                    Core four = CoreAtPos(new Vector2(x, y - 1));

                    //All non-null and distinct
                    if(one != null && two != null && three != null && four != null)
                    {
                        //Debug.Log("Non-null found!");

                        if (    one != two && one != three && one != four
                                && two != three && two != four
                                && three != four)
                        {
                            int min =
                                 Mathf.Min(
                                     Mathf.Min(
                                        Mathf.Min((int)one.inventoryWidth, (int)one.inventoryHeight),
                                        Mathf.Min((int)two.inventoryWidth, (int)two.inventoryHeight)
                                    ),
                                     Mathf.Min(
                                        Mathf.Min((int)three.inventoryWidth, (int)three.inventoryHeight),
                                        Mathf.Min((int)four.inventoryWidth, (int)four.inventoryHeight)
                                ));
                          //  Debug.Log("SPAWN!!!");
                            attachmentSlots.Add(pos, new AttachmentSlot(min, one, two, four, three));
                        }
                    }
                }
            }
        }
    }*/
    //Returns the core at position pos or null if out of bounds or if there is no one
    public Core CoreAtPos(Vector2 pos)
    {
        //Out of bounds check
        if(pos.x < 0 || pos.y < 0 || pos.x >= equippedCoreMatrix.GetLength(0) || pos.y >= equippedCoreMatrix.GetLength(1))
        {
            return null;
        }
        //Null check
        if(equippedCoreMatrix[(int)pos.x, (int)pos.y].occupant == null)
        {
            return null;
        }
        //Else
        return equippedCoreMatrix[(int)pos.x, (int)pos.y].occupant;
    }
    //Returns a list of all connections in the current matrix
    public DictionaryList<InventoryBlock, Vector2> GetConnections()
    {
        int width = this.equippedCoreMatrix.GetLength(0);
        int height = this.equippedCoreMatrix.GetLength(1);

        if (equippedCores.Count == 0)
        {
            return new DictionaryList<InventoryBlock, Vector2>()
            {
                { equippedCoreMatrix[
                    (int)GetLegAttachmentInventorySpot().x,
                    (int)GetLegAttachmentInventorySpot().y],
                    GetLegAttachmentInventorySpot()}
            };
        }

        DictionaryList<InventoryBlock, Vector2> ret = new DictionaryList<InventoryBlock, Vector2>();

        for (int x = 0; x < width; x += 1)
        {
            for (int y = 0; y < height; y += 1)
            {
                if(equippedCoreMatrix[x,y].type == InventoryBlockType.Connected)
                {
                    ret.Add(equippedCoreMatrix[x, y], new Vector2(x, y));
                    //Debug.Log("Connection found: "+new Vector2(x, y));
                }
            }
        }
        return ret;
    }
    //Returns the attachment position granted by the legs
    public Vector2 GetLegAttachmentInventorySpot()
    {
        int width = equippedCoreMatrix.GetLength(0);
        return new Vector2(Mathf.FloorToInt(((float)width) / 2f), 1);
    }
    //Merges squares when an item is added
    private InventoryBlockType MergeSquares(InventoryBlock inv, InventoryBlockType spot, Core item)
    {
        if(spot == InventoryBlockType.Occupied)
        {
            inv.occupant = item;

        }else if(spot == InventoryBlockType.Connected)
        {
            inv.connectors.AddIfNotContains(item);
        }
        else if (spot == InventoryBlockType.Blocked)
        {
            inv.occupant = item;
            inv.blockers.AddIfNotContains(item);
        }
        return spot;
    }
    //Merges squaretypes when an item is added
    private InventoryBlockType MergeSquareTypes(InventoryBlockType item, InventoryBlockType inv)
    {
        if(inv == InventoryBlockType.Blocked || item == InventoryBlockType.Blocked)
        {
            return InventoryBlockType.Blocked;

        }else if(inv == InventoryBlockType.Connected && item == InventoryBlockType.Vacant)
        {
            return InventoryBlockType.Connected;
        }else if(inv == InventoryBlockType.Occupied || item == InventoryBlockType.Occupied)
        {
            return InventoryBlockType.Occupied;
        }
        else if(item == InventoryBlockType.Connected)
        {
            return InventoryBlockType.Connected;
        }
        return InventoryBlockType.Vacant;
    }
    //Checks if a position is blocked when an item is added
    private bool CheckSquareBlocked(InventoryBlockType item, InventoryBlockType inv)
    {
        if( 
            // Trying to put something on blocked inventory
            (item == InventoryBlockType.Occupied && (inv == InventoryBlockType.Blocked || inv == InventoryBlockType.Occupied))
            //The item would block something that is already equipped
            || (item == InventoryBlockType.Blocked && inv == InventoryBlockType.Occupied)
            )
        {
            return true;
        }
        return false;
    }
    //Checks if a position is out of bounds when an item is added
    private bool WithinBounds(Vector2[] toCheck, Vector2 bounds)
    {
        foreach(Vector2 check in toCheck)
        {
            if(check.x < 0 || check.x > bounds.x || check.y < 0 || check.y > bounds.y)
            {
                Debug.Log("Out of bounds: " + check);
                return false;
            }
        }
        return true;
    }
    //Creates or updates the existing equipped matrix
    private void CreateOrUpdateEquippedMatrix()
    {
        //Save old values
        InventoryBlock[,] oldMatrix = this.equippedCoreMatrix;
        Vector2 oldmid = new Vector2(0, 0);
        if (oldMatrix != null)
        {
            oldmid = GetLegAttachmentInventorySpot();
        }
        //Add 1 square that is always vacant to the left, right, top bottom
        this.equippedCoreMatrix = new InventoryBlock[legs.widthCapacity+2, legs.heightCapacity+2];
        int width = this.equippedCoreMatrix.GetLength(0);
        int height = this.equippedCoreMatrix.GetLength(1);

        //Add in oldmatrix into newmatrix
        if(oldMatrix != null)
        {
            Vector2 newmid = GetLegAttachmentInventorySpot();
            float xShift = (newmid.x - oldmid.x);

            for (int x = 0; x < width; x += 1)
            {
                for (int y = 0; y < height; y += 1)
                {
                    //Block top, left, right etc

                    Vector2 posInOld = new Vector2(x-xShift, y);
                    InventoryBlock ibt;
                    if (posInOld.x >= 0 && posInOld.x < oldMatrix.GetLength(0) && posInOld.y < oldMatrix.GetLength(1))
                    {
                        ibt = oldMatrix[(int)posInOld.x, (int)posInOld.y];

                        //Removes old border along edges
                        if (ibt.type == InventoryBlockType.Blocked)
                        {
                            if(ibt.blockers.Count == 0)
                            {
                                if (ibt.connectors.Count > 0)
                                {
                                    ibt.type = InventoryBlockType.Connected;
                                }
                                else
                                {
                                    ibt.type = InventoryBlockType.Vacant;
                                }
                            }
                        }
                    }
                    else
                    {
                        ibt = new InventoryBlock(InventoryBlockType.Vacant);
                    }
                    //Add new border
                    if (x == 0 || x == width - 1 || y == height - 1 || y == 0) {
                        ibt.type = InventoryBlockType.Blocked;
                    }
                    this.equippedCoreMatrix[x, y] = ibt;
                }
            }
            //Shift cores to the new center position
            foreach(Core core in equippedCores)
            {
                equippedCores[core] = new Vector2(
                  equippedCores[core].x + xShift,
                  equippedCores[core].y);
            }
            //Shift attachments to the new center position
           /* DictionaryList<Vector2, AttachmentSlot> newAttachmentSlots = new DictionaryList<Vector2, AttachmentSlot>(new Vector2Comparer());
            foreach (Vector2 attSlot in attachmentSlots)
            {
                AttachmentSlot att = attachmentSlots[attSlot];
                newAttachmentSlots.Add(new Vector2(attSlot.x + xShift, attSlot.y), att);
            }
            attachmentSlots.Clear();
            attachmentSlots = newAttachmentSlots;*/


        }
        else
        {

            for (int x = 0; x < width; x += 1)
            {
                for (int y = 0; y < height; y += 1)
                {
                    //Block top, left, right etc
                    InventoryBlock ibt = new InventoryBlock((x == 0 || x == width - 1 || y == height - 1 || y == 0) ? InventoryBlockType.Blocked : InventoryBlockType.Vacant);
                    if (y == 1 && x == Mathf.FloorToInt(((float)width) / 2f))
                    {
                        //This is the first connectionpoint
                        ibt.type = InventoryBlockType.Connected;
                        //connections.Add(ibt,new Vector2(x,y));
                    }
                    this.equippedCoreMatrix[x, y] = ibt;
                }
            }
        }

        //Make sure block rays are updated when size is changed
        UpdateBlockRays();


        if (DevelopmentSettings.DEBUG_INVENTORY)
        {
            DebugPrintInventory();
        }
    }
    //Updates blockrays from blocking objects (used after leg switch)
    private void UpdateBlockRays()
    {
       foreach(Core core in equippedCores)
       {
            UpdateBlockRayForCore(core);
       }
    }
    //Updates block rays for a single core
    private void UpdateBlockRayForCore(Core core)
    {

        Vector2 initPos = equippedCores[core];

        //Get connectionpoint in item
        Vector2 itemInitPos = core.connection;
        InventoryBlockType[,] ibt = core.inventorySpace;

        int width = this.equippedCoreMatrix.GetLength(0);
        int height = this.equippedCoreMatrix.GetLength(1);

        int itemWidth = ibt.GetLength(0);
        int itemHeight = ibt.GetLength(1);

        //Get bounds
        Vector2 topLeftStart = initPos + (new Vector2(0, itemHeight - 1) - itemInitPos);
        Vector2 topRightStart = new Vector2(topLeftStart.x + (itemWidth - 1), topLeftStart.y);
        Vector2 bottomRightEnd = new Vector2(topRightStart.x, topRightStart.y - (itemHeight - 1));

        for (int y = (int)topLeftStart.y, yItem = itemHeight - 1; y >= bottomRightEnd.y; y--, yItem--)
        {
            for (int x = (int)topLeftStart.x, xItem = 0; x <= topRightStart.x; x++, xItem++)
            {
                if (ibt[xItem, yItem] == InventoryBlockType.Blocked)
                {
                    if (y == topLeftStart.y)
                    {
                        for (int yBlock = (int)topLeftStart.y; yBlock < height; yBlock++)
                        {
                            equippedCoreMatrix[x, yBlock].type = InventoryBlockType.Blocked;
                        }
                    }
                    if (x == topLeftStart.x)
                    {
                        for (int xBlock = (int)topLeftStart.x; xBlock > 0; xBlock--)
                        {
                            equippedCoreMatrix[xBlock, y].type = InventoryBlockType.Blocked;
                        }
                    }
                    if (x == topRightStart.x)
                    {
                        for (int xBlock = (int)topRightStart.x; xBlock < width; xBlock++)
                        {
                            equippedCoreMatrix[xBlock, y].type = InventoryBlockType.Blocked;
                        }
                    }

                }
            }
        }
    }
    //Puts all connections points of a core inside the supplied listhash
    private void GetConnectionsOfCore(Core core, ListHash<Vector2> positions)
    {
        //Get connectionpoint in inventory
        Vector2 initPos = equippedCores[core];

        //Get connectionpoint in item
        Vector2 itemInitPos = core.connection;
        InventoryBlockType[,] ibt = core.inventorySpace;
        int itemWidth = ibt.GetLength(0);
        int itemHeight = ibt.GetLength(1);
        //int width = equippedMatrix.GetLength(0);
        //int height = equippedMatrix.GetLength(1);

        //Get bounds
        Vector2 topLeftStart = initPos + (new Vector2(0, itemHeight - 1) - itemInitPos);
        Vector2 topRightStart = new Vector2(topLeftStart.x + (itemWidth - 1), topLeftStart.y);
        Vector2 bottomRightEnd = new Vector2(topRightStart.x, topRightStart.y - (itemHeight - 1));

        for (int y = (int)topLeftStart.y, yItem = itemHeight - 1; y >= bottomRightEnd.y; y--, yItem--)
        {
            for (int x = (int)topLeftStart.x, xItem = 0; x <= topRightStart.x; x++, xItem++)
            {
                
                if(ibt[xItem, yItem] == InventoryBlockType.Connected)
                {
                    positions.AddIfNotContains(new Vector2(x, y));
                }
            }
        }
    }
    //Checks if the equipped core at the specific locations has any of the listed inventory types in its item definition
    public bool HasBlockTypeAtPositions(Core core, ListHash<Vector2> positions, ListHash<InventoryBlockType> types)
    {
        //Get connectionpoint in inventory
        Vector2 initPos = equippedCores[core];

        //Get connectionpoint in item
        Vector2 itemInitPos = core.connection;
        InventoryBlockType[,] ibt = core.inventorySpace;
        int itemWidth = ibt.GetLength(0);
        int itemHeight = ibt.GetLength(1);
        //int width = equippedMatrix.GetLength(0);
        //int height = equippedMatrix.GetLength(1);

        //Get bounds
        Vector2 topLeftStart = initPos + (new Vector2(0, itemHeight - 1) - itemInitPos);
        Vector2 topRightStart = new Vector2(topLeftStart.x + (itemWidth - 1), topLeftStart.y);
        Vector2 bottomRightEnd = new Vector2(topRightStart.x, topRightStart.y - (itemHeight - 1));

        for (int y = (int)topLeftStart.y, yItem = itemHeight - 1; y >= bottomRightEnd.y; y--, yItem--)
        {
            for (int x = (int)topLeftStart.x, xItem = 0; x <= topRightStart.x; x++, xItem++)
            {
                if(types.Contains(ibt[xItem, yItem]))
                    //ibt[xItem, yItem] == InventoryBlockType.Blocked
                    //    || ibt[xItem, yItem] == InventoryBlockType.Occupied)
                {
                   // Debug.Log("Found bp, pos: <" + x + "," + y + ">");

                    if (positions.Contains(new Vector2(x, y))){
                        return true;
                    }

                }
            }
        }
        return false;

    }
    //Debug prints the inventory
    public void DebugPrintInventory()
    {
        int width = this.equippedCoreMatrix.GetLength(0);
        int height = this.equippedCoreMatrix.GetLength(1);


        for (int y = height - 1; y >= 0; y--)
        {
            string row = "";
            for (int x = 0; x < width; x++)
            {
                row += this.equippedCoreMatrix[x, y].DebugString(x, y);
            }
            Debug.Log(row);
        }
    }
    //Finds any blockers before a new leg is equipped
    public ListHash<Core> FindCoresBlockingNewLeg(Legs newLeg)
    {
        int width = this.equippedCoreMatrix.GetLength(0);
        int height = this.equippedCoreMatrix.GetLength(1);

        int heightDiff = legs.heightCapacity - newLeg.heightCapacity;
        int widthDiff = legs.widthCapacity - newLeg.widthCapacity;
        int midLeftSwitch = legs.widthCapacity % 2 == 0 && newLeg.widthCapacity % 2 != 0 ? 1 : 0;

        ListHash<Core> blockers = new ListHash<Core>();
        //These slots can't be occupied or blocked
        ListHash<Vector2> invalidOccupyOrBlockPositions = new ListHash<Vector2>();
        //These slots can't be occupied
        ListHash<Vector2> invalidOccupyPositions = new ListHash<Vector2>();
        // 
        ListHash<InventoryBlockType> occupyOrBlockHash = new ListHash<InventoryBlockType>()
        {
            {InventoryBlockType.Occupied},
            {InventoryBlockType.Blocked}
        };
        ListHash<InventoryBlockType> occupyHash = new ListHash<InventoryBlockType>()
        {
            {InventoryBlockType.Occupied}
        };

        //Find positions invalidated by height diff
        if (heightDiff > 0)
        {
            for(int hDiff = 0; hDiff <= heightDiff; hDiff++)
            {
                for(int x = 0; x < width; x++)
                {
                    int y = height - 1 - hDiff;
                    if(hDiff == heightDiff)
                    {
                        invalidOccupyPositions.AddIfNotContains(new Vector2(x, y));
                    }
                    else
                    {
                        invalidOccupyOrBlockPositions.AddIfNotContains(new Vector2(x, y));
                    }
                }
            }
        }
        //Find positions invalidated by width diff
        if (widthDiff > 0)
        {
            //Debug.Log("old legs diff: "+ legs.widthCapacity + " new legs: "+ newLeg.widthCapacity+", mid switch: "+ midLeftSwitch);
            float endLeft = Mathf.RoundToInt(((float)widthDiff) / 2f) + midLeftSwitch;
            float endRight = Mathf.RoundToInt(((float)widthDiff) / 2f);

            for (int sideDiff = 0; sideDiff <= Mathf.Max(endLeft,endRight); sideDiff++)
            {
                for (int y = 0; y < height; y++)
                {
                    int xRight = width - 1 - sideDiff;
                    int xLeft = sideDiff;

                    if(sideDiff == endLeft)
                    {
                        invalidOccupyPositions.AddIfNotContains(new Vector2(xLeft, y));
                    }
                    else
                    {
                        invalidOccupyOrBlockPositions.AddIfNotContains(new Vector2(xLeft, y));
                    }

                    if (sideDiff == endRight)
                    {
                        invalidOccupyPositions.AddIfNotContains(new Vector2(xRight, y));
                    }
                    else if(sideDiff < endRight)
                    {
                        invalidOccupyOrBlockPositions.AddIfNotContains(new Vector2(xRight, y));
                    }
                }
            }
        }
        //Check if the positions are invalidated by the shrinkage
        foreach(Core c in equippedCores)
        {
            if (HasBlockTypeAtPositions(c, invalidOccupyOrBlockPositions, occupyOrBlockHash))
            {
                blockers.AddIfNotContains(c);
            }
            if (HasBlockTypeAtPositions(c, invalidOccupyPositions, occupyHash))
            {
                blockers.AddIfNotContains(c);
            }
        }

        return blockers;
    }
    //Equips a leg
    public ListHash<Core> EquipLegs(MechItem mi)
    {
        ListHash<Core> blockers = FindCoresBlockingNewLeg((Legs)mi);
        if(blockers.Count == 0)
        {
            //Remove old legs
            this.mechItems.Remove(legs);
            //Debug.Log("EQ1");
            mount.parent = Global.References[SceneReferenceNames.Main]; //Global.instance.MAIN;
            //Hide jetpack first
            jetpack.HideAndUnequip();
            //Hide
            legs.Hide();
            //Set new legs
            legs = (Legs)mi;
            Show();
        }
        return blockers;
    }
    //Shows the mech
    public void Show()
    {
        Transform parent = owner.body;

        this.mechItems.Add(legs);
        //Equip
        Equip(legs);
        //Show
        legs.Show(Global.References[SceneReferenceNames.Main]);//Global.instance.MAIN);

        if(soundEngine == null)
        {
            soundEngine = new SoundEngine(owner);
        }

        //Detect footsteps
        legs.visualItem.gameObject.AddComponent<FootstepEventReceiver>().receiver = owner.movement; //.body.GetComponent<LegMovement>();
        //Equip the jetpack
        if (legs.movement.Contains((int)LegMovementType.Jetpack) || legs.movement.Contains((int)LegMovementType.Rolling))
        {
            jetpack.ShowAndEquipAndPlaySounds(legs);
        }
        //Set audio source
        source = GetOrSetAudioSource();

        //Add top mount to legs'        
        if (mount == null)
        {
            mount = legs.GetPointOfInterest(PointOfInterest.Grab);
        }

        //Unmount while rotating legs to forward position
        mount.parent = Global.References[SceneReferenceNames.Main]; //Global.instance.MAIN;

        //Rotate mount
        mount.rotation = legs.visualItem.rotation;
        mount.Rotate(0, -90, 0);

        //Set parent
        mount.parent = legs.GetPointOfInterest(PointOfInterest.LegMountSlot);
        mount.localPosition = legs.mountAlignment;

        //Debug.Log("Mech show10");
        CreateOrUpdateEquippedMatrix();

        //Update body
        ResizeBodyToFitEquippedGear();

        //Add highlight marking
        UpdateHighlights();
    }
    public void UpdateHighlights()
    {
        if (!owner.isPlayer)
        {
            Outline o = legs.visualItem.gameObject.GetComponent<Outline>();
            if (o != null)
            {
                o.Reload(); 
            }
            else
            {
                Outline ol = legs.visualItem.gameObject.AddComponent<Outline>();
                ol.outlineColor = SELECTION_COLOR_ENEMY;
                ol.enabled = false;
                ol.outlineWidth = SELECTION_WIDTH;
                Highlight hl = owner.shield.gameObject.AddComponent<Highlight>();
                hl.outline = ol;
                hl.owner = owner;
            }
        }
    }

    public virtual void Tick(float actionspeed, Vector3 mousePosition, Vector3 mousePositionOnZPlane, GameUnit target)
    {

        owner.mech.MountShake(actionspeed);

        foreach (Core c in owner.mech.equippedCores)
        {
            c.Tick(actionspeed, mousePosition, target, mousePositionOnZPlane);
        }
    }

    //Plays engine sounds (used by show)

    //Private method used by show
    private AudioSource GetOrSetAudioSource()
    {
        AudioSource src = owner.body.gameObject.GetComponent<AudioSource>();

        if (src == null)
        {
            src = owner.body.gameObject.AddComponent<AudioSource>();
        }
        return src;
    }
    //Gets the occupied bounds size
    public Vector2 GetCoresOccupiedSize(){
        return new Vector2(
            GetMaxOccupiedWidth()- GetLeastOccupiedWidth()
            , GetMaxOcuppiedHeight());
    }
    //Get the max height
    private float GetMaxOcuppiedHeight()
    {
        int width = this.equippedCoreMatrix.GetLength(0);
        int height = this.equippedCoreMatrix.GetLength(1);

        for (int y = height - 1; y >= 0; y--)
        {
            for (int x = 0; x < width; x++)
            {
                if (equippedCoreMatrix[x, y].occupant != null)
                {
                    return y;
                }
            }
        }
        return 0;
    }
    //Get the least width
    private float GetLeastOccupiedWidth()
    {
        int width = this.equippedCoreMatrix.GetLength(0);
        int height = this.equippedCoreMatrix.GetLength(1);

 
        for (int x = 0; x < width; x++)
            {
            for (int y = height - 1; y >= 0; y--)
            {
                if (equippedCoreMatrix[x, y].occupant != null)
                {
                    return x;
                }
            }
        }
        return 0;
    }
    //Get the max widht pos
    private float GetMaxOccupiedWidth()
    {
        int width = this.equippedCoreMatrix.GetLength(0);
        int height = this.equippedCoreMatrix.GetLength(1);


        for (int x = width-1; x > 0; x--)
        {
            for (int y = height - 1; y >= 0; y--)
            {
                if (equippedCoreMatrix[x, y].occupant != null)
                {
                    return x;
                }
            }
        }
        return 0;
    }
}

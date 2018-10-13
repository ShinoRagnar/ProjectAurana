using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public enum EffectScaling
{
    NoScaling = 10,
    SizeScaling = 20,
    BillboardScaling = 30,
    TextOverTimeScaling = 40

   // ParticleScaling
}
public enum EffectWhen
{
    Default = 10,
    Shooting = 20,
    //Hit effects
    ShieldHit = 30,
    HealthHit = 40,
    GroundHit = 50,
    //Crater effects
    Crater = 60,
    CraterCreation = 70,
    //Detachment effects
    Detach = 80,
    //Weapon
    SwingTrail = 90,
    SwingMarker = 100,
    //Shield
    Shield = 110,
    Block = 115,
    //On hit when
    OnHitLocal = 200,
    OnHitWorld = 210,
    OnDurationEnd = 250
}
public class Effect {

    private class EffectSettings
    {
        public RectTransform rect;

        public float overlapYDistance = 0;
        public float overlapXDistance = 0;
        public float overlapZDistance = 0;

        public Vector3 startPos = Vector3.zero;

        public ListHash<Transform> allowedTransforms;// = new List<string>();

        public bool cleanedUp = false;

        public EffectSettings(float yoverlap = 0, float xoverlap = 0, float zoverlap = 0)
        {
            this.overlapYDistance = yoverlap;
            this.overlapXDistance = xoverlap;
            this.overlapZDistance = zoverlap;
        }
    }

    public static readonly float MIN_OVERLAP_DISTANCE = 0.001f;

    //Permanent
    private Stack<Transform> available = new Stack<Transform>();
    private DictionaryList<Transform, EffectSettings> settings = new DictionaryList<Transform, EffectSettings>();

    //Temporary
    private DictionaryList<Transform, float> times = new DictionaryList<Transform, float>();
    private List<Transform> handles = new List<Transform>();

    private List<GameObject> toDestroy = new List<GameObject>();

    private Transform effect;
    private Transform[] effectVariations;

    private float playtime;
    public bool modeHandles;
    private EffectScaling scaling;
    private bool randomZRotation;
    private bool yOverlapHasDistance;
    private bool xOverlapHasDistance;
    private bool zOverlapHasDistance;
    private bool cleanUpInnerTransforms = false;
    private float cleanUpTime = 0;
    private float currentYOverlapDistance = 0;
    private float currentXOverlapDistance = 0;
    private float currentZOverlapDistance = 0;
    // private int activeEffects = 0;

    private bool isFirstSpawn = true;
    private Vector2 rectStartSize = Vector2.zero;

    private bool isStatic = false;

    private int currentVariationPos = -1;



    public Effect(EffectData data): this(
        Global.Resources[data.prefab], 
        data.duration, 
        data.handles, 
        data.scaling, 
        data.randomZRotation, 
        data.hasYDistance, 
        data.hasXDistance, 
        data.hasZDistance, 
        data.isStatic,
        data.cleanupInnerTransforms,
        data.cleanupStartAt,
        PrefabNamesArrayToTransformArray(data.prefabAlterations)
        )
    {}

    public Effect(Transform effectVal,
                        float playtimeVal,
                        bool handles = false,
                        EffectScaling effectScaling = EffectScaling.NoScaling,
                        bool randomZRotationVal = false,
                        bool yOverlapHasDistanceVal = false,
                        bool xOverlapHasDistanceVal = false,
                        bool zOverlapHasDistanceVal = false,
                        bool isStatic = false,
                        bool cleanupInnerTransforms = false,
                        float cleanupTime = 0,
                        Transform[] effectVariationsVal = null
        )
    {
        this.cleanUpTime = cleanupTime;
        this.cleanUpInnerTransforms = cleanupInnerTransforms;
        this.effectVariations = effectVariationsVal;
        this.effect = effectVal;
        this.playtime = playtimeVal;
        this.modeHandles = handles;
        this.scaling = effectScaling;
        this.randomZRotation = randomZRotationVal;
        this.yOverlapHasDistance = yOverlapHasDistanceVal;
        this.xOverlapHasDistance = xOverlapHasDistanceVal;
        this.zOverlapHasDistance = zOverlapHasDistanceVal;
        this.isStatic = isStatic;
    }

    public void Update()
    {
        if(times.Count > 0)
        {
            foreach (Transform t in times)
            {
                float tim = times[t];
                tim += Time.deltaTime;

                //Cleanup anything that may have been added to the effect
                if (cleanUpInnerTransforms && !settings[t].cleanedUp && (tim > cleanUpTime || tim > playtime))
                {
                    foreach (Transform child in t)
                    {
                        if (!settings[t].allowedTransforms.Contains(child))
                        {
                            toDestroy.Add(child.gameObject);
                        }
                    }
                    foreach (GameObject g in toDestroy)
                    {
                        Global.Destroy(g);
                    }
                    toDestroy.Clear();
                    settings[t].cleanedUp = true;
                }
                //Push the effect back on the stack when its time is up
                if (tim > playtime)
                {
                    times.RemoveLater(t);
                    t.gameObject.SetActive(false);
                    available.Push(t);

                //Text scaling changes over time
                }else if(scaling == EffectScaling.TextOverTimeScaling)
                {
                    float prcntg = Mathf.Min(tim / playtime,1);
                    prcntg = Mathf.Sin(prcntg * ((Mathf.PI*3)/2));
                    settings[t].rect.sizeDelta = rectStartSize * (0.5f+((prcntg+1)/2)*0.5f);
                    t.position = settings[t].startPos + new Vector3(0, rectStartSize.y * ((prcntg > 0) ? prcntg/2 : prcntg));
                }
                times[t] = tim;
            }
            times.Remove();
        }

        /*if(times.Count > 0)
        {
            Debug.Log("Effect: " + available.Count + " " + times.Count);
        }*/

    }
    public bool ReturnHandle(Transform transform)
    {
        if (handles.Contains(transform))
        {
            handles.Remove(transform);
            times.Add(transform, 0);
            transform.parent = Global.References[SceneReferenceNames.NodeClone]; //Global.instance.X_CLONES;
            return true;
        }
        return false;
    }



    public Transform Spawn(
        Transform parent,
        float xRot = 0,
        float yRot = 0,
        float zRot = 0,
        float scaleFactor = 1,
        string text = null)
    {
        return Spawn(parent.position,parent,xRot,yRot,zRot,scaleFactor,text);
    }

    public Transform Spawn(
        Vector3 position,
        Transform parent = null,
        float xRot = 0, 
        float yRot = 0, 
        float zRot = 0,
        float scaleFactor = 1,
        string text = null)
    {
        Transform spawned = null;
        if(available.Count > 0)
        {
            spawned = available.Pop();
            if(parent != null)
            {
                spawned.SetParent(parent,true);
            }
        }
        else
        {
            
            spawned = Global.PoolOrCreate(
                currentVariationPos == -1 ? effect : effectVariations[currentVariationPos]
                , parent == null ? Global.References[SceneReferenceNames.NodeClone] : parent);

            if(effectVariations != null)
            {
                currentVariationPos++;
                if(currentVariationPos >= effectVariations.Length)
                {
                    currentVariationPos = -1;
                }
            }


            //Add overlap for each added effect
            if (yOverlapHasDistance){
                currentYOverlapDistance += MIN_OVERLAP_DISTANCE;
            }
            if (xOverlapHasDistance)
            {
                currentXOverlapDistance += MIN_OVERLAP_DISTANCE;
            }
            if (zOverlapHasDistance)
            {
                currentZOverlapDistance += MIN_OVERLAP_DISTANCE;
            }
        }


        if (isStatic)
        {
            spawned.gameObject.isStatic = isStatic;
        }

        //Add settings
        settings.AddIfNotContains(spawned, new EffectSettings(currentYOverlapDistance,currentXOverlapDistance,currentZOverlapDistance));

        //Make sure we keep only the original transforms
        if (cleanUpInnerTransforms)
        {
            settings[spawned].allowedTransforms = new ListHash<Transform>();
            settings[spawned].cleanedUp = false;
            Global.FindAllNames(settings[spawned].allowedTransforms,spawned,false);
        }

        //Requires us to return the item before we start count down for release
        if (modeHandles)
        {
            handles.Add(spawned);
        }
        //Counts down for release
        else
        {
            times.Add(spawned, 0);
        }
        if (randomZRotation)
        {
            zRot = Random.Range(-360, 360);
        }

        spawned.position = position+new Vector3(
            settings[spawned].overlapXDistance, 
            settings[spawned].overlapYDistance, 
            settings[spawned].overlapZDistance);

        settings[spawned].startPos = spawned.position;

        spawned.rotation = Quaternion.Euler(xRot, yRot, zRot);

        if(scaling == EffectScaling.SizeScaling || scaling == EffectScaling.BillboardScaling)
        {
            spawned.localScale = new Vector3(scaleFactor,scaleFactor,scaleFactor);
        }
        if (scaling == EffectScaling.BillboardScaling)
        {
            spawned.position += new Vector3(0, scaleFactor / 2);

        }
        if (scaling == EffectScaling.TextOverTimeScaling)
        {
            RectTransform rt = spawned.GetComponent<RectTransform>();
            if (rt != null)
            {
                if (isFirstSpawn)
                {
                    rectStartSize = rt.sizeDelta;
                }
                else
                {
                    rt.sizeDelta = rectStartSize;
                }
                settings[spawned].rect = rt;
            }

        }

        //Text on text effects
        if(text != null)
        {
            TextMeshPro tex = spawned.GetComponent<TextMeshPro>();
            if(tex != null)
            {
                tex.text = text;
            }
        }

        spawned.gameObject.SetActive(true);
        isFirstSpawn = false;

        return spawned;
    }

    public static Transform[] PrefabNamesArrayToTransformArray(PrefabNames[] names)
    {
        if (names != null)
        {
            List<Transform> prefabs = new List<Transform>();
            foreach (PrefabNames nam in names)
            {
                prefabs.Add(Global.Resources[nam]);
            }
            return prefabs.ToArray();
        }
        return null;
    }

}
/*private class InventoryEffect
{
    public static readonly float SELECTION_TRAIL_LAP_TIME = 5;
    public Transform selectionTrail;
    public Vector3 selectionTrailCurrentPosition;
    public Vector2[] selectionTrailPath;
    public float currentSelectionTrailProgress = 0;
    public MechItem target;
    public Effect effect;
    public bool moving;

    public InventoryEffect(Effect effectVal, MechItem targetVal, Transform panel, bool movingVal = true, float startPercentage = 0)
    {
        target = targetVal;
        effect = effectVal;
        moving = movingVal;

        selectionTrail = effectVal.Spawn(panel);

        //selectionTrail = Global.instance.EFFECT_SELECTION_TRAIL.Spawn(Global.instance.PANEL_ON_MOUSE_TOP_EFFECTS);
        selectionTrailPath = CalculateSelectionPath(targetVal);
        currentSelectionTrailProgress = SELECTION_TRAIL_LAP_TIME * startPercentage;
        selectionTrailCurrentPosition = new Vector2(0, 0);
    }

    public void UpdateEffect(Vector3 pos)
    {

        //onMouseItem.inventoryItem.visualItem.position;
        selectionTrail.position = pos;

        if (moving)
        {

            float laptime = (SELECTION_TRAIL_LAP_TIME / selectionTrailPath.Length);
            float progress = currentSelectionTrailProgress / laptime;
            float t = (currentSelectionTrailProgress - Mathf.FloorToInt(progress) * laptime) / laptime;
            int currentTarget = Mathf.FloorToInt(1 + progress) % (selectionTrailPath.Length);
            int previousTarget = Mathf.FloorToInt(selectionTrailPath.Length + progress) % (selectionTrailPath.Length);

            currentSelectionTrailProgress += Time.deltaTime;


            selectionTrailCurrentPosition =
                Vector3.Lerp(selectionTrailPath[previousTarget], selectionTrailPath[currentTarget], t);

            selectionTrail.localPosition += selectionTrailCurrentPosition;
        }
        else
        {
            selectionTrail.localPosition += new Vector3(
                target.inventoryWidth * SLOT_WIDTH / 2,
                -target.inventoryHeight * SLOT_HEIGHT / 2);
        }
    }

    public void EndEffect()
    {
        effect.ReturnHandle(selectionTrail);
    }


    private Vector2[] CalculateSelectionPath(MechItem mi)
    {
        float width = mi.inventoryWidth * SLOT_WIDTH;
        float height = mi.inventoryHeight * SLOT_HEIGHT;
        //Debug.Log("Creating trail with: " + width + " height: " + height);
        return new Vector2[] {
            new Vector2(0,0),
            new Vector2(width,0),
            new Vector2(width,-height),
            new Vector2(0,-height)
            };

    }

}*/
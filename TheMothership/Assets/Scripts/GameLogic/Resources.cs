using System;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

[CreateAssetMenu(fileName = "NewResource", menuName = "Game/Resource", order = 3)]
public class Resources : ScriptableObject {


    [Serializable]
    public struct NamedPrefab
    {
        public string name;
        public Transform prefab;
    }
    [Serializable]
    public struct NamedSprite
    {
        public string name;
        public Sprite prefab;
    }
    [Serializable]
    public struct NamedAudio
    {
        public string name;
        public AudioClip[] prefab;
    }
    [Serializable]
    public struct NamedMaterial
    {
        public string name;
        public Material prefab;
    }
    /*[Serializable]
    public struct NamedCore
    {
        public string name;
        public CoreData data;
    }*/

    //Prefab
    private DictionaryList<int, Transform> prefabDict = null;
    private DictionaryList<int, Sprite> spriteDict = null;
    private DictionaryList<int, Material> materialDict = null;
    private DictionaryList<int, AudioClip[]> audioDict = null;

    //Created
    private DictionaryList<int, Item> itemDict = null;
    private DictionaryList<int, Core> coreDict = null;

    private DictionaryList<int, AudioContainer> audioContainerDict = null;

    private DictionaryList<int, Buff> buffDict = null;
    private DictionaryList<int, Effect> effectDict = null;
    private DictionaryList<int, Bullet> bulletDict = null;
    private DictionaryList<int, Gun> gunDict = null;
    private DictionaryList<int, Crystal> crystalDict = null;
    private DictionaryList<int, Legs> legDict = null;
    private DictionaryList<int, Socketable> socketbleDict = null;
    private DictionaryList<int, MechData> mechDict = null;
    private DictionaryList<int, GunArray> gunArrayDict = null;
    private DictionaryList<int, Weapon> weaponDict = null;
    private DictionaryList<int, Shield> shieldDict = null;
    private DictionaryList<int, OnHit> onHitDict = null;

    // private DictionaryList<int, Effect> effectDict = null;*/



    //Used in inspector to create new items
    [Header("Prefabs")]
    [SerializeField]
    public NamedPrefab[] menues;
    [SerializeField]
    public NamedPrefab[] cores;
    [SerializeField]
    public NamedPrefab[] attachments;
    [SerializeField]
    public NamedPrefab[] legs;
    [SerializeField]
    public NamedPrefab[] socketables;
    [SerializeField]
    public NamedPrefab[] inventory;
    [SerializeField]
    public NamedPrefab[] ui;
    [SerializeField]
    public NamedPrefab[] buffs;
    [SerializeField]
    public NamedPrefab[] effects;
    [SerializeField]
    public NamedPrefab[] characters;
    [SerializeField]
    public NamedPrefab[] other;

    [Header("Sounds")]
    [SerializeField]
    public NamedAudio[] inventorySounds;
    [SerializeField]
    public NamedAudio[] gameSounds;
    [SerializeField]
    public NamedAudio[] uiSounds;

    [Header("Materials")]
    [SerializeField]
    public NamedMaterial[] materials;

    [Header("Sprites")]
    [SerializeField]
    public NamedSprite[] sprites;
    [SerializeField]
    public NamedSprite[] buffSprites;

    [Space(10)]
    [Header("Items")]
    [SerializeField]
    public ItemData[] items;

    [Space(10)]
    [Header("Audio")]
    [SerializeField]
    public AudioContainerData[] audioContainer;

    [Space(10)]
    [Header("Buffs")]
    [SerializeField]
    public BuffData[] buff;

    [Space(10)]
    [Header("Effect")]
    [SerializeField]
    public EffectData[] effect;

    [Space(10)]
    [Header("Bullet")]
    [SerializeField]
    public BulletData[] bullet;

    [Space(10)]
    [Header("OnHit")]
    [SerializeField]
    public OnHitData[] onHit;

    [Space(10)]
    [Header("MechItems")]
    [SerializeField]
    public CoreData[] mechCores;
    [SerializeField]
    public GunData[] gun;
    [SerializeField]
    public GunArrayData[] gunarray;
    [SerializeField]
    public CrystalData[] crystal;
    [SerializeField]
    public LegData[] leg;
    [SerializeField]
    public WeaponData[] weapons;
    [SerializeField]
    public ShieldData[] shields;

    [Space(10)]
    [Header("Mechs")]
    [SerializeField]
    public MechData[] mech;

    [Space(10)]
    [Header("Imports")]
    [SerializeField]
    public UnityEngine.Object[] mechImports;


    //[Header("MechItems")]
    //public MechItemData[] items;

    //Returns the prefab by name
    public Transform this[PrefabNames t]
    {
        
        get
        {
            return prefabDict[(int)t];
        }
        set { }
    }
    public Sprite this[SpriteNames t]
    {
        get
        {
            return spriteDict[(int)t];
        }
        set { }
    }
    public AudioClip[] this[AudioNames t]
    {
        get
        {
            return audioDict[(int)t];
        }
        set { }
    }
    public Material this[MaterialNames t]
    {
        get
        {
            return materialDict[(int)t];
        }
        set { }
    }
    public Item this[ItemNames t]
    {
        get
        {
            return itemDict[(int)t].Clone();
        }
        set { }
    }
    public List<Buff> this[BuffNames[] t]
    {
        get
        {
            List<Buff> ret = new List<Buff>();
            foreach(BuffNames nam in t)
            {
                ret.Add(this[nam]);
            }
            return ret;
        }
        set { }
    }
    public Socketable this[SocketableNames t]
    {
        get
        {
            return socketbleDict[(int)t].CloneSocketable();
        }
        set { }
    }
    public Core this[CoreNames t]
    {
        get
        {
            return coreDict[(int)t].Clone();
        }
        set { }
    }
    public AudioContainer this[AudioContainerNames t]
    {
        get
        {
            return audioContainerDict[(int)t];
        }
        set { }
    }
    public Buff this[BuffNames t]
    {
        get
        {
            return buffDict[(int)t].Clone();
        }
        set { }
    }
    public Effect this[EffectNames t]
    {
        get
        {
            Effect ret = effectDict[(int)t];
            Global.AddEffect(ret);
            return ret;
        }
        set { }
    }
    public Bullet this[BulletNames t]
    {
        get
        {
            return bulletDict[(int)t].Clone();
        }
        set { }
    }
    public Gun this[GunNames t]
    {
        get
        {
            return gunDict[(int)t].Clone();
        }
        set { }
    }
    public Crystal this[CrystalNames t]
    {
        get
        {
            return crystalDict[(int)t].Clone();
        }
        set { }
    }
    public Weapon this[WeaponNames t]
    {
        get
        {
            return weaponDict[(int)t].Clone();
        }
        set { }
    }
    public Shield this[ShieldNames t]
    {
        get
        {
            return shieldDict[(int)t].Clone();
        }
        set { }
    }
    public GunArray this[GunArrayNames t]
    {
        get
        {
            return gunArrayDict[(int)t].Clone();
        }
        set { }
    }
    public Legs this[LegNames t]
    {
        get
        {
            return legDict[(int)t].Clone();
        }
        set { }
    }
    public Mech this[MechNames t, GameUnit gu, string name]
    {
        get
        {
            return new Mech(mechDict[(int)t],gu,name);
        }
        set { }
    }
    public OnHit this[OnHitNames t]
    {
        get
        {
            return onHitDict[(int)t].Clone();
        }
        set { }
    }
    public OnHit[] this[OnHitNames[] t]
    {
        get
        {
            if(t == null)
            {
                return null;
            }
            OnHit[] hits = new OnHit[t.Length];

            for(int i = 0; i < t.Length; i++)
            {
                hits[i] = onHitDict[(int)t[i]].Clone();
            }
            return hits;
        }
        set { }
    }

    public void Clear()
    {
        prefabDict = new DictionaryList<int, Transform>();
        spriteDict = new DictionaryList<int, Sprite>();
        materialDict = new DictionaryList<int, Material>();
        audioDict = new DictionaryList<int, AudioClip[]>();
        itemDict = new DictionaryList<int, Item>();
        coreDict = new DictionaryList<int, Core>();
        audioContainerDict = new DictionaryList<int, AudioContainer>();
        buffDict = new DictionaryList<int, Buff>();
        effectDict = new DictionaryList<int, Effect>();
        bulletDict = new DictionaryList<int, Bullet>();
        gunDict = new DictionaryList<int, Gun>();
        crystalDict = new DictionaryList<int, Crystal>();
        legDict = new DictionaryList<int, Legs>();
        socketbleDict = new DictionaryList<int, Socketable>();
        mechDict = new DictionaryList<int, MechData>();
        gunArrayDict = new DictionaryList<int, GunArray>();
        weaponDict = new DictionaryList<int, Weapon>();
        shieldDict = new DictionaryList<int, Shield>();
        onHitDict = new DictionaryList<int, OnHit>();
    }

    //Init
    public void Init()
    {
        //Debug.Log("[Resources] Init started");

        Clear();

        //Prefabs
        Add(menues);
        Add(cores);
        Add(attachments);
        Add(legs);
        Add(socketables);
        Add(inventory);
        Add(ui);
        Add(buffs);
        Add(effects);
        Add(characters);
        Add(other);
        //Sounds
        Add(inventorySounds);
        Add(gameSounds);
        Add(uiSounds);
        //Materials
        Add(materials);
        Add(sprites);
        Add(buffSprites);
        //Audio
        Add(audioContainer);
        //Buffs
        Add(buff);
        //Effect
        Add(effect);
        //OnHit
        Add(onHit);
        //Items
        Add(items);
        //Bullet
        Add(bullet);
        //Cores
        Add(mechCores);
        //Guns
        Add(gun);
        //Crystal
        Add(crystal);
        //Weapons
        Add(weapons);
        //Shields
        Add(shields);
        //Gunarrays
        Add(gunarray);
        //Leg
        Add(leg);
        //Mech
        Add(mech);


        //Debug.Log("[Resources]  Added names to dictionaries");

    }
    //Adds for later reference
    public void Add(NamedPrefab[] prefabList)
    {
        foreach (NamedPrefab pref in prefabList)
        {
            if (pref.name != null && pref.name.Length > 0)
            {
                try
                {
                    prefabDict.Add(
                        (int)((PrefabNames)Enum.Parse(typeof(PrefabNames), pref.name.Trim()))
                        , pref.prefab
                        );

                }
                catch (Exception)
                {
                    Debug.LogError("Could not find enum: " + pref.name.Trim() + "Did you forget to save?");
                }
            }

        }
    }
    public void Add(NamedSprite[] prefabList)
    {
        foreach (NamedSprite pref in prefabList)
        {
            if (pref.name != null && pref.name.Length > 0)
            {
                try
                {
                    spriteDict.Add(
                        (int)((SpriteNames)Enum.Parse(typeof(SpriteNames), pref.name.Trim()))
                        , pref.prefab
                        );

                }
                catch (Exception)
                {
                    Debug.LogError("Could not find enum: " + pref.name.Trim() + "Did you forget to save?");
                }
            }
        }
    }
    public void Add(NamedMaterial[] prefabList)
    {
        foreach (NamedMaterial pref in prefabList)
        {
            if (pref.name != null && pref.name.Length > 0)
            {
                try
                {
                    materialDict.Add(
                        (int)((MaterialNames)Enum.Parse(typeof(MaterialNames), pref.name.Trim()))
                        , pref.prefab
                        );

                }
                catch (Exception)
                {
                    Debug.LogError("Could not find enum: " + pref.name.Trim() + "Did you forget to save?");
                }
            }
        }
    }
    public void Add(NamedAudio[] prefabList)
    {
        foreach (NamedAudio pref in prefabList)
        {
            if (pref.name != null && pref.name.Length > 0)
            {
                try
                {
                    //Debug.Log(pref.name + " was parsed to " + ((AudioNames)Enum.Parse(typeof(AudioNames), pref.name)));
                    audioDict.Add(
                        (int)((AudioNames)Enum.Parse(typeof(AudioNames), pref.name.Trim()))
                        , pref.prefab
                        );

                }
                catch (Exception)
                {
                    Debug.LogError("Could not find enum: " + pref.name.Trim() + "Did you forget to save?");
                }
            }
        }
    }
    public void Add(ItemData[] itemList)
    {
       foreach (ItemData item in itemList)
        {
            Item i = null;

            try
            {
                i = new Item(item);
            }
            catch (Exception)
            {
                Debug.LogError("Error loading: " + item.itemName.Trim() + "Did you forget to save?");
            }

            try
            {
                //Debug.Log(pref.name + " was parsed to " + ((AudioNames)Enum.Parse(typeof(AudioNames), pref.name)));
                itemDict.Add(
                    (int)((ItemNames)Enum.Parse(typeof(ItemNames), item.itemName.Trim())),i

                    );

            }
            catch (Exception)
            {
                Debug.LogError("Could not find enum: " + item.itemName.Trim() + "Did you forget to save?");
            }
        }
    }
    public void Add(CoreData[] coreList)
    {
       foreach (CoreData core in coreList)
        {
            Core c = null;
            CoreNames nam = CoreNames.NoName;

            try
            {
                nam = ((CoreNames)Enum.Parse(typeof(CoreNames), core.enumName.Trim()));
            }
            catch (Exception e)
            {
                Debug.LogError("Could not find enum: " + core.enumName.Trim() + " Did you forget to save?");
                Debug.LogError(e.StackTrace);
            }

            try
            {
                c = new Core(core,nam);
            }
            catch (Exception e)
            {
                Debug.LogError("Unable to create core for: "+core.enumName);
                Debug.LogError(e.Message);
                Debug.LogError(e.StackTrace);
            }

            coreDict.Add((int)nam, c);
        }
    }
    public void Add(OnHitData[] onhitlist)
    {
        foreach (OnHitData ohd in onhitlist)
        {
            OnHit c = null;
            
            try
            {
                c = new OnHit(ohd, ((OnHitNames)Enum.Parse(typeof(OnHitNames), ohd.enumName.Trim())));
            }
            catch (Exception e)
            {
                Debug.LogError("Unable to create onhit for: " + ohd.enumName);
                Debug.LogError(e.Message);
                Debug.LogError(e.StackTrace);
            }

            onHitDict.Add((int)c.origin, c);
        }
    }
    public void Add(GunArrayData[] coreList)
    {
        foreach (GunArrayData acd in coreList)
        {
            GunArray b = null;

            SocketableNames sock = SocketableNames.NothingSocketed;

            try
            {
                sock = ((SocketableNames)Enum.Parse(typeof(SocketableNames), acd.enumName.Trim()));
            }
            catch (Exception e)
            {
                Debug.LogError("[Resources] Unable to find crystal for: " + acd.name);
                Debug.LogError(e.Message);
                Debug.LogError(e.StackTrace);
            }

            try
            {
                b = new GunArray(acd, sock);
            }
            catch (Exception e)
            {
                Debug.LogError("[Resources] Unable to create crystal for: " + acd.name);
                Debug.LogError(e.Message);
                Debug.LogError(e.StackTrace);
            }

            try
            {
                gunArrayDict.Add((int)((GunArrayNames)Enum.Parse(typeof(GunArrayNames), acd.enumName.Trim())), b);
                socketbleDict.Add((int)sock, b);
            }
            catch (Exception e)
            {
                Debug.LogError("Could not find enum: " + acd.name.Trim() + " Did you forget to save?");
                Debug.LogError(e.StackTrace);
            }
        }
    }
    public void Add(AudioContainerData[] audioData)
    {
        foreach (AudioContainerData acd in audioData)
        {
            AudioContainer c = null;

            try
            {
                c = new AudioContainer(acd);
            }
            catch (Exception e)
            {
                Debug.LogError("[Resources] Unable to create audio container data for: " + acd.enumName);
                Debug.LogError(e.Message);
                Debug.LogError(e.StackTrace);
            }

            try
            {
                audioContainerDict.Add((int)((AudioContainerNames)Enum.Parse(typeof(AudioContainerNames), acd.enumName.Trim())), c);
            }
            catch (Exception e)
            {
                Debug.LogError("Could not find enum: " + acd.enumName.Trim() + " Did you forget to save?");
                Debug.LogError(e.StackTrace);
            }
        }
    }
    public void Add(BuffData[] buffData)
    {
        foreach (BuffData acd in buffData)
        {
            Buff b = null;

            try
            {
                b = new Buff(acd, ((BuffNames)Enum.Parse(typeof(BuffNames), acd.enumName.Trim())));
            }
            catch (Exception e)
            {
                Debug.LogError("[Resources] Unable to create buff for: " + acd.enumName);
                Debug.LogError(e.Message);
                Debug.LogError(e.StackTrace);
            }

            try
            {
                buffDict.Add((int)((BuffNames)Enum.Parse(typeof(BuffNames), acd.enumName.Trim())), b);
            }
            catch (Exception e)
            {
                Debug.LogError("Could not find enum: " + acd.enumName.Trim() + " Did you forget to save?");
                Debug.LogError(e.StackTrace);
            }
        }
    }
    public void Add(EffectData[] effectData)
    {
        foreach (EffectData acd in effectData)
        {
            Effect b = null;

            try
            {
                b = new Effect(acd);
            }
            catch (Exception e)
            {
                Debug.LogError("[Resources] Unable to create effect for: " + acd.name);
                Debug.LogError(e.Message);
                Debug.LogError(e.StackTrace);
            }

            try
            {
                effectDict.Add((int)((EffectNames)Enum.Parse(typeof(EffectNames), acd.enumName.Trim())), b);
            }
            catch (Exception e)
            {
                Debug.LogError("Could not find enum: " + acd.name.Trim() + " Did you forget to save?");
                Debug.LogError(e.StackTrace);
            }
        }
    }
    public void Add(BulletData[] effectData)
    {
        foreach (BulletData acd in effectData)
        {
            Bullet b = null;

            try
            {
                b = new Bullet(acd);
            }
            catch (Exception e)
            {
                Debug.LogError("[Resources] Unable to create bullet for: " + acd.name);
                Debug.LogError(e.Message);
                Debug.LogError(e.StackTrace);
            }

            try
            {
                //TODO add bullet
                bulletDict.Add((int)((BulletNames)Enum.Parse(typeof(BulletNames), acd.enumName.Trim())), b);
            }
            catch (Exception e)
            {
                Debug.LogError("Could not find enum: " + acd.name.Trim() + " Did you forget to save?");
                Debug.LogError(e.StackTrace);
            }
        }
    }
    public void Add(GunData[] effectData)
    {
        foreach (GunData acd in effectData)
        {
            Gun b = null;
            SocketableNames sock = SocketableNames.NothingSocketed;

            try
            {
                sock = ((SocketableNames)Enum.Parse(typeof(SocketableNames), acd.enumName.Trim()));
            }
            catch (Exception e)
            {
                Debug.LogError("[Resources] Unable to find crystal for: " + acd.name);
                Debug.LogError(e.Message);
                Debug.LogError(e.StackTrace);
            }
            try
            {
                b = new Gun(acd,sock);
            }
            catch (Exception e)
            {
                Debug.LogError("[Resources] Unable to create gun for: " + acd.name);
                Debug.LogError(e.Message);
                Debug.LogError(e.StackTrace);
            }

            try
            {
                //TODO add bullet
                gunDict.Add((int)((GunNames)Enum.Parse(typeof(GunNames), acd.enumName.Trim())), b);
                socketbleDict.Add((int)sock, b);
            }
            catch (Exception e)
            {
                Debug.LogError("Could not find enum: " + acd.name.Trim() + " Did you forget to save?");
                Debug.LogError(e.StackTrace);
            }
        }
    }
    public void Add(CrystalData[] effectData)
    {
        foreach (CrystalData acd in effectData)
        {
            Crystal b = null;

            SocketableNames sock = SocketableNames.NothingSocketed;

            try
            {
                sock = ((SocketableNames)Enum.Parse(typeof(SocketableNames), acd.enumName.Trim()));
            }
            catch(Exception e)
            {
                Debug.LogError("[Resources] Unable to find crystal for: " + acd.name);
                Debug.LogError(e.Message);
                Debug.LogError(e.StackTrace);
            }

            try
            {
                b = new Crystal(acd, sock);
            }
            catch (Exception e)
            {
                Debug.LogError("[Resources] Unable to create crystal for: " + acd.name);
                Debug.LogError(e.Message);
                Debug.LogError(e.StackTrace);
            }

            try
            {
                crystalDict.Add((int)((CrystalNames)Enum.Parse(typeof(CrystalNames), acd.enumName.Trim())), b);
                socketbleDict.Add((int)sock, b);
            }
            catch (Exception e)
            {
                Debug.LogError("Could not find enum: " + acd.name.Trim() + " Did you forget to save?");
                Debug.LogError(e.StackTrace);
            }
        }
    }
    public void Add(WeaponData[] effectData)
    {
        foreach (WeaponData acd in effectData)
        {
            Weapon b = null;

            SocketableNames sock = SocketableNames.NothingSocketed;

            try
            {
                sock = ((SocketableNames)Enum.Parse(typeof(SocketableNames), acd.enumName.Trim()));
            }
            catch (Exception e)
            {
                Debug.LogError("[Resources] Unable to find weapon for: " + acd.name);
                Debug.LogError(e.Message);
                Debug.LogError(e.StackTrace);
            }

            try
            {
                b = new Weapon(acd, sock);
            }
            catch (Exception e)
            {
                Debug.LogError("[Resources] Unable to create weapon for: " + acd.name);
                Debug.LogError(e.Message);
                Debug.LogError(e.StackTrace);
            }

            try
            {
                weaponDict.Add((int)((WeaponNames)Enum.Parse(typeof(WeaponNames), acd.enumName.Trim())), b);
                socketbleDict.Add((int)sock, b);
            }
            catch (Exception e)
            {
                Debug.LogError("Could not find enum: " + acd.name.Trim() + " Did you forget to save?");
                Debug.LogError(e.StackTrace);
            }
        }
    }
    public void Add(ShieldData[] effectData)
    {
        foreach (ShieldData acd in effectData)
        {
            Shield b = null;

            SocketableNames sock = SocketableNames.NothingSocketed;

            try
            {
                sock = ((SocketableNames)Enum.Parse(typeof(SocketableNames), acd.enumName.Trim()));
            }
            catch (Exception e)
            {
                Debug.LogError("[Resources] Unable to find shield for: " + acd.name);
                Debug.LogError(e.Message);
                Debug.LogError(e.StackTrace);
            }

            try
            {
                b = new Shield(acd, sock);
            }
            catch (Exception e)
            {
                Debug.LogError("[Resources] Unable to create shield for: " + acd.name);
                Debug.LogError(e.Message);
                Debug.LogError(e.StackTrace);
            }

            try
            {
                shieldDict.Add((int)((ShieldNames)Enum.Parse(typeof(ShieldNames), acd.enumName.Trim())), b);
                socketbleDict.Add((int)sock, b);
            }
            catch (Exception e)
            {
                Debug.LogError("Could not find enum: " + acd.name.Trim() + " Did you forget to save?");
                Debug.LogError(e.StackTrace);
            }
        }
    }
    public void Add(LegData[] effectData)
    {
        foreach (LegData acd in effectData)
        {
            Legs c = null;
            LegNames nam = LegNames.NoName;

            try
            {
                nam = ((LegNames)Enum.Parse(typeof(LegNames), acd.enumName.Trim()));
            }
            catch (Exception e)
            {
                Debug.LogError("Could not find enum: " + acd.enumName.Trim() + " Did you forget to save?");
                Debug.LogError(e.StackTrace);
            }
            try
            {
                c = new Legs(acd, nam);
            }
            catch (Exception e)
            {
                Debug.LogError("Unable to create legs for: " + acd.enumName);
                Debug.LogError(e.Message);
                Debug.LogError(e.StackTrace);
            }

            legDict.Add((int)nam, c);
        }
    }
    public void Add(MechData[] effectData)
    {
        foreach (MechData acd in effectData)
        {
            mechDict.Add((int)((MechNames)Enum.Parse(typeof(MechNames), acd.enumName.Trim())), acd);
        }
    }




    //Make sure to init at startup
    /*public void Awake()
    {
        Init();
    }*/

    /*public static T[] GetAtPath<T>(string path)
    {

        ArrayList al = new ArrayList();
        string[] fileEntries = Directory.GetFiles(Application.dataPath + "/" + path);
        foreach (string fileName in fileEntries)
        {
            int index = fileName.LastIndexOf("/");
            string localPath = "Assets/" + path;

            if (index > 0)
                localPath += fileName.Substring(index);

            UnityEngine.Object t = UnityEditor.AssetDatabase.LoadAssetAtPath(localPath, typeof(T));//Resources.LoadAssetAtPath();

            if (t != null)
                al.Add(t);
        }
        T[] result = new T[al.Count];
        for (int i = 0; i < al.Count; i++)
            result[i] = (T)al[i];

        return result;
    }*/
}

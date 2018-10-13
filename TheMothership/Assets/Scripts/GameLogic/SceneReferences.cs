using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SceneReferences : MonoBehaviour {

    [Serializable]
    public struct NamedPrefab
    {
        public string name;
        public Transform prefab;
    }
    /*[Serializable]
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
    }*/

    //Prefabs used by global // private DictionaryList<PrefabNames, Transform> 
    private DictionaryList<int, Transform> prefabDict = null;
   /* private DictionaryList<int, Sprite> spriteDict = null;
    private DictionaryList<int, Material> materialDict = null;
    private DictionaryList<int, AudioClip[]> audioDict = null;*/


    //[Header("Prefabs")]
    //public Resources resource;

    //Used in inspector to create new items
    [Header("Scene References")]
    public NamedPrefab[] panels;
    public NamedPrefab[] nodes;
    /*
    public NamedPrefab[] menues;
    
    public NamedPrefab[] cores;
    public NamedPrefab[] attachments;
    public NamedPrefab[] legs;
    public NamedPrefab[] socketables;
    public NamedPrefab[] inventory;
    public NamedPrefab[] ui;
    public NamedPrefab[] buffs;
    public NamedPrefab[] effects;
    public NamedPrefab[] characters;
    public NamedPrefab[] other;

    [Header("Sounds")]
    public NamedAudio[] inventorySounds;
    public NamedAudio[] gameSounds;
    public NamedAudio[] uiSounds;

    [Header("Materials")]
    public NamedMaterial[] materials;

    [Header("Sprites")]
    public NamedSprite[] sprites;
    */
    //Returns the prefab by name
    public Transform this[SceneReferenceNames t]
    {
        get
        {
            if(prefabDict[(int)t] == null){
                Debug.LogError("[SceneReferences].this[SceneReferenceNames t] Missing reference: " + t);
            }
            return prefabDict[(int)t];
        }
        set {  }
    }
    /*
    //Returns sprite by name
    public Sprite this[SpriteNames t]
    {
        get
        {
            if (spriteDict == null)
            {
                Init();
            }
            return spriteDict[(int)t];
        }
        set { }
    }
    //Returns audio by name
    public AudioClip[] this[AudioNames t]
    {
        get
        {
            if (audioDict == null)
            {
                Init();
            }
            return audioDict[(int)t];
        }
        set { }
    }
    //Returns materials by name
    public Material this[MaterialNames t]
    {
        get
        {
            if (materialDict == null)
            {
                Init();
            }
            return materialDict[(int)t];
        }
        set { }
    }*/

    //Init
    public void Init()
    {
            prefabDict = new DictionaryList<int, Transform>();
            /*spriteDict = new DictionaryList<int, Sprite>();
            materialDict = new DictionaryList<int, Material>();
            audioDict = new DictionaryList<int, AudioClip[]>();*/
            //Prefabs
            /*resource.panels = new Resources.NamedPrefab[panels.Length];
            resource.menues = new Resources.NamedPrefab[menues.Length];
            resource.nodes = new Resources.NamedPrefab[nodes.Length];
            resource.cores = new Resources.NamedPrefab[cores.Length];
            resource.attachments = new Resources.NamedPrefab[attachments.Length];
            resource.legs = new Resources.NamedPrefab[legs.Length];
            resource.socketables = new Resources.NamedPrefab[socketables.Length];
            resource.inventory = new Resources.NamedPrefab[inventory.Length];
            resource.ui = new Resources.NamedPrefab[ui.Length];
            resource.buffs = new Resources.NamedPrefab[buffs.Length];
            resource.effects = new Resources.NamedPrefab[effects.Length];
            resource.characters = new Resources.NamedPrefab[characters.Length];
            resource.other = new Resources.NamedPrefab[other.Length];

            resource.inventorySounds = new Resources.NamedAudio[inventorySounds.Length];
            resource.gameSounds = new Resources.NamedAudio[gameSounds.Length];
            resource.uiSounds = new Resources.NamedAudio[uiSounds.Length];

            resource.materials = new Resources.NamedMaterial[materials.Length];

            resource.sprites = new Resources.NamedSprite[sprites.Length];*/

            Add(panels); //, resource.panels);
            Add(nodes);//, resource.nodes);

           /* Add(menues); //, resource.menues);
           
            Add(cores); //, resource.cores);
            Add(attachments);//, resource.attachments);
            Add(legs);//, resource.legs);
            Add(socketables);//, resource.socketables);
            Add(inventory);//;, resource.inventory);
            Add(ui);//, resource.ui);
            Add(buffs);//, resource.buffs);
            Add(effects);//, resource.effects);
            Add(characters);//, resource.characters);
            Add(other);//, resource.other);
            //Sounds
            Add(inventorySounds);//, resource.inventorySounds);
            Add(gameSounds);//, resource.gameSounds);
            Add(uiSounds);//, resource.uiSounds);
            //Materials
            Add(materials);//, resource.materials);
            Add(sprites);//, resource.sprites);
            */
        
    }
    //Adds for later reference
    public void Add(NamedPrefab[] prefabList) //, Resources.NamedPrefab[] target)
    {
        for(int i = 0; i < prefabList.Length; i++)
        {
            NamedPrefab pref = prefabList[i];
            /*target[i] = new Resources.NamedPrefab
            {
                name = pref.name,
                prefab = pref.prefab
            };*/

            if (pref.name != null && pref.name.Length > 0)
            {
                try
                {
                    prefabDict.Add(
                        (int)((SceneReferenceNames)Enum.Parse(typeof(SceneReferenceNames), pref.name))
                        , pref.prefab
                        );

                }
                catch (Exception)
                {
                    Debug.LogError("Could not find enum: " + pref.name + " Did you forget to save?");
                }
            }
        }
        //foreach (NamedPrefab pref in prefabList)
        //{


        //}
    }
    
    /*public void Add(NamedSprite[] prefabList) //, Resources.NamedSprite[] target)
    {
        for (int i = 0; i < prefabList.Length; i++)
        {
            NamedSprite pref = prefabList[i];
            target[i] = new Resources.NamedSprite
            {
                name = pref.name,
                prefab = pref.prefab
            };

            if (pref.name != null && pref.name.Length > 0)
            {
                try
                {
                    spriteDict.Add(
                        (int)((SpriteNames)Enum.Parse(typeof(SpriteNames), pref.name))
                        , pref.prefab
                        );

                }
                catch (ArgumentException)
                {
                    Debug.LogError("Could not find enum: " + pref.name + "Did you forget to save?");
                }
            }
        }
    }
    public void Add(NamedMaterial[] prefabList)//, Resources.NamedMaterial[] target)
    {
        for (int i = 0; i < prefabList.Length; i++)
        {
            NamedMaterial pref = prefabList[i];
            target[i] = new Resources.NamedMaterial
            {
                name = pref.name,
                prefab = pref.prefab
            };

            if (pref.name != null && pref.name.Length > 0)
            {
                try
                {
                    materialDict.Add(
                        (int)((MaterialNames)Enum.Parse(typeof(MaterialNames), pref.name))
                        , pref.prefab
                        );

                }
                catch (ArgumentException)
                {
                    Debug.LogError("Could not find enum: " + pref.name + "Did you forget to save?");
                }
            }
        }
    }
    public void Add(NamedAudio[] prefabList)//, Resources.NamedAudio[] target)
    {
        for (int i = 0; i < prefabList.Length; i++)
        {
            NamedAudio pref = prefabList[i];
            target[i] = new Resources.NamedAudio
            {
                name = pref.name,
                prefab = pref.prefab
            };

            if (pref.name != null && pref.name.Length > 0)
            {
                try
                {
                    audioDict.Add(
                        (int)((AudioNames)Enum.Parse(typeof(AudioNames), pref.name))
                        , pref.prefab
                        );

                }
                catch (ArgumentException)
                {
                    Debug.LogError("Could not find enum: " + pref.name + "Did you forget to save?");
                }
            }
        }
    }*/
    //Make sure to init at startup
}
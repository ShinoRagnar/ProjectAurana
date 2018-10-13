using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;

[CustomEditor(typeof(Resources))]
public class ResourceEditor : Editor
{
    Resources resources;

    string filePath = "Assets/Scripts/Enums/";

    string prefabEnumName = "PrefabNames";
    string spriteEnumName = "SpriteNames";
    string materialEnumName = "MaterialNames";
    string audioEnumName = "AudioNames";
    string itemEnumName = "ItemNames";
    string coreEnumName = "CoreNames";
    string audioContainerEnumName = "AudioContainerNames";
    string buffEnumName = "BuffNames";
    string effectEnumName = "EffectNames";
    string bulletEnumName = "BulletNames";
    string gunEnumName = "GunNames";
    string crystalEnumName = "CrystalNames";
    string gunArrayEnumName = "GunArrayNames";
    string legEnumName = "LegNames";
    string mechEnumName = "MechNames";
    string weaponEnumName = "WeaponNames";
    string shieldEnumName = "ShieldNames";
    string onHitEnumName = "OnHitNames";

    //Multi
    string socketableEnumName = "SocketableNames";

    private void OnEnable()
    {
        resources = (Resources)target;
    }

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        //filePath = EditorGUILayout.TextField("Path", filePath);
        //fileName = EditorGUILayout.TextField("Name", fileName);

        if (GUILayout.Button("Save Enumerators"))
        {
            ListHash<string> prefabNames = new ListHash<string>();
            ListHash<string> spriteNames = new ListHash<string>();
            ListHash<string> audioNames = new ListHash<string>();
            ListHash<string> materialNames = new ListHash<string>();
            ListHash<string> itemNames = new ListHash<string>();
            ListHash<string> coreNames = new ListHash<string>();
            ListHash<string> audioContainerNames = new ListHash<string>();
            ListHash<string> buffNames = new ListHash<string>();
            ListHash<string> effectNames = new ListHash<string>();
            ListHash<string> bulletNames = new ListHash<string>();
            ListHash<string> gunNames = new ListHash<string>();
            ListHash<string> crystalNames = new ListHash<string>();
            ListHash<string> legNames = new ListHash<string>();
            ListHash<string> socketNames = new ListHash<string>();
            ListHash<string> mechNames = new ListHash<string>();
            ListHash<string> gunArrayNames = new ListHash<string>();
            ListHash<string> weaponNames = new ListHash<string>();
            ListHash<string> shieldNames = new ListHash<string>();
            ListHash<string> onHitNames = new ListHash<string>();

            //Prefabs
            Add(resources.menues, prefabNames);
            Add(resources.cores, prefabNames);
            Add(resources.attachments, prefabNames);
            Add(resources.legs, prefabNames);
            Add(resources.socketables, prefabNames);
            Add(resources.inventory, prefabNames);
            Add(resources.ui, prefabNames);
            Add(resources.buffs, prefabNames);
            Add(resources.effects, prefabNames);
            Add(resources.characters, prefabNames);
            Add(resources.other, prefabNames);
            //Sprites
            Add(resources.sprites, spriteNames);
            Add(resources.buffSprites, spriteNames);
            //Sounds
            Add(resources.gameSounds, audioNames);
            Add(resources.inventorySounds, audioNames);
            Add(resources.uiSounds, audioNames);
            //Materials
            Add(resources.materials, materialNames);
            //Audio Container
            Add(resources.audioContainer, audioContainerNames);
            //Buff
            Add(resources.buff, buffNames);
            //Effect
            Add(resources.effect, effectNames);
            //OnHit
            Add(resources.onHit, onHitNames);
            //Items
            Add(resources.items, itemNames);
            //Bullet
            Add(resources.bullet, bulletNames);
            //Cores
            Add(resources.mechCores, coreNames);
            coreNames.Add("NoName");
            //Gun
            Add(resources.gun, gunNames);
            //Crystal
            Add(resources.crystal, crystalNames);
            //Weapon
            Add(resources.weapons, weaponNames);
            //Shield
            Add(resources.shields, shieldNames);
            //GunArray
            Add(resources.gunarray, gunArrayNames);
            //Leg
            Add(resources.leg, legNames);
            legNames.Add("NoName");
            //Socketable
            Add(resources.gunarray, socketNames);
            Add(resources.crystal, socketNames);
            Add(resources.gun, socketNames);
            Add(resources.weapons, socketNames);
            Add(resources.shields, socketNames);
            socketNames.Add("NothingSocketed");
            //Mech
            Add(resources.mech, mechNames);

            EditorMethods.WriteToEnum(filePath, prefabEnumName, prefabNames.ToSortedList());
            EditorMethods.WriteToEnum(filePath, spriteEnumName, spriteNames.ToSortedList());
            EditorMethods.WriteToEnum(filePath, materialEnumName, materialNames.ToSortedList());
            EditorMethods.WriteToEnum(filePath, audioEnumName, audioNames.ToSortedList());
            EditorMethods.WriteToEnum(filePath, itemEnumName, itemNames.ToSortedList());
            EditorMethods.WriteToEnum(filePath, coreEnumName, coreNames.ToSortedList());
            EditorMethods.WriteToEnum(filePath, audioContainerEnumName, audioContainerNames.ToSortedList());
            EditorMethods.WriteToEnum(filePath, buffEnumName, buffNames.ToSortedList());
            EditorMethods.WriteToEnum(filePath, effectEnumName, effectNames.ToSortedList());
            EditorMethods.WriteToEnum(filePath, bulletEnumName, bulletNames.ToSortedList());
            EditorMethods.WriteToEnum(filePath, gunEnumName, gunNames.ToSortedList());
            EditorMethods.WriteToEnum(filePath, crystalEnumName, crystalNames.ToSortedList());
            EditorMethods.WriteToEnum(filePath, legEnumName, legNames.ToSortedList());
            EditorMethods.WriteToEnum(filePath, socketableEnumName, socketNames.ToSortedList());
            EditorMethods.WriteToEnum(filePath, mechEnumName, mechNames.ToSortedList());
            EditorMethods.WriteToEnum(filePath, gunArrayEnumName, gunArrayNames.ToSortedList());
            EditorMethods.WriteToEnum(filePath, weaponEnumName, weaponNames.ToSortedList());
            EditorMethods.WriteToEnum(filePath, shieldEnumName, shieldNames.ToSortedList());
            EditorMethods.WriteToEnum(filePath, onHitEnumName, onHitNames.ToSortedList());

            resources.Clear();
        }
        if (GUILayout.Button("Import"))
        {
            foreach(Object o in resources.mechImports)
            {

                MechData ma = ScriptableObject.CreateInstance<MechData>();
                JsonUtility.FromJsonOverwrite(o.ToString(), ma);
                string assetPathAndName = AssetDatabase.GenerateUniqueAssetPath("Assets/Data/Mech/" + ma.enumName + ".asset");
                AssetDatabase.CreateAsset(ma, assetPathAndName);
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
                //EditorUtility.FocusProjectWindow();
            }
        }
    }

    private void Add(Resources.NamedPrefab[] nam, ListHash<string> prefabNames)
    {
        foreach (Resources.NamedPrefab pref in nam) { prefabNames.AddIfNotContains(pref.name.Trim()); }
    }
    private void Add(Resources.NamedMaterial[] nam, ListHash<string> prefabNames)
    {
        foreach (Resources.NamedMaterial pref in nam) { prefabNames.AddIfNotContains(pref.name.Trim()); }
    }
    private void Add(Resources.NamedAudio[] nam, ListHash<string> prefabNames)
    {
        foreach (Resources.NamedAudio pref in nam) { prefabNames.AddIfNotContains(pref.name.Trim()); }
    }
    private void Add(Resources.NamedSprite[] nam, ListHash<string> prefabNames)
    {
        foreach (Resources.NamedSprite pref in nam) { prefabNames.AddIfNotContains(pref.name.Trim()); }
    }
    private void Add(ItemData[] nam, ListHash<string> prefabNames)
    {
        foreach (ItemData pref in nam)
        {
            if (pref.itemName.Length < 1)
            {
                pref.itemName = pref.prefab.ToString();
                EditorUtility.SetDirty(pref);
            }
            prefabNames.AddIfNotContains(pref.itemName.Trim());

        }
    }
    private void Add(CoreData[] nam, ListHash<string> prefabNames)
    {
        foreach (CoreData pref in nam) { prefabNames.AddIfNotContains(pref.enumName.Trim()); }
    }
    private void Add(AudioContainerData[] nam, ListHash<string> prefabNames)
    {
        foreach (AudioContainerData pref in nam) { prefabNames.AddIfNotContains(pref.enumName.Trim()); }
    }
    private void Add(BuffData[] nam, ListHash<string> prefabNames)
    {
        foreach (BuffData pref in nam) { prefabNames.AddIfNotContains(pref.enumName.Trim()); }
    }
    private void Add(EffectData[] nam, ListHash<string> prefabNames)
    {
        foreach (EffectData pref in nam) { prefabNames.AddIfNotContains(pref.enumName.Trim()); }
    }
    private void Add(BulletData[] nam, ListHash<string> prefabNames)
    {
        foreach (BulletData pref in nam) { prefabNames.AddIfNotContains(pref.enumName.Trim()); }
    }
    private void Add(GunData[] nam, ListHash<string> prefabNames)
    {
        foreach (GunData pref in nam) { prefabNames.AddIfNotContains(pref.enumName.Trim()); }
    }
    private void Add(CrystalData[] nam, ListHash<string> prefabNames)
    {
        foreach (CrystalData pref in nam) { prefabNames.AddIfNotContains(pref.enumName.Trim()); }

    }
    private void Add(LegData[] nam, ListHash<string> prefabNames)
    {
        foreach (LegData pref in nam) { prefabNames.AddIfNotContains(pref.enumName.Trim()); }

    }
    private void Add(MechData[] nam, ListHash<string> prefabNames)
    {
        foreach (MechData pref in nam) { prefabNames.AddIfNotContains(pref.enumName.Trim()); }

    }
    private void Add(GunArrayData[] nam, ListHash<string> prefabNames)
    {
        foreach (GunArrayData pref in nam) { prefabNames.AddIfNotContains(pref.enumName.Trim()); }

    }
    private void Add(WeaponData[] nam, ListHash<string> prefabNames)
    {
        foreach (WeaponData pref in nam) { prefabNames.AddIfNotContains(pref.enumName.Trim()); }

    }
    private void Add(ShieldData[] nam, ListHash<string> prefabNames)
    {
        foreach (ShieldData pref in nam) { prefabNames.AddIfNotContains(pref.enumName.Trim()); }

    }
    private void Add(OnHitData[] nam, ListHash<string> prefabNames)
    {
        foreach (OnHitData pref in nam) { prefabNames.AddIfNotContains(pref.enumName.Trim()); }

    }



}
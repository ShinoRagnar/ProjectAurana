using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(SceneReferences))]
public class SceneReferencesEditor : Editor {

    SceneReferences prefab;
    string filePath = "Assets/Scripts/GameLogic/";
    string sceneRefNam = "SceneReferenceNames";

    private void OnEnable()
    {
        prefab = (SceneReferences)target;
    }

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        if (GUILayout.Button("Save References"))
        {
            ListHash<string> prefabNames = new ListHash<string>();

            //Prefabs
            Add(prefab.panels, prefabNames);
            Add(prefab.nodes, prefabNames);

            EditorMethods.WriteToEnum(filePath, sceneRefNam, prefabNames.ToSortedList());
        }
    }

    private void Add(SceneReferences.NamedPrefab[] nam, ListHash<string> prefabNames)
    {
        foreach (SceneReferences.NamedPrefab pref in nam) { prefabNames.AddIfNotContains(pref.name.Trim()); }
    }
}

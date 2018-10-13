using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(OrbColor))]
public class OrbColorEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
        if(!Application.isPlaying)
        {
            if (GUILayout.Button("Get from current material"))
            {
                ((OrbColor)target).GetFromMaterial();
            }
            if (GUILayout.Button("Apply on current material"))
            {
                ((OrbColor)target).ApplyToMaterial();
            }
        }
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(MassFromChildren))]
[ExecuteInEditMode]
[System.Serializable]
public class MassFromChildrenEditor : Editor
{
    public bool autoUpdate = false;

    public override void OnInspectorGUI()
    {
        MassFromChildren t = (MassFromChildren)target;

        EditorUtility.SetDirty(t);

        if (GUILayout.Button("Calculate Mass From Density"))
        {
            t.Calculate();
        }
    }
}
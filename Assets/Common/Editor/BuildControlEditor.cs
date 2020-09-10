using System.Collections;
using System.Collections.Generic;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;

#if UNITY_EDITOR
[CustomEditor(typeof(BuildControl))]
public class BuildControlEditor : Editor
{
    public override void OnInspectorGUI()
    {

        DrawDefaultInspector();

        BuildControl myScript = (BuildControl)target;

        if (GUILayout.Button("Add Build List"))
        {

            myScript.SetEditorBuildSettingsScenes();

        }

    }
}
#endif

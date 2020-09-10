using System.Collections;
using System.Collections.Generic;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;
using UnityEngine.SceneManagement;


#if UNITY_EDITOR
public class BuildControl : MonoBehaviour
{
    public List<SceneAsset> m_SceneAssets = new List<SceneAsset>();

    public void SetEditorBuildSettingsScenes()
    {
        // Find valid Scene paths and make a list of EditorBuildSettingsScene
        List<EditorBuildSettingsScene> editorBuildSettingsScenes = new List<EditorBuildSettingsScene>();
        foreach (var sceneAsset in m_SceneAssets)
        {
            string scenePath = AssetDatabase.GetAssetPath(sceneAsset);
            //Debug.Log(scenePath.Replace("Assets/","").Replace(".unity", ""));
            if (!string.IsNullOrEmpty(scenePath))
                editorBuildSettingsScenes.Add(new EditorBuildSettingsScene(scenePath, true));
        }

        // Set the Build Settings window Scene list
        EditorBuildSettings.scenes = editorBuildSettingsScenes.ToArray();
    }
}
#endif

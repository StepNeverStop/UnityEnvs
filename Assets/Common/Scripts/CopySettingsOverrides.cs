using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents;
using System;
using UnityEngine.SceneManagement;
using UnityEnvs;

public class CopySettingsOverrides : MonoBehaviour {
    [Tooltip("Training Area. Tag with TrainingArea of GameObject will be searched if not specified.")]
    public GameObject AreaPrefab;
    [Tooltip("Gap of several Training Areas.")]
    public Vector3 CopyGap = new Vector3(10.0f, 0.0f, 10.0f);
    [Tooltip("Deault copy nums.")]
    public int NAgents = 1;

    private FreedomMonitor fm;

    private void Awake() {
        
        List<string> commandLineArgs = new List<string>(Environment.GetCommandLineArgs());
        int index = commandLineArgs.IndexOf("--n_agents");
        if (index != -1) {
            NAgents = Convert.ToInt32(commandLineArgs[index + 1]);
        }

        if (AreaPrefab == null) {
            AreaPrefab = GameObject.FindGameObjectWithTag("TrainingArea");
            if (AreaPrefab != null) {
                AreaPrefab = AreaPrefab.gameObject;
            }
        }

        if (NAgents > 1) {
            if (AreaPrefab == null) {
#if UNITY_EDITOR
                UnityEditor.EditorApplication.isPlaying = false;
                Debug.Log("Cannot find GameObject with tag of 'TrainingArea'.");
#else
                Application.Quit();
#endif
            }

            GetAreaInfo gai = AreaPrefab.GetComponent<GetAreaInfo>();
            Vector3 AreaSize = new Vector3(10.0f, 0.0f, 10.0f);
            if (gai != null)
            {
                AreaSize = gai.GetSize();
            }
            Vector3 AreaDistance = AreaSize + CopyGap;

            fm = Camera.main.GetComponent<FreedomMonitor>();
            if (fm != null)
            {
                fm.minHigh = Mathf.Max(AreaSize.x, AreaSize.z) / 2;
                var width = Mathf.CeilToInt(Mathf.Sqrt(NAgents));
                width = width % 2 == 0 ? width + 1 : width;
                fm.maxHigh = width * Mathf.Max(AreaDistance.x, AreaDistance.z) / 2;
            }

            int level = 1;
            int curr = 0;
            int[] matIndex = new int[] { 0, 1 };
            int[] direction = new int[] { 1, -1, -1, 1 };

            for (int i = 0; i < NAgents - 1; i++) {
                Instantiate(AreaPrefab, new Vector3(matIndex[0] * AreaDistance.x, 0, matIndex[1] * AreaDistance.z), Quaternion.identity);

                if (Math.Abs(matIndex[curr % 2]) == level && curr == 3) {
                    curr = 0;
                    level += 1;
                    matIndex = new int[] { -level + 1, level };
                }
                else {
                    if (Math.Abs(matIndex[curr % 2]) == level)
                        curr = (curr + 1) % 4;

                    matIndex[curr % 2] += direction[curr];
                }
            }
        }
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents;
using System;

public class CopySettingsOverrides : MonoBehaviour {
    [Tooltip("Training Area. Tag with TrainingArea of GameObject will be searched if not specified.")]
    public GameObject AreaPrefab;
    [Tooltip("Gap of several Training Areas.")]
    public Vector3 CopyGap = new Vector3(10.0f, 0.0f, 10.0f);
    [Tooltip("Deault copy nums.")]
    public int NAgents = 1;

    private void Awake() {

        List<string> commandLineArgs = new List<string>(Environment.GetCommandLineArgs());
        int index = commandLineArgs.IndexOf("--n_agents");
        if (index != -1) {
            NAgents = Convert.ToInt32(commandLineArgs[index + 1]);
        }

        if (AreaPrefab == null)
        {
            AreaPrefab = GameObject.FindGameObjectWithTag("TrainingArea");
            if (AreaPrefab != null)
            {
                AreaPrefab = AreaPrefab.gameObject;
            }
        }

        if (NAgents > 1)
        {
            if (AreaPrefab == null)
            {
#if UNITY_EDITOR
                UnityEditor.EditorApplication.isPlaying = false;
                Debug.Log("Cannot find GameObject with tag of 'TrainingArea'.");
#else
                Application.Quit();
#endif
            }

            Vector3 AreaSize = AreaPrefab.GetComponent<GetAreaInfo>().GetSize() + CopyGap;

            int rowNum = Mathf.CeilToInt(Mathf.Sqrt(NAgents));

            for (int i = 0; i < rowNum; i++)
            {
                for (int j = 0; j < rowNum; j++)
                {
                    if (i == 0 && j == 0)
                        continue;
                    if (i * rowNum + j + 1 > NAgents)
                    {
                        break;
                    }

                    int mid = rowNum / 2;
                    if (j <= mid)
                        Instantiate(AreaPrefab, new Vector3(j * AreaSize.x, 0, i * AreaSize.z), Quaternion.identity);
                    else
                        Instantiate(AreaPrefab, new Vector3((mid - j) * AreaSize.x, 0, i * AreaSize.z), Quaternion.identity);
                }
            }
        }
    }

}

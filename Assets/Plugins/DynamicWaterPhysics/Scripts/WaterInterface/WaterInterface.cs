using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Class for retrieving water height at position.
/// Attach this script to your water system and set object's tag to "Water".
/// </summary>
public class WaterInterface : MonoBehaviour
{
    // Replace "WaterSystem" with your class.
    // WaterSystem myWaterSystem;

    private void Start()
    {
        // Replace the following line with the one corresponding to your class.
        // myWaterSystem = GetComponent<WaterSystem>();
    }

    /// <summary>
    /// Return water height y in world coordinates at world point x, z
    /// </summary>
    public float GetWaterHeightAtLocation(float x, float z)
    {
        // Replace "return transform.position.y" line with the function from your water system, e.g:
        // return myWaterSystem.GetWaterHeightAtLocation(float x, float z);
        // x and z are world coordinates.

        return transform.position.y;
    }

}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FloatingObjectFromScript : MonoBehaviour {

    // Generating Floating Objects from script at runtime is limited in features and it is recommended
    // to do it from Editor if possible.

    // !!! Do not use mesh simplification during the runtime as it can take up to few seconds !!!
    // Below is the example of all the things that you can do with the FloatingObject.cs script,
    // but it is recommended that you use prefabs rather than generate objects during runtime, 
    // and then modify the values of the instantiated prefab using GetComponent<>().

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            // Create game object
            GameObject go = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            go.name = "Demo Floating Object From Script";
            go.transform.position = new Vector3(0, 3, 0);

            // Add rigidbody
            Rigidbody rb = go.AddComponent<Rigidbody>();
            rb.mass = 200;

            // Add floating object script
            FloatingObject fo = go.AddComponent<FloatingObject>();

            // Optional params (can be removed, just for demo)
            fo.FluidDensity = 1000;
            fo.DynamicForceFactor = 1f;

            // Manually assigning water - flat
            // If not set floating object will use the nearest water object with tag "Water",
            // or if no such objects exist will assume water is at height 0.

            //fo.water = myWaterGameObject;

            // Assigning water - waves

            //fo.water = GameObject.Find("WaterWithWaves");
            //fo.WaterHeightFunction = ExampleWaterHeightFunction; 

            // Get underwater triangle data
            // List<BuoyTri> tris = fo.underwaterTris;
        }
    }	

    /// <summary>
    /// Fetch the water height for the Ocean Community system (example).
    /// FloatingObject.WaterHeightFunction must return float and accept 2 floats as parameters
    /// to conform with the delegate.
    /// Replace myWaterSystem with your water system and WaterheightFunctionAtSomePoint with that system's 
    /// water height function. If the system has waves and cannot provide water height, it can not be used 
    /// with this script.
    /// </summary>
    public float ExampleWaterHeightFunction(float x, float z)
    {
        // return myWaterSystem.WaterHeightFunctionAtSomePoint([functionParams]);
        return 0;
    }
}

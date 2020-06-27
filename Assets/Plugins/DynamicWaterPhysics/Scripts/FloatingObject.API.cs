using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NWH;

public partial class FloatingObject : MonoBehaviour
{

    /// <summary>
    /// Rigidbody to which the forces will be applied. Must be self or parent.
    /// </summary>
    public Rigidbody TargetRigidbody
    {
        get { return rb; }
        set { rb = value; }
    }

    /// <summary>
    /// Transform which will be used for point and vector transforms. Usually this.transform.
    /// </summary>
    public Transform TargetTransform
    {
        get { return t; }
        set { t = value; }
    }

    /// <summary>
    /// Object's original mesh.
    /// </summary>
    public Mesh OriginalMesh
    {
        get { return originalMesh; }
        set
        {
            originalMesh = value;
        }
    }

    /// <summary>
    /// Mesh used for simulation.
    /// </summary>
    public Mesh DummyMesh
    {
        get { return dummyMesh; }
        set
        {
            dummyMesh = value;
        }
    }

    /// <summary>
    /// Sets the fluid density to be used in simulation.
    /// </summary>
    public float FluidDensity
    {
        get { return fluidDensity; }
        set { fluidDensity = value; }
    }

    /// <summary>
    /// Determines if UpdateDummyMesh() should generate simplified mesh.
    /// </summary>
    public bool DoSimplifyMesh
    {
        get { return simplifyMesh; }
        set { simplifyMesh = value; }
    }

    /// <summary>
    /// Determines if UpdateDummyMesh() should generate dummy mesh.
    /// </summary>
    public bool DoConvexifyMesh
    {
        get { return convexMesh; }
        set { convexMesh = value; }
    }

    /// <summary>
    /// Percentage of the original triangles that will be present in the dummy mesh.  
    /// </summary>
    public float SimplificationRatio
    {
        get { return simplificationRatio; }
        set { simplificationRatio = value; }
    }

    /// <summary>
    /// Generates simplified mesh from original one. Not recommended during runtime.
    /// </summary>
    public void UpdateDummyMesh()
    {
        DummyMesh = ManipulateMesh(originalMesh);
    }

    /// <summary>
    /// Swaps original mesh with dummy mesh for preview.
    /// </summary>
    public bool PreviewDummyMesh
    {
        get { return previewDummyMesh; }
        set { previewDummyMesh = value;  }
    }

    /// <summary>
    /// Returns true if object has at least one vertex inside water.
    /// </summary>
    public bool IsTouchingWater
    {
        get { return isTouchingWater; }
        set { isTouchingWater = value; }
    }

    /// <summary>
    /// Multiplier for the hydrodynamic forces acting upon the floating body. 
    /// </summary>
    public float DynamicForceFactor
    {
        get { return dynamicForceFactor; }
        set { dynamicForceFactor = value; }
    }

    /// <summary>
    /// Density of the material. Not that density is calculated as if the mesh was full.
    /// </summary>
    public float MaterialDensity
    {
        get { return materialDensity; }
        set { materialDensity = value; }
    }

    /// <summary>
    /// Mass of the object.
    /// </summary>
    public float MaterialMass
    {
        get { return materialMass; }
        set { materialMass = value; }
    }

    /// <summary>
    /// Volume of the dummy mesh.
    /// </summary>
    public float MeshVolume
    {
        get { return meshVolume; }
        set { meshVolume = value; }
    }

    /// <summary>
    /// Enables sleep when object is sitting still meaning forces will not be recalculated until object moves out of treshold values.
    /// </summary>
    public bool ReuseForces
    {
        get { return sleepEnabled; }
        set { sleepEnabled = value; }
    }

    /// <summary>
    /// Distance object must travel for forces to be recalculated.
    /// </summary>
    public float ReusePositionTreshold
    {
        get { return sleepPositionTreshold; }
        set { sleepPositionTreshold = value; }
    }

    /// <summary>
    /// Change in angle that must happen for forces to be recalculated.
    /// </summary>
    public float ReuseAngleTreshold
    {
        get { return sleepAngleTreshold; }
        set { sleepAngleTreshold = value; }
    }
   
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
[ExecuteInEditMode]
public class CenterOfMass : MonoBehaviour {

    private Vector3 centerOfMass;
    public Vector3 centerOfMassOffset = Vector3.zero;
    private Vector3 prevOffset = Vector3.zero;
    private Rigidbody rb;
    public bool showCOM = true;

	void Start () {
        rb = GetComponent<Rigidbody>();
        centerOfMass = rb.centerOfMass;
	}
	

	void Update () {
		if(centerOfMassOffset != prevOffset)
        {
            rb.centerOfMass = centerOfMass + centerOfMassOffset;
        }
        prevOffset = centerOfMassOffset;
	}

    private void OnDrawGizmos()
    {
        if (showCOM && rb != null)
        {
            float radius = 0.1f;
            try
            {
                radius = GetComponent<MeshFilter>().sharedMesh.bounds.size.z / 10f;
            }
            catch { }

            Gizmos.color = Color.green;
            Gizmos.DrawSphere(rb.transform.TransformPoint(rb.centerOfMass), radius);
        }
    }
    
}

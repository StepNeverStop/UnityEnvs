using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(AdvancedShipController))]
[RequireComponent(typeof(CenterOfMass))]
public class Submarine : MonoBehaviour
{
    [Range(0, 1000)]
    public float requestedDepth = 0f;
    [Range(0, 1)]
    public float depthSensitivity = 1f;
    [Range(1, 2)]
    public float maxMassFactor = 1.4f;
    public bool keepHorizontal = true;
    public float keepHorizontalSensitivity = 1f;
    public float maxMassOffset = 5f;

    private Rigidbody rb;
    private CenterOfMass com;
    private float initialMass;
    private Vector3 initialCOM;
    private float depth;
    private float depthCeoff;
    private float mass;
    private float zOffset;

	void Start ()
    {
        rb = GetComponent<Rigidbody>();
        com = GetComponent<CenterOfMass>();
        initialMass = rb.mass;
        initialCOM = com.centerOfMassOffset;
	}


	void FixedUpdate ()
    {
        depth = Mathf.Abs(transform.position.y);

        depthCeoff = Mathf.Clamp((requestedDepth - depth) * depthSensitivity * 0.1f, 0f, 1f);
        mass = Mathf.Clamp(1f + depthCeoff, 1f, maxMassFactor) * initialMass;
        rb.mass = mass;

        if(keepHorizontal)
        {
            float angle = Vector3.SignedAngle(transform.up, Vector3.up, transform.right);
            zOffset = Mathf.Clamp(Mathf.Sign(angle) * Mathf.Pow(angle * 0.2f, 2f) * keepHorizontalSensitivity, -maxMassOffset, maxMassOffset);
            com.centerOfMassOffset = new Vector3(initialCOM.x, initialCOM.y, initialCOM.z + zOffset);
        }
	}


    public void SetDepth(float d)
    {
        requestedDepth = d;
    }
}

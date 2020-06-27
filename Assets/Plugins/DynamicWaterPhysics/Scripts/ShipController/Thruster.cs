using UnityEngine;
using System.Collections;

/// <summary>
/// Bow or stern thrusters. 
/// Can be multiple of each.
/// </summary>
[System.Serializable]
public class Thruster
{
    private AdvancedShipController sc;

    [Tooltip("Name of the thruster - can be any string.")]
    public string name = "Thruster";

    [Tooltip("Relative force application position.")]
    public Vector3 position;

    [Tooltip("Max thrust in [N].")]
    public float maxThrust;

    [Tooltip("Input mapping to which this thruster will react.")]
    public AdvancedShipController.InputMapping inputMapping = AdvancedShipController.InputMapping.BowThruster;

    [Tooltip("Time needed to reach maxThrust.")]
    public float spinUpSpeed = 1f;

    [Tooltip("Optional. Transform representing a propeller. Visual only.")]
    public Transform propellerTransform;

    public enum RotationDirection { Left, Right }
    [Tooltip("Rotation direction of the propeller. Visual only.")]
    public RotationDirection rotationDirection = RotationDirection.Right;

    [Tooltip("Rotation speed of the propeller if assigned. Visual only.")]
    public float propellerRotationSpeed = 1000f;

    private float thrust;

    public Vector3 WorldPosition
    {
        get
        {
            return sc.transform.TransformPoint(position);
        }
    }

    public float Input
    {
        get
        {
            float input = 0;
            if (inputMapping == AdvancedShipController.InputMapping.BowThruster)
            {
                input = -sc.input.bowThruster;
            }
            else if (inputMapping == AdvancedShipController.InputMapping.SternThruster)
            {
                input = -sc.input.sternThruster;
            }
            else
            {
                Debug.LogError("Thrusters can only be mapped to Bow or Stern Thruster input mapping.");
            }
            return input;
        }
    }

    public void Initialize(AdvancedShipController sc)
    {
        this.sc = sc;
    }

    public void Update()
    {

        float newThurst = maxThrust * -Input;
        thrust = Mathf.MoveTowards(thrust, newThurst, spinUpSpeed * maxThrust * Time.fixedDeltaTime);
        sc.ShipRigidbody.AddForceAtPosition(thrust * sc.transform.right, WorldPosition);

        if(propellerTransform != null)
        {
            float zRotation = Input * propellerRotationSpeed * Time.fixedDeltaTime;
            if (rotationDirection == RotationDirection.Right) zRotation = -zRotation;
            propellerTransform.RotateAround(propellerTransform.position, propellerTransform.forward, zRotation);
        }
    }
}

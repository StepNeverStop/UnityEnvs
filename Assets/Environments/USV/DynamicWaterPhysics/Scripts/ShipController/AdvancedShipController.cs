using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Script for controling ships, boats and other vessels.
/// </summary>
[RequireComponent(typeof(Rigidbody))]
[System.Serializable]
public class AdvancedShipController : MonoBehaviour
{
    [Tooltip("Optional. Used to check if propellers are under water.")]
    [SerializeField]
    public WaterInterface waterInterface;

    [Tooltip("Handles all the user input.")]
    [SerializeField]
    public InputHandler input = new InputHandler();

    /// <summary>
    /// Possible inputs.
    /// </summary>
    public enum InputMapping { LeftThrottle, RightThrottle, Throttle, BowThruster, SternThruster }

    [Tooltip("List of engines. Each engine is a propulsion system in itself consisting of the engine and the propeller.")]
    [SerializeField]
    public List<Engine> engines = new List<Engine>();

    [Tooltip("List of rudders.")]
    [SerializeField]
    public List<Rudder> rudders = new List<Rudder>();

    [Tooltip("List of either bow or stern thrusters.")]
    [SerializeField]
    public List<Thruster> thrusters = new List<Thruster>();

    private Rigidbody rb;

    public Rigidbody ShipRigidbody
    {
        get
        {
            return rb;
        }
    }

    /// <summary>
    /// Local Velocity vector of the rigidbody.
    /// </summary>
    public Vector3 LocalVelocity
    {
        get { return transform.InverseTransformDirection(ShipRigidbody.velocity); }
    }

    /// <summary>
    /// Speed in m/s.
    /// </summary>
    public float Speed
    {
        get { return LocalVelocity.z; }
    }

    /// <summary>
    /// Speed in knots.
    /// </summary>
    public float SpeedKnots
    {
        get { return Speed * 1.944f; }
    }

    void Start()
    {
        rb = GetComponent<Rigidbody>();

        foreach (Thruster thruster in thrusters)
            thruster.Initialize(this);

        foreach (Rudder rudder in rudders)
            rudder.Initialize(this);

        foreach (Engine engine in engines)
        {
            engine.Initialize(this);
        }

        input.Initialize(this);
    }

    void FixedUpdate()
    {
        input.Update();

        foreach (Engine engine in engines)
            engine.Update();

        foreach (Rudder rudder in rudders)
            rudder.Update();

        foreach (Thruster thruster in thrusters)
            thruster.Update();
    }

    private void OnDrawGizmos()
    {
        Start();

        foreach(Rudder rudder in rudders)
        {
            Gizmos.color = Color.magenta;
        }

        foreach(Engine e in engines)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawSphere(transform.TransformPoint(e.thrustPosition), 0.2f);
            Gizmos.DrawRay(new Ray(e.WorldPosition, e.WorldDirection));
        }

        foreach(Thruster thruster in thrusters)
        {
            if (thruster.inputMapping == AdvancedShipController.InputMapping.BowThruster)
                Gizmos.color = Color.yellow;
            else
                Gizmos.color = Color.cyan;

            Gizmos.DrawSphere(transform.TransformPoint(thruster.position), 0.2f);
            Gizmos.DrawRay(new Ray(thruster.WorldPosition, transform.right));
        }
    }
}


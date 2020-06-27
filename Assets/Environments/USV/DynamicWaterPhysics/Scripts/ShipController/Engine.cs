using UnityEngine;
using System.Collections;
using UnityEngine.Serialization;

/// <summary>
/// Engine object. Contains all the parameters related to ships's propulsion systems.
/// </summary>
[System.Serializable]
public class Engine
{
    public string name = "Engine";
    public AdvancedShipController.InputMapping inputMapping = AdvancedShipController.InputMapping.Throttle;

    [Header("Engine")]
    public float minRPM = 800;
    public float maxRPM = 6000;
    public float maxThrust = 5000;
    public float spinUpTime = 2f;

    [Header("Propeller")]

    [Tooltip("Position at which the force will be applied.")]
    public Vector3 thrustPosition;

    [Tooltip("Amount of thrust that will be applied if ship is reversing.")]
    public float reverseThrustCoefficient = 0.3f;

    [Tooltip("Speed at which propeller will reach it's maximum speed.")]
    public float maxSpeed = 20f;

    [Tooltip("Thrust curve of the propeller. X axis is speed in m/s and y axis is efficiency.")]
    public AnimationCurve thrustCurve = new AnimationCurve(new Keyframe[3] {
                new Keyframe(0f, 1f),
                new Keyframe(0.5f, 0.95f),
                new Keyframe(1f, 0f)
            });

    [Tooltip("Optional. Only use if you vessel has propeller mounted to the rudder (as in outboard engines). Propuslion force direction will be rotated with rudder if assigned.")]
    public Transform rudderTransform;

    [Header("Animation")]

    [Tooltip("Optional. Propeller transform. Visual rotation only, does not affect physics.")]
    public Transform propellerTransform;

    [Tooltip("Engine RPM will be multiplied by this value to get rotation speed of the propeller. Animation only.")]
    public float propellerRpmRatio = 0.1f;

    public enum RotationDirection { Left, Right }

    [Tooltip("Direction of propeller rotation. Animation only.")]
    public RotationDirection rotationDirection = RotationDirection.Right;

    [Header("Sound")]

    [Tooltip("Engine sound audio source. Source should be set on loop.")]
    public AudioSource audioSource;

    [Tooltip("Base (idle) volume of the engine.")]
    [Range(0, 2)]
    public float volume = 0.2f;

    [Tooltip("Base (idle) pitch of the engine.")]
    [Range(0, 2)]
    public float pitch = 0.5f;

    [Tooltip("Volume range of the engine.")]
    [Range(0, 2)]
    public float volumeRange = 0.8f;

    [Tooltip("Pitch range of the engine.")]
    [Range(0, 2)]
    public float pitchRange = 1f;

    private bool submerged;
    private float rpm;
    private float thrust;
    private float spinVelocity;

    private AdvancedShipController sc;

    /// <summary>
    /// Engine RPM.
    /// </summary>
    public float RPM
    {
        get { return Mathf.Clamp(rpm, minRPM, maxRPM); }
    }

    /// <summary>
    /// True if engine's thrust postion is under water.
    /// </summary>
    public bool Submerged
    {
        get { return submerged; }
    }

    public float Input
    {
        get
        {
            float input = 0;
            if (inputMapping == AdvancedShipController.InputMapping.LeftThrottle)
            {
                if(sc.input.throttle == 0)
                {
                    input = sc.input.leftThrottle;
                }
                else
                {
                    input = sc.input.throttle;
                    sc.input.leftThrottle = 0;
                }
            }
            else if (inputMapping == AdvancedShipController.InputMapping.RightThrottle)
            {
                if (sc.input.throttle == 0)
                {
                    input = sc.input.rightThrottle;
                }
                else
                {
                    input = sc.input.throttle;
                    sc.input.rightThrottle = 0;
                }
            }
            else
            {
                input = sc.input.throttle;
            }
            return input;
        }
    }

    public Vector3 WorldPosition
    {
        get
        {
            return sc.transform.TransformPoint(thrustPosition);
        }
    }

    public Vector3 WorldDirection
    {
        get
        {
            if (rudderTransform == null)
            {
                return sc.transform.forward;
            }
            else
            {
                return rudderTransform.forward;
            }
        }
    }

    public void Initialize(AdvancedShipController sc)
    {
        this.sc = sc;
    }

    public void Update()
    {
        // Calculate RPM
        float newRPM = (0.7f + 0.3f * (sc.Speed / maxSpeed)) * Mathf.Abs(Input) * maxRPM;
        if (!submerged) newRPM = maxRPM;
        rpm = Mathf.SmoothDamp(rpm, newRPM, ref spinVelocity, spinUpTime);
        rpm = Mathf.Clamp(rpm, minRPM, maxRPM);

        // Check if propeller under water
        submerged = false;
        if (sc.waterInterface == null || (sc.waterInterface != null && sc.waterInterface.GetWaterHeightAtLocation(WorldPosition.x, WorldPosition.z) < WorldPosition.y))
        {
            submerged = true;
        }

        // Check if thrust can be applied
        thrust = 0;
        if (submerged && maxRPM != 0 && maxSpeed != 0 && RPM > minRPM && Input != 0)
        {
            thrust = Mathf.Sign(Input) * (rpm / maxRPM) * thrustCurve.Evaluate(Mathf.Abs(sc.Speed) / maxSpeed) * maxThrust;
            thrust = Mathf.Sign(Input) == 1 ? thrust : thrust * reverseThrustCoefficient;
        }

        sc.ShipRigidbody.AddForceAtPosition(thrust * WorldDirection, WorldPosition);

        if (propellerTransform != null)
        {
            float zRotation = rpm * propellerRpmRatio * 6.0012f * Time.fixedDeltaTime;
            if (rotationDirection == RotationDirection.Right) zRotation = -zRotation;
            propellerTransform.RotateAround(propellerTransform.position, propellerTransform.forward, zRotation);
        }

        if(audioSource != null)
        {
            SoundUpdate();
        }
    }

    void SoundUpdate()
    {
        float rpmModifier = Mathf.Clamp01((RPM - minRPM) / maxRPM);

        // Pitch
        audioSource.pitch = pitch + rpmModifier * pitchRange;

        // Volume
        audioSource.volume = volume + rpmModifier * volumeRange;
    }
}

using UnityEngine;
using System.Collections;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;

public class TestAgent : Agent
{
    Rigidbody rBody;
    public float speed=3.0f;
    void Start()
    {
        rBody = GetComponent<Rigidbody>();
    }

    public Transform Target;
    public override void OnEpisodeBegin()
    {
        if (this.transform.position.y < 0)
        {
            // If the Agent fell, zero its momentum
            this.rBody.angularVelocity = Vector3.zero;
            this.rBody.velocity = Vector3.zero;
            this.transform.position = new Vector3(0, 0.5f, 0);
        }

        // Move the target to a new spot
        Target.position = new Vector3(Random.value * 8 - 4,
                                      0.5f,
                                      Random.value * 8 - 4);
    }

    /// <summary>
    /// Moves the agent according to the selected action.
    /// </summary>
    public void MoveAgent(float[] act)
    {
        var dirToGo = Vector3.zero;
        var rotateDir = Vector3.zero;

        var action = Mathf.FloorToInt(act[0]);

        // Goalies and Strikers have slightly different action spaces.
        switch (action)
        {
            case 1:
                dirToGo = transform.forward * 1f;
                break;
            case 2:
                dirToGo = transform.forward * -1f;
                break;
            case 3:
                rotateDir = transform.up * 1f;
                break;
            case 4:
                rotateDir = transform.up * -1f;
                break;
            case 5:
                dirToGo = transform.right * -0.75f;
                break;
            case 6:
                dirToGo = transform.right * 0.75f;
                break;
        }
        transform.Rotate(rotateDir, Time.fixedDeltaTime * 200f);
        rBody.AddForce(dirToGo * speed,
            ForceMode.VelocityChange);
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        // Target and Agent positions
        sensor.AddObservation(Target.position);
        sensor.AddObservation(this.transform.position);

        // Agent velocity
        sensor.AddObservation(rBody.velocity.x);
        sensor.AddObservation(rBody.velocity.z);
    }

    /// <summary>
    /// Called every step of the engine. Here the agent takes an action.
    /// </summary>
    public override void OnActionReceived(float[] vectorAction)
    {
        // Move the agent using the action.
        MoveAgent(vectorAction);

        float distanceToTarget = Vector3.Distance(this.transform.position,
                                          Target.position);

        //SetReward(-0.01f);

        // Reached target
        if (distanceToTarget < 1.42f)
        {
            SetReward(1.0f);
            EndEpisode();
        }

        // Fell off platform
        if (this.transform.position.y < 0)
        {
            //SetReward(-1.0f);
            EndEpisode();
        }
    }

    public override void Heuristic(float[] actionsOut)
    {
        if (Input.GetKey(KeyCode.D))
        {
            actionsOut[0] = 3;
        }
        if (Input.GetKey(KeyCode.W))
        {
            actionsOut[0] = 1;
        }
        if (Input.GetKey(KeyCode.A))
        {
            actionsOut[0] = 4;
        }
        if (Input.GetKey(KeyCode.S))
        {
            actionsOut[0] = 2;
        }
        actionsOut[0] = 0;
    }
}

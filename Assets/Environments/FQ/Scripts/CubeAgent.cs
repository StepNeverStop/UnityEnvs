using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using Unity.MLAgents.Policies;

public class CubeAgent : Agent
{
    Rigidbody rBody;
    public Transform Target;
    public ControlStatic cs;
    public ControlDynamic cd;
    public List<string> done_tags;

    private GameObject ground;
    private SpaceType action_type;
    private CubeMove cm;

    void Start()
    {
        rBody = GetComponent<Rigidbody>();
        ground = GameObject.FindGameObjectWithTag("ground");
        action_type = this.gameObject.GetComponent<BehaviorParameters>().BrainParameters.VectorActionSpaceType;
        cm = this.gameObject.GetComponent<CubeMove>();
    }

    public override void OnEpisodeBegin()
    {
        if (this.transform.position.y < 0)
        {
            // If the Agent fell, zero its momentum
            this.rBody.angularVelocity = Vector3.zero;
            this.rBody.velocity = Vector3.zero;
            
        }
        this.transform.position = new Vector3(Random.Range(-1.0f, 1.0f) * (ground.transform.localScale.x * 5.0f - transform.localScale.x * 0.5f),
                                    0.5f,
                                    Random.Range(-1.0f, 1.0f) * (ground.transform.localScale.x * 5.0f - transform.localScale.x * 0.5f));
        // Move the target to a new spot
        Target.position = new Vector3(Random.Range(-1.0f, 1.0f) * (ground.transform.localScale.x * 5.0f - transform.localScale.x * 0.5f),
                                    0.5f,
                                    Random.Range(-1.0f, 1.0f) * (ground.transform.localScale.x * 5.0f - transform.localScale.x * 0.5f));
        cs.ResetPosition();
        cd.ResetPosition();
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        // Target and Agent positions
        //sensor.AddObservation(Target.position - this.transform.position);

        //sensor.AddObservation(Target.position);
        //sensor.AddObservation(this.transform.position);
        // Agent velocity
        //sensor.AddObservation(rBody.velocity.x);
        //sensor.AddObservation(rBody.velocity.z);
    }


    /// <summary>
    /// Called every step of the engine. Here the agent takes an action.
    /// </summary>
    public override void OnActionReceived(float[] vectorAction)
    {
        // Move the agent using the action.
        cm.MoveAgent(action_type, this.transform, rBody, vectorAction);
        //ContinuousMoveAent(vectorAction);
        //Debug.Log(rBody.velocity);

        // Penalty given each step to encourage agent to finish task quickly.
        //AddReward(-1f / maxStep);

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
        if (action_type == SpaceType.Continuous)
        {
            actionsOut[0] = Input.GetAxis("Horizontal");
            actionsOut[1] = Input.GetAxis("Vertical");
        }
        else
        {
            actionsOut[0] = 0;
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
        }
    }


    private void OnCollisionEnter(Collision collision)
    {
        // Touched goal.
        foreach(string tag in done_tags)
        {
            if (collision.gameObject.CompareTag(tag))
            {
                SetReward(-1.0f);
                EndEpisode();
                break;
            }
        }
    }
}
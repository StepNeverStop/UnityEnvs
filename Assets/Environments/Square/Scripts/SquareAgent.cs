using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents;
using System.Linq;
using Unity.MLAgents.Sensors;
using Unity.MLAgents.SideChannels;

namespace Square {
    public class BaseSquareAgent : Agent {
        protected Rigidbody m_AgentRb;
        protected Rigidbody m_TargetRb;
        protected EnvironmentParameters m_ResetParams;
        protected bool forceReset = false;

        protected bool wallCollided = false;
        protected bool targetCollided = false;
        protected RayPerceptionSensorComponent3D[] rays;

        public Transform Target;
        public float SpawnRadius = 9;
        public float Speed = 2;

        public override void Initialize() {
            m_AgentRb = GetComponent<Rigidbody>();
            m_TargetRb = Target.GetComponent<Rigidbody>();
            m_ResetParams = Academy.Instance.EnvironmentParameters;
            m_ResetParams.RegisterCallback("force_reset", v => {
                forceReset = System.Convert.ToBoolean(v);
            });
            rays = GetComponentsInChildren<RayPerceptionSensorComponent3D>();
        }

        protected virtual void GenerateTarget() {
            Target.localPosition = new Vector3(SpawnRadius * (Random.value * 2 - 1),
                                                1f,
                                                SpawnRadius * (Random.value * 2 - 1));
            Target.rotation = Quaternion.Euler(new Vector3(0f, Random.Range(0, 360)));
            m_TargetRb.velocity = Vector3.zero;
            m_TargetRb.angularVelocity = Vector3.zero;
        }

        void OnCollisionEnter(Collision collision) {
            if (collision.gameObject.CompareTag("wall")) {
                wallCollided = true;
            }
            else if (collision.gameObject.CompareTag("target")) {
                targetCollided = true;
            }
        }

        public override void Heuristic(float[] actionsOut) {
            actionsOut[0] = Input.GetAxis("Horizontal");
            actionsOut[1] = Input.GetAxis("Vertical");
        }
    }


    public class SquareAgent : BaseSquareAgent {
        public bool AvoidWall = false;
        public bool TargetObservation = true;

        protected virtual void GenerateAgent() {
            transform.localPosition = new Vector3(SpawnRadius * (Random.value * 2 - 1),
                                                        0.5f,
                                                        SpawnRadius * (Random.value * 2 - 1));
            transform.rotation = Quaternion.Euler(new Vector3(0f, Random.Range(0, 360)));
            m_AgentRb.velocity = Vector3.zero;
        }

        public override void OnEpisodeBegin() {
            AvoidWall = System.Convert.ToBoolean(m_ResetParams.GetWithDefault("avoid_wall", System.Convert.ToSingle(AvoidWall)));

            foreach (var ray in rays) {
                float rayLength = m_ResetParams.GetWithDefault("ray_length", ray.RayLength);
                ray.RayLength = rayLength;
            }

            // Generate agent
            if (forceReset || (AvoidWall && wallCollided)) {
                GenerateAgent();
            }

            GenerateTarget();

            wallCollided = false;
            targetCollided = false;
        }

        public override void CollectObservations(VectorSensor sensor) {
            forceReset = false;

            if (sensor != null) {
                if (TargetObservation) {
                    sensor.AddObservation(Target.localPosition.x / 10f);
                    sensor.AddObservation(Target.localPosition.z / 10f);
                }

                sensor.AddObservation(transform.localPosition.x / 10f);
                sensor.AddObservation(transform.localPosition.z / 10f);

                // Agent forward direction
                sensor.AddObservation(transform.forward.x);
                sensor.AddObservation(transform.forward.z);

                // Agent velocity
                var velocity = transform.InverseTransformDirection(m_AgentRb.velocity);
                sensor.AddObservation(velocity.x);
                sensor.AddObservation(velocity.z);
            }
        }

        public override void OnActionReceived(float[] vectorAction) {
            if (targetCollided) {
                SetReward(1.0f);
                EndEpisode();
            }
            else if (AvoidWall && wallCollided) {
                SetReward(-1.0f);
                EndEpisode();
            }
            else {
                AddReward(-1f / MaxStep);
            }

            var dirToGo = transform.forward * vectorAction[0];
            var rotateDir = transform.up * vectorAction[1];
            transform.Rotate(rotateDir, Time.deltaTime * 200f);
            m_AgentRb.AddForce(dirToGo * Speed, ForceMode.VelocityChange);
        }
    }
}

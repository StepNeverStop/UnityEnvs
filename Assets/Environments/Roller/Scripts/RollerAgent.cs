using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using Unity.MLAgents.SideChannels;
using UnityEngine;

namespace Roller {
    public class RollerAgent : Agent {
        protected Rigidbody m_AgentRb;
        protected EnvironmentParameters m_ResetParams;

        private bool forceReset = false;

        public Transform Target;

        public override void Initialize() {
            m_AgentRb = GetComponent<Rigidbody>();
            m_ResetParams = Academy.Instance.EnvironmentParameters;
            m_ResetParams.RegisterCallback("force_reset", v => {
                forceReset = System.Convert.ToBoolean(v);
            });
        }

        protected bool IsOutOfRegion() {
            return transform.localPosition.x > 5 || transform.localPosition.x < -5
                || transform.localPosition.z > 5 || transform.localPosition.z < -5;
        }

        public override void OnEpisodeBegin() {
            if (forceReset || IsOutOfRegion()) {
                transform.localPosition = new Vector3(0, 0.5f, 0);
                m_AgentRb.angularVelocity = Vector3.zero;
                m_AgentRb.velocity = Vector3.zero;
            }

            Target.localPosition = new Vector3(Random.value * 8 - 4,
                                                  0.5f,
                                                  Random.value * 8 - 4);
        }

        public override void CollectObservations(VectorSensor sensor) {
            if (sensor != null) {
                sensor.AddObservation(Target.localPosition.x / 5);
                sensor.AddObservation(Target.localPosition.z / 5);

                sensor.AddObservation(transform.localPosition.x / 5);
                sensor.AddObservation(transform.localPosition.z / 5);

                // Agent velocity
                sensor.AddObservation(m_AgentRb.velocity.x / 5);
                sensor.AddObservation(m_AgentRb.velocity.z / 5);
            }
        }

        public float speed = 10;

        public override void OnActionReceived(float[] vectorAction) {
            forceReset = false;

            // Rewards
            float distanceToTarget = Vector3.Distance(transform.localPosition, Target.localPosition);

            if (distanceToTarget < 1.42f) { // Reached target
                SetReward(1.0f);
                EndEpisode();
            }
            else { // Time penalty
                SetReward(-0.01f);
            }

            // Fell off platform
            if (IsOutOfRegion()) {
                SetReward(-1.0f);
                EndEpisode();
            }

            // Actions, size = 2
            Vector3 controlSignal = Vector3.zero;
            controlSignal.x = vectorAction[0];
            controlSignal.z = vectorAction[1];
            m_AgentRb.AddForce(controlSignal * speed);
        }


        public override void Heuristic(float[] actionsOut) {
            actionsOut[0] = Input.GetAxis("Horizontal");
            actionsOut[1] = Input.GetAxis("Vertical");
        }
    }
}
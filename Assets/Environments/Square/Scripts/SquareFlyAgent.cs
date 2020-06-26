using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents.Sensors;

namespace Square {
    public class SquareFlyAgent : BaseSquareAgent {
        protected void GenerateAgent() {
            transform.localPosition = new Vector3(SpawnRadius * (Random.value * 2 - 1),
                                                        5f,
                                                        SpawnRadius * (Random.value * 2 - 1));
            transform.rotation = Quaternion.Euler(new Vector3(0f, Random.Range(0, 360)));
            m_AgentRb.velocity = Vector3.zero;
        }

        public bool IsAgentOut() {
            return transform.localPosition.x < -15 || transform.localPosition.x > 15
                || transform.localPosition.z < -15 || transform.localPosition.z > 15;
        }

        public override void OnEpisodeBegin() {
            foreach (var ray in rays) {
                float rayLength = m_ResetParams.GetWithDefault("ray_length", ray.RayLength);
                ray.RayLength = rayLength;
            }

            if (forceReset || IsAgentOut()) {
                GenerateAgent();
            }

            GenerateTarget();
        }

        public override void CollectObservations(VectorSensor sensor) {
            forceReset = false;

            if (sensor != null) {
                // Agent velocity
                var velocity = transform.InverseTransformDirection(m_AgentRb.velocity);
                sensor.AddObservation(velocity.x);
                sensor.AddObservation(velocity.z);

                sensor.AddObservation(transform.localPosition.x / 10f);
                sensor.AddObservation(transform.localPosition.z / 10f);

                sensor.AddObservation(Target.localPosition.x / 10f);
                sensor.AddObservation(Target.localPosition.z / 10f);
            }
        }

        public override void OnActionReceived(float[] vectorAction) {
            if (Vector2.Distance(new Vector2(Target.transform.localPosition.x, Target.transform.localPosition.z),
                    new Vector2(transform.localPosition.x, transform.localPosition.z)) <= 2f) {
                SetReward(1.0f);
                EndEpisode();
            }
            else if (IsAgentOut()) {
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
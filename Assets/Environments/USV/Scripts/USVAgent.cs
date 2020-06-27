using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using Unity.MLAgents.SideChannels;

namespace MLBoat
{
    public class USVAgent : Agent
    {
        public Transform Target;
        private AdvancedShipController controller;

        public override void Initialize()
        {
            controller = GetComponent<AdvancedShipController>();
            controller.Initialize();
        }

        private bool IsOutOfRegion()
        {
            return (new Vector2(transform.localPosition.x, transform.localPosition.z)).magnitude > 38;
        }

        private void GenerateTarget()
        {
            while (true)
            {
                float angle = Random.value * Mathf.PI * 2;
                var newPosition = new Vector3(20 * Mathf.Cos(angle),
                                                  0f,
                                                  20 * Mathf.Sin(angle));

                if (Vector3.Distance(transform.localPosition, newPosition) > 10f)
                {
                    Target.localPosition = newPosition;
                    break;
                }
            }
        }

        public override void OnEpisodeBegin()
        {
            if (IsOutOfRegion())
            {
                transform.rotation = new Quaternion(0, 0, 0, 0);
                transform.localPosition = new Vector3(Random.value, Random.value, Random.value);
                controller.ShipRigidbody.angularVelocity = Vector3.zero;
                controller.ShipRigidbody.velocity = Vector3.zero;
            }
            GenerateTarget();
        }

        public override void CollectObservations(VectorSensor sensor)
        {
            sensor.AddObservation(transform.localPosition); //3
            sensor.AddObservation(Target.localPosition);    //3

            sensor.AddObservation(controller.ShipRigidbody.velocity.x);
            sensor.AddObservation(controller.ShipRigidbody.velocity.z);

            sensor.AddObservation(transform.forward.x);
            sensor.AddObservation(transform.forward.z);
        }
        public override void OnActionReceived(float[] vectorAction)
        {
            controller.Act(vectorAction[0], vectorAction[1]);
            //vectorAction[0] up down
            float distanceToTarget = Vector3.Distance(transform.localPosition, Target.localPosition);

            SetReward(-0.01f);
            //SetReward((Vector3.Dot(transform.forward.normalized, controller.ShipRigidbody.velocity.normalized) - 1f) / 100);

            if (distanceToTarget < 4f)
            {
                SetReward(10.0f);
                EndEpisode();
            }
            else if (IsOutOfRegion())
            {
                SetReward(-1.0f);
                EndEpisode();
            }
        }


        public override void Heuristic(float[] actionsOut)
        {
            actionsOut[0] = Input.GetAxis("Vertical");
            actionsOut[1] = Input.GetAxis("Horizontal");
        }
    }
}

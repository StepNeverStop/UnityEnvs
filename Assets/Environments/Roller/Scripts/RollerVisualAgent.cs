using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents;
using System.Linq;
using Unity.MLAgents.SideChannels;
using Unity.MLAgents.Sensors;

namespace Roller {
    public class RollerVisualAgent : RollerAgent {
        public override void CollectObservations(VectorSensor sensor) {
            if (sensor != null) {
                // Agent velocity
                sensor.AddObservation(m_AgentRb.velocity.x / 5);
                sensor.AddObservation(m_AgentRb.velocity.z / 5);
            }
        }
    }
}
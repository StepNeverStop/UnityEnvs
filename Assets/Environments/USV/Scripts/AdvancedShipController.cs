using UnityEngine;
using System.Collections.Generic;

namespace MLBoat
{
    public class AdvancedShipController : MonoBehaviour {
        [Tooltip("Optional. Used to check if propellers are under water.")]
        [SerializeField]
        public WaterInterface waterInterface;

        [Tooltip("Handles all the user input.")]
        [SerializeField]
        public InputHandler input = new InputHandler();

        [Tooltip("List of engines. Each engine is a propulsion system in itself consisting of the engine and the propeller.")]
        [SerializeField]
        public List<Engine> engines = new List<Engine>();

        [Tooltip("List of rudders.")]
        [SerializeField]
        public List<Rudder> rudders = new List<Rudder>();

        public Rigidbody ShipRigidbody { get; private set; }

        /// <summary>
        /// Local Velocity vector of the rigidbody.
        /// </summary>
        public Vector3 LocalVelocity {
            get { return transform.InverseTransformDirection(ShipRigidbody.velocity); }
        }

        /// <summary>
        /// Speed in m/s.
        /// </summary>
        public float Speed {
            get { return LocalVelocity.z; }
        }

        /// <summary>
        /// Speed in knots.
        /// </summary>
        public float SpeedKnots {
            get { return Speed * 1.944f; }
        }

        public void Initialize() {
            ShipRigidbody = GetComponent<Rigidbody>();

            foreach (Rudder rudder in rudders)
                rudder.Initialize(this);

            foreach (Engine engine in engines) {
                engine.Initialize(this);
            }
        }

        public void Act(float throttle = 0, float rudder = 0) {
            input.Update(throttle, rudder);
            rudders.ForEach(p =>
            {
                p.Update();
            });

            engines.ForEach(p => {
                p.Update();
            });
            
        }

        private void OnDrawGizmos() {
            Initialize();

            foreach (Rudder rudder in rudders) {
                Gizmos.color = Color.magenta;
            }

            foreach (Engine e in engines) {
                Gizmos.color = Color.red;
                Gizmos.DrawSphere(transform.TransformPoint(e.thrustPosition), 0.2f);
                Gizmos.DrawRay(new Ray(e.WorldPosition, e.WorldDirection));
            }
        }
    }
}
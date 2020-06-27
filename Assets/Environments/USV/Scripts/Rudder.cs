using UnityEngine;
using System.Collections;

namespace MLBoat {
    /// <summary>
    /// Represents a single rudder. If rudder has a floating object component it will also be used for steering and not be visual-only.
    /// </summary>
    [System.Serializable]
    public class Rudder {
        [Tooltip("Name of the rudder. Can be any string.")]
        public string name = "Rudder";

        [Tooltip("Transform representing the rudder.")]
        public Transform rudderTransform;

        [Tooltip("Max angle in degrees rudder will be able to reach.")]
        public float maxAngle = 45f;

        [Tooltip("Rotation speed in degrees per second.")]
        public float rotationSpeed = 100f;

        private AdvancedShipController sc;
        private float angle=0f;

        public float Angle {
            get { return angle; }
        }

        public float AnglePercent {
            get { return Angle / maxAngle; }
        }

        public void Initialize(AdvancedShipController sc) {
            this.sc = sc;
        }

        public void Update() {
            if (rudderTransform != null) {
                float targetAngle = -sc.input.rudder * maxAngle;
                angle = Mathf.MoveTowardsAngle(Angle, targetAngle, rotationSpeed * Time.fixedDeltaTime);
                //rudderTransform.localEulerAngles = new Vector3(rudderTransform.localEulerAngles.x, angle, rudderTransform.localEulerAngles.z);
                rudderTransform.localRotation = Quaternion.Euler(0, angle, 0);
            }
        }
    }

}
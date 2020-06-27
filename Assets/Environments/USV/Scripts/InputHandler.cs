using UnityEngine;
using UnityEngine.UI;
using System.Collections;

namespace MLBoat {
    public class InputHandler {
        [Range(-1f, 1f)]
        public float rudder;

        [Range(-1f, 1f)]
        public float throttle;

        public void Update(float throttle = 0, float rudder = 0) {
            // Clamp input values to -1, 1
            this.throttle = Mathf.Clamp(throttle, -1f, 1f);
            this.rudder = Mathf.Clamp(rudder, -1f, 1f);
        }
    }
}
using UnityEngine;
using System.Collections;

namespace MLBoat {
    public class CameraDriver : MonoBehaviour {
        public Transform target;
        [Range(0f, 1f)]
        public float positionModifier = 0.05f;
        public float positionSmoothing = 0.2f;

        // Mouse look direction
        public bool mouseLook = true;
        [Range(0, 20f)]
        public float mouseSpeed = 20f;
        public float horizontalMaxAngle = 110f;
        public float verticalMaxAngle = 40f;

        private Vector3 initialPosition;
        private Vector3 rotation;

        private Vector3 velocity;
        private Vector3 prevVelocity;
        private Vector3 acceleration;
        private Vector3 accSpeed;

        public AdvancedShipController shipController;
        public Rigidbody shipRigidbody;

        void Start() {
            shipController = target.GetComponent<AdvancedShipController>();
            initialPosition = shipController.transform.InverseTransformPoint(transform.position);
        }

        void Update() {
            prevVelocity = velocity;
            velocity = shipRigidbody.velocity;
            acceleration = Vector3.SmoothDamp(acceleration, (velocity - prevVelocity) / Time.deltaTime, ref accSpeed, positionSmoothing);
            transform.position = shipController.transform.TransformPoint(initialPosition - acceleration * positionModifier * 0.1f);

            rotation.y = Mathf.Clamp(Mathf.Lerp(rotation.y, xMouse() * horizontalMaxAngle, Time.deltaTime * mouseSpeed), -horizontalMaxAngle, horizontalMaxAngle);
            rotation.x = Mathf.Clamp(Mathf.Lerp(rotation.x, -yMouse() * verticalMaxAngle, Time.deltaTime * mouseSpeed), -verticalMaxAngle, verticalMaxAngle);

            if (mouseLook && Input.GetMouseButton(1)) transform.localRotation = Quaternion.Euler(rotation);
        }

        private float xMouse() {
            float percent = Input.mousePosition.x / Screen.width;
            if (percent < 0.5f)
                return -(0.5f - percent) * 2.0f;
            return (percent - 0.5f) * 2.0f;
        }

        private float yMouse() {
            float percent = Input.mousePosition.y / Screen.height;
            if (percent < 0.5f)
                return -(0.5f - percent) * 2.0f;
            return (percent - 0.5f) * 2.0f;
        }
    }

}
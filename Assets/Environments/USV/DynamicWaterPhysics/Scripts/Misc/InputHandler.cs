using UnityEngine;
using System.Collections;

namespace NWH
{
    [System.Serializable]
    public class InputHandler
    {
        public enum InputMode
        {
            Mouse, Keyboard
        }
        public InputMode inputMode;

        public bool shiftUp;
        public bool shiftDown;

        public float xAxis;
        public float yAxis;
        public bool handbrake;

        // Update mappings
        public void Update()
        {
            if(inputMode == InputMode.Keyboard)
            {
                xAxis = Input.GetAxis("Horizontal");
                yAxis = Input.GetAxis("Vertical");
            }
            else
            {
                xAxis = xMouse();
                yAxis = yMouse();
            }
        }

        private float xMouse()
        {
            float percent = Mathf.Clamp(Input.mousePosition.x / Screen.width, -1f, 1f);
            if (percent < 0.5f)
                return -(0.5f - percent) * 2.0f;
            return (percent - 0.5f) * 2.0f;
        }

        private float yMouse()
        {
            float percent = Mathf.Clamp(Input.mousePosition.y / Screen.height, -1f, 1f);
            if (percent < 0.5f)
                return -(0.5f - percent) * 2.0f;
            return (percent - 0.5f) * 2.0f;
        }
    }
}


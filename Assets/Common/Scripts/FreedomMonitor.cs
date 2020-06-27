using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UnityEnvs
{
    public class FreedomMonitor : MonoBehaviour
    {
        private Camera cam;

        /// <summary>
        /// 移动的参量,改变参数值来改变摄像机的偏移差值
        /// </summary>
        public Vector2 moveOffest = new Vector2(45, 45);
        public float scrollSpeed = 4.0f;
        public float pullSpeed = 0.1f;
        Vector2 currentMousePos;

        [ReadOnly]
        [Tooltip("摄像机的最小高度")]
        public float minHigh;
        [ReadOnly]
        [Tooltip("摄像机的最大高度")]
        public float maxHigh;

        // Start is called before the first frame update
        void Start()
        {
            cam = Camera.main;
            minHigh /= Mathf.Tan(Mathf.Deg2Rad * cam.fieldOfView / 2);
            maxHigh /= Mathf.Tan(Mathf.Deg2Rad * cam.fieldOfView / 2);
        }

        // Update is called once per frame
        void Update()
        {
            Vector3 cam_pos = cam.transform.position;
            Vector3 cam_rot = cam.transform.rotation.eulerAngles;
            if (Input.GetMouseButtonDown(0) || Input.GetMouseButtonDown(1))
            {
                currentMousePos = Input.mousePosition;
            }
            if (Input.GetMouseButton(0))
            {
                float x_offset = (currentMousePos.x - Input.mousePosition.x) / moveOffest.x;
                float z_offset = (currentMousePos.y - Input.mousePosition.y) / moveOffest.y;
                currentMousePos = Input.mousePosition;
                cam_pos.x += x_offset;
                cam_pos.z += z_offset;
            }

            if (Input.GetMouseButton(1))
            {
                cam_rot.x += Mathf.Sign(currentMousePos.y - Input.mousePosition.y) * pullSpeed;
                cam_rot.x = Mathf.Clamp(cam_rot.x, 30.0f, 90.0f);
            }

            cam_pos.y += Input.GetAxis("Mouse ScrollWheel") * -1 * scrollSpeed;
            cam_pos.y = Mathf.Clamp(cam_pos.y, minHigh, maxHigh);

            cam.transform.position = cam_pos;
            cam.transform.rotation = Quaternion.Euler(cam_rot);
        }
    }
}
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DragObject : MonoBehaviour {

    private Rigidbody rb;
    private bool dragging;
    private Ray mouseRay;
    private RaycastHit hit;
    private Vector3 localHitPoint;
    private Vector3 globalHitPoint;

    private Vector3 rbScreenPos;
    private Camera cam;

    private float distance;
    private float forceMagnitude;

    private Vector3 direction;
    private Vector3 force;
    private float forceXZ;
    private Vector3 resultantForce;

    private void Start()
    {
        cam = GetComponent<Camera>();
    }

    void Update()
    {
        mouseRay = cam.ScreenPointToRay(Input.mousePosition);

        if (Input.GetMouseButtonDown(2))
        {
            if(Physics.Raycast(mouseRay, out hit, 600f))
            {
                if (rb = hit.transform.GetComponent<Rigidbody>())
                {
                    dragging = true;
                    localHitPoint = rb.transform.InverseTransformPoint(hit.point);
                }
                else
                {
                    dragging = false;
                }
            }
        }

        if (Input.GetMouseButtonUp(2))
        {
            dragging = false;
        }      
    }

    void FixedUpdate()
    {
        if(dragging)
        {
            globalHitPoint = rb.transform.TransformPoint(localHitPoint);
            rbScreenPos = cam.WorldToScreenPoint(globalHitPoint);

            distance = Vector2.Distance(Input.mousePosition, rbScreenPos);
            forceMagnitude = distance * rb.mass * 0.1f;

            direction = (Input.mousePosition - rbScreenPos).normalized;
            force = forceMagnitude * direction;
            forceXZ = force.x + force.z;
            resultantForce = new Vector3(forceXZ * transform.right.x, force.y, forceXZ * transform.right.z);
            resultantForce = Vector3.ClampMagnitude(resultantForce, rb.mass * 100f);
            rb.AddForceAtPosition(resultantForce, globalHitPoint);
        }
    }

    void OnDrawGizmos()
    {
        if (dragging)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawSphere(globalHitPoint, 0.01f);
        }
    }

}

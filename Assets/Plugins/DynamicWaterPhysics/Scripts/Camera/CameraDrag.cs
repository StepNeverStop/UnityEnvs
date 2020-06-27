using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class CameraDrag : MonoBehaviour {
	public Transform TargetLookAt;
	public float Distance = 25.0f;
	public float DistanceMin = 10.0f;
	public float DistanceMax = 50.0f;    
	private float mouseX = 0.0f;
	private float mouseY  = 0.0f;
	private float startingDistance = 0.0f;    
	private float desiredDistance = 0.0f;    
	public float X_MouseSensitivity = 5.0f;
	public float Y_MouseSensitivity = 5.0f;
	public float MouseWheelSensitivity = 5.0f;
	public float Y_MinLimit = -40.0f;
	public float Y_MaxLimit = 80.0f;    
	public float DistanceSmooth = 0.05f;    
	private float velocityDistance = 0.0f;    
	public Vector3 desiredPosition = Vector3.zero;    
	public float X_Smooth = 0.05f;
	public float Y_Smooth = 0.1f;
	private float velX = 0.0f;
	private float velY = 0.0f;
	private float velZ = 0.0f;
	private Vector3 position = Vector3.zero;
    public bool useFixedUpdate = false;
    public bool keepAboveWater = true;

    void Start() 
	{
		 Distance = Mathf.Clamp(Distance, DistanceMin, DistanceMax);
		 startingDistance = Distance;
		 Reset();
	}

	void LateUpdate()
	{
        if (!useFixedUpdate) CameraUpdate();
    }

    private void FixedUpdate()
    {
        if (useFixedUpdate) CameraUpdate();
    }

    void CameraUpdate()
    {
        if (TargetLookAt == null)
            return;

        HandlePlayerInput();
        CalculateDesiredPosition();
        UpdatePosition();
    }

	public void HandlePlayerInput()
	{
		float deadZone = 0.02f;

        if (Input.GetMouseButton(1))
        {
            mouseX += Input.GetAxis("Mouse X") * X_MouseSensitivity;
            mouseY -= Input.GetAxis("Mouse Y") * Y_MouseSensitivity;
        }

        // this is where the mouseY is limited - Helper script
        mouseY = ClampAngle(mouseY, Y_MinLimit, Y_MaxLimit);
		 
		 // get Mouse Wheel Input
		 if (Input.GetAxis("Mouse ScrollWheel") < -deadZone || Input.GetAxis("Mouse ScrollWheel") > deadZone)
		 {
		     desiredDistance = Mathf.Clamp(Distance - (Input.GetAxis("Mouse ScrollWheel") * MouseWheelSensitivity), 
		                                                                         DistanceMin, DistanceMax);
		 }
	}

	public void CalculateDesiredPosition()
	{
		 // Evaluate distance
		 Distance = Mathf.SmoothDamp(Distance, desiredDistance, ref velocityDistance, DistanceSmooth);
		 
		 // Calculate desired position -> Note : mouse inputs reversed to align to WorldSpace Axis
		 desiredPosition = CalculatePosition(mouseY, mouseX, Distance);

         if (keepAboveWater && desiredPosition.y < 0.4f) desiredPosition.y = 0.4f;
	}

	public Vector3 CalculatePosition(float rotationX, float rotationY, float distance)
	{
		Vector3 direction = new Vector3(0, 0, -distance);
		Quaternion rotation = Quaternion.Euler(rotationX, rotationY, 0);
	 	return TargetLookAt.position + (rotation * direction);
	}

	public void UpdatePosition()
	{
		 var posX = Mathf.SmoothDamp(position.x, desiredPosition.x, ref velX, X_Smooth);
		 var posY = Mathf.SmoothDamp(position.y, desiredPosition.y, ref velY, Y_Smooth);
		 var posZ = Mathf.SmoothDamp(position.z, desiredPosition.z, ref velZ, X_Smooth);
		 position = new Vector3(posX, posY, posZ);
		 
		 transform.position = position;
		 
		 transform.LookAt(TargetLookAt);
	}

	public void Reset() 
	{
		 mouseX = 0;
		 mouseY = 10;
		 Distance = startingDistance;
		 desiredDistance = Distance;
	}

	public float  ClampAngle(float angle, float min, float max)
	{
		 while (angle < -360 || angle > 360)
		 {
		     if (angle < -360)
		         angle += 360;
		     if (angle > 360)
		         angle -= 360;
		 }
		 
		 return Mathf.Clamp(angle, min, max);
	}
}
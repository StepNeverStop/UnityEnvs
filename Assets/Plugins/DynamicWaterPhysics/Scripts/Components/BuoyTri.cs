using UnityEngine;
using System.Collections;

public class BuoyTri
{
    /// <summary>
    /// Center of the triangle.
    /// </summary>
    public Vector3 center;

    /// <summary>
    /// Normal of the triangle.
    /// </summary>
    public Vector3 normal;

    /// <summary>
    /// Distance from the center of the triangle to the water surface.
    /// </summary>
    public float distanceToSurface = 0;

    /// <summary>
    /// Triangle area.
    /// </summary>
    public float area = 0;

    /// <summary>
    /// Triangle velocity.
    /// </summary>
    public Vector3 velocity;

    /// <summary>
    /// Velocity vector length.
    /// </summary>
    public float velocityMagnitude = 0;

    public Vector3 u;
    public Vector3 v;
    public Vector3 crossUV;
    public float crossMagnitude = 0;

    /// <summary>
    /// Velocity direction.
    /// </summary>
    public Vector3 velocityNormalized;

    /// <summary>
    /// Dot between triangle velocity and triangle normal. Positive denotes velocity and normal have same direction. [-1, 1]
    /// </summary>
    public float dotNormalVelocityNormal = 0;

    /// <summary>
    /// Velocity component in the direction of the triangle normal.
    /// </summary>
    public float velocityMagTimesDot = 0;

    /// <summary>
    /// Force as a result of velocity component parallel to the triangle normal.
    /// </summary>
    public Vector3 dynamicForce;

    /// <summary>
    /// Force as a result of triangle volume.
    /// </summary>
    public Vector3 buoyantForce;

    /// <summary>
    /// Sum of buoyant, dynamic and surface force.
    /// </summary>
    public Vector3 resultantForce;

    //public bool underPressure = false;

    /// <summary>
    /// Triangle vertex
    /// </summary>
    public Vector3 p1, p2, p3;

    // This triangle is part of another, partially submerged triangle
    // Manually multipy vectors to avoid function calls.
    public bool atSurface = false;

    private float resistance, upDot, fa;

    public void Set(Vector3 p1, Vector3 p2, Vector3 p3, FloatingObject fo, int atSurface)
    {
        this.p1 = p1;
        this.p2 = p2;
        this.p3 = p3;

        if(atSurface < 3)
        {
            this.atSurface = true;
        }

        center.x = (p1.x + p2.x + p3.x) / 3f;
        center.y = (p1.y + p2.y + p3.y) / 3f;
        center.z = (p1.z + p2.z + p3.z) / 3f;

        u.x = p2.x - p1.x;
        u.y = p2.y - p1.y;
        u.z = p2.z - p1.z;

        v.x = p3.x - p1.x;
        v.y = p3.y - p1.y;
        v.z = p3.z - p1.z;

        crossUV = Vector3.Cross(u, v);
        crossMagnitude = Mathf.Sqrt(crossUV.x * crossUV.x + crossUV.y * crossUV.y + crossUV.z * crossUV.z);

        // Normal
        if(crossMagnitude == 0)
        {
            normal.x = normal.y = normal.z = 0f;
        }
        else
        {
            normal.x = crossUV.x / crossMagnitude;
            normal.y = crossUV.y / crossMagnitude;
            normal.z = crossUV.z / crossMagnitude;
        }

        // Surface distance
        distanceToSurface = fo.GetDistanceToSurface(center, fo.WaterHeightFunction);

        // Area
        area = crossMagnitude * 0.5f;

        // Velocity
        velocity = fo.rb.GetPointVelocity(center);

        if (distanceToSurface < 0)
        {
            // Get velocity magnitude
            velocityMagnitude = Mathf.Sqrt(velocity.x * velocity.x + velocity.y * velocity.y + velocity.z * velocity.z);

            // Get normalized velocity
            if (velocityMagnitude == 0)
            {
                velocityNormalized.x = velocityNormalized.y = velocityNormalized.z = 0f;
            }
            else
            {
                velocityNormalized.x = velocity.x / velocityMagnitude;
                velocityNormalized.y = velocity.y / velocityMagnitude;
                velocityNormalized.z = velocity.z / velocityMagnitude;
            }

            // Calculate dynamic forces
            dotNormalVelocityNormal = Vector3.Dot(velocityNormalized, normal);
            velocityMagTimesDot = velocityMagnitude * dotNormalVelocityNormal;
            resistance = -velocityMagTimesDot * fo.dynamicForceFactor;

            // Calculate resistance force
            dynamicForce.x = normal.x * resistance;
            dynamicForce.y = normal.y * resistance;
            dynamicForce.z = normal.z * resistance;

            // Calculated buoyant force (positive)
            upDot = Vector3.Dot(normal, Physics.gravity.normalized) * distanceToSurface;
            Vector3 gravityDir = Physics.gravity.y * Vector3.up;
            buoyantForce.x = gravityDir.x * upDot;
            buoyantForce.y = gravityDir.y * upDot;
            buoyantForce.z = gravityDir.z * upDot;

            // Sum forces
            fa = fo.fluidDensity * area;
            resultantForce.x = (buoyantForce.x + dynamicForce.x) * fa;
            resultantForce.y = (buoyantForce.y + dynamicForce.y) * fa;
            resultantForce.z = (buoyantForce.z + dynamicForce.z) * fa;
        }
    }
}  

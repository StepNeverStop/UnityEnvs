using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents.Policies;

public class CubeMove : MonoBehaviour
{
    public float discrete_speed = 1.0f;
    public float continouse_speed = 400.0f;
    public float neg_factor = 0.2f;


    public void MoveAgent(SpaceType action_type, Transform transform, Rigidbody rBody, float[] act)
    {
        if (action_type == SpaceType.Continuous)
        {
            ContinuousMove(transform, rBody, act);
        }
        else
        {
            DiscreteMove(transform, rBody, act);
        }

    }

    public void DiscreteMove(Transform transform, Rigidbody rBody, float[] act)
    {

        var dirToGo = Vector3.zero;
        var rotateDir = Vector3.zero;

        var action = Mathf.FloorToInt(act[0]);

        // Goalies and Strikers have slightly different action spaces.
        switch (action)
        {
            case 1:
                dirToGo = transform.forward * 1f;
                break;
            case 2:
                dirToGo = transform.forward * -1f;
                break;
            case 3:
                rotateDir = transform.up * 1f;
                break;
            case 4:
                rotateDir = transform.up * -1f;
                break;
            case 5:
                dirToGo = transform.right * -0.75f;
                break;
            case 6:
                dirToGo = transform.right * 0.75f;
                break;
        }
        transform.Rotate(rotateDir, Time.fixedDeltaTime * 200f);
        rBody.AddForce(dirToGo * discrete_speed, ForceMode.VelocityChange);
    }

    public void ContinuousMove(Transform transform, Rigidbody rBody, float[] act)
    {
        transform.Rotate(transform.up, Time.fixedDeltaTime * act[0] * 100.0f);
        Vector3 force_vector = transform.forward * act[1] * continouse_speed;
        if (act[1] < 0)
        {
            force_vector *= neg_factor;
        }
        rBody.AddForceAtPosition(force_vector, transform.position);
    }

}

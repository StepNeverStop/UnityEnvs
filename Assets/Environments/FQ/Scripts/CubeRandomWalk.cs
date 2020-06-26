using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CubeRandomWalk : MonoBehaviour
{
    public Vector2Int last_range = Vector2Int.zero;
    public float speed = 20.0f;
    public float rot_speed = 20.0f;
    private Rigidbody rb;
    private int count = 0;
    private int end_count = 0;
    private float rot;
    private float force;
    // Start is called before the first frame update
    void Start()
    {
        rb = gameObject.GetComponent<Rigidbody>();
    }

    // Update is called once per frame
    void Update()
    {
        if(count < end_count && isActiveAndEnabled)
        {
            RunDynamic();
            count++;
        }
        else
        {
            ResetDynamic();
        }
    }

    /// <summary>
    /// 重新设置随机运动
    /// </summary>
    public void ResetDynamic()
    {
        count = 0;
        end_count = Random.Range(last_range[0], last_range[1]);
        rot = Random.Range(-1.0f, 1.0f);
        force = Random.Range(-0.6f, 1.0f);
    }

    public void RunDynamic()
    {
        this.transform.Rotate(transform.up, Time.fixedDeltaTime * rot * rot_speed);
        Vector3 force_vector = transform.forward * force * speed;
        //rb.AddForceAtPosition(force_vector, this.transform.position);
        rb.AddForce(force_vector, ForceMode.VelocityChange);

        if (this.transform.position.y <0)
        {
            gameObject.SetActive(false);
        }
    }
}

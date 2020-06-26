using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AutoDestroy : MonoBehaviour
{

    public float life_time = 1.0f;

    // Start is called before the first frame update
    void Start()
    {
        DestroyObjectDelayed();
    }


    void DestroyObjectDelayed()
    {
        // 5秒后销毁当前gameobject
        Destroy(gameObject, life_time);
    }

    //private void OnCollisionEnter(Collision collision)
    //{
    //    if (collision.gameObject.CompareTag("agent"))
    //    {
    //        Destroy(gameObject);
    //    }
    //}

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("agent"))
        {
            Destroy(gameObject);
        }
    }
}

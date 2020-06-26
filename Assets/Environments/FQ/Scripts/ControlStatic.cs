using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ControlStatic : MonoBehaviour
{
    public string obj_tag;
    public GameObject Ground;
    private List<GameObject> objs = new List<GameObject>();

    void Start()
    {
        Transform father = transform.parent.gameObject.GetComponent<Transform>();
        foreach (Transform child in father)
        {
            if (child.CompareTag(obj_tag))
            {
                objs.Add(child.gameObject);
            }
        }
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void ResetPosition()
    {
        foreach(var dobj in objs)
        {
            dobj.transform.position = new Vector3(Random.Range(-1.0f, 1.0f) * (Ground.transform.localScale.x * 5.0f - transform.localScale.x * 0.5f),
                                                0.5f,
                                                Random.Range(-1.0f, 1.0f) * (Ground.transform.localScale.x * 5.0f - transform.localScale.x * 0.5f));
            dobj.GetComponent<Rigidbody>().angularVelocity = Vector3.zero;
            dobj.GetComponent<Rigidbody>().velocity = Vector3.zero;
        }
    }
}

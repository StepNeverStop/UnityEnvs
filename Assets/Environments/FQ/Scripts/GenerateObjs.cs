using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GenerateObjs : MonoBehaviour
{

    public GameObject[] Objs;
    public int[] nums;
    // Start is called before the first frame update

    private void Awake()
    {
        for(int i=0; i<nums.Length; i++)
        {
            for (int j=0; j<nums[i]; j++)
            {
                GameObject prefabInstance = Instantiate(Objs[i]);
                prefabInstance.transform.parent = this.transform;
            }
        }
    }
}

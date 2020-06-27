using System.Collections;
using System.Collections.Generic;
using System.Linq.Expressions;
using Unity.Barracuda;
using UnityEngine;

/// <summary>
/// 获取训练区域的信息，包括训练区域的尺寸大小
/// </summary>
public class GetAreaInfo : MonoBehaviour
{
    /// <summary>
    /// 训练区域的界限物体，一般为地板平面
    /// </summary>
    public GameObject BoundObject;
    /// <summary>
    /// 界限物体的默认名称
    /// </summary>
    public string DefaultBoundName = "Floor";
    /// <summary>
    /// 界限物体的默认标签
    /// </summary>
    public string DefaultBoundTag = "floor";

    private static List<Transform> objs = new List<Transform>();

    /// <summary>
    /// 训练区域的尺寸大小，分别为x、y、z轴的长度
    /// </summary>
    [HideInInspector]
    public Vector3 size = Vector3.zero;

    // Start is called before the first frame update
    public Vector3 GetSize()
    {
        GetAllChilds(this.transform);

        if(BoundObject == null)
        {
            foreach(Transform t in objs)
            {
                //Debug.Log(t.name);
                if (t.name == DefaultBoundName || t.tag == DefaultBoundTag)
                {
                    BoundObject = t.gameObject;
                    break;
                }
            }
        }
        //Debug.Log(BoundObject);
        size = BoundObject.GetComponent<MeshFilter>().mesh.bounds.size;
        size = new Vector3(size.x * BoundObject.transform.localScale.x, size.y * BoundObject.transform.localScale.y, size.z * BoundObject.transform.localScale.z);
        return size;
    }

    private void GetAllChilds(Transform T)
    {
        int childNum = T.childCount;
        if (childNum > 0)
        {
            for (int i=0; i<childNum;i++)
            {
                GetAllChilds(T.GetChild(i));
            }
        }
        objs.Add(T);
    }
}

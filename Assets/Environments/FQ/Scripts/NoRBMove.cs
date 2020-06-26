using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NoRBMove : MonoBehaviour
{

    public GameObject traj;
    public GameObject floor;
    public Rigidbody hunter;
    public float detect_range = 5.0f;
    public float move_range = 2.0f;
    //private Rigidbody rb;
    public float prob = 0.1f;

    private List<Vector3> pos_candidate = new List<Vector3>();

    private float sqrt_move_range;
    private GameObject ground;

    // Start is called before the first frame update
    void Start()
    {
        //rb = this.GetComponent<Rigidbody>();
        ground = GameObject.FindGameObjectWithTag("ground");
        sqrt_move_range = Mathf.Sqrt(move_range);
    }


    public void OnDecisionMove()
    {
        if(Random.value < prob)
        {
            float dis = Mathf.Sqrt((hunter.transform.localPosition - this.transform.localPosition).sqrMagnitude);

            if (dis <= detect_range)
            {
                pos_candidate.Add(new Vector3(this.transform.localPosition.x - move_range, this.transform.localPosition.y, this.transform.localPosition.z));
                pos_candidate.Add(new Vector3(this.transform.localPosition.x + move_range, this.transform.localPosition.y, this.transform.localPosition.z));
                pos_candidate.Add(new Vector3(this.transform.localPosition.x, this.transform.localPosition.y, this.transform.localPosition.z - move_range));
                pos_candidate.Add(new Vector3(this.transform.localPosition.x, this.transform.localPosition.y, this.transform.localPosition.z + move_range));
                pos_candidate.Add(new Vector3(this.transform.localPosition.x + sqrt_move_range, this.transform.localPosition.y, this.transform.localPosition.z + sqrt_move_range));
                pos_candidate.Add(new Vector3(this.transform.localPosition.x + sqrt_move_range, this.transform.localPosition.y, this.transform.localPosition.z - sqrt_move_range));
                pos_candidate.Add(new Vector3(this.transform.localPosition.x - sqrt_move_range, this.transform.localPosition.y, this.transform.localPosition.z + sqrt_move_range));
                pos_candidate.Add(new Vector3(this.transform.localPosition.x - sqrt_move_range, this.transform.localPosition.y, this.transform.localPosition.z - sqrt_move_range));

                int max_index = 0;
                float max_dis = 0.0f;
                for (int i = 0; i < pos_candidate.Count; i++)
                {
                    if (CheckOutOfRange(pos_candidate[i]))
                    {
                        float v_dis = (pos_candidate[i] - hunter.transform.localPosition).sqrMagnitude;
                        if (v_dis > max_dis)
                        {
                            max_dis = v_dis;
                            max_index = i;
                        }
                    }
                }

                GameObject prefabInstance = Instantiate(traj);
                prefabInstance.transform.parent = this.transform.parent.transform;
                prefabInstance.transform.localPosition = this.transform.localPosition;


                this.transform.localPosition = pos_candidate[max_index];

                pos_candidate.Clear();
            }
        }
        
    }

    private bool CheckOutOfRange(Vector3 v)
    {
        if ((-ground.transform.localScale.x * 4.5f < v.x && v.x < ground.transform.localScale.x * 4.5f)
            &&
            (-ground.transform.localScale.z * 4.5f < v.z && v.z < ground.transform.localScale.z * 4.5f))
        {
            return true;
        }
        else
        {
            return false;
        }
    }
}

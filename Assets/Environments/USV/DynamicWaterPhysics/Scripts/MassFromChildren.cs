using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
[ExecuteInEditMode]
public class MassFromChildren : MonoBehaviour {

    private FloatingObject[] fos;
    private Rigidbody rb;

    public void Calculate()
    {
        fos = GetComponentsInChildren<FloatingObject>();

        if (fos.Length > 0)
        {
            rb = GetComponent<Rigidbody>();
            float massSum = 0;

            foreach (FloatingObject fo in fos)
            {
                massSum += fo.MaterialMass;
            }

            FloatingObject f = null;
            if (f = GetComponent<FloatingObject>()){
                massSum += f.MaterialMass;
            }

            if(massSum > 1)
            {
                rb.mass = massSum;
            }
        }
    }

}

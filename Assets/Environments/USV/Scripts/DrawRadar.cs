using UnityEngine;
using System.Collections;

public class DrawRadar : MonoBehaviour {
    [Range(0.01f, 0.1f)]
    public float ThetaScale = 0.01f;
    public float radius = 3f;
    private int Size;
    private LineRenderer LineDrawer;
    private float Theta = 0f;

    void Start() {
        LineDrawer = GetComponent<LineRenderer>();
    }

    void Update() {
        Size = (int)(1f / ThetaScale) + 1;
        LineDrawer.positionCount = Size;
        for (int i = 0; i < Size; i++) {
            Theta = 2 * Mathf.PI * ThetaScale * i;
            float x = radius * Mathf.Cos(Theta);
            float y = radius * Mathf.Sin(Theta);
            LineDrawer.SetPosition(i, new Vector3(x, 0, y));
        }
    }
}
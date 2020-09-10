using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaterLine
{
    public Vector3 p0;
    public Vector3 p1;
    public BuoyTri tri;

    public WaterLine(Vector3 p0, Vector3 p1, BuoyTri tri)
    {
        this.p0 = p0;
        this.p1 = p1;
        this.tri = tri;
    }
}

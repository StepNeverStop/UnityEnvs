using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace NWH
{
    public class History
    {
        public int id;
        public List<int> removedTriangles = new List<int>();
        public List<ArrayList> replacedVertex = new List<ArrayList>();

        public void RemovedTriangle(int f)
        {
            removedTriangles.Add(f);
        }

        public void ReplaceVertex(int f, int u, int v, Vector3 normal, Vector2 uv, int newV, Vector3 newNormal, Vector2 newUv)
        {
            ArrayList list = new ArrayList();
            list.Insert(0, f);
            list.Insert(1, u);
            list.Insert(2, v);
            list.Insert(3, normal);
            list.Insert(4, uv);
            list.Insert(5, newV);
            list.Insert(6, newNormal);
            list.Insert(7, newUv);
            replacedVertex.Add(list);
        }
    }
}


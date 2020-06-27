using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class MeshUtils {

    public static Mesh RasterizeMesh(Mesh oMesh, int resolution)
    {
        List<Vert> verts = GetVerts(oMesh);
        List<Tri> tris = GetTris(oMesh, verts);

        RemoveDuplicateVerts(tris, verts);

        Vector3 bounds = new Vector3(oMesh.bounds.size.x, oMesh.bounds.size.y, oMesh.bounds.size.z);
        Vector3 steps = new Vector3(bounds.x / resolution, bounds.y / resolution, bounds.z / resolution);

        List<Vector3> raster = new List<Vector3>();
        for (int x = 0; x <= resolution; x++)
        {
            for (int y = 0; y <= resolution; y++)
            {
                for (int z = 0; z <= resolution; z++)
                {
                    raster.Add(new Vector3(-(bounds.x / 2f) + x * steps.x, -(bounds.y / 2f) + y * steps.y, -(bounds.z / 2f) + z * steps.z));
                }
            }
        }

        for (int i = 0; i < verts.Count; i++)
        {
            Vert v = verts[i];

            float minDist = Mathf.Infinity;
            int minIndex = -1;
            for (int j = 0; j < raster.Count; j++)
            {
                float dist = Vector3.Distance(v.pos, raster[j]);
                if (dist < minDist)
                {
                    minDist = dist;
                    minIndex = j;
                }
            }

            v.pos = raster[minIndex];
        }

        RemoveDuplicateVerts(tris, verts);

        return GenerateMesh(verts, tris);
    }

    public static Mesh GenerateMesh(List<Vert> verts, List<Tri> tris)
    {
        var oVerts = new List<Vector3>();
        var oTris = new List<int>();
        var oNormals = new List<Vector3>();

        for (int i = 0; i < verts.Count; i++)
        {
            oVerts.Add(verts[i].pos);
            oNormals.Add(verts[i].normal);
        }

        for (int i = 0; i < tris.Count; i++)
        {
            oTris.Add(tris[i].verts[0].id);
            oTris.Add(tris[i].verts[1].id);
            oTris.Add(tris[i].verts[2].id);
        }

        Mesh mesh = new Mesh();
        mesh.name = "GeneratedMesh";
        mesh.vertices = oVerts.ToArray();
        mesh.triangles = oTris.ToArray();
        mesh.normals = oNormals.ToArray();
        mesh.RecalculateBounds();
        mesh.RecalculateNormals();

        return mesh;
    }

    public static List<Vert> GetVerts(Mesh oMesh)
    {
        List<Vector3> oVerts = oMesh.vertices.ToList<Vector3>();
        List<Vector3> oNormals = oMesh.normals.ToList<Vector3>();
        List<Vert> verts = new List<Vert>();

        // Add new verts
        for (int i = 0; i < oVerts.Count; i++)
        {
            verts.Add(new Vert(i, oVerts[i], oNormals[i]));
        }

        return verts;
    }

    public static List<Tri> GetTris(Mesh oMesh, List<Vert> verts)
    {
        List<int> oTris = oMesh.triangles.ToList<int>();
        List<Tri> tris = new List<Tri>();

        // Add new tris
        for (int i = 0; i < oTris.Count; i += 3)
        {
            tris.Add(new Tri(verts[oTris[i]], verts[oTris[i+1]], verts[oTris[i+2]]));
        }
        return tris;
    }

    public static List<Vert> CollapseVert(Vert replaceWithVert, Vert replaceVert, List<Tri> tris, List<Vert> verts)
    {
        var triIds = TrianglesContainingVerts(replaceWithVert, replaceVert, tris);

        while (triIds.Count > 0){
            tris.RemoveAt(triIds[0]);
            triIds = TrianglesContainingVerts(replaceWithVert, replaceVert, tris);
        }

        List <List<int>> toReplaceVerts = TrianglesContainingVert(replaceVert, tris);

        foreach (List<int> t in toReplaceVerts)
        {
            tris[t[0]].verts[t[1]] = verts[replaceWithVert.id];
        }

        return verts;
    }


    public static List<List<int>> TrianglesContainingVert(Vert u, List<Tri> tris)
    {
        List<List<int>> ids = new List<List<int>>();

        for (int i = 0; i < tris.Count; i++)
        {
            for (int j = 0; j < 3; j++)
            {
                Vert currentVert = tris[i].verts[j];
                if (tris[i].verts[j].id == u.id)
                {
                    List<int> tmp = new List<int>();
                    tmp.Add(i);
                    tmp.Add(j);
                    ids.Add(tmp);
                }
            }
        }

        return ids;
    }

    public static List<int> TrianglesContainingVerts(Vert u, Vert v, List<Tri> tris)
    {
        List<int> triIds = new List<int>();

        for (int i = 0; i < tris.Count; i++)
        {
            for (int j = 0; j < 3; j++)
            {
                Vert currentVert = tris[i].verts[j];

                if (tris[i].verts.Any(p => p.id == u.id) && tris[i].verts.Any(p => p.id == v.id))
                {
                    triIds.Add(i);
                }
            }
        }

        return triIds;
    }

    public static void RemoveDuplicateVerts(List<Tri> tris, List<Vert> verts)
    {
        Vector3 normalSum = Vector3.zero;
        int triCount = 1;

        for (int i = 0; i < verts.Count; i++) if(!verts[i].deleted)
        {
            triCount = 1;
            normalSum = verts[i].normal;
            Vert currentVert = verts[i];

            for (int j = 0; j < verts.Count; j++) if (i != j && !verts[j].deleted)
            {
                    Vert testingVert = verts[j];

                    if (currentVert.pos == testingVert.pos)
                    {
                        normalSum += testingVert.normal;
                        triCount++;

                        List<List<int>> dupliVertIndexes = TrianglesContainingVert(testingVert, tris);

                        for(int x = 0; x < dupliVertIndexes.Count; x++)
                        {
                            CollapseVert(currentVert, testingVert, tris, verts);
                        }

                        verts[j].deleted = true;
                    }
            }

            verts[i].normal = normalSum / triCount;
        }
    }
}

public class Vert
{
    public int id;
    public Vector3 pos;
    public Vector3 normal;
    public bool deleted;
    public float cost;
    public int cheapestNeighbor;

    public List<Vert> neighbors = new List<Vert>();

    public Vert(int id, Vector3 pos, Vector3 normal)
    {
        this.id = id;
        this.pos = pos;
        this.normal = normal;
    }

    public static List<Vert> FindNeighbors(Vert v, List<Vert> verts, List<Tri> tris)
    {
        var triIds = MeshUtils.TrianglesContainingVert(v, tris);

        List<Vert> neighbors = new List<Vert>();
        for(int i = 0; i < triIds.Count; i++)
        {
            for(int j = 0; j < 3; j++)
            {
                Vert testingVert = tris[triIds[i][0]].verts[j];

                if (!neighbors.Any(p => p.id == testingVert.id) && testingVert.id != v.id)
                {
                    neighbors.Add(testingVert);
                }
            }
        }

        return neighbors;
    }
}

public class Tri
{
    private List<Vert> vs;

    public List<Vert> verts
    {
        get
        {
            return vs;
        }
        set
        {
            vs = value;
            normal = ComputeNormal();
        }
    }

    public Vector3 normal;

    public Tri()  { }

    public Tri(List<Vert> verts)
    {
        this.verts = verts;
        this.normal = ComputeNormal();
    }

    public Tri(Vert v0, Vert v1, Vert v2)
    {
        this.verts = new List<Vert>() { v0, v1, v2 };
        this.normal = ComputeNormal();
    }

    public bool HasVertex(int id)
    {
        if(verts[0].id == id || verts[1].id == id || verts[2].id == id)
        {
            return true;
        }
        return false;
    }

    public Vector3 ComputeNormal()
    {
        Vector3 n = Vector3.zero;

        Vector3 p1 = this.verts[0].pos;
        Vector3 p2 = this.verts[1].pos;
        Vector3 p3 = this.verts[2].pos;

        Vector3 u;
        u.x = p2.x - p1.x;
        u.y = p2.y - p1.y;
        u.z = p2.z - p1.z;

        Vector3 v;
        v.x = p3.x - p1.x;
        v.y = p3.y - p1.y;
        v.z = p3.z - p1.z;

        Vector3 cross = Vector3.Cross(u, v);

        float crossMag = Mathf.Sqrt(cross.x * cross.x + cross.y * cross.y + cross.z * cross.z);

        if (crossMag == 0)
        {
            normal.x = normal.y = normal.z = 0f;
        }
        else
        {
            normal.x = cross.x / crossMag;
            normal.y = cross.y / crossMag;
            normal.z = cross.z / crossMag;
        }

        return n;
    }
}

using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

//	Original Algorithm :
//	Progressive Mesh type Polygon Reduction Algorithm
//	by Stan Melax (c) 1998
//	http://www.melax.com/polychop/

namespace NWH
{
    public class MeshDecimate
    {
        public float ratio = 0.5f;
        public float smoothAngle = 45.0f;
        private float smoothAngleDot;
        public bool lockSelPoint = true;
        public List<Vector3> selectedVertices = new List<Vector3>();
        public bool bRecalculateNormals;

        public float lodDataSize = 0;

        private Tri[] myTriangles;
        private Vert[] myLODVertices;
        private History[] collapseHistory;
        private List<Vert> cache = new List<Vert>();
        private int cacheSize;
        private int[] triOrder;

        private int[] originalTriangles;
        private Vector3[] originalVertices;
        private Vector2[] originalUVs;
        private Vector3[] originalNormals;

        private int[] sharedTriangles;
        private Vector3[] sharedVertices;

        public Vector3[] finalVertices;
        public Vector3[] finalNormals;
        public Vector2[] finalUVs;
        public int[] finalTriangles;

        public bool preCalculateDone = false;
        public int lastTarget;

        private int currentcnt;
        private int searchIndex;


        private float ComputeEdgeCollapseCosts(Vert u, Vert v)
        {
            int i;
            int j;
            Tri faceU;
            Tri faceV;

            float edgelength = (v.position - u.position).sqrMagnitude;
            float cost = 0;

            // find the "vFaces" triangles that are on the edge uv
            List<Tri> vFaces = new List<Tri>();
            int uFaceCount = u.face.Count;
            for (i = 0; i < uFaceCount; ++i)
            {
                faceU = u.face[i];

                if (faceU.HasVertex(v))
                {
                    vFaces.Add(faceU);
                }
            }

            // use the triangle facing most away from the sides 
            // to determine our curvature term
            int vFaceCount = vFaces.Count;
            for (i = 0; i < uFaceCount; ++i)
            {
                float mindot = 1; // curve for face i and closer side to it
                faceU = u.face[i];
                Vector3 faceN = faceU.normal;
                for (j = 0; j < vFaceCount; ++j)
                {
                    // use dot product of face normals. '^' defined in vector
                    faceV = vFaces[j];
                    Vector3 ns = faceV.normal;
                    float dot = (1 - (faceN.x * ns.x + faceN.y * ns.y + faceN.z * ns.z)) * 0.5f;
                    if (dot < mindot) mindot = dot;
                }
                if (mindot > cost) cost = mindot;
            }

            if (u.IsBorder() && vFaceCount > 1)
                cost = 1.0f;

            // texture UV check
            // if neighbor face has different uv
            // means that shouldn't be collapsed.
            // set its priority as higher cost.
            int found = 0;
            for (i = 0; i < uFaceCount; ++i)
            {
                faceU = u.face[i];
                Vector2 uv = faceU.uvAt(u);
                for (j = 0; j < vFaceCount; ++j)
                {
                    faceV = vFaces[j];
                    if (uv == faceV.uvAt(u)) break;
                }
                if (j == vFaceCount)
                    ++found;
            }
            // all neighbor faces share same uv
            // so set u as higher cost.
            if (found > 0) cost = 1.0f;

            if (u.selected && lockSelPoint)
                cost = 6553.5f;

            // the more coplanar the lower the curvature term
            // cost 0 means u and v are on the same plane.
            return edgelength * cost;
        }


        private void ComputeEdgeCostAtVertex(Vert v)
        {
            // compute the edge collapse cost for all edges that start
            // from vertex v.  Since we are only interested in reducing
            // the object by selecting the min cost edge at each step, we
            // only cache the cost of the least cost edge at this vertex
            // (in member variable collapse) as well as the value of the 
            // cost (in member variable cost).
            if (v.neighbor.Count == 0)
            {
                // v doesn't have neighbors so it costs nothing to collapse
                v.collapse = null;
                v.cost = 0;//-0.01f;
                return;
            }
            v.cost = 65535;
            v.collapse = null;
            // search all neighboring edges for "least cost" edge
            int neighborCount = v.neighbor.Count;
            float cost;
            for (int i = 0; i < neighborCount; ++i)
            {
                cost = ComputeEdgeCollapseCosts(v, v.neighbor[i]);
                if (cost < v.cost)
                {
                    v.collapse = v.neighbor[i]; // candidate for edge collapse
                    v.cost = cost;                      // cost of the collapse
                }
            }
        }


        private void ComputeAllEdgeCollapseCosts()
        {
            // For all the edges, compute the difference it would make
            // to the model if it was collapsed.  The least of these
            // per vertex is cached in each vertex object.
            int count = myLODVertices.Length;
            for (int i = 0; i < count; ++i)
            {
                Vert v = myLODVertices[i];
                ComputeEdgeCostAtVertex(v);
                cache.Insert(i, v);
            }
        }


        private void UnCollapse(History his)
        {
            int i;
            int n;
            Tri t;

            var l = his.removedTriangles;
            n = l.Count;
            for (i = 0; i < n; ++i)
                myTriangles[l[i]].deleted = false;

            var list = his.replacedVertex;
            n = list.Count;
            for (i = 0; i < n; ++i)
            {
                var tmp = list[i];
                t = myTriangles[(int)tmp[0]];
                int changedIndex = (int)tmp[1];

                if (changedIndex == 0)
                {
                    t.v0 = myLODVertices[(int)tmp[2]];
                    t.vn0 = (Vector3)tmp[3];
                    t.uv0 = (Vector2)tmp[4];
                }
                else if (changedIndex == 1)
                {
                    t.v1 = myLODVertices[(int)tmp[2]];
                    t.vn1 = (Vector3)tmp[3];
                    t.uv1 = (Vector2)tmp[4];
                }
                else
                {
                    t.v2 = myLODVertices[(int)tmp[2]];
                    t.vn2 = (Vector3)tmp[3];
                    t.uv2 = (Vector2)tmp[4];
                }
            }

        }

        private void Collapse(History his)
        {
            int i;
            int n;
            Tri t;

            var l = his.removedTriangles;
            n = l.Count;
            for (i = 0; i < n; ++i)
                myTriangles[l[i]].deleted = true;

            var list = his.replacedVertex;
            n = list.Count;
            for (i = 0; i < n; ++i)
            {
                var tmp = list[i];
                t = myTriangles[(int)tmp[0]];
                int changedIndex = (int)tmp[1];

                if (changedIndex == 0)
                {
                    t.v0 = myLODVertices[(int)tmp[5]];
                    t.vn0 = (Vector3)tmp[6];
                    t.uv0 = (Vector2)tmp[7];
                }
                else if (changedIndex == 1)
                {
                    t.v1 = myLODVertices[(int)tmp[5]];
                    t.vn1 = (Vector3)tmp[6];
                    t.uv1 = (Vector2)tmp[7];
                }
                else
                {
                    t.v2 = myLODVertices[(int)tmp[5]];
                    t.vn2 = (Vector3)tmp[6];
                    t.uv2 = (Vector2)tmp[7];
                }
            }

        }


        private void CollapseTest()
        {
            Vert u = cache[searchIndex++];
            Vert v = u.collapse;

            // which Vert will be collapsed.
            History his = new History();
            collapseHistory[currentcnt - 1] = his;

            // u is a vertex all by itself so just delete it
            if (v != null && v.deleted)
            {
                u.RemoveVert();
                return;
            }
            else if (v == null)
            {
                u.RemoveVert();
                return;
            }

            int i;
            int j;
            Tri uFace;
            Tri vFace;
            int vFaceCount;
            int neighborCount = u.neighbor.Count;
            Vert[] neighbors = new Vert[neighborCount];
            int count = u.face.Count;

            // make tmp a list of all the neighbors of u
            for (i = 0; i < neighborCount; ++i)
                neighbors[i] = u.neighbor[i];

            // make a list and add face to the list if it has v.
            List<Tri> vFaces = new List<Tri>();
            for (i = 0; i < count; ++i)
            {
                uFace = u.face[i];
                if (uFace.HasVertex(v))
                {
                    vFaces.Add(uFace);
                }
            }
            vFaceCount = vFaces.Count;

            // delete triangles on edge uv:
            for (i = u.face.Count - 1; i >= 0; --i)
            {
                try
                {
                    uFace = u.face[i];
                    if (uFace.HasVertex(v))
                    {
                        uFace.RemoveTriangle(his);
                    }
                }
                catch { }

            }

            // update remaining triangles to have v instead of u
            Vector2 u_uv;
            Vector2 foundUV = new Vector2();
            Vector3 foundVN = new Vector3();

            for (i = u.face.Count - 1; i >= 0; --i)
            {
                uFace = u.face[i];
                if (!uFace.deleted)
                {
                    u_uv = uFace.uvAt(u);

                    for (j = 0; j < vFaceCount; ++j)
                    {
                        vFace = vFaces[j];
                        if (u_uv == vFace.uvAt(u))
                        {
                            foundUV = vFace.uvAt(v);
                            foundVN = vFace.normalAt(v);
                            break;
                        }
                    }
                    uFace.ReplaceVertex(u, v, foundUV, foundVN, his);
                }
            }
            u.RemoveVert();

            // recompute the edge collapse costs in neighborhood
            Vert neighbor;
            float oldCost;
            for (i = 0; i < neighborCount; ++i)
            {
                neighbor = neighbors[i];
                oldCost = neighbor.cost;
                ComputeEdgeCostAtVertex(neighbor);

                if (oldCost > neighbor.cost) SortLeft(neighbor);
                else SortRight(neighbor);
            }
        }

        private void SortRight(Vert v)
        {
            int cacheIndex = cache.IndexOf(v);
            if (cacheIndex == cacheSize - 1) return;

            float cost = v.cost;
            Vert c2 = cache[cacheIndex + 1];
            if (cost == c2.cost) return;

            int maxIndex = cacheSize - 2;
            while (cost > c2.cost && cacheIndex < maxIndex)
            {
                cache[cacheIndex++] = c2;
                c2 = cache[cacheIndex + 1];
            }
            if (cost > c2.cost)
                cache[cacheIndex++] = c2;
            cache[cacheIndex] = v;
        }

        private void SortLeft(Vert v)
        {
            int cacheIndex = cache.IndexOf(v);
            if (cacheIndex == searchIndex) return;

            float cost = v.cost;
            Vert c2 = cache[cacheIndex - 1];
            if (cost == c2.cost) return;

            while (cost < c2.cost && cacheIndex > searchIndex + 2)
            {
                cache[cacheIndex--] = c2;
                c2 = cache[cacheIndex - 1];
            }
            if (cost < c2.cost)
                cache[cacheIndex--] = c2;
            cache[cacheIndex] = v;
        }

        public void PreCalculate(Mesh tmpMesh)
        {
            int i;
            int j;

            smoothAngleDot = 1 - (smoothAngle / 90.0f);

            int[] tris = tmpMesh.triangles;

            originalTriangles = tmpMesh.triangles;
            originalVertices = tmpMesh.vertices;
            if(tmpMesh.uv.Length > 0)
            {
                originalUVs = tmpMesh.uv;
            }
            else
            {
                List<Vector2> uvs = new List<Vector2>();
                foreach (Vector2 nr in tmpMesh.normals)
                {
                    uvs.Add(nr);
                }
                originalUVs = uvs.ToArray();
            }
            originalNormals = tmpMesh.normals;

            int triNum = tris.Length;
            int vertNum = originalVertices.Length;
            List<Vector3> newVertices = new List<Vector3>();

            int n;
            int foundAt = -1;
            int indice;
            Vector3 v;

            for (i = 0; i < triNum; ++i)
            {
                indice = tris[i];
                v = originalVertices[indice];

                n = newVertices.Count;
                foundAt = -1;
                for (j = 0; j < n; ++j)
                    if (newVertices[j] == v) { foundAt = j; break; }

                if (foundAt != -1)
                    tris[i] = foundAt;
                else
                {
                    tris[i] = n;
                    newVertices.Insert(n, v);
                }
            }

            sharedTriangles = tris;
            sharedVertices = newVertices.ToArray();

            myTriangles = new Tri[sharedTriangles.Length / 3];
            myLODVertices = new Vert[sharedVertices.Length];

            ComputeProgressiveMesh();
            preCalculateDone = true;

            //	calculate triangle remove order
            triOrder = new int[myTriangles.Length];
            n = collapseHistory.Length;
            int cnt = 0;
            for (i = 0; i < n; ++i)
            {
                History his = collapseHistory[i];
                List<int> list = his.removedTriangles;
                int m = list.Count;
                for (j = 0; j < m; ++j)
                    triOrder[cnt++] = list[j];
            }
        }


        public void Calculate(Mesh tmpMesh)
        {
            ProgressiveMesh(ratio);

            int i;
            int j;
            int foundAt = -1;
            Vector3 v = new Vector3();
            Vector3 vn = new Vector3();
            Vector3 dvn = new Vector3();
            Vector2 vuv = new Vector2();
            //History his = new History();

            int cnt = 0;
            int vertsCount = myLODVertices.Length;
            int trisCount = myTriangles.Length;

            int reducedTriCount = 0;
            foreach (Tri t in myTriangles)
            {
                if (t.deleted) continue;
                ++reducedTriCount;
            }

            int minTriCount = reducedTriCount * 3;
            int[] tris = new int[minTriCount];
            Vector3[] verts = new Vector3[minTriCount];
            Vector2[] uvs = new Vector2[minTriCount];
            Vector3[] norms = new Vector3[minTriCount];
            int[] indices = new int[minTriCount];

            for (i = 0; i < reducedTriCount; ++i)
            {
                Tri tri = myTriangles[triOrder[i]];

                int cnt1 = cnt + 1;
                int cnt2 = cnt + 2;
                Vert v0 = tri.v0;
                Vert v1 = tri.v1;
                Vert v2 = tri.v2;

                verts[cnt] = v0.position;
                verts[cnt1] = v1.position;
                verts[cnt2] = v2.position;
                tris[cnt] = cnt;
                tris[cnt1] = cnt1;
                tris[cnt2] = cnt2;
                uvs[cnt] = tri.uv0;
                uvs[cnt1] = tri.uv1;
                uvs[cnt2] = tri.uv2;
                norms[cnt] = tri.vn0;
                norms[cnt1] = tri.vn1;
                norms[cnt2] = tri.vn2;

                indices[cnt] = tri.defaultIndex0;
                indices[cnt1] = tri.defaultIndex1;
                indices[cnt2] = tri.defaultIndex2;

                cnt += 3;
            }

            int triNum = tris.Length;
            List<Vector3> newVertices = new List<Vector3>();
            List<Vector2> newUVs = new List<Vector2>();
            List<Vector3> newNormals = new List<Vector3>();
            List<Vector3> newDNormals = new List<Vector3>();

            if (bRecalculateNormals)
            {

                for (i = 0; i < triNum; ++i)
                {
                    v = verts[i];
                    vuv = uvs[i];
                    vn = norms[i];
                    var n = newVertices.Count;
                    foundAt = -1;
                    for (j = 0; j < n; ++j)
                        if (newVertices[j] == v && newUVs[j] == vuv &&
                        Vector3.Dot(newNormals[j], vn) > smoothAngleDot) { foundAt = j; break; }

                    if (foundAt != -1) tris[i] = foundAt;
                    else
                    {
                        tris[i] = n;
                        newVertices[n] = v;
                        newUVs[n] = vuv;
                        newNormals[n] = vn;
                        newDNormals[n] = dvn;
                    }
                }

            }
            else
            {
                for (i = 0; i < triNum; ++i)
                {
                    v = verts[i];
                    vuv = uvs[i];
                    vn = norms[i];
                    dvn = originalNormals[indices[i]];
                    int n = newVertices.Count;
                    foundAt = -1;
                    for (j = 0; j < n; ++j)
                        if (newVertices[j] == v && newUVs[j] == vuv && newDNormals[j] == dvn) { foundAt = j; break; }

                    if (foundAt != -1) tris[i] = foundAt;
                    else
                    {
                        tris[i] = n;
                        newVertices.Insert(n, v);
                        newUVs.Insert(n, vuv);
                        newNormals.Insert(n, vn);
                        newDNormals.Insert(n, dvn);
                    }
                }
            }

            finalVertices = newVertices.ToArray();
            finalNormals = newNormals.ToArray();
            finalUVs = newUVs.ToArray();
            finalTriangles = tris;
        }


        private void ComputeProgressiveMesh()
        {
            int i;
            int j;
            int n;
            Tri t;

            int vertexCount = sharedVertices.Length;
            int triangleCount = sharedTriangles.Length;

            System.Array.Clear(myLODVertices, 0, myLODVertices.Length);
            for (i = 0; i < vertexCount; ++i)
            {
                Vector3 dv = sharedVertices[i];
                bool sel = false;

                n = selectedVertices.Count;
                for (j = 0; j < n; ++j)
                {
                    if (selectedVertices[j] == dv)
                    {
                        sel = true;
                        break;
                    }
                }
                myLODVertices[i] = new Vert(dv, i, sel);
            }

            // new myTris
            System.Array.Clear(myTriangles, 0, myTriangles.Length);
            int cnt = 0;
            for (i = 0; i < triangleCount; i += 3)
            {
                t = new Tri( cnt,
                            myLODVertices[sharedTriangles[i]],
                            myLODVertices[sharedTriangles[i + 1]],
                            myLODVertices[sharedTriangles[i + 2]],
                            originalUVs[originalTriangles[i]],
                            originalUVs[originalTriangles[i + 1]],
                            originalUVs[originalTriangles[i + 2]]);

                t.SetDefaultIndices(originalTriangles[i], originalTriangles[i + 1], originalTriangles[i + 2]);
                if (bRecalculateNormals)
                    t.vn0 = t.vn1 = t.vn2 = t.normal;
                else
                {
                    t.vn0 = originalNormals[originalTriangles[i]];
                    t.vn1 = originalNormals[originalTriangles[i + 1]];
                    t.vn2 = originalNormals[originalTriangles[i + 2]];
                }

                myTriangles[cnt] = t;
                ++cnt;
            }

            cache = new List<Vert>();
            cacheSize = vertexCount;

            if (bRecalculateNormals)
                RecalculateNormal(); // set normals for vertex.
            ComputeAllEdgeCollapseCosts(); // cache all edge collapse costs
            //System.Array.Sort(cache, myComparer); // lower cost to the left.
            cache = cache.OrderBy(p => p.cost).ToList();

            collapseHistory = new History[vertexCount];

            currentcnt = myLODVertices.Length + 1;
            searchIndex = 0;
            while (--currentcnt > 0)
            {
                CollapseTest();
            }


            // LOD Data size calculation
            n = collapseHistory.Length;
            int tmpBytes = 0;
            History tmpHis;
            for (i = 0; i < n; ++i)
            {
                tmpHis = collapseHistory[i];
                tmpBytes += (
                    tmpHis.removedTriangles.Count * 2 +
                    tmpHis.replacedVertex.Count * 14
                ) * 4;
            }
            lodDataSize = tmpBytes;
        }


        private void ProgressiveMesh(float ratio)
        {
            int i;
            int target = Mathf.FloorToInt(ratio * sharedVertices.Length);

            if (lastTarget < target)
            {
                for (i = lastTarget; i < target; ++i)
                    UnCollapse(collapseHistory[i]);
            }
            else
            {
                for (i = lastTarget - 1; i >= target; --i)
                    Collapse(collapseHistory[i]);
            }
            lastTarget = target;
        }

        private void RecalculateNormal()
        {
            int n = myTriangles.Length;
            for (int i = 0; i < n; ++i)
            {
                Tri f = myTriangles[i];
                if (f.deleted)
                    continue;
                f.RecalculateAvgNormals(smoothAngleDot);
            }
        }
    }
}

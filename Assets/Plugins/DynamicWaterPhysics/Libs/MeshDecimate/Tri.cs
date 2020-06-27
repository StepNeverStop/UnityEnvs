using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace NWH
{
    public class Tri
    {
        public int id;
        public Vert v0;
        public Vert v1;
        public Vert v2;

        public int defaultIndex0;
        public int defaultIndex1;
        public int defaultIndex2;

        public Vector2 uv0;
        public Vector2 uv1;
        public Vector2 uv2;

        public Vector3 vn0;
        public Vector3 vn1;
        public Vector3 vn2;

        public Vector3 normal;
        public bool deleted = false;

        public Tri (int id, Vert v0, Vert v1, Vert v2, Vector2 uv0, Vector2 uv1, Vector2 uv2)
        {
            this.id = id;
            this.v0 = v0;
            this.v1 = v1;
            this.v2 = v2;
            this.uv0 = uv0;
            this.uv1 = uv1;
            this.uv2 = uv2;

            RecalculateNormal();

            v0.AddFace(this);
            v1.AddFace(this);
            v2.AddFace(this);

            v0.AddNeighbor(v1);
            v0.AddNeighbor(v2);
            v1.AddNeighbor(v0);
            v1.AddNeighbor(v2);
            v2.AddNeighbor(v0);
            v2.AddNeighbor(v1);
        }

        public void SetDefaultIndices(int n0, int n1, int n2)
        {
            defaultIndex0 = n0;
            defaultIndex1 = n1;
            defaultIndex2 = n2;
        }

        public void RemoveTriangle(History his)
        {
            v0.RemoveFace(this);
            v1.RemoveFace(this);
            v2.RemoveFace(this);
            
            v0.RemoveIfNonNeighbor(v1);
            v0.RemoveIfNonNeighbor(v2);
            v1.RemoveIfNonNeighbor(v0);
            v1.RemoveIfNonNeighbor(v2);
            v2.RemoveIfNonNeighbor(v1);
            v2.RemoveIfNonNeighbor(v0);

            deleted = true;
            his.RemovedTriangle(id);
        }

        public Vector2 uvAt(Vert v)
        {
            Vector3 vec = v.position;
            if (vec == v0.position) return uv0;
            else if (vec == v1.position) return uv1;
            else if (vec == v2.position) return uv2;
            return new Vector2();
        }

        public Vector3 normalAt(Vert v)
        {
            Vector3 vec = v.position;
            if (vec == v0.position) return vn0;
            else if (vec == v1.position) return vn1;
            else if (vec == v2.position) return vn2;
            return new Vector3();
        }

        public void setUV(Vert v, Vector2 newuv)
        {
            Vector3 vec = v.position;
            if (vec == v0.position) uv0 = newuv;
            else if (vec == v1.position) uv1 = newuv;
            else if (vec == v2.position) uv2 = newuv;
        }


        public void setVN(Vert v, Vector3 newNormal)
        {
            Vector3 vec = v.position;
            if (vec == v0.position) vn0 = newNormal;
            else if (vec == v1.position) vn1 = newNormal;
            else if (vec == v2.position) vn2 = newNormal;
        }


        public bool HasVertex(Vert v)
        {
            Vector3 vec = v.position;
            return (vec == v0.position || vec == v1.position || vec == v2.position);
        }


        public void RecalculateNormal()
        {
            Vector3 v1pos = v1.position;
            normal = Vector3.Cross(v1pos - v0.position, v2.position - v1pos);
            if (normal.magnitude == 0) return;
            normal.Normalize();
        }


        // Only called if 'Recalculate Normals' is enabled.
        // This will smooth out normals event at uv seams.
        public void RecalculateAvgNormals(float smoothAngleDot)
        {
            int i;
            List<Tri> flist = new List<Tri>();
            List<Tri> slist = new List<Tri>();
            int n = flist.Count;
            Tri f;
            Vector3 fn;

            flist = v0.face;
            slist.Clear();
            for (i = 0; i < n; ++i)
            {
                f = flist[i];
                fn = f.normal;
                if (fn.x * normal.x + fn.y * normal.y + fn.z * normal.z > smoothAngleDot)
                {
                    vn0 += fn;
                    slist.Add(f);
                }
            }
            vn0.Normalize();
            n = slist.Count;
            for (i = 0; i < n; ++i) { f = slist[i]; f.setVN(v0, vn0); }

            flist = v1.face;
            n = flist.Count;
            slist.Clear();
            for (i = 0; i < n; ++i)
            {
                f = flist[i];
                fn = f.normal;
                if (fn.x * normal.x + fn.y * normal.y + fn.z * normal.z > smoothAngleDot)
                {
                    vn1 += fn;
                    slist.Add(f);
                }
            }
            vn1.Normalize();
            n = slist.Count;
            for (i = 0; i < n; ++i) { f = slist[i]; f.setVN(v1, vn1); }

            flist = v2.face;
            n = flist.Count;
            slist.Clear();
            for (i = 0; i < n; ++i)
            {
                f = flist[i];
                fn = f.normal;
                if (fn.x * normal.x + fn.y * normal.y + fn.z * normal.z > smoothAngleDot)
                {
                    vn2 += fn;
                    slist.Add(f);
                }
            }
            vn2.Normalize();
            n = slist.Count;
            for (i = 0; i < n; ++i) { f = slist[i]; f.setVN(v2, vn2); }
        }

        public void ReplaceVertex( Vert vo, Vert vnew, Vector2 newUV, Vector3 newVN, History his)
        {
            Vector3 vec = vo.position;
            Vert changedVertex = v2;
            int changedVertexId = 2;
            Vector3 changedNormal = vn2;
            Vector2 changedUV = uv2;

            if (vec == v0.position)
            {
                changedVertex = v0;
                changedVertexId = 0;
                changedNormal = vn0;
                changedUV = uv0;
                v0 = vnew;
                vn0 = newVN;
                uv0 = newUV;
            }
            else if (vec == v1.position)
            {
                changedVertex = v1;
                changedVertexId = 1;
                changedNormal = vn1;
                changedUV = uv1;
                v1 = vnew;
                vn1 = newVN;
                uv1 = newUV;
            }
            else
            {
                v2 = vnew;
                vn2 = newVN;
                uv2 = newUV;
            }

            vo.RemoveFace(this);
            vnew.AddFace(this);

            vo.RemoveIfNonNeighbor(v0);
            v0.RemoveIfNonNeighbor(vo);
            vo.RemoveIfNonNeighbor(v1);
            v1.RemoveIfNonNeighbor(vo);
            vo.RemoveIfNonNeighbor(v2);
            v2.RemoveIfNonNeighbor(vo);

            v0.AddNeighbor(v1);
            v0.AddNeighbor(v2);
            v1.AddNeighbor(v0);
            v1.AddNeighbor(v2);
            v2.AddNeighbor(v0);
            v2.AddNeighbor(v1);

            RecalculateNormal();

            his.ReplaceVertex(id, changedVertexId, changedVertex.id, changedNormal, changedUV, vnew.id, newVN, newUV);
        }
    }
}


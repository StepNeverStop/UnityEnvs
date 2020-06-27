using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace NWH
{
    public class Vert
    {
        public Vector3 position;
        public List<Tri> face = new List<Tri>();
        public List<Vert> neighbor = new List<Vert>();

        public int id;
        public float cost;
        public Vert collapse;
        public bool selected;
        public bool deleted = false;

        public Vert(Vector3 position, int id, bool selected)
        {
            this.position = position;
            this.id = id;
            this.selected = selected;

            cost = 0f;
            collapse = null;
        }

        public void RemoveVert()
        {
            Vert nb;
            while (neighbor.Count > 0)
            {
                nb = neighbor[0];
                nb.neighbor.Remove(this);
                neighbor.Remove(nb);
            }
            deleted = true;
        }

        public bool IsBorder()
        {
            int j;
            int n = neighbor.Count;
            Vert nb;
            int face_len;
            Tri f;
            int count = 0;

            for (int i = 0; i < n; ++i)
            {
                count = 0;
                nb = neighbor[i];
                face_len = face.Count;
                for (j = 0; j < face_len; ++j)
                {
                    f = face[j];
                    if (f.HasVertex(nb))
                        ++count;
                }
                if (count == 1) return true;
            }
            return false;
        }


        public void AddFace(Tri f)
        {
            face.Add(f);
        }

        public void RemoveFace(Tri f)
        {
            face.Remove(f);
        }


        public void AddNeighbor(Vert v)
        {
            int i;
            int foundAt = -1;
            int n = neighbor.Count;

            for (i = 0; i < n; ++i)
                if (neighbor[i] == v) { foundAt = i; break; }

            if (foundAt == -1)
                neighbor.Add(v);
        }


        public void RemoveIfNonNeighbor(Vert v)
        {
            int i;
            int foundAt = -1;
            int n = neighbor.Count;
            Tri f;

            for (i = 0; i < n; ++i)
                if (neighbor[i] == v) { foundAt = i; break; }

            if (foundAt == -1) return;

            n = face.Count;
            for (i = 0; i < n; ++i)
            {
                f = face[i];
                if (f.HasVertex(v)) return;
            }

            neighbor.Remove(v);
        }
    }

}

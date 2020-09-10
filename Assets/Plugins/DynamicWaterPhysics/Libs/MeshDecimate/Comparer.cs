using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace NWH
{
    public class Comparer : IComparer
    {
        Vert vx;
        Vert vy;

        public int Compare( System.Object x, System.Object y)
        {
            vx = (Vert)x;
            vy = (Vert)y;
            if (vx == vy) return 0;
            else if (vx.cost < vy.cost) return -1;
            return 1;
        }
    }
}

using System;
using System.Collections.Generic;
using System.Text;
using LODGenerator.Common;

/*
  copyright s-hull.org 2011
  released under the contributors beerware license

  contributors: Phil Atkin, Dr Sinclair.
*/
namespace DelaunayTriangulator
{
    public class Vertex
    {
        public float x, y;
        public float cx, cy;

        protected Vertex() { }

        public Vertex(float x, float y) 
        {
            this.x = x; this.y = y;
            this.cx = x; this.cy = y;
        }

        public float distance2To(Vertex other)
        {
            float dx = x - other.x;
            float dy = y - other.y;
            return dx * dx + dy * dy;
        }

        public float distanceTo(Vertex other)
        {
            return (float)Math.Sqrt(distance2To(other));
        }

        public static Vertex operator -(Vertex a, Vertex b)
        {
            return new Vertex(a.x - b.x, a.y - b.y);
        }

        public static float Cross(Vertex A, Vertex B)
        {
            return A.x * B.y - A.y * B.x;
        }

        public static bool Clockwise(Vertex A, Vertex B, Vertex C)
        {
            Vertex A2B = B - A;
            Vertex B2C = C - B;
            if (Cross(A2B, B2C) > 0)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public override bool Equals(System.Object obj)
        {
            if (obj == null)
            {
                return false;
            }
            Vertex p = obj as Vertex;
            if ((System.Object)p == null)
            {
                return false;
            }
            return (x == p.x) && (y == p.y);
        }

        public bool Equals(Vertex p)
        {
            if ((object)p == null)
            {
                return false;
            }
            return (x == p.x) && (y == p.y);
        }

        public override int GetHashCode()
        {
            return x.GetHashCode() ^ y.GetHashCode();
        }

        public static bool operator ==(Vertex a, Vertex b)
        {
            if (System.Object.ReferenceEquals(a, b))
            {
                return true;
            }
            if (((object)a == null) || ((object)b == null))
            {
                return false;
            }
            return a.x == b.x && a.y == b.y;
        }

        public static bool operator !=(Vertex a, Vertex b)
        {
            return !(a == b);
        }

        public class SortCounterClockwise : IComparer<Vertex>
        {
            public int Compare(Vertex a, Vertex b)
            {
                if (a == null || b == null)
                {
                    return 0;
                }
                if (a.x - a.cx >= 0 && b.x - a.cx < 0)
                {
                    return -1;
                }
                if (a.x - a.cx < 0 && b.x - a.cx >= 0)
                {
                    return 1;
                }
                if (a.x - a.cx == 0 && b.x - a.cx == 0)
                {
                    if (a.y - a.cy >= 0 || b.y - a.cy >= 0)
                    {
                        if (a.y > b.y)
                        {
                            return -1;
                        }
                        else if (a.y < b.y)
                        {
                            return 1;
                        }
                    }
                    else
                    {
                        if (b.y > a.y)
                        {
                            return -1;
                        }
                        else if (b.y < a.y)
                        {
                            return 1;
                        }
                    }
                }

                float det = (a.x - a.cx) * (b.y - a.cy) - (b.x - a.cx) * (a.y - a.cy);

                if (det < 0)
                {
                    return -1;
                }
                else if (det > 0)
                {
                    return 1;
                }

                float d1 = (a.x - a.cx) * (a.x - a.cx) + (a.y - a.cy) * (a.y - a.cy);
                float d2 = (b.x - a.cx) * (b.x - a.cx) + (b.y - a.cy) * (b.y - a.cy);
                if (d1 > d2)
                {
                    return -1;
                }
                else if (d1 < d2)
                {
                    return 1;
                }
                return 0;
            }
        }

        public override string ToString()
        {
            return string.Format("({0},{1})", x, y);
        }
    }
    
}

using System;
using System.Collections.Generic;

namespace LODGenerator.Common
{
    [Serializable]
    public class UVCoord
    {
        private float u;
        private float v;

        public float this[int i]
        {
            get
            {
                switch (i)
                {
                    case 0:
                        return this.u;
                    case 1:
                        return this.v;
                    default:
                        return float.NaN;
                }
            }
            set
            {
                switch (i)
                {
                    case 0:
                        this.u = value;
                        break;
                    case 1:
                        this.v = value;
                        break;
                }
            }
        }

        public UVCoord(float _u, float _v)
        {
            this.u = _u;
            this.v = _v;
        }

        public static UVCoord operator -(UVCoord a, UVCoord b)
        {
            return new UVCoord(a[0] - b[0], a[1] - b[1]);
        }

        public static float Cross(UVCoord A, UVCoord B)
        {
            return A[0] * B[1] - A[1] * B[0];
        }

        public static float Area(UVCoord a, UVCoord b, UVCoord c)
        {
            return (float)Math.Abs((a[0] - c[0]) * (b[1] - a[1]) - (a[0] - b[0]) * (c[1] - a[1])) * 0.5f;
        }

        public static float SignedArea(UVCoord a, UVCoord b, UVCoord c)
        {
            return ((a[0] - c[0]) * (b[1] - a[1]) - (a[0] - b[0]) * (c[1] - a[1])) * 0.5f;
        }

        public float distance2To(UVCoord other)
        {
            float dx = u - other.u;
            float dy = v - other.v;
            return dx * dx + dy * dy;
        }

        public float distanceTo(UVCoord other)
        {
            return (float)Math.Sqrt(distance2To(other));
        }

        public static float Circumference(UVCoord a, UVCoord b, UVCoord c)
        {
            return a.distanceTo(b) + b.distanceTo(c) + c.distanceTo(a);
        }

        public static bool Clockwise(UVCoord A, UVCoord B, UVCoord C)
        {
            UVCoord A2B = B - A;
            UVCoord B2C = C - B;
            if (Cross(A2B, B2C) > 0)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public static float Dot(UVCoord A, UVCoord B)
        {
            return (float)((double)A[0] * (double)B[0] + (double)A[1] * (double)B[1]);
        }

        public override string ToString()
        {
            return this.u.ToString() + ", " + this.v.ToString();
        }
    }
}

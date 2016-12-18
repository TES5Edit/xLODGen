using System;

namespace LODGenerator.Common
{

    [Serializable]
    public class Vector3
    {
        private float x;
        private float y;
        private float z;

        public float Length
        {
            get
            {
                return (float)Math.Sqrt(this.x * this.x + this.y * this.y + this.z * this.z);
            }
        }

        public float squaredLength
        {
            get
            {
                return this.x * this.x + this.y * this.y + this.z * this.z;
            }
        }

        public float this[int i]
        {
            get
            {
                switch (i)
                {
                    case 0:
                        return this.x;
                    case 1:
                        return this.y;
                    case 2:
                        return this.z;
                    default:
                        return float.NaN;
                }
            }
            set
            {
                switch (i)
                {
                    case 0:
                        this.x = value;
                        break;
                    case 1:
                        this.y = value;
                        break;
                    case 2:
                        this.z = value;
                        break;
                }
            }
        }

        public Vector3()
        {
            this.x = 0.0f;
            this.y = 0.0f;
            this.z = 0.0f;
        }

        public Vector3(float _x, float _y, float _z)
        {
            this.x = _x;
            this.y = _y;
            this.z = _z;
        }

        public static Vector3 operator *(Vector3 v, Matrix44 m)
        {
            Vector3 vector3 = new Vector3();
            vector3[0] = (v[0] * m[0][0] + v[1] * m[1][0] + v[2] * m[2][0]) + m[3][0];
            vector3[1] = (v[0] * m[0][1] + v[1] * m[1][1] + v[2] * m[2][1]) + m[3][1];
            vector3[2] = (v[0] * m[0][2] + v[1] * m[1][2] + v[2] * m[2][2]) + m[3][2];
            return vector3;
        }

        public static Vector3 operator *(Vector3 v, float f)
        {
            Vector3 vector3 = new Vector3();
            vector3[0] = v[0] * f;
            vector3[1] = v[1] * f;
            vector3[2] = v[2] * f;
            return vector3;
        }

        public static Vector3 operator /(Vector3 i, float v)
        {
            Vector3 vector3 = new Vector3();
            vector3[0] = i[0] / v;
            vector3[1] = i[1] / v;
            vector3[2] = i[2] / v;
            return vector3;
        }

        public static Vector3 operator -(Vector3 a, Vector3 b)
        {
            return new Vector3(a.x - b.x, a.y - b.y, a.z - b.z);
        }

        public static Vector3 operator +(Vector3 a, Vector3 b)
        {
            return new Vector3(a.x + b.x, a.y + b.y, a.z + b.z);
        }

        public static float Angle(Vector3 v1, Vector3 v2)
        {
            float dot = Dot(v1, v2);
            if (dot > 1.0)
            {
                return 0.0f;
            }
            else if (dot < -1.0)
            {
                return (float)Math.PI;
            }
            else if (dot == 0.0)
            {
                return (float)Math.PI / 2;
            }
            return (float)Math.Acos(dot);
        }

        public static Vector3 Cross(Vector3 A, Vector3 B)
        {
            Vector3 vector3 = new Vector3();
            vector3[0] = A[1] * B[2] - A[2] * B[1];
            vector3[1] = A[2] * B[0] - A[0] * B[2];
            vector3[2] = A[0] * B[1] - A[1] * B[0];
            return vector3;
        }

        public static bool OnLine(Vector3 p, Vector3 a, Vector3 b)
        {

            Vector3 x = b - a;
            double ba = Math.Sqrt(x[0] * x[0] + x[1] * x[1] + x[2] * x[2]);
            x = p - a;
            double pa = Math.Sqrt(x[0] * x[0] + x[1] * x[1] + x[2] * x[2]);
            x = b - p;
            double bp = Math.Sqrt(x[0] * x[0] + x[1] * x[1] + x[2] * x[2]);

            //Console.WriteLine(ba + " = " + pa + " + " + bp + " ===" + (pa + bp));

            if (Utils.QLow(ba) == Utils.QLow(pa + bp))
            {
                return true;
            }
            else
            {
                //Console.WriteLine(Utils.QHigh(ba) + " = " + pa + " + " + bp + " ===" + Utils.QHigh(pa + bp));
                return false;
            }
       }

        public static Vector3 MoveToLine(Vector3 q, Vector3 p1, Vector3 p2)
        {
            Vector3 u = p2 - p1;
            Vector3 pq = q - p1;
            Vector3 w2 = pq - (u * Dot(pq, u) / u.squaredLength);
            return q - w2;
        }

        public static float Dot(Vector3 A, Vector3 B)
        {
            return A[0] * B[0] + A[1] * B[1] + A[2] * B[2];
        }

        public void Normalize()
        {
            float m = this.Length;
            if (m > 0)
            {
                m = 1.0f / m;
            }
            else
            {
                m = 1.0f;
            }
            this.x *= m;
            this.y *= m;
            this.z *= m;
        }

        public override string ToString()
        {
            return Utils.xQUV(this.x).ToString() + ", " + Utils.xQUV(this.y).ToString() + ", " + Utils.xQUV(this.z).ToString();
        }
    }
}

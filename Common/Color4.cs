using System;

namespace LODGenerator.Common
{
    [Serializable]
    public class Color4
    {
        private float r;
        private float g;
        private float b;
        private float a;

        public float this[int i]
        {
            get
            {
                switch (i)
                {
                    case 0:
                        return this.r;
                    case 1:
                        return this.g;
                    case 2:
                        return this.b;
                    case 3:
                        return this.a;
                    default:
                        return float.NaN;
                }
            }
            set
            {
                switch (i)
                {
                    case 0:
                        this.r = value;
                        break;
                    case 1:
                        this.g = value;
                        break;
                    case 2:
                        this.b = value;
                        break;
                    case 3:
                        this.a = value;
                        break;
                }
            }
        }

        public Color4(float _r, float _g, float _b, float _a)
        {
            this.r = _r;
            this.g = _g;
            this.b = _b;
            this.a = _a;
        }

        public static Color4 operator +(Color4 a, Color4 b)
        {
            Color4 v = new Color4(0, 0, 0, 0);
            v[0] = a[0] + b[0];
            v[1] = a[1] + b[1];
            v[2] = a[2] + b[2];
            v[3] = a[3] + b[3];
            return v;
        }

        public static Color4 operator *(Color4 a, float b)
        {
            Color4 v = new Color4(0, 0, 0, 0);
            v[0] = a[0] * b;
            v[1] = a[1] * b;
            v[2] = a[2] * b;
            v[3] = a[3] * b;
            return v;
        }

        public static Color4 operator *(Color4 a, double c)
        {
            return a * (float)c;
        }

        public static Color4 operator /(Color4 a, float b)
        {
            Color4 v = new Color4(0, 0, 0, 0);
            v[0] = a[0] / b;
            v[1] = a[1] / b;
            v[2] = a[2] / b;
            v[3] = a[3] / b;
            return v;
        }

        public override string ToString()
        {
            return "[" + this.r + ", " + this.g + ", " + this.b + ", " + this.a + "]";
        }
    }
}
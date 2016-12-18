using System.Collections.Generic;
using LODGenerator.Common;

namespace LODGenerator
{
    public struct StaticDesc
    {
        public string refID;
        public int refFlags;
        public uint enableParent;
        public float x;
        public float y;
        public float z;
        public float rotX;
        public float rotY;
        public float rotZ;
        public float scale;
        public Matrix44 transrot;
        public float transscale;
        public byte alphaThreshold;
        public string staticName;
        public int staticFlags;
        public string materialName;
        public string staticFullModel;
        public string[] staticModels;
        public Dictionary<string, string> materialSwap;

        public override string ToString()
        {
            return this.staticName;
        }
    }
}

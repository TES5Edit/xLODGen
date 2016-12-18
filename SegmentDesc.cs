using System;

namespace LODGenerator
{
    [Serializable]
    public class SegmentDesc
    {
        public int id;
        public uint startTriangle;
        public ushort numTriangles;

        public SegmentDesc()
        {
            this.id = 0;
            this.startTriangle = 0U;
            this.numTriangles = (ushort)0;
        }
    }
}

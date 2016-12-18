namespace LODGenerator.NifMain
{
    public class BSSITSSegment
    {
        public uint triangleOffset;
        public uint triangleCount;
        public uint unknownHash;
        public uint numSegments;
        public int subSegment;

        public BSSITSSegment(uint sTriangle, ushort nTriangle)
        {
            this.triangleOffset = sTriangle;
            this.triangleCount = nTriangle;
            this.unknownHash = 4294967295;
            this.numSegments = 0;
            this.subSegment = -1;
        }
    }
}
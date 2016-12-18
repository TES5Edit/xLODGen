namespace LODGenerator.NifMain
{
    public class BSSegment
    {
        public byte unknownByte1;
        public uint startTriangle;
        public uint numTriangles;

        public BSSegment(uint sTriangle, ushort nTriangle)
        {
            this.unknownByte1 = (byte)0;
            this.startTriangle = sTriangle;
            this.numTriangles = nTriangle;
        }
    }
}

using System;

namespace LODGenerator.Common
{
    [Serializable]
    public class BSVertexData
    {
        public Vector3 vertex;
        public float bitangentX;
        public ushort unknownShort1;
        public uint unknownInt1;
        public UVCoord uvcoords;
        public Vector3 normal;
        public float bitangentY;
        public Vector3 tangent;
        public float bitangentZ;
        public Color4 vertexColors;
        public float[] boneWeights;
        public byte[] boneIndices;
        public uint unknownInt2;

        public BSVertexData()
        {
            this.vertex = new Vector3(0.0f, 0.0f, 0.0f);
            this.bitangentX = 0.0f;
            this.unknownShort1 = 0;
            this.unknownInt1 = 0;
            this.uvcoords = new UVCoord(0.0f, 0.0f);
            this.normal = new Vector3(0.0f, 0.0f, 0.0f);
            this.bitangentY = 0.0f;
            this.tangent = new Vector3(0.0f, 0.0f, 0.0f);
            this.bitangentZ = 0.0f;
            this.vertexColors = new Color4(1.0f, 1.0f, 1.0f, 1.0f);
            this.boneWeights = new float[4];
            this.boneWeights[0] = 0.0f;
            this.boneWeights[1] = 0.0f;
            this.boneWeights[2] = 0.0f;
            this.boneWeights[3] = 0.0f;
            this.boneIndices = new byte[4];
            this.boneIndices[0] = 0;
            this.boneIndices[1] = 0;
            this.boneIndices[2] = 0;
            this.boneIndices[3] = 0;
            this.unknownInt2 = 0;
        }
    }
}

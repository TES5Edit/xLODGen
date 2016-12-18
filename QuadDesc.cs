using LODGenerator.Common;
using System.Collections.Generic;

namespace LODGenerator
{
    public struct QuadDesc
    {
        public int x;
        public int y;
        public List<StaticDesc> statics;
        public QuadTree terrainQuadTree;
        public QuadTree waterQuadTree;
        public bool hasTerrainVertices;
        public BBox boundingBox;
        public OutDesc outValues;
        public Dictionary<string, int> textureBlockIndex;
        public Dictionary<string, int> textureBlockIndexPassThru;
        public Dictionary<string, int> dataBlockIndex;
        public Dictionary<string, int> shaderBlockIndex;

        public QuadDesc(bool val)
        {
            this.x = 0;
            this.y = 0;
            this.statics = new List<StaticDesc>();
            this.terrainQuadTree = new QuadTree();
            this.waterQuadTree = new QuadTree();
            this.hasTerrainVertices = false;
            this.boundingBox = new BBox();
            this.outValues = new OutDesc();
            this.outValues.totalTriCount = 0;
            this.outValues.reducedTriCount = 0;
            this.textureBlockIndex = new Dictionary<string, int>();
            this.textureBlockIndexPassThru = new Dictionary<string, int>();
            this.dataBlockIndex = new Dictionary<string, int>();
            this.shaderBlockIndex = new Dictionary<string, int>();
        }

        public override string ToString()
        {
            return "(" + this.x + ", " + this.y + "): " + this.statics.Count + " statics";
        }

    }
}
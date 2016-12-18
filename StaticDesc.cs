namespace LODGenerator
{
    public struct StaticDesc
    {
        public string refID;
        public int refFlags;
        public float x;
        public float y;
        public float z;
        public float rotX;
        public float rotY;
        public float rotZ;
        public float scale;
        public string staticName;
        public int staticFlags;
        public string materialName;
        public string staticFullModel;
        public string[] staticModels;

        public override string ToString()
        {
            return this.staticName;
        }
    }
}

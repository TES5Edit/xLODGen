using LODGenerator.Common;
using System.IO;

namespace LODGenerator.NifMain
{
    public class BSMultiBoundOBB : BSMultiBoundData
    {
        protected Vector3 center;
        protected Vector3 size;
        protected Matrix33 rotation;

        public BSMultiBoundOBB()
        {
            this.center = new Vector3(0f, 0f, 0f);
            this.size = new Vector3(0f, 0f, 0f);
            this.rotation = new Matrix33(true);
        }

        public override void Read(NiHeader header, BinaryReader reader)
        {
            base.Read(header, reader);
            this.center = Utils.ReadVector3(reader);
            this.size = Utils.ReadVector3(reader);
            this.rotation = Utils.ReadMatrix33(reader);
        }

        public override void Write(NiHeader header, BinaryWriter writer)
        {
            base.Write(header, writer);
            Utils.WriteVector3(writer, this.center);
            Utils.WriteVector3(writer, this.size);
            Utils.WriteMatrix33(writer, this.rotation);
        }

        public override uint GetSize(NiHeader header)
        {
            return base.GetSize(header) + 60U;
        }

        public override string GetClassName()
        {
            return "BSMultiBoundOBB";
        }

        public override bool IsDerivedType(string type)
        {
            bool flag = base.IsDerivedType(type);
            if (!flag)
                flag = type == "BSMultiBoundOBB";
            return flag;
        }
    }
}
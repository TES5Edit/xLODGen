using LODGenerator.Common;
using System.IO;

namespace LODGenerator.NifMain
{
    public class BSOrderedNode : NiNode
    {
        protected Vector4 alphaSortBound;
        protected byte isStaticBound;

        public BSOrderedNode()
        {
            this.alphaSortBound = new Vector4(0.0f, 0.0f, 0.0f, 0.0f);
            this.isStaticBound = (byte)0;
        }

        public override void Read(NiHeader header, BinaryReader reader)
        {
            base.Read(header, reader);
            this.alphaSortBound = Utils.ReadVector4(reader);
            this.isStaticBound = reader.ReadByte();
        }

        public override void Write(NiHeader header, BinaryWriter writer)
        {
            base.Write(header, writer);
            Utils.WriteVector4(writer, this.alphaSortBound);
            writer.Write(this.isStaticBound);
        }

        public override uint GetSize(NiHeader header)
        {
            return base.GetSize(header) + 17;
        }

        public override string GetClassName()
        {
            return "BSOrderedNode";
        }

        public override bool IsDerivedType(string type)
        {
            bool flag = base.IsDerivedType(type);
            if (!flag)
                flag = type == "BSOrderedNode";
            return flag;
        }
    }
}

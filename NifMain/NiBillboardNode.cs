using System;
using System.IO;

namespace LODGenerator.NifMain
{
    public class NiBillboardNode : NiNode
    {
        protected ushort billboardMode;

        public override void Read(NiHeader header, BinaryReader reader)
        {
            base.Read(header, reader);
            this.billboardMode = reader.ReadUInt16();
        }

        public override void Write(NiHeader header, BinaryWriter writer)
        {
            base.Write(header, writer);
            writer.Write(this.billboardMode);
        }

        public override uint GetSize(NiHeader header)
        {
            return base.GetSize(header) + 2;
        }

        public override string GetClassName()
        {
            return "NiBillboardNode";
        }

        public override bool IsDerivedType(string type)
        {
            bool flag = base.IsDerivedType(type);
            if (!flag)
                flag = type == "NiBillboardNode";
            return flag;
        }
    }
}
using LODGenerator.Common;
using System;
using System.Collections.Generic;
using System.IO;

namespace LODGenerator.NifMain
{
    public class NiSwitchNode : NiNode
    {
        protected ushort unknownFlags1;
        protected int unknownInt1;

        public NiSwitchNode()
        {
            this.unknownFlags1 = 0;
            this.unknownInt1 = 0;
        }

        public override void Read(NiHeader header, BinaryReader reader)
        {
            base.Read(header, reader);
            this.unknownFlags1 = reader.ReadUInt16();
            this.unknownInt1 = reader.ReadInt32();
        }

        public override void Write(NiHeader header, BinaryWriter writer)
        {
            base.Write(header, writer);
            writer.Write(this.unknownFlags1);
            writer.Write(this.unknownInt1);
        }

        public override uint GetSize(NiHeader header)
        {
            return base.GetSize(header) + 6;
        }

        public override string GetClassName()
        {
            return "NiSwitchNode";
        }

        public override bool IsDerivedType(string type)
        {
            bool flag = base.IsDerivedType(type);
            if (!flag)
                flag = type == "NiSwitchNode";
            return flag;
        }
    }
}

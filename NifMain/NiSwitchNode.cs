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

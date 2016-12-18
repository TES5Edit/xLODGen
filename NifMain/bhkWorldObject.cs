using System;
using System.Collections.Generic;
using System.IO;

namespace LODGenerator.NifMain
{
    public class bhkWorldObject : bhkSerializable
    {
        private int shape;
        private uint stuff;

        public bhkWorldObject()
        {
            this.shape = -1;
            this.stuff = 0;
        }

        public override void Read(NiHeader header, BinaryReader reader)
        {
            base.Read(header, reader);
            this.shape = reader.ReadInt32();
            this.stuff = reader.ReadUInt32();
        }

        public override void Write(NiHeader header, BinaryWriter writer)
        {
            List<int> blockReferences = header.GetBlockReferences();
            if (blockReferences.Count > 0)
            {
                if (this.shape != -1)
                {
                    this.shape = blockReferences[this.shape];
                }
            }
            base.Write(header, writer);
            writer.Write(this.shape);
            writer.Write(this.stuff);
        }

        public override uint GetSize(NiHeader header)
        {
            return base.GetSize(header) + 8;
        }

        public override string GetClassName()
        {
            return "bhkWorldObject";
        }

        public override bool IsDerivedType(string type)
        {
            bool flag = base.IsDerivedType(type);
            if (!flag)
                flag = type == "bhkWorldObject";
            return flag;
        }
    }
}
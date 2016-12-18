using System;
using System.Collections.Generic;
using System.IO;

namespace LODGenerator.NifMain
{
    public class bhkNiCollisionObject : NiCollisionObject
    {
        private ushort flags;
        private int body;

        public bhkNiCollisionObject()
        {
            this.flags = 0;
            this.body = -1;
        }

        public override void Read(NiHeader header, BinaryReader reader)
        {
            base.Read(header, reader);
            this.flags = reader.ReadUInt16();
            this.body = reader.ReadInt32();
        }

        public override void Write(NiHeader header, BinaryWriter writer)
        {
            List<int> blockReferences = header.GetBlockReferences();
            if (blockReferences.Count > 0)
            {
                if (this.body != -1)
                {
                    this.body = blockReferences[this.body];
                }
            }
            base.Write(header, writer);
            writer.Write(this.flags);
            writer.Write(this.body);
        }

        public override uint GetSize(NiHeader header)
        {
            return base.GetSize(header) + 6;
        }

        public override string GetClassName()
        {
            return "bhkNiCollisionObject";
        }

        public override bool IsDerivedType(string type)
        {
            bool flag = base.IsDerivedType(type);
            if (!flag)
                flag = type == "bhkNiCollisionObject";
            return flag;
        }
    }
}
using System;
using System.Collections.Generic;
using System.IO;

namespace LODGenerator.NifMain
{
    public class NiCollisionObject : NiObject
    {
        private int target;

        public NiCollisionObject()
        {
            this.target = -1;
        }

        public override void Read(NiHeader header, BinaryReader reader)
        {
            base.Read(header, reader);
            this.target = reader.ReadInt32();
        }

        public override void Write(NiHeader header, BinaryWriter writer)
        {
            List<int> blockReferences = header.GetBlockReferences();
            if (blockReferences.Count > 0)
            {
                if (this.target != -1)
                {
                    this.target = blockReferences[this.target];
                }
            }
            base.Write(header, writer);
            writer.Write(this.target);
        }

        public override uint GetSize(NiHeader header)
        {
            return base.GetSize(header) + 4;
        }

        public override string GetClassName()
        {
            return "NiCollisionObject";
        }

        public override bool IsDerivedType(string type)
        {
            bool flag = base.IsDerivedType(type);
            if (!flag)
                flag = type == "NiCollisionObject";
            return flag;
        }
    }
}
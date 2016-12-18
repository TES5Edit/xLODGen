using System;
using System.Collections.Generic;
using System.IO;

namespace LODGenerator.NifMain
{
    public class bhkCompressedMeshShape : bhkSerializable
    {
        private int target;
        //48
        private byte[] stuff;
        private int data;

        public bhkCompressedMeshShape()
        {
            this.target = -1;
            this.stuff = new byte[48];
            this.data = -1;
        }

        public override void Read(NiHeader header, BinaryReader reader)
        {
            base.Read(header, reader);
            this.target = reader.ReadInt32();
            this.stuff = reader.ReadBytes(48);
            this.data = reader.ReadInt32();
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
                if (this.data != -1)
                {
                    this.data = blockReferences[this.data];
                }

            }
            base.Write(header, writer);
            writer.Write(this.target);
            writer.Write(this.stuff);
            writer.Write(this.data);
        }

        public override uint GetSize(NiHeader header)
        {
            return base.GetSize(header) + 56;
        }

        public override string GetClassName()
        {
            return "bhkCompressedMeshShape";
        }

        public override bool IsDerivedType(string type)
        {
            bool flag = base.IsDerivedType(type);
            if (!flag)
                flag = type == "bhkCompressedMeshShape";
            return flag;
        }
    }
}
using System;
using System.Collections.Generic;
using System.IO;

namespace LODGenerator.NifMain
{
    public class bhkMoppBvTreeShape : bhkBvTreeShape
    {
        private int shape;
        //16
        private byte[] stuff;
        private uint moppSize;
        // 17
        private byte[] stuff2;
        private byte[] moppData;

        public bhkMoppBvTreeShape()
        {
            this.shape = -1;
            this.stuff = new byte[16];
            this.moppSize = 0;
            this.stuff2 = new byte[17];
            this.moppData = new byte[0];
        }

        public override void Read(NiHeader header, BinaryReader reader)
        {
            base.Read(header, reader);
            this.shape = reader.ReadInt32();
            this.stuff = reader.ReadBytes(16);
            this.moppSize = reader.ReadUInt32();
            this.stuff2 = reader.ReadBytes(17);
            this.moppData = reader.ReadBytes((int)this.moppSize);
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
            writer.Write(this.moppSize);
            writer.Write(this.stuff2);
            for (int index = 0; index < this.moppSize; index++)
            {
                writer.Write(this.moppData[index]);
            }
        }

        public override uint GetSize(NiHeader header)
        {
            return base.GetSize(header) + 41 + moppSize;
        }

        public override string GetClassName()
        {
            return "bhkMoppBvTreeShape";
        }

        public override bool IsDerivedType(string type)
        {
            bool flag = base.IsDerivedType(type);
            if (!flag)
                flag = type == "bhkMoppBvTreeShape";
            return flag;
        }
    }
}
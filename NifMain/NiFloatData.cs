using System;
using System.Collections.Generic;
using System.IO;

namespace LODGenerator.NifMain
{
    public class NiFloatData : NiObject
    {
        private uint numKeys;
        private uint keyType;
        private List<byte[]> data;

        public NiFloatData()
        {
            this.numKeys = 0;
            this.keyType = 0;
            this.data = new List<byte[]>();
        }

        public override void Read(NiHeader header, BinaryReader reader)
        {
            base.Read(header, reader);
            this.numKeys = reader.ReadUInt32();
            this.keyType = reader.ReadUInt32();
            for (int index = 0; index < numKeys; ++index)
            {
                this.data.Add(reader.ReadBytes(8));
                if (this.keyType == 2)
                {
                    this.data.Add(reader.ReadBytes(8));
                }
                else if (this.keyType == 3)
                {
                    this.data.Add(reader.ReadBytes(12));
                }
            }
        }

        public override void Write(NiHeader header, BinaryWriter writer)
        {
            base.Write(header, writer);
            writer.Write(this.numKeys);
            writer.Write(this.keyType);
            for (int index = 0; index < this.data.Count; index++)
            {
                writer.Write(this.data[index]);
            }
        }

        public override uint GetSize(NiHeader header)
        {
            uint num = 8;
            if (this.keyType == 2)
            {
                num += 8;
            }
            else if (this.keyType == 3)
            {
                num += 12;
            }
            return base.GetSize(header) + 8 + (this.numKeys * num);
        }

        public override string GetClassName()
        {
            return "NiFloatData";
        }

        public override bool IsDerivedType(string type)
        {
            bool flag = base.IsDerivedType(type);
            if (!flag)
                flag = type == "NiFloatData";
            return flag;
        }
    }
}

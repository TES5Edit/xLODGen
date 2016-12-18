using System;
using System.IO;

namespace LODGenerator.NifMain
{
    public class BSDistantObjectLargeRefExtraData : NiExtraData
    {
        private byte unknownByte;

        public BSDistantObjectLargeRefExtraData()
        {
            this.unknownByte = 0;
        }

        public override void Read(NiHeader header, BinaryReader reader)
        {
            base.Read(header, reader);
            this.unknownByte = reader.ReadByte();
        }

        public override void Write(NiHeader header, BinaryWriter writer)
        {
            base.Write(header, writer);
            writer.Write(this.unknownByte);
        }

        public override uint GetSize(NiHeader header)
        {
            return base.GetSize(header) + 1;
        }

        public void SetByte(byte value)
        {
            this.unknownByte = value;
        }

        public override string GetClassName()
        {
            return "BSDistantObjectLargeRefExtraData";
        }

        public override bool IsDerivedType(string type)
        {
            bool flag = base.IsDerivedType(type);
            if (!flag)
                flag = type == "BSDistantObjectLargeRefExtraData";
            return flag;
        }
    }
}
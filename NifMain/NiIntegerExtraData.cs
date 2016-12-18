using System;
using System.IO;

namespace LODGenerator.NifMain
{
    public class NiIntegerExtraData : NiExtraData
    {
        private uint integerData;

        public NiIntegerExtraData()
        {
            this.integerData = 0U;
        }

        public override void Read(NiHeader header, BinaryReader reader)
        {
            base.Read(header, reader);
            this.integerData = reader.ReadUInt32();
        }

        public override void Write(NiHeader header, BinaryWriter writer)
        {
            base.Write(header, writer);
            writer.Write(this.integerData);
        }

        public override uint GetSize(NiHeader header)
        {
            return base.GetSize(header) + 4U;
        }

        public void SetIntegerData(uint value)
        {
            this.integerData = value;
        }

        public override string GetClassName()
        {
            return "NiIntegerExtraData";
        }

        public override bool IsDerivedType(string type)
        {
            bool flag = base.IsDerivedType(type);
            if (!flag)
                flag = type == "NiIntegerExtraData";
            return flag;
        }
    }
}

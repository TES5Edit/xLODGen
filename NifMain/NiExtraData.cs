using LODGenerator.Common;
using System;
using System.IO;

namespace LODGenerator.NifMain
{
    public class NiExtraData : NiObject
    {
        protected string name;
        protected int nameIdx;

        public NiExtraData()
        {
            this.name = "";
            this.nameIdx = -1;
        }

        public override void Read(NiHeader header, BinaryReader reader)
        {
            base.Read(header, reader);
            if (header.GetVersion() > 335544325U)
            {
                this.nameIdx = reader.ReadInt32();
            }
            else
            {
                this.name = Utils.ReadSizedString(reader);
            }
        }

        public override void Write(NiHeader header, BinaryWriter writer)
        {
            base.Write(header, writer);
            writer.Write(this.nameIdx);
        }

        public override uint GetSize(NiHeader header)
        {
            return base.GetSize(header) + 4U;
        }

        public void SetNameIndex (int value)
        {
            this.nameIdx = value;
        }

        public override string GetClassName()
        {
            return "NiExtraData";
        }

        public override bool IsDerivedType(string type)
        {
            bool flag = base.IsDerivedType(type);
            if (!flag)
                flag = type == "NiExtraData";
            return flag;
        }
    }
}

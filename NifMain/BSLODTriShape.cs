using System;
using System.IO;

namespace LODGenerator.NifMain
{
    [Serializable]
    public class BSLODTriShape : NiTriBasedGeom
    {
        protected uint level0Size;
        protected uint level1Size;
        protected uint level2Size;

        public BSLODTriShape()
        {
            this.level0Size = 0U;
            this.level1Size = 0U;
            this.level2Size = 0U;
        }

        public override void Read(NiHeader header, BinaryReader reader)
        {
            base.Read(header, reader);
            this.level0Size = reader.ReadUInt32();
            this.level1Size = reader.ReadUInt32();
            this.level2Size = reader.ReadUInt32();
        }

        public override void Write(NiHeader header, BinaryWriter writer)
        {
            base.Write(header, writer);
            writer.Write(this.level0Size);
            writer.Write(this.level1Size);
            writer.Write(this.level2Size);
        }

        public override uint GetSize(NiHeader header)
        {
            return base.GetSize(header) + 12;
        }

        public override string GetClassName()
        {
            return "BSLODTriShape";
        }

        public override bool IsDerivedType(string type)
        {
            bool flag = base.IsDerivedType(type);
            if (!flag)
                flag = type == "BSLODTriShape";
            return flag;
        }
    }
}
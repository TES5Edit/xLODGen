using System;
using System.IO;
using System.Collections.Generic;

namespace LODGenerator.NifMain
{
    public class BSMeshLODTriShape : BSTriShape
    {
        protected uint lod0Size;
        protected uint lod1Size;
        protected uint lod2Size;

        public BSMeshLODTriShape()
        {
            this.lod0Size = 0;
            this.lod1Size = 0;
            this.lod2Size = 0;
        }

        public override void Read(NiHeader header, BinaryReader reader)
        {
            base.Read(header, reader);
            this.lod0Size = reader.ReadUInt32();
            this.lod1Size = reader.ReadUInt32();
            this.lod2Size = reader.ReadUInt32();
        }

        public override void Write(NiHeader header, BinaryWriter writer)
        {
            base.Write(header, writer);
            writer.Write(this.lod0Size);
            writer.Write(this.lod1Size);
            writer.Write(this.lod2Size);
        }

        public override uint GetSize(NiHeader header)
        {
            return base.GetSize(header) + 12;
        }

        public uint GetLOD0Size()
        {
            return this.lod0Size;
        }

        public uint GetLOD1Size()
        {
            return this.lod1Size;
        }

        public uint GetLOD2Size()
        {
            return this.lod2Size;
        }

        public override string GetClassName()
        {
            return "BSMeshLODTriShape";
        }

        public override bool IsDerivedType(string type)
        {
            bool flag = base.IsDerivedType(type);
            if (!flag)
            {
                flag = type == "BSMeshLODTriShape";
            }
            return flag;
        }
    }
}

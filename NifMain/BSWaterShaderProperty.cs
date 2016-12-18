using LODGenerator.Common;
using System;
using System.Collections.Generic;
using System.IO;

namespace LODGenerator.NifMain
{
    [Serializable]
    public class BSWaterShaderProperty : NiProperty
    {
        protected uint shaderFlags1;
        protected uint shaderFlags2;
        protected UVCoord uvOffset;
        protected UVCoord uvScale;
        protected byte waterShaderFlags;
        protected byte waterDirection;
        protected ushort unknown;

        public BSWaterShaderProperty()
        {
            this.shaderFlags1 = 0;
            this.shaderFlags2 = 0;
            this.uvOffset = new UVCoord(0.0f, 0.0f);
            this.uvScale = new UVCoord(1f, 1f);
            this.waterShaderFlags = 0;
            this.waterDirection = 0;
            this.unknown = 0;
        }

        public override void Read(NiHeader header, BinaryReader reader)
        {
            base.Read(header, reader);
            this.shaderFlags1 = reader.ReadUInt32();
            this.shaderFlags2 = reader.ReadUInt32();
            this.uvOffset = Utils.ReadUVCoord(reader);
            this.uvScale = Utils.ReadUVCoord(reader);
            this.waterShaderFlags = reader.ReadByte();
            this.waterDirection = reader.ReadByte();
            this.unknown = reader.ReadUInt16();
        }

        public override void Write(NiHeader header, BinaryWriter writer)
        {
            base.Write(header, writer);
            writer.Write(this.shaderFlags1);
            writer.Write(this.shaderFlags2);
            Utils.WriteUVCoord(writer, this.uvOffset);
            Utils.WriteUVCoord(writer, this.uvScale);
            writer.Write(this.waterShaderFlags);
            writer.Write(this.waterDirection);
            writer.Write(this.unknown);
        }

        public override uint GetSize(NiHeader header)
        {
            return base.GetSize(header) + 28U;
        }

        public override string GetClassName()
        {
            return "BSWaterShaderProperty";
        }

        public override bool IsDerivedType(string type)
        {
            bool flag = base.IsDerivedType(type);
            if (!flag)
                flag = type == "BSWaterShaderProperty";
            return flag;
        }
    }
}
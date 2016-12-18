using System;
using System.Collections.Generic;
using System.IO;

namespace LODGenerator.NifMain
{
    public class BSEffectShaderPropertyColorController : NiFloatInterpController
    {
        private uint controledColorType;

        public BSEffectShaderPropertyColorController()
        {
            this.controledColorType = 0;
        }

        public override void Read(NiHeader header, BinaryReader reader)
        {
            base.Read(header, reader);
            this.controledColorType = reader.ReadUInt32();
        }

        public override void Write(NiHeader header, BinaryWriter writer)
        {
            base.Write(header, writer);
            writer.Write(this.controledColorType);
        }

        public override uint GetSize(NiHeader header)
        {
            return base.GetSize(header) + 4;
        }

        public override string GetClassName()
        {
            return "BSEffectShaderPropertyColorController";
        }

        public override bool IsDerivedType(string type)
        {
            bool flag = base.IsDerivedType(type);
            if (!flag)
                flag = type == "BSEffectShaderPropertyColorController";
            return flag;
        }
    }
}

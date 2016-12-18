using System;
using System.IO;

namespace LODGenerator.NifMain
{
    public class NiFloatInterpController : NiSingleInterpController
    {
        public override void Read(NiHeader header, BinaryReader reader)
        {
            base.Read(header, reader);
        }

        public override void Write(NiHeader header, BinaryWriter writer)
        {
            base.Write(header, writer);
        }

        public override uint GetSize(NiHeader header)
        {
            return base.GetSize(header);
        }

        public override string GetClassName()
        {
            return "NiFloatInterpController";
        }

        public override bool IsDerivedType(string type)
        {
            bool flag = base.IsDerivedType(type);
            if (!flag)
                flag = type == "NiFloatInterpController";
            return flag;
        }
    }
}
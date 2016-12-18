using System;
using System.Collections.Generic;
using System.IO;

namespace LODGenerator.NifMain
{
    public class NiTransformController : NiKeyframeController
    {

        public NiTransformController()
        {
        }

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
            return "NiTransformController";
        }

        public override bool IsDerivedType(string type)
        {
            bool flag = base.IsDerivedType(type);
            if (!flag)
                flag = type == "NiTransformController";
            return flag;
        }
    }
}
using System;
using System.Collections.Generic;
using System.IO;

namespace LODGenerator.NifMain
{
    public class NiSingleInterpController : NiInterpController
    {
        private int interpolator;

        public NiSingleInterpController()
        {
            this.interpolator = -1;
        }

        public override void Read(NiHeader header, BinaryReader reader)
        {
            base.Read(header, reader);
            this.interpolator = reader.ReadInt32();
        }

        public override void Write(NiHeader header, BinaryWriter writer)
        {
            List<int> blockReferences = header.GetBlockReferences();
            if (blockReferences.Count > 0)
            {
                if (this.interpolator != -1)
                {
                    this.interpolator = blockReferences[this.interpolator];
                }
            }
            base.Write(header, writer);
            writer.Write(this.interpolator);
        }

        public override uint GetSize(NiHeader header)
        {
            return base.GetSize(header) + 4;
        }

        public override string GetClassName()
        {
            return "NiSingleInterpController";
        }

        public override bool IsDerivedType(string type)
        {
            bool flag = base.IsDerivedType(type);
            if (!flag)
                flag = type == "NiSingleInterpController";
            return flag;
        }
    }
}

using System.IO;
using System.Collections.Generic;

namespace LODGenerator.NifMain
{
    public class NiFloatInterpolator : NiKeyBasedInterpolator
    {
        private float value;
        private int data;

        public NiFloatInterpolator()
        {
            this.value = 0;
            this.data = -1;
        }

        public override void Read(NiHeader header, BinaryReader reader)
        {
            base.Read(header, reader);
            this.value = reader.ReadSingle();
            this.data = reader.ReadInt32();
        }

        public override void Write(NiHeader header, BinaryWriter writer)
        {
            List<int> blockReferences = header.GetBlockReferences();
            if (blockReferences.Count > 0)
            {
                if (this.data != -1)
                {
                    this.data = blockReferences[this.data];
                }
            }
            base.Write(header, writer);
            writer.Write(this.value);
            writer.Write(this.data);
        }

        public override uint GetSize(NiHeader header)
        {
            return base.GetSize(header) + 8;
        }

        public override string GetClassName()
        {
            return "NiFloatInterpolator";
        }

        public override bool IsDerivedType(string type)
        {
            bool flag = base.IsDerivedType(type);
            if (!flag)
                flag = type == "NiFloatInterpolator";
            return flag;
        }
    }
}
using LODGenerator.Common;
using System.Collections.Generic;
using System.IO;

namespace LODGenerator.NifMain
{
    public class NiPoint3Interpolator : NiObject
    {
        private Vector3 value;
        private int data;

        public NiPoint3Interpolator()
        {
            this.value = new Vector3(0f, 0f, 0f);
            this.data = -1;
        }

        public override void Read(NiHeader header, BinaryReader reader)
        {
            base.Read(header, reader);
            this.value = Utils.ReadVector3(reader);
            this.data = reader.ReadInt32();
        }

        public override void Write(NiHeader header, BinaryWriter writer)
        {
            List<int> blockReferences = header.GetBlockReferences();
            if (blockReferences.Count > 0)
            {
                if (this.data != 1)
                {
                    this.data = blockReferences[this.data];
                }
            }
            base.Write(header, writer);
            Utils.WriteVector3(writer, this.value);
            writer.Write(this.data);
        }

        public override uint GetSize(NiHeader header)
        {
            return base.GetSize(header) + 16;
        }

        public override string GetClassName()
        {
            return "NiPoint3Interpolator";
        }

        public override bool IsDerivedType(string type)
        {
            bool flag = base.IsDerivedType(type);
            if (!flag)
                flag = type == "NiPoint3Interpolator";
            return flag;
        }
    }
}

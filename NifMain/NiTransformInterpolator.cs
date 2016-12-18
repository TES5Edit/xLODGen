using LODGenerator.Common;
using System.Collections.Generic;
using System.IO;

namespace LODGenerator.NifMain
{
    public class NiTransformInterpolator : NiKeyBasedInterpolator
    {
        private Vector3 translation;
        private Vector4 rotation;
        private float scale;
        private int data;

        public NiTransformInterpolator()
        {
            this.translation = new Vector3(0f, 0f, 0f);
            this.rotation = new Vector4(0f, 0f, 0f, 0f);
            this.scale = 1;
            this.data = -1;
        }

        public override void Read(NiHeader header, BinaryReader reader)
        {
            base.Read(header, reader);
            this.translation = Utils.ReadVector3(reader);
            this.rotation = Utils.ReadVector4(reader);
            this.scale = reader.ReadSingle();
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
            Utils.WriteVector3(writer, this.translation);
            Utils.WriteVector4(writer, this.rotation);
            writer.Write(this.scale);
            writer.Write(this.data);
        }

        public override uint GetSize(NiHeader header)
        {
            return base.GetSize(header) + 36;
        }

        public override string GetClassName()
        {
            return "NiTransformInterpolator";
        }

        public override bool IsDerivedType(string type)
        {
            bool flag = base.IsDerivedType(type);
            if (!flag)
                flag = type == "NiTransformInterpolator";
            return flag;
        }
    }
}
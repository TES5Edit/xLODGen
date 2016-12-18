using System;
using System.IO;

namespace LODGenerator.NifMain
{
    public class BSInvMarker : NiExtraData
    {
        private ushort rotationX;
        private ushort rotationY;
        private ushort rotationZ;
        private float zoom;

        public BSInvMarker()
        {
            this.rotationX = 0;
            this.rotationY = 0;
            this.rotationZ = 0;
            this.zoom = 0;
        }

        public override void Read(NiHeader header, BinaryReader reader)
        {
            base.Read(header, reader);
            this.rotationX = reader.ReadUInt16();
            this.rotationY = reader.ReadUInt16();
            this.rotationZ = reader.ReadUInt16();
            this.zoom = reader.ReadSingle();
        }

        public override void Write(NiHeader header, BinaryWriter writer)
        {
            base.Write(header, writer);
            writer.Write(this.rotationX);
            writer.Write(this.rotationY);
            writer.Write(this.rotationZ);
            writer.Write(this.zoom);
        }

        public override uint GetSize(NiHeader header)
        {
            return base.GetSize(header) + 10;
        }

        public override string GetClassName()
        {
            return "BSInvMarker";
        }

        public override bool IsDerivedType(string type)
        {
            bool flag = base.IsDerivedType(type);
            if (!flag)
                flag = type == "BSInvMarker";
            return flag;
        }
    }
}

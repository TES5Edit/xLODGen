using System.IO;

namespace LODGenerator.NifMain
{
    public class NiAlphaProperty : NiProperty
    {
        private ushort flags;
        private byte threshold;

        public NiAlphaProperty()
        {
            this.flags = (ushort)236;
            this.threshold = (byte)0;
        }

        public override void Read(NiHeader header, BinaryReader reader)
        {
            base.Read(header, reader);
            this.flags = reader.ReadUInt16();
            this.threshold = reader.ReadByte();
        }

        public override void Write(NiHeader header, BinaryWriter writer)
        {
            base.Write(header, writer);
            writer.Write(this.flags);
            writer.Write(this.threshold);
        }

        public override uint GetSize(NiHeader header)
        {
            return base.GetSize(header) + 3U;
        }

        public void SetFlags(ushort value)
        {
            this.flags = value;
        }

        public void SetThreshold(byte value)
        {
            this.threshold = value;
        }

        public byte GetThreshold()
        {
            return this.threshold;
        }

        public override string GetClassName()
        {
            return "NiAlphaProperty";
        }

        public override bool IsDerivedType(string type)
        {
            bool flag = base.IsDerivedType(type);
            if (!flag)
                flag = type == "NiAlphaProperty";
            return flag;
        }
    }
}

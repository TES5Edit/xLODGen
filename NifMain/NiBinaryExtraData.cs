using System.Collections.Generic;
using System.IO;
using LODGenerator.Common;

namespace LODGenerator.NifMain
{
    public class NiBinaryExtraData : NiExtraData
    {
        protected uint binarySize;
        protected List<byte> binaryData;

        public NiBinaryExtraData()
        {
            this.binarySize = 0U;
            this.binaryData = new List<byte>();
        }

        public override void Read(NiHeader header, BinaryReader reader)
        {
            base.Read(header, reader);
            this.binarySize = reader.ReadUInt32();
            for (int index = 0; (long)index < (long)this.binarySize; ++index)
                this.binaryData.Add(reader.ReadByte());
        }

        public List<byte> GetBinaryData()
        {
            return this.binaryData;
        }

        public List<Vector3> GetBitangents()
        {
            List<Vector3> v = new List<Vector3>();
            for (int index = binaryData.Count / 2; index < binaryData.Count; index += 12)
            {
                byte[] bytes = new byte[12];
                for (int index2 = 0; index2 < 12; ++index2)
                {
                    bytes[index2] = binaryData[index + index2];
                }
                float float1 = System.BitConverter.ToSingle(bytes, 0);
                float float2 = System.BitConverter.ToSingle(bytes, 4);
                float float3 = System.BitConverter.ToSingle(bytes, 8);
                v.Add(new Vector3(float1, float2, float3));
            }
            return v;
        }

        public List<Vector3> GetTangents()
        {
            List<Vector3> v = new List<Vector3>();
            for (int index = 0; index < binaryData.Count / 2; index += 12)
            {
                byte[] bytes = new byte[12];
                for (int index2 = 0; index2 < 12; ++index2)
                {
                    bytes[index2] = binaryData[index + index2];
                }
                float float1 = System.BitConverter.ToSingle(bytes, 0);
                float float2 = System.BitConverter.ToSingle(bytes, 4);
                float float3 = System.BitConverter.ToSingle(bytes, 8);
                v.Add(new Vector3(float1, float2, float3));
            }
            return v;
        }

        public override uint GetSize(NiHeader header)
        {
            return (uint)((int)base.GetSize(header) + 4 + this.binaryData.Count);
        }

        public override string GetClassName()
        {
            return "NiBinaryExtraData";
        }

        public override bool IsDerivedType(string type)
        {
            bool flag = base.IsDerivedType(type);
            if (!flag)
                flag = type == "NiBinaryExtraData";
            return flag;
        }
    }
}

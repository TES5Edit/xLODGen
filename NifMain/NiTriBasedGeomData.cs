using System.IO;

namespace LODGenerator.NifMain
{
    public class NiTriBasedGeomData : NiGeometryData
    {
        protected ushort numTriangles;

        public NiTriBasedGeomData()
        {
            this.numTriangles = (ushort)0;
        }

        public override void Read(NiHeader header, BinaryReader reader)
        {
            base.Read(header, reader);
            this.numTriangles = reader.ReadUInt16();
        }

        public override void Write(NiHeader header, BinaryWriter writer)
        {
            base.Write(header, writer);
            writer.Write(this.numTriangles);
        }

        public override uint GetSize(NiHeader header)
        {
            return base.GetSize(header) + 2U;
        }

        public override string GetClassName()
        {
            return "NiTriBasedGeomData";
        }

        public void SetNumTriangles(ushort value)
        {
            this.numTriangles = value;
        }

        public ushort GetNumTriangles()
        {
            return this.numTriangles;
        }

        public override bool IsDerivedType(string type)
        {
            bool flag = base.IsDerivedType(type);
            if (!flag)
                flag = type == "NiTriBasedGeom";
            return flag;
        }
    }
}

using System.IO;

namespace LODGenerator.NifMain
{
    public class NiTriBasedGeom : NiGeometry
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
            return "NiTriBasedGeom";
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

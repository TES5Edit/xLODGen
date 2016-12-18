using System.IO;

namespace LODGenerator.NifMain
{
    public class NiTriStrips : NiTriBasedGeom
    {
        public override void Read(NiHeader header, BinaryReader reader)
        {
            base.Read(header, reader);
        }

        public override uint GetSize(NiHeader header)
        {
            return base.GetSize(header);
        }

        public override string GetClassName()
        {
            return "NiTriStrips";
        }

        public override bool IsDerivedType(string type)
        {
            bool flag = base.IsDerivedType(type);
            if (!flag)
                flag = type == "NiTriStrips";
            return flag;
        }
    }
}

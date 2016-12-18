using System;
using System.IO;

namespace LODGenerator.NifMain
{
    [Serializable]
    public class NiObject
    {
        public virtual void Read(NiHeader header, BinaryReader reader)
        {
        }

        public virtual void Write(NiHeader header, BinaryWriter writer)
        {
        }

        public virtual string GetClassName()
        {
            return "NiObject";
        }

        public virtual uint GetSize(NiHeader header)
        {
            return 0U;
        }

        public virtual bool IsDerivedType(string type)
        {
            return type == "NiObject";
        }
    }
}

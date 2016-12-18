using LODGenerator.Common;
using System;
using System.Collections.Generic;
using System.IO;

namespace LODGenerator.NifMain
{
    public class BSTreeNode : NiNode
    {
        protected uint numBones;
        protected List<int> bones;
        protected uint numBones2;
        protected List<int> bones2;

        public BSTreeNode()
        {
            this.numBones = 0U;
            this.bones = new List<int>();
            this.numBones2 = 0U;
            this.bones2 = new List<int>();
        }

        public override void Read(NiHeader header, BinaryReader reader)
        {
            base.Read(header, reader);
            this.numBones = reader.ReadUInt32();
            for (int index = 0; (long)index < (long)this.numBones; ++index)
            {
                this.bones.Add(reader.ReadInt32());
            }
            this.numBones2 = reader.ReadUInt32();
            for (int index = 0; (long)index < (long)this.numBones2; ++index)
            {
                this.bones2.Add(reader.ReadInt32());
            }
        }

        public override string GetClassName()
        {
            return "BSTreeNode";
        }

        public override bool IsDerivedType(string type)
        {
            bool flag = base.IsDerivedType(type);
            if (!flag)
                flag = type == "BSTreeNode";
            return flag;
        }
    }
}

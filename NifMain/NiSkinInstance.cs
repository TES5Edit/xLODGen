using System;
using System.Collections.Generic;
using System.IO;

namespace LODGenerator.NifMain
{
    public class NiSkinInstance : NiObject
    {
        private int data;
        private int skinPartition;
        private int skeletonRoot;
        private uint numBones;
        private List<int> bones;

        public NiSkinInstance()
        {
            this.data = -1;
            this.skinPartition = -1;
            this.skeletonRoot = -1;
            this.numBones = 0;
            this.bones = new List<int>();
        }

        public override void Read(NiHeader header, BinaryReader reader)
        {
            base.Read(header, reader);
            this.data = reader.ReadInt32();
            this.skinPartition = reader.ReadInt32();
            this.skeletonRoot = reader.ReadInt32();
            this.numBones = reader.ReadUInt32();
            for (int index = 0; index < this.numBones; index++)
            {
                bones.Add(reader.ReadInt32());
            }
        }

        public override void Write(NiHeader header, BinaryWriter writer)
        {
            List<int> blockReferences = header.GetBlockReferences();
            if (blockReferences.Count > 0)
            {
                if (this.data != -1)
                {
                    this.data = blockReferences[this.data];
                }
                if (this.skinPartition != -1)
                {
                    this.skinPartition = blockReferences[this.skinPartition];
                }
                if (this.skeletonRoot != -1)
                {
                    this.skeletonRoot = blockReferences[this.skeletonRoot];
                }
            }
            base.Write(header, writer);
            writer.Write(this.data);
            writer.Write(this.skinPartition);
            writer.Write(this.skeletonRoot);
            writer.Write(this.numBones);
            for (int index = 0; index < this.numBones; index++)
            {
                if (this.bones[index] != -1)
                {
                    this.bones[index] = blockReferences[this.bones[index]];
                }
                writer.Write(this.bones[index]);
            }
        }

        public override uint GetSize(NiHeader header)
        {
            return base.GetSize(header) + 56;
        }

        public override string GetClassName()
        {
            return "NiSkinInstance";
        }

        public override bool IsDerivedType(string type)
        {
            bool flag = base.IsDerivedType(type);
            if (!flag)
                flag = type == "NiSkinInstance";
            return flag;
        }
    }
}
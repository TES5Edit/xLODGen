using System;
using System.Collections.Generic;
using System.IO;

namespace LODGenerator.NifMain
{
    public class NiTimeController : NiObject
    {
        private int nextController;
        private ushort flags;
        private float frequency;
        private float phase;
        private float startTime;
        private float stopTime;
        private int target;

        public NiTimeController()
        {
            this.nextController = -1;
            this.flags = 0;
            this.frequency = 1;
            this.phase = 0;
            this.startTime = 0;
            this.stopTime = 0;
            this.target = -1;
        }

        public override void Read(NiHeader header, BinaryReader reader)
        {
            base.Read(header, reader);
            this.nextController = reader.ReadInt32();
            this.flags = reader.ReadUInt16();
            this.frequency = reader.ReadSingle();
            this.phase = reader.ReadSingle();
            this.startTime = reader.ReadSingle();
            this.stopTime = reader.ReadSingle();
            this.target = reader.ReadInt32();
        }

        public override void Write(NiHeader header, BinaryWriter writer)
        {
            List<int> blockReferences = header.GetBlockReferences();
            if (blockReferences.Count > 0)
            {
                if (this.nextController != -1)
                {
                    this.nextController = blockReferences[this.nextController];
                }
                if (this.target != -1)
                {
                    this.target = blockReferences[this.target];
                }
            }
            base.Write(header, writer);
            writer.Write(this.nextController);
            writer.Write(this.flags);
            writer.Write(this.frequency);
            writer.Write(this.phase);
            writer.Write(this.startTime);
            writer.Write(this.stopTime);
            writer.Write(this.target);
        }

        public override uint GetSize(NiHeader header)
        {
            return base.GetSize(header) + 26;
        }

        public override string GetClassName()
        {
            return "NiTimeController";
        }

        public override bool IsDerivedType(string type)
        {
            bool flag = base.IsDerivedType(type);
            if (!flag)
                flag = type == "NiTimeController";
            return flag;
        }
    }
}

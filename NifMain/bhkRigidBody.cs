using System;
using System.Collections.Generic;
using System.IO;

namespace LODGenerator.NifMain
{
    public class bhkRigidBody : bhkEntity
    {
        // 236
        private byte[] stuff;
        private uint numConstraints;
        private List<int> constraints;
        private int unknownInt9;
        private ushort unknownInt91;

        public bhkRigidBody()
        {
            stuff = new byte[236];
            this.numConstraints = 0;
            this.constraints = new List<int>();
            this.unknownInt9 = 0;
            this.unknownInt91 = 0;
        }

        public override void Read(NiHeader header, BinaryReader reader)
        {
            base.Read(header, reader);
            this.stuff = reader.ReadBytes(236);
            this.numConstraints = reader.ReadUInt32();
            for (int index = 0; index < numConstraints; index++)
            {
                this.constraints.Add(reader.ReadInt32());
            }
            if (header.GetUserVersion() <= 11)
            {
                this.unknownInt9 = reader.ReadInt32();
            }
            else
            {
                this.unknownInt91 = reader.ReadUInt16();
            }
        }

        public override void Write(NiHeader header, BinaryWriter writer)
        {
            List<int> blockReferences = header.GetBlockReferences();
            base.Write(header, writer);
            writer.Write(this.stuff);
            writer.Write(this.numConstraints);
            for (int index = 0; index < numConstraints; index++)
            {
                if (blockReferences.Count > 0)
                {
                    this.constraints[index] = blockReferences[this.constraints[index]];
                }
                writer.Write(this.constraints[index]);
            }
            if (header.GetUserVersion() <= 11)
            {
                writer.Write(this.unknownInt9);
            }
            else
            {
                writer.Write(this.unknownInt91);
            }
        }

        public override uint GetSize(NiHeader header)
        {
            uint num = 236;
            if (header.GetUserVersion() <= 11)
            {
                num += 2;
            }
            return base.GetSize(header) + num + (4 * numConstraints);
        }

        public override string GetClassName()
        {
            return "bhkRigidBody";
        }

        public override bool IsDerivedType(string type)
        {
            bool flag = base.IsDerivedType(type);
            if (!flag)
                flag = type == "bhkRigidBody";
            return flag;
        }
    }
}
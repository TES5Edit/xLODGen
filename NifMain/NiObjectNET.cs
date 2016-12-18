using LODGenerator.Common;
using System;
using System.Collections.Generic;
using System.IO;

namespace LODGenerator.NifMain
{
    [Serializable]
    public class NiObjectNET : NiObject
    {
        protected int nameIdx;
        protected string name;
        protected uint numExtraData;
        protected List<int> extraData;
        protected int controller;

        public NiObjectNET()
        {
            this.nameIdx = -1;
            this.name = "";
            this.numExtraData = 0U;
            this.extraData = new List<int>();
            this.controller = -1;
        }

        public override void Read(NiHeader header, BinaryReader reader)
        {
            Utils.ReadString(reader, header.GetVersion(), out this.nameIdx, out this.name);
            this.numExtraData = reader.ReadUInt32();
            for (int index = 0; (long)index < (long)this.numExtraData; ++index)
            {
                this.extraData.Add(reader.ReadInt32());
            }
            this.controller = reader.ReadInt32();
        }

        public override void Write(NiHeader header, BinaryWriter writer)
        {
            List<int> blockReferences = header.GetBlockReferences();
            if (blockReferences.Count > 0)
            {
                if (this.controller != -1)
                {
                    this.controller = blockReferences[this.controller];
                }
            }
            base.Write(header, writer);
            if (header.GetVersion() <= 335544325U)
            {
                if (this.name != "")
                {
                    Utils.WriteSizedString(writer, this.name);
                }
                else if (this.nameIdx == -1)
                {
                    Utils.WriteSizedString(writer, "Scene Root");
                }
                else
                {
                    Utils.WriteSizedString(writer, header.GetString((uint)this.nameIdx));
                }
            }
            else
            {
                writer.Write(this.nameIdx);
            }
            writer.Write(this.extraData.Count);
            if (this.extraData.Count > 0)
            {
                for (int index = 0; index < this.extraData.Count; index++)
                {
                    if (blockReferences.Count > 0)
                    {
                        if (this.extraData[index] != -1)
                        {
                            this.extraData[index] = blockReferences[this.extraData[index]];
                        }
                    }
                    writer.Write(this.extraData[index]);
                }
            }
            writer.Write(controller);
        }

        public override uint GetSize(NiHeader header)
        {
            return (uint)((int)base.GetSize(header) + 12 + 4 * (int)this.numExtraData);
        }

        public override string GetClassName()
        {
            return "NiObjectNET";
        }

        public string GetName()
        {
            return this.name;
        }

        public int GetNameIndex()
        {
            return this.nameIdx;
        }

        public void SetNameIndex(int value)
        {
            this.nameIdx = value;
        }

        public uint GetNumExtraData()
        {
            return this.numExtraData;
        }

        public void SetExtraData(List<int> value)
        {
            this.extraData = value;
            this.numExtraData = (uint)this.extraData.Count;
        }

        public List<int> GetExtraData()
        {
            return this.extraData;
        }

        public int GetController()
        {
            return this.controller;
        }

        public override bool IsDerivedType(string type)
        {
            bool flag = base.IsDerivedType(type);
            if (!flag)
                flag = type == "NiObjectNET";
            return flag;
        }
    }
}

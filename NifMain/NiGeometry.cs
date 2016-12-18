using LODGenerator.Common;
using System;
using System.Collections.Generic;
using System.IO;

namespace LODGenerator.NifMain
{
    [Serializable]
    public class NiGeometry : NiAVObject
    {
        protected int data;
        protected Vector3 center;
        protected float radius;
        protected int skinInstance;
        protected bool hasShader;
        protected string shaderName;
        protected int unknownInt;
        protected uint numMaterials;
        protected List<uint> materialNames;
        protected List<int> materialExtraData;
        protected int activeMaterial;
        protected bool dirtyFlag;
        protected int[] bsProperties;

        public NiGeometry()
        {
            this.data = -1;
            this.center = new Vector3(0, 0, 0);
            this.radius = 0;
            this.skinInstance = -1;
            this.numMaterials = 0U;
            this.materialNames = new List<uint>();
            this.materialExtraData = new List<int>();
            this.activeMaterial = 0;
            this.dirtyFlag = false;
            this.bsProperties = new int[2];
            this.bsProperties[0] = -1;
            this.bsProperties[1] = -1;
        }

        public override void Read(NiHeader header, BinaryReader reader)
        {
            base.Read(header, reader);
            if (header.GetUserVersion2() == 130)
            {
                this.center = Utils.ReadVector3(reader);
                this.radius = reader.ReadSingle();
                this.skinInstance = reader.ReadInt32();
                this.bsProperties[0] = reader.ReadInt32();
                this.bsProperties[1] = reader.ReadInt32();
            }
            else
            {
                this.data = reader.ReadInt32();
                this.skinInstance = reader.ReadInt32();
                if (header.GetVersion() > 335544325U)
                {
                    this.numMaterials = reader.ReadUInt32();
                    for (int index = 0; (long)index < (long)this.numMaterials; ++index)
                    {
                        this.materialNames.Add(reader.ReadUInt32());
                    }
                    for (int index = 0; (long)index < (long)this.numMaterials; ++index)
                    {
                        this.materialExtraData.Add(reader.ReadInt32());
                    }
                    this.activeMaterial = reader.ReadInt32();
                    this.dirtyFlag = Utils.ReadBool(reader);
                    if (header.GetUserVersion() == 12)
                    {
                        this.bsProperties[0] = reader.ReadInt32();
                        this.bsProperties[1] = reader.ReadInt32();
                    }
                }
                else
                {
                    this.hasShader = Utils.ReadBool(reader);
                    if (!this.hasShader)
                    {
                        return;
                    }
                    this.shaderName = Utils.ReadSizedString(reader);
                    this.unknownInt = reader.ReadInt32();
                }
            }
        }

        public override void Write(NiHeader header, BinaryWriter writer)
        {
            base.Write(header, writer);
            if (header.GetUserVersion2() == 130)
            {
                Utils.WriteVector3(writer, this.center);
                writer.Write(this.radius);
                writer.Write(this.skinInstance);
                writer.Write(this.bsProperties[0]);
                writer.Write(this.bsProperties[1]);
            }
            else
            {
                writer.Write(this.data);
                writer.Write(this.skinInstance);
                if (header.GetVersion() > 335544325U)
                {
                    writer.Write(this.numMaterials);
                    for (int index = 0; (long)index < (long)this.numMaterials; ++index)
                    {
                        writer.Write(this.materialNames[index]);
                    }
                    for (int index = 0; (long)index < (long)this.numMaterials; ++index)
                    {
                        writer.Write(this.materialExtraData[index]);
                    }
                    writer.Write(this.activeMaterial);
                    Utils.WriteBool(writer, this.dirtyFlag);
                    if (header.GetUserVersion() == 12)
                    {
                        writer.Write(this.bsProperties[0]);
                        writer.Write(this.bsProperties[1]);
                    }
                }
                else
                {
                    Utils.WriteBool(writer, this.hasShader);
                    if (!this.hasShader)
                    {
                        return;
                    }
                    Utils.WriteSizedString(writer, this.shaderName);
                    writer.Write(this.unknownInt);
                }
            }
        }

        public override uint GetSize(NiHeader header)
        {
            if (header.GetUserVersion2() == 130)
            {
                return base.GetSize(header) + 28U;
            }
            else
            {
                if (header.GetVersion() > 335544325U)
                {
                    if (header.GetUserVersion() == 12)
                    {
                        return base.GetSize(header) + 25U + this.numMaterials * (uint)this.materialNames.Count + this.numMaterials * (uint)this.materialExtraData.Count;
                    }
                    else
                    {
                        return base.GetSize(header) + 17U + this.numMaterials * (uint)this.materialNames.Count + this.numMaterials * (uint)this.materialExtraData.Count;
                    } 
                }
                else
                {
                    if (this.hasShader)
                    {
                        return base.GetSize(header) + 13U + (uint)this.shaderName.Length;
                    }
                    else
                    {
                        return base.GetSize(header) + 9U;
                    }
                }
            }
        }

        public override string GetClassName()
        {
            return "NiGeometry";
        }

        public int GetData()
        {
            return this.data;
        }

        public void SetData(int value)
        {
            this.data = value;
        }

        public int GetSkinInstance()
        {
            return this.skinInstance;
        }

        public uint GetNumMaterials()
        {
            return this.numMaterials;
        }

        public List<uint> GetMaterialNames()
        {
            return this.materialNames;
        }

        public List<int> GetMaterialExtraData()
        {
            return this.materialExtraData;
        }

        public int GetActiveMaterial()
        {
            return this.activeMaterial;
        }

        public bool GetDirtyFlag()
        {
            return this.dirtyFlag;
        }

        public int GetBSProperty(int index)
        {
            return this.bsProperties[index];
        }

        public void SetBSProperty(int index, int value)
        {
            this.bsProperties[index] = value;
        }

        public override bool IsDerivedType(string type)
        {
            bool flag = base.IsDerivedType(type);
            if (!flag)
                flag = type == "NiGeometry";
            return flag;
        }
    }
}

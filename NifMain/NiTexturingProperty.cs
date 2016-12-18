using LODGenerator.Common;
using System.Collections.Generic;
using System;
using System.IO;

namespace LODGenerator.NifMain
{
    [Serializable]
    public class NiTexturingProperty : NiProperty
    {
        protected uint applyMode;
        protected uint textureCount;
        protected bool hasBaseTexture;
        protected TexDesc baseTexture;
        protected bool hasDarkTexture;
        protected TexDesc darkTexture;
        protected bool hasDetailTexture;
        protected TexDesc detailTexture;
        protected bool hasGlossTexture;
        protected TexDesc glossTexture;
        protected bool hasGlowTexture;
        protected TexDesc glowTexture;
        protected bool hasBumpMapTexture;
        protected TexDesc bumpMapTexture;
        protected float bumpMapLumaScale;
        protected float bumpMapLumaOffset;
        protected float[][] bumpMapMatrix;
        protected bool hasDecalTexture0;
        protected TexDesc decalTexture0;
        protected bool hasDecalTexture1;
        protected TexDesc decalTexture1;
        protected bool hasDecalTexture2;
        protected TexDesc decalTexture2;
        protected bool hasDecalTexture3;
        protected TexDesc decalTexture3;
        protected uint numShaderTextures;
        protected List<ShaderTexDesc> shaderTextures;

        public NiTexturingProperty()
        {
            this.applyMode = 0U;
            this.textureCount = 0U;
            this.hasBaseTexture = false;
            this.baseTexture = (TexDesc)null;
            this.hasDarkTexture = false;
            this.darkTexture = (TexDesc)null;
            this.hasDetailTexture = false;
            this.detailTexture = (TexDesc)null;
            this.hasGlossTexture = false;
            this.glossTexture = (TexDesc)null;
            this.hasGlowTexture = false;
            this.glowTexture = (TexDesc)null;
            this.hasBumpMapTexture = false;
            this.bumpMapTexture = (TexDesc)null;
            this.bumpMapLumaScale = 0.0f;
            this.bumpMapLumaOffset = 0.0f;
            this.bumpMapMatrix = (float[][])null;
            this.hasDecalTexture0 = false;
            this.decalTexture0 = (TexDesc)null;
            this.hasDecalTexture1 = false;
            this.decalTexture1 = (TexDesc)null;
            this.hasDecalTexture2 = false;
            this.decalTexture2 = (TexDesc)null;
            this.hasDecalTexture3 = false;
            this.decalTexture3 = (TexDesc)null;
            this.numShaderTextures = 0U;
            this.shaderTextures = new List<ShaderTexDesc>();
        }

        public override void Read(NiHeader header, BinaryReader reader)
        {
            base.Read(header, reader);
            this.applyMode = reader.ReadUInt32();
            this.textureCount = reader.ReadUInt32();
            this.hasBaseTexture = Utils.ReadBool(reader);
            this.baseTexture = this.hasBaseTexture ? Utils.ReadTexDesc(reader) : (TexDesc)null;
            this.hasDarkTexture = Utils.ReadBool(reader);
            this.darkTexture = this.hasDarkTexture ? Utils.ReadTexDesc(reader) : (TexDesc)null;
            this.hasDetailTexture = Utils.ReadBool(reader);
            this.detailTexture = this.hasDetailTexture ? Utils.ReadTexDesc(reader) : (TexDesc)null;
            this.hasGlossTexture = Utils.ReadBool(reader);
            this.glossTexture = this.hasGlossTexture ? Utils.ReadTexDesc(reader) : (TexDesc)null;
            this.hasGlowTexture = Utils.ReadBool(reader);
            this.glowTexture = this.hasGlowTexture ? Utils.ReadTexDesc(reader) : (TexDesc)null;
            this.hasBumpMapTexture = Utils.ReadBool(reader);
            if (this.hasBumpMapTexture)
            {
                this.bumpMapTexture = Utils.ReadTexDesc(reader);
                this.bumpMapLumaScale = reader.ReadSingle();
                this.bumpMapLumaOffset = reader.ReadSingle();
                this.bumpMapMatrix = (float[][])null;
            }
            this.hasDecalTexture0 = Utils.ReadBool(reader);
            this.decalTexture0 = this.hasDecalTexture0 ? Utils.ReadTexDesc(reader) : (TexDesc)null;
            if (this.textureCount >= 8U)
            {
                this.hasDecalTexture1 = Utils.ReadBool(reader);
                this.decalTexture1 = this.hasDecalTexture1 ? Utils.ReadTexDesc(reader) : (TexDesc)null;
                if (this.textureCount >= 9U)
                {
                    this.hasDecalTexture2 = Utils.ReadBool(reader);
                    this.decalTexture2 = this.hasDecalTexture2 ? Utils.ReadTexDesc(reader) : (TexDesc)null;
                    if (this.textureCount >= 10U)
                    {
                        this.hasDecalTexture3 = Utils.ReadBool(reader);
                        this.decalTexture3 = this.hasDecalTexture3 ? Utils.ReadTexDesc(reader) : (TexDesc)null;
                    }
                }
            }
            this.numShaderTextures = reader.ReadUInt32();
            for (int index = 0; (long)index < (long)this.numShaderTextures; ++index)
                this.shaderTextures.Add(new ShaderTexDesc()
                {
                    isUsed = Utils.ReadBool(reader),
                    textureData = Utils.ReadTexDesc(reader),
                    mapIndex = reader.ReadUInt32()
                });
        }

        public override void Write(NiHeader header, BinaryWriter writer)
        {
            base.Write(header, writer);
            writer.Write(this.applyMode);
            writer.Write(this.textureCount);
            Utils.WriteBool(writer, this.hasBaseTexture);
            if (this.hasBaseTexture)
            {
                Utils.WriteTexDesc(writer, this.baseTexture);
            }
            Utils.WriteBool(writer, this.hasDarkTexture);
            if (this.hasDarkTexture)
            {
                Utils.WriteTexDesc(writer, this.darkTexture);
            }
            Utils.WriteBool(writer, this.hasDetailTexture);
            if (this.hasDetailTexture)
            {
                Utils.WriteTexDesc(writer, this.detailTexture);
            }
            Utils.WriteBool(writer, this.hasGlossTexture);
            if (this.hasGlossTexture)
            {
                Utils.WriteTexDesc(writer, this.glossTexture);
            }
            Utils.WriteBool(writer, this.hasGlowTexture);
            if (this.hasGlowTexture)
            {
                Utils.WriteTexDesc(writer, this.glowTexture);
            }
            Utils.WriteBool(writer, this.hasBumpMapTexture);
            if (this.hasBumpMapTexture)
            {
                Utils.WriteTexDesc(writer, this.bumpMapTexture);
                writer.Write(this.bumpMapLumaScale);
                writer.Write(this.bumpMapLumaOffset);
                writer.Write(this.bumpMapMatrix[0][0]);
                writer.Write(this.bumpMapMatrix[1][0]);
                writer.Write(this.bumpMapMatrix[0][1]);
                writer.Write(this.bumpMapMatrix[1][1]);
            }
            Utils.WriteBool(writer, this.hasDecalTexture0);
            if (this.hasDecalTexture0)
            {
                Utils.WriteTexDesc(writer, this.decalTexture0);
            }
            if (this.textureCount >= 8U)
            {
                Utils.WriteBool(writer, this.hasDecalTexture1);
                if (this.hasDecalTexture1)
                {
                    Utils.WriteTexDesc(writer, this.decalTexture1);
                }
                if (this.textureCount >= 9U)
                {
                    Utils.WriteBool(writer, this.hasDecalTexture2);
                    if (this.hasDecalTexture2)
                    {
                        Utils.WriteTexDesc(writer, this.decalTexture2);
                    }
                    if (this.textureCount >= 10U)
                    {
                        Utils.WriteBool(writer, this.hasDecalTexture3);
                        if (this.hasDecalTexture3)
                        {
                            Utils.WriteTexDesc(writer, this.decalTexture3);
                        }
                    }
                }
            }
            writer.Write(this.numShaderTextures);
            for (int index = 0; (long)index < (long)this.numShaderTextures; ++index)
            {
                Utils.WriteBool(writer, this.shaderTextures[index].isUsed);
                Utils.WriteTexDesc(writer, this.shaderTextures[index].textureData);
                writer.Write(this.shaderTextures[index].mapIndex);
            }
        }

        public override uint GetSize(NiHeader header)
        {
            // should update this, but not needed for oblivion
            return base.GetSize(header);
        }

        public bool HasBaseTexture()
        {
            return this.hasBaseTexture;
        }

        public void SetHasBaseTexture(bool value)
        {
            this.hasBaseTexture = value;
        }

        public TexDesc GetBaseTexture()
        {
            return this.baseTexture;
        }

        public void SetBaseTexture(TexDesc value)
        {
            this.baseTexture = value;
        }

        public bool HasDarkTexture()
        {
            return this.hasDarkTexture;
        }

        public void SetHasDarkTexture(bool value)
        {
            this.hasDarkTexture = value;
        }

        public TexDesc GetDarkTexture()
        {
            return this.darkTexture;
        }

        public void SetDarkTexture(TexDesc value)
        {
            this.darkTexture = value;
        }

        public bool HasDetailTexture()
        {
            return this.hasDetailTexture;
        }

        public void SetHasDetailTexture(bool value)
        {
            this.hasDetailTexture = value;
        }

        public TexDesc GetDetailTexture()
        {
            return this.detailTexture;
        }

        public void SetDetailTexture(TexDesc value)
        {
            this.detailTexture = value;
        }

        public bool HasGlowTexture()
        {
            return this.hasGlowTexture;
        }

        public void SetHasGlowTexture(bool value)
        {
            this.hasGlowTexture = value;
        }

        public TexDesc GetGlowTexture()
        {
            return this.glowTexture;
        }

        public void SetGlowTexture(TexDesc value)
        {
            this.glowTexture = value;
        }

        public bool HasGlossTexture()
        {
            return this.hasGlossTexture;
        }

        public void SetHasGlossTexture(bool value)
        {
            this.hasGlossTexture = value;
        }

        public TexDesc GetGlossTexture()
        {
            return this.glossTexture;
        }

        public bool HasBumpMapTexture()
        {
            return this.hasBumpMapTexture;
        }

        public void SetHasBumpMapTexture(bool value)
        {
            this.hasBumpMapTexture = value;
        }

        public TexDesc GetBumpMapTexture()
        {
            return this.bumpMapTexture;
        }

        public void SetBumpMapTexture(TexDesc value)
        {
            this.bumpMapTexture = value;
        }

        public bool HasDecalTexture0()
        {
            return this.hasDecalTexture0;
        }

        public void SetHasDecalTexture0(bool value)
        {
            this.hasDecalTexture0 = value;
        }

        public TexDesc GetDecalTexture0()
        {
            return this.decalTexture0;
        }

        public void SetDecalTexture0(TexDesc value)
        {
            this.decalTexture0 = value;
        }

        public override string GetClassName()
        {
            return "NiTexturingProperty";
        }

        public override bool IsDerivedType(string type)
        {
            bool flag = base.IsDerivedType(type);
            if (!flag)
                flag = type == "NiTexturingProperty";
            return flag;
        }
    }
}

using LODGenerator.Common;
using System;
using System.Collections.Generic;
using System.IO;

namespace LODGenerator.NifMain
{
    [Serializable]
    public class BSLightingShaderProperty : NiProperty
    {
        protected uint shaderType;
        protected uint shaderFlags1;
        protected uint shaderFlags2;
        protected UVCoord uvOffset;
        protected UVCoord uvScale;
        protected int textureSet;
        protected Color3 emissiveColor;
        protected float emissiveMultiple;
        protected int wetMaterialIdx;
        protected string wetMaterialName;
        protected uint textureClampMode;
        protected float alpha;
        protected float refractionStrength;
        protected float glossiness;
        protected Color3 specularColor;
        protected float specularStrength;
        protected float lightingEffect1;
        protected float lightingEffect2;
        protected float subsurfaceRolloff;
        protected float unkownFloat1;
        protected float backlightPower;
        protected float grayscaleToPaletteScale;
        protected float fresnelPower;
        protected float wetnessSpecScale;
        protected float wetnessSpecPower;
        protected float wetnessMinVar;
        protected float wetnessEnvMapScale;
        protected float wetnessFresnelPower;
        protected float wetnessMetalness;
        protected float environmentMapScale;
        protected ushort unkownEnvMapInt;
        protected Color3 skinTintColor;
        protected uint unkownSkinTintInt;
        protected Color3 hairTintColor;
        protected float maxPasses;
        protected float scale;
        protected float parallaxInnerLayerThickness;
        protected float parallaxRefractionScale;
        protected UVCoord parallaxInnerLayerTextureScale;
        protected float parallaxEnvmapStrength;
        protected Vector4 sparkleParameters;
        protected float eyeCubemapScale;
        protected Vector3 leftEyeReflectionCenter;
        protected Vector3 rightEyeReflectionCenter;

        public BSLightingShaderProperty()
        {
            this.shaderType = 0U;
            this.shaderFlags1 = 2185233152U;
            this.shaderFlags2 = 32801U;
            this.uvOffset = new UVCoord(0.0f, 0.0f);
            this.uvScale = new UVCoord(1f, 1f);
            this.textureSet = -1;
            this.emissiveColor = new Color3(0.0f, 0.0f, 0.0f);
            this.emissiveMultiple = 1f;
            this.wetMaterialIdx = -1;
            this.wetMaterialName = "";
            this.textureClampMode = 3U;
            this.alpha = 1f;
            this.refractionStrength = 0.0f;
            this.glossiness = 80f;
            this.specularColor = new Color3(1f, 1f, 1f);
            this.specularStrength = 1f;
            this.lightingEffect1 = 0.3f;
            this.lightingEffect2 = 2f;
            this.subsurfaceRolloff = 0.0f;
            this.unkownFloat1 = float.MaxValue;
            this.backlightPower = 0.0f;
            this.grayscaleToPaletteScale = 1.0f; ;
            this.fresnelPower = 5.0f;
            this.wetnessSpecScale = -1.0f;
            this.wetnessSpecPower = -1.0f;
            this.wetnessMinVar = -1.0f;
            this.wetnessEnvMapScale = -1.0f;
            this.wetnessFresnelPower = -1.0f;
            this.wetnessMetalness = -1.0f;
        }

        public override void Write(NiHeader header, BinaryWriter writer)
        {
            List<int> blockReferences = header.GetBlockReferences();
            if (blockReferences.Count > 0)
            {
                if (this.textureSet != -1)
                {
                    this.textureSet = blockReferences[this.textureSet];
                }
            }
            writer.Write(this.shaderType);
            base.Write(header, writer);
            writer.Write(this.shaderFlags1);
            writer.Write(this.shaderFlags2);
            Utils.WriteUVCoord(writer, this.uvOffset);
            Utils.WriteUVCoord(writer, this.uvScale);
            writer.Write(this.textureSet);
            Utils.WriteColor3(writer, this.emissiveColor);
            writer.Write(this.emissiveMultiple);
            if (header.GetVersion() == 335675399U && header.GetUserVersion2() == 130)
            {
                writer.Write(this.wetMaterialIdx);
            }
            writer.Write(this.textureClampMode);
            writer.Write(this.alpha);
            writer.Write(this.refractionStrength);
            writer.Write(this.glossiness);
            Utils.WriteColor3(writer, this.specularColor);
            writer.Write(this.specularStrength);
            if (header.GetVersion() == 335675399U && header.GetUserVersion2() == 130)
            {
                writer.Write(this.subsurfaceRolloff);
                writer.Write(this.unkownFloat1);
                writer.Write(this.backlightPower);
                writer.Write(this.grayscaleToPaletteScale);
                writer.Write(this.fresnelPower);
                writer.Write(this.wetnessSpecScale);
                writer.Write(this.wetnessSpecPower);
                writer.Write(this.wetnessMinVar);
                writer.Write(this.wetnessEnvMapScale);
                writer.Write(this.wetnessFresnelPower);
                writer.Write(this.wetnessMetalness);
            }
            else
            {
                writer.Write(this.lightingEffect1);
                writer.Write(this.lightingEffect2);
            }
            if ((int)this.shaderType == 1)
            {
                writer.Write(this.environmentMapScale);
            }
            else if ((int)this.shaderType == 5)
            {
                Utils.WriteColor3(writer, this.skinTintColor);
            }
            else if ((int)this.shaderType == 6)
            {
                Utils.WriteColor3(writer, this.hairTintColor);
            }
            else if ((int)this.shaderType == 7)
            {
                writer.Write(this.maxPasses);
                writer.Write(this.scale);
            }
            else if ((int)this.shaderType == 11)
            {
                writer.Write(this.parallaxInnerLayerThickness);
                writer.Write(this.parallaxRefractionScale);
                Utils.WriteUVCoord(writer, this.parallaxInnerLayerTextureScale);
                writer.Write(this.parallaxEnvmapStrength);
            }
            else if ((int)this.shaderType == 14)
            {
                Utils.WriteVector4(writer, this.sparkleParameters);
            }
            else
            {
                if ((int)this.shaderType != 16)
                {
                    return;
                }
                writer.Write(this.eyeCubemapScale);
                Utils.WriteVector3(writer, this.leftEyeReflectionCenter);
                Utils.WriteVector3(writer, this.rightEyeReflectionCenter);
            }
        }

        public override void Read(NiHeader header, BinaryReader reader)
        {
            this.shaderType = reader.ReadUInt32();
            base.Read(header, reader);
            this.shaderFlags1 = reader.ReadUInt32();
            this.shaderFlags2 = reader.ReadUInt32();
            this.uvOffset = Utils.ReadUVCoord(reader);
            this.uvScale = Utils.ReadUVCoord(reader);
            this.textureSet = reader.ReadInt32();
            this.emissiveColor = Utils.ReadColor3(reader);
            this.emissiveMultiple = reader.ReadSingle();
            if (header.GetVersion() == 335675399U && header.GetUserVersion2() == 130)
            {
                this.wetMaterialIdx = reader.ReadInt32();
                if (this.wetMaterialIdx != -1)
                {
                    this.wetMaterialName = header.GetString((uint)this.wetMaterialIdx);
                }
            }
            this.textureClampMode = reader.ReadUInt32();
            this.alpha = reader.ReadSingle();
            this.refractionStrength = reader.ReadSingle();
            this.glossiness = reader.ReadSingle();
            this.specularColor = Utils.ReadColor3(reader);
            this.specularStrength = reader.ReadSingle();
            if (header.GetVersion() == 335675399U && header.GetUserVersion2() == 130)
            {
                this.subsurfaceRolloff = reader.ReadSingle();
                this.unkownFloat1 = reader.ReadSingle();
                this.backlightPower = reader.ReadSingle();
                this.grayscaleToPaletteScale = reader.ReadSingle();
                this.fresnelPower = reader.ReadSingle();
                this.wetnessSpecScale = reader.ReadSingle();
                this.wetnessSpecPower = reader.ReadSingle();
                this.wetnessMinVar = reader.ReadSingle();
                this.wetnessEnvMapScale = reader.ReadSingle();
                this.wetnessFresnelPower = reader.ReadSingle();
                this.wetnessMetalness = reader.ReadSingle();
            }
            else
            {
                this.lightingEffect1 = reader.ReadSingle();
                this.lightingEffect2 = reader.ReadSingle();
            }
            if ((int)this.shaderType == 1)
            {
                this.environmentMapScale = reader.ReadSingle();
                if (header.GetVersion() == 335675399U && header.GetUserVersion2() == 130)
                {
                    this.unkownEnvMapInt = reader.ReadUInt16();
                }
            }
            else if ((int)this.shaderType == 5)
            {
                this.skinTintColor = Utils.ReadColor3(reader);
                if (header.GetVersion() == 335675399U && header.GetUserVersion2() == 130)
                {
                    this.unkownSkinTintInt = reader.ReadUInt32();
                }
            }
            else if ((int)this.shaderType == 6)
            {
                this.hairTintColor = Utils.ReadColor3(reader);
            }
            else if ((int)this.shaderType == 7)
            {
                this.maxPasses = reader.ReadSingle();
                this.scale = reader.ReadSingle();
            }
            else if ((int)this.shaderType == 11)
            {
                this.parallaxInnerLayerThickness = reader.ReadSingle();
                this.parallaxRefractionScale = reader.ReadSingle();
                this.parallaxInnerLayerTextureScale = Utils.ReadUVCoord(reader);
                this.parallaxEnvmapStrength = reader.ReadSingle();
            }
            else if ((int)this.shaderType == 14)
            {
                this.sparkleParameters = Utils.ReadVector4(reader);
            }
            else
            {
                if ((int)this.shaderType != 16)
                {
                    return;
                }
                this.eyeCubemapScale = reader.ReadSingle();
                this.leftEyeReflectionCenter = Utils.ReadVector3(reader);
                this.rightEyeReflectionCenter = Utils.ReadVector3(reader);
            }
        }

        public override uint GetSize(NiHeader header)
        {
            uint num = base.GetSize(header) + 88U;
            if (header.GetVersion() == 335675399U && header.GetUserVersion2() == 130)
            {
                num += 40U;
            }
            if ((int)this.shaderType == 1)
                num += 4U;
            else if ((int)this.shaderType == 5)
                num += 12U;
            else if ((int)this.shaderType == 6)
                num += 12U;
            else if ((int)this.shaderType == 7)
                num += 8U;
            else if ((int)this.shaderType == 11)
                num += 20U;
            else if ((int)this.shaderType == 14)
                num += 16U;
            else if ((int)this.shaderType == 16)
                num += 28U;
            return num;
        }

        public override string GetClassName()
        {
            return "BSLightingShaderProperty";
        }

        public uint GetShaderType()
        {
            return this.shaderType;
        }

        public void SetShaderType(uint value)
        {
            this.shaderType = value;
        }

        public uint GetShaderFlags1()
        {
            return this.shaderFlags1;
        }

        public void SetShaderFlags1(uint value)
        {
            this.shaderFlags1 = value;
        }

        public uint GetShaderFlags2()
        {
            return this.shaderFlags2;
        }

        public void SetShaderFlags2(uint value)
        {
            this.shaderFlags2 = value;
        }

        public UVCoord GetUVOffset()
        {
            return this.uvOffset;
        }

        public void SetUVOffset(UVCoord value)
        {
            this.uvOffset = value;
        }

        public UVCoord GetUVScale()
        {
            return this.uvScale;
        }

        public void SetUVScale(UVCoord value)
        {
            this.uvScale = value;
        }

        public int GetTextureSet()
        {
            return this.textureSet;
        }

        public void SetTextureSet(int value)
        {
            this.textureSet = value;
        }

        public Color3 GetEmissiveColor()
        {
            return this.emissiveColor;
        }

        public void SetEmissiveColor(Color3 value)
        {
            this.emissiveColor = value;
        }

        public float GetEmissiveMultiple()
        {
            return this.emissiveMultiple;
        }

        public void SetEmissiveMultiple(float value)
        {
            this.emissiveMultiple = value;
        }

        public void SetWetMaterialIndex(int value)
        {
            this.wetMaterialIdx = value;
        }

        public uint GetTextureClampMode()
        {
            return this.textureClampMode;
        }

        public void SetTextureClampMode(uint value)
        {
            this.textureClampMode = value;
        }

        public float GetAlpha()
        {
            return this.alpha;
        }

        public void SetAlpha(float value)
        {
            this.alpha = value;
        }

        public float GetUnknownFloat2()
        {
            return this.refractionStrength;
        }

        public void SetUnknownFloat2(float value)
        {
            this.refractionStrength = value;
        }

        public float GetGlossiness()
        {
            return this.glossiness;
        }

        public void SetGlossiness(float value)
        {
            this.glossiness = value;
        }

        public Color3 GetSpecularColor()
        {
            return this.specularColor;
        }

        public void SetSpecularColor(Color3 value)
        {
            this.specularColor = value;
        }

        public float GetSpecularStrength()
        {
            return this.specularStrength;
        }

        public void SetSpecularStrength(float value)
        {
            this.specularStrength = value;
        }

        public float GetLightingEffect1()
        {
            return this.lightingEffect1;
        }

        public void SetLightingEffect1(float value)
        {
            this.lightingEffect1 = value;
        }

        public float GetLightingEffect2()
        {
            return this.lightingEffect2;
        }

        public void SetLightingEffect2(float value)
        {
            this.lightingEffect2 = value;
        }

        public void SetBacklightPower(float value)
        {
            this.backlightPower = value;
        }

        public float GetBacklightPower()
        {
            return this.backlightPower;
        }

        public override bool IsDerivedType(string type)
        {
            bool flag = base.IsDerivedType(type);
            if (!flag)
                flag = type == "BSLightingShaderProperty";
            return flag;
        }
    }
}
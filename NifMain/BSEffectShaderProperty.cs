using LODGenerator.Common;
using System;
using System.IO;

namespace LODGenerator.NifMain
{
    [Serializable]
    public class BSEffectShaderProperty : NiProperty
    {
        protected uint shaderFlags1;
        protected uint shaderFlags2;
        protected UVCoord uvOffset;
        protected UVCoord uvScale;
        protected string sourceTexture;
        protected uint textureClampMode;
        protected float falloffStartAngle;
        protected float falloffStopAngle;
        protected float falloffStartOpacity;
        protected float falloffStopOpacity;
        protected Color4 emissiveColor;
        protected float emissiveMultiple;
        protected float softFalloffDepth;
        protected string greyscaleTexture;
        protected string envMapTexture;
        protected string normalTexture;
        protected string envMaskTexture;
        protected float enviromentMapScale;

        public BSEffectShaderProperty()
        {
            this.shaderFlags1 = 0U;
            this.shaderFlags2 = 0U;
            this.uvOffset = new UVCoord(0.0f, 0.0f);
            this.uvScale = new UVCoord(1f, 1f);
            this.sourceTexture = "";
            this.textureClampMode = 0U;
            this.falloffStartAngle = 0.0f;
            this.falloffStopAngle = 0.0f;
            this.falloffStartOpacity = 1f;
            this.falloffStopOpacity = 1f;
            this.emissiveColor = new Color4(0.0f, 0.0f, 0.0f, 0.0f);
            this.emissiveMultiple = 1f;
            this.softFalloffDepth = 0.0f;
            this.greyscaleTexture = "";
            this.envMapTexture = "";
            this.normalTexture = "";
            this.envMaskTexture = "";
            this.enviromentMapScale = 1f;
        }

        public override void Read(NiHeader header, BinaryReader reader)
        {
            base.Read(header, reader);
            this.shaderFlags1 = reader.ReadUInt32();
            this.shaderFlags2 = reader.ReadUInt32();
            this.uvOffset = Utils.ReadUVCoord(reader);
            this.uvScale = Utils.ReadUVCoord(reader);
            this.sourceTexture = Utils.ReadSizedString(reader);
            this.textureClampMode = reader.ReadUInt32();
            this.falloffStartAngle = reader.ReadSingle();
            this.falloffStopAngle = reader.ReadSingle();
            this.falloffStartOpacity = reader.ReadSingle();
            this.falloffStopOpacity = reader.ReadSingle();
            this.emissiveColor = Utils.ReadColor4(reader);
            this.emissiveMultiple = reader.ReadSingle();
            this.softFalloffDepth = reader.ReadSingle();
            this.greyscaleTexture = Utils.ReadSizedString(reader);
            if (header.GetVersion() == 335675399U && header.GetUserVersion2() == 130)
            {
                this.envMapTexture = Utils.ReadSizedString(reader);
                this.normalTexture = Utils.ReadSizedString(reader);
                this.envMaskTexture = Utils.ReadSizedString(reader);
                this.enviromentMapScale = reader.ReadSingle();
            }
        }

        public override void Write(NiHeader header, BinaryWriter writer)
        {
            base.Write(header, writer);
            writer.Write(this.shaderFlags1);
            writer.Write(this.shaderFlags2);
            Utils.WriteUVCoord(writer, this.uvOffset);
            Utils.WriteUVCoord(writer, this.uvScale);
            Utils.WriteSizedString(writer, this.sourceTexture);
            writer.Write(this.textureClampMode);
            writer.Write(this.falloffStartAngle);
            writer.Write(this.falloffStopAngle);
            writer.Write(this.falloffStartOpacity);
            writer.Write(this.falloffStopOpacity);
            Utils.WriteColor4(writer, this.emissiveColor);
            writer.Write(this.emissiveMultiple);
            writer.Write(this.softFalloffDepth);
            Utils.WriteSizedString(writer, this.greyscaleTexture);
        }

        public override string GetClassName()
        {
            return "BSEffectShaderProperty";
        }

        public override uint GetSize(NiHeader header)
        {
            uint num = base.GetSize(header) + 68U;
            num += (uint)(4 + this.sourceTexture.Length);
            num += (uint)(4 + this.greyscaleTexture.Length);
            return num;
        }

        public string GetSourceTexture()
        {
            return this.sourceTexture;
        }

        public void SetSourceTexture(string value)
        {
            this.sourceTexture = value; 
        }

        public override bool IsDerivedType(string type)
        {
            bool flag = base.IsDerivedType(type);
            if (!flag)
                flag = type == "BSEffectShaderProperty";
            return flag;
        }

        // get ShaderFlags2;
        public uint GetShaderFlags1()
        {
            return this.shaderFlags1;
        }

        public void SetShaderFlags1(uint value)
        {
            this.shaderFlags1 = value;
        }

        // get ShaderFlags2;
        public uint GetShaderFlags2()
        {
            return this.shaderFlags2;
        }

        public void SetShaderFlags2(uint value)
        {
            this.shaderFlags2 = value;
        }

        // get ClampMode;
        public uint GetTextureClampMode()
        {
            return this.textureClampMode;
        }

        public void SetTextureClampMode(uint value)
        {
            this.textureClampMode = value;
        }

        public Color4 GetEmissiveColor()
        {
            return this.emissiveColor;
        }

        public float GetEmissiveMultiple()
        {
            return this.emissiveMultiple;
        }

    }
}

using LODGenerator.Common;
using System;
using System.IO;

namespace LODGenerator.NifMain
{
    [Serializable]
    public class NiSourceTexture : NiTexture
    {
        protected byte useExternal;
        protected string fileName;
        protected int unknownLink;
        protected int pixelData;
        protected uint pixelLayout;
        protected uint useMipMaps;
        protected uint alphaFormat;
        protected byte isStatic;
        protected bool directRender;

        public NiSourceTexture()
        {
            this.useExternal = (byte)0;
            this.fileName = "";
            this.unknownLink = -1;
            this.pixelData = -1;
            this.pixelLayout = 0U;
            this.useMipMaps = 0U;
            this.alphaFormat = 0U;
            this.isStatic = (byte)0;
            this.directRender = false;
        }

        public override void Read(NiHeader header, BinaryReader reader)
        {
            base.Read(header, reader);
            this.useExternal = reader.ReadByte();
            if ((int)this.useExternal == 1)
            {
                this.fileName = Utils.ReadSizedString(reader);
                this.unknownLink = reader.ReadInt32();
            }
            else
            {
                this.fileName = Utils.ReadSizedString(reader);
                this.pixelData = reader.ReadInt32();
            }
            this.pixelLayout = reader.ReadUInt32();
            this.useMipMaps = reader.ReadUInt32();
            this.alphaFormat = reader.ReadUInt32();
            this.isStatic = reader.ReadByte();
            this.directRender = Utils.ReadBool(reader);
        }

        public override void Write(NiHeader header, BinaryWriter writer)
        {
            base.Write(header, writer);
            writer.Write(this.useExternal);
            if ((int)this.useExternal == 1)
            {
                Utils.WriteSizedString(writer, this.fileName);
                writer.Write(this.unknownLink);
            }
            else
            {
                Utils.WriteSizedString(writer, this.fileName);
                writer.Write(this.pixelData);
            }
            writer.Write(this.pixelLayout);
            writer.Write(this.useMipMaps);
            writer.Write(this.alphaFormat);
            writer.Write(this.isStatic);
            Utils.WriteBool(writer, this.directRender);
        }

        public override uint GetSize(NiHeader header)
        {
            //todo 
            return base.GetSize(header);
        }

        public string GetFileName()
        {
            return this.fileName;
        }

        public void SetFileName(string value)
        {
            this.fileName = value;
        }

        public override string GetClassName()
        {
            return "NiSourceTexture";
        }

        public override bool IsDerivedType(string type)
        {
            bool flag = base.IsDerivedType(type);
            if (!flag)
                flag = type == "NiSourceTexture";
            return flag;
        }
    }
}
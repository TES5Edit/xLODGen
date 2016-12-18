using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Runtime.Serialization;
using System.Security.Cryptography;
using System.Text;
using DelaunayTriangulator;

namespace LODGenerator.Common
{
    public static class Game
    {
        static string _mode;
        static float _sampleSize;
        static float _atlastoleranceMin;
        static float _atlastoleranceMax;

        public static string Mode
        {
            get
            {
                return _mode;
            }
            set
            {
                _mode = value;
            }
        }

        public static float sampleSize
        {
            get
            {
                return _sampleSize;
            }
            set
            {
                _sampleSize = value;
            }
        }

        public static float atlasToleranceMin
        {
            get
            {
                return _atlastoleranceMin;
            }
            set
            {
                _atlastoleranceMin = value;
            }
        }

        public static float atlasToleranceMax
        {
            get
            {
                return _atlastoleranceMax;
            }
            set
            {
                _atlastoleranceMax = value;
            }
        }
    }

    public static class Utils
    {
        public static float ByteToFloat(byte b)
        {
            return (float)b / 255 * 2 - 1;
        }

        public static byte FloatToByte(float f)
        {
            return (byte)Math.Round((f + 1) / 2 * 255, MidpointRounding.AwayFromZero);
        }

        public static byte FloatToUByte(float f)
        {
            double f2 = Math.Max(0.0, Math.Min(1.0, f));
            return (byte)Math.Floor(f2 == 1.0 ? 255 : f2 * 256.0);
        }

        public static float ShortToFloat(short value)
        {
            int mant = value & 0x03ff;
            int exp = value & 0x7c00;
            if (exp == 0x7c00)
            {
                exp = 0x3fc00;
            }
            else if (exp != 0)
            {
                exp += 0x1c000;
            }
            else if (mant != 0)
            {
                exp = 0x1c400; 
                do
                {
                    mant <<= 1;
                    exp -= 0x400;
                } while ((mant & 0x400) == 0);
                mant &= 0x3ff;
            }
            return BitConverter.ToSingle(BitConverter.GetBytes((value & 0x8000) << 16 | (exp | mant) << 13), 0);
        }

        public static short FloatToShort(float fval)
        {
            int fbits = BitConverter.ToInt32(BitConverter.GetBytes(fval), 0);
            int sign = fbits >> 16 & 0x8000;
            int val = (fbits & 0x7fffffff) + 0x1000;

            if (val >= 0x47800000)
            {
                if ((fbits & 0x7fffffff) >= 0x47800000)
                {
                    if (val < 0x7f800000)
                    {
                        return (short)(sign | 0x7c00);
                    }
                    return (short)(sign | 0x7c00 | (fbits & 0x007fffff) >> 13);
                }
                return (short)(sign | 0x7bff);
            }
            if (val >= 0x38800000)
            {
                return (short)(sign | val - 0x38000000 >> 13);
            }
            if (val < 0x33000000)
            {
                return (short)sign;
            }
            val = (fbits & 0x7fffffff) >> 23;
            return (short)(sign | ((fbits & 0x7fffff | 0x800000) + (0x800000 >> val - 102) >> 126 - val));
        }

        public static string ReadShortString(BinaryReader NifReader)
        {
            byte num = NifReader.ReadByte();
            string str = "";
            for (int index = 0; index < (int)num; ++index)
            {
                str = str + (object)(char)NifReader.ReadByte();
            }
            return str;
        }

        public static void WriteShortString(BinaryWriter writer, string value)
        {
            writer.Write((byte)value.Length);
            for (int index = 0; index < value.Length; ++index)
            {
                writer.Write((byte)Enumerable.ElementAt<char>((IEnumerable<char>)value, index));
            }
        }

        public static void WriteHeaderString(BinaryWriter writer, string value)
        {
            writer.Write((byte)(value.Length + 1));
            for (int index = 0; index < value.Length; ++index)
            {
                writer.Write((byte)Enumerable.ElementAt<char>((IEnumerable<char>)value, index));
            }
            writer.Write((byte)0);
        }

        public static string ReadSizedString(BinaryReader NifReader)
        {
            uint num = NifReader.ReadUInt32();
            string str = "";
            for (int index = 0; (long)index < (long)num; ++index)
            {
                str = str + (object)(char)NifReader.ReadByte();
            }
            return str;
        }

        public static void ReadString(BinaryReader NifReader, uint version, out int StringIdx, out string StringValue)
        {
            StringIdx = -1;
            StringValue = (string)null;
            if (version > 335544325U)
            {
                StringIdx = NifReader.ReadInt32();
            }
            else
            {
                StringValue = Utils.ReadSizedString(NifReader);
            }
        }

        public static void WriteSizedString(BinaryWriter writer, string value)
        {
            writer.Write((uint)value.Length);
            for (int index = 0; index < value.Length; ++index)
            {
                writer.Write((byte)Enumerable.ElementAt<char>((IEnumerable<char>)value, index));
            }
        }

        public static Vector3 ReadVector3(BinaryReader NifReader)
        {
            return new Vector3(NifReader.ReadSingle(), NifReader.ReadSingle(), NifReader.ReadSingle());
        }

        public static void WriteVector3(BinaryWriter writer, Vector3 value)
        {
            writer.Write(value[0]);
            writer.Write(value[1]);
            writer.Write(value[2]);
        }

        public static Vector4 ReadVector4(BinaryReader NifReader)
        {
            return new Vector4(NifReader.ReadSingle(), NifReader.ReadSingle(), NifReader.ReadSingle(), NifReader.ReadSingle());
        }

        public static void WriteVector4(BinaryWriter writer, Vector4 value)
        {
            writer.Write(value[0]);
            writer.Write(value[1]);
            writer.Write(value[2]);
            writer.Write(value[3]);
        }

        public static Matrix33 ReadMatrix33(BinaryReader NifReader)
        {
            return new Matrix33(NifReader.ReadSingle(), NifReader.ReadSingle(), NifReader.ReadSingle(), NifReader.ReadSingle(), NifReader.ReadSingle(), NifReader.ReadSingle(), NifReader.ReadSingle(), NifReader.ReadSingle(), NifReader.ReadSingle());
        }

        public static void WriteMatrix33(BinaryWriter writer, Matrix33 value)
        {
            for (int index1 = 0; index1 < 3; ++index1)
            {
                for (int index2 = 0; index2 < 3; ++index2)
                    writer.Write(value[index1][index2]);
            }
        }

        public static bool ReadBool(BinaryReader NifReader)
        {
            return (int)NifReader.ReadByte() != 0;
        }

        public static void WriteBool(BinaryWriter writer, bool value)
        {
            writer.Write(value ? (byte)1 : (byte)0);
        }

        public static Color4 ReadColor4(BinaryReader NifReader)
        {
            return new Color4(NifReader.ReadSingle(), NifReader.ReadSingle(), NifReader.ReadSingle(), NifReader.ReadSingle());
        }

        public static void WriteColor4(BinaryWriter writer, Color4 value)
        {
            writer.Write(value[0]);
            writer.Write(value[1]);
            writer.Write(value[2]);
            writer.Write(value[3]);
        }

        public static Color3 ReadColor3(BinaryReader NifReader)
        {
            return new Color3(NifReader.ReadSingle(), NifReader.ReadSingle(), NifReader.ReadSingle());
        }

        public static void WriteColor3(BinaryWriter writer, Color3 value)
        {
            writer.Write(value[0]);
            writer.Write(value[1]);
            writer.Write(value[2]);
        }

        public static UVCoord ReadUVCoord(BinaryReader NifReader)
        {
            return new UVCoord(NifReader.ReadSingle(), NifReader.ReadSingle());
        }

        public static void WriteUVCoord(BinaryWriter writer, UVCoord value)
        {
            writer.Write(value[0]);
            writer.Write(value[1]);
        }

        public static Triangle ReadTriangle(BinaryReader NifReader)
        {
            return new Triangle(NifReader.ReadUInt16(), NifReader.ReadUInt16(), NifReader.ReadUInt16());
        }

        public static void WriteTriangle(BinaryWriter writer, Triangle value)
        {
            writer.Write(value[0]);
            writer.Write(value[1]);
            writer.Write(value[2]);
        }

        public static TexDesc ReadTexDesc(BinaryReader NifReader)
        {
            TexDesc texDesc = new TexDesc();
            texDesc.source = NifReader.ReadInt32();
            texDesc.clampMode = NifReader.ReadUInt32();
            texDesc.filterMode = NifReader.ReadUInt32();
            texDesc.uvSet = NifReader.ReadUInt32();
            texDesc.hasTextureTransform = Utils.ReadBool(NifReader);
            if (texDesc.hasTextureTransform)
            {
                texDesc.translation = Utils.ReadUVCoord(NifReader);
                texDesc.tiling = Utils.ReadUVCoord(NifReader);
                texDesc.wRotation = NifReader.ReadSingle();
                texDesc.transformType = NifReader.ReadUInt32();
                texDesc.centerOffset = Utils.ReadUVCoord(NifReader);
            }
            return texDesc;
        }

        public static void WriteTexDesc(BinaryWriter writer, TexDesc texDesc)
        {
            writer.Write(texDesc.source);
            writer.Write(texDesc.clampMode);
            writer.Write(texDesc.filterMode);
            writer.Write(texDesc.uvSet);
            Utils.WriteBool(writer, texDesc.hasTextureTransform);
            if (texDesc.hasTextureTransform)
            {
                Utils.WriteUVCoord(writer, texDesc.translation);
                Utils.WriteUVCoord(writer, texDesc.tiling);
                writer.Write(texDesc.wRotation);
                writer.Write(texDesc.transformType);
                Utils.WriteUVCoord(writer, texDesc.centerOffset);
            }
        }

        public static float ToRadians(float val)
        {
            return (float)(val * Math.PI / 180.0);
        }

        public static bool PointInTriangle(UVCoord p, UVCoord t1, UVCoord t2, UVCoord t3, out float u, out float v)
        {
            UVCoord vector2_1 = t3 - t1;
            UVCoord vector2_2 = t2 - t1;
            UVCoord B = p - t1;
            float num1 = UVCoord.Dot(vector2_1, vector2_1);
            float num2 = UVCoord.Dot(vector2_1, vector2_2);
            float num3 = UVCoord.Dot(vector2_1, B);
            float num4 = UVCoord.Dot(vector2_2, vector2_2);
            float num5 = UVCoord.Dot(vector2_2, B);
            float num6 = (float)(1.0 / ((double)num1 * (double)num4 - (double)num2 * (double)num2));
            u = (float)((double)num4 * (double)num3 - (double)num2 * (double)num5) * num6;
            v = (float)((double)num1 * (double)num5 - (double)num2 * (double)num3) * num6;
            if ((double)u >= 0.0 && (double)v >= 0.0)
            {
                return (double)u + (double)v <= 1;
            }
            else
            {
                return false;
            }
        }

        public static bool PointInTriangle(Vector2 p, Vector2 t1, Vector2 t2, Vector2 t3, out float u, out float v)
        {
            Vector2 vector2_1 = t3 - t1;
            Vector2 vector2_2 = t2 - t1;
            Vector2 B = p - t1;
            float num1 = Vector2.Dot(vector2_1, vector2_1);
            float num2 = Vector2.Dot(vector2_1, vector2_2);
            float num3 = Vector2.Dot(vector2_1, B);
            float num4 = Vector2.Dot(vector2_2, vector2_2);
            float num5 = Vector2.Dot(vector2_2, B);
            float num6 = (float)(1.0 / ((double)num1 * (double)num4 - (double)num2 * (double)num2));
            u = (float)((double)num4 * (double)num3 - (double)num2 * (double)num5) * num6;
            v = (float)((double)num1 * (double)num5 - (double)num2 * (double)num3) * num6;
            if ((double)u >= 0.0 && (double)v >= 0.0)
            {
                return (double)u + (double)v <= 1.0;
            }
            else
            {
                return false;
            }
        }

        //                    Math.Round(1.1, 0)

        public static float xQUV(float value)
        {
            if (value < 0)
            {
                return (float)Math.Ceiling(value * 100) / 100;
            }
            else
            {
                return (float)Math.Floor(value * 100) / 100;
            }
        }

        public static Int64 QUV(float value)
        {
            if (value < 0)
            {
                return (Int64)Math.Ceiling(value * 8192);
            }
            else
            {
                return (Int64)Math.Floor(value * 8192);
            }
        }

        public static Int64 QHigh(float value)
        {
            if (value < 0)
            {
                return (Int64)Math.Ceiling(value * 100); // + 0.5) / 1000;
            }
            else
            {
                return (Int64)Math.Floor(value * 100); // + 0.5) / 1000;
            }
        }

        public static Int64 QLow(double value)
        {
            if (value < 0)
            {
                return (Int64)Math.Ceiling(value * 10); // + 0.5) / 1000;
            }
            else
            {
                return (Int64)Math.Floor(value * 10); // + 0.5) / 1000;
            }
        }

        public static string GetDiffuseTextureName(string value)
        {
            return value.Replace("_d.dds", ".dds");
        }

        public static string GetNormalTextureName(string value)
        {
            return Utils.GetDiffuseTextureName(value).Replace(".dds", "_n.dds");
        }

        public static string GetSpecularTextureName(string value)
        {
            return Utils.GetDiffuseTextureName(value).Replace(".dds", "_s.dds");
        }

        public static FileStream GetFileStream(FileInfo file, LogFile logFile)
        {
            try
            {
                return file.Open(FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
            }
            catch (IOException ex)
            {
                logFile.WriteLog(ex.Message);
                logFile.Close();
                System.Environment.Exit(404);
                return (FileStream)null;
            }
        }

        // Convert an object to a byte array
        public static byte[] ObjectToByteArray(Object obj)
        {
            if (obj == null)
                return null;
            BinaryFormatter bf = new BinaryFormatter();
            MemoryStream ms = new MemoryStream();
            bf.Serialize(ms, obj);
            return ms.ToArray();
        }

        public static string GetHash(byte[] objectAsBytes)
        {
            SHA512CryptoServiceProvider md5 = new SHA512CryptoServiceProvider();
            try
            {
                byte[] result = md5.ComputeHash(objectAsBytes);

                // Build the final string by converting each byte
                // into hex and appending it to a StringBuilder
                StringBuilder sb = new StringBuilder();
                for (int i = 0; i < result.Length; i++)
                {
                    sb.Append(result[i].ToString("X2"));
                }

                // And return it
                return sb.ToString();
            }
            catch (ArgumentNullException ane)
            {
                //If something occurred during serialization, 
                //this method is called with a null argument. 
                Console.WriteLine("Hash has not been generated. " + ane);
                return null;
            }
        }
    }
}

using LODGenerator.Common;
using System;
using System.Collections.Generic;
using System.IO;

namespace LODGenerator.NifMain
{
    [Serializable]
    public class BSTriShape : NiAVObject
    {
        protected Vector3 center;
        protected float radius;
        protected int skinInstance;
        protected int[] bsProperties;
        protected byte vertexSize;
        protected byte floatSize;
        protected byte vertexFlag3;
        protected byte vertexFlag4;
        protected byte vertexFlag5;
        protected ushort vertexFlags;
        protected byte vertexFlag8;
        protected uint numTriangles;
        protected ushort numVertices;
        protected uint dataSize;
        protected List<BSVertexData> vertexData;
        protected List<Triangle> triangles;
        protected List<Vector3> vertices;
        protected List<Vector3> normals;
        protected List<Vector3> tangents;
        protected List<Vector3> bitangents;
        protected List<Color4> vertexColors;
        protected List<UVCoord> uvCoords;
        protected uint particleDataSize;

        public BSTriShape()
        {
            this.center = new Vector3(0, 0, 0);
            this.radius = 0;
            this.skinInstance = -1;
            this.bsProperties = new int[2];
            this.bsProperties[0] = -1;
            this.bsProperties[1] = -1;
            this.vertexSize = 5;
            this.floatSize = 2;
            this.vertexFlag3 = 67;
            this.vertexFlag4 = 0;
            this.vertexFlag5 = 0;
            this.vertexFlags = 432;
            //this.vertexFlag7 = 1;
            this.vertexFlag8 = 0;
            this.numTriangles = 0;
            this.numVertices = 0;
            this.dataSize = 0;
            this.vertexData = new List<BSVertexData>();
            this.triangles = new List<Triangle>();
            this.vertices = new List<Vector3>();
            this.normals = new List<Vector3>();
            this.tangents = new List<Vector3>();
            this.bitangents = new List<Vector3>();
            this.vertexColors = new List<Color4>();
            this.uvCoords = new List<UVCoord>();
            particleDataSize = 0;
        }

        public BSTriShape(NiTriBasedGeom shape)
        {
            this.name = shape.GetName();
            this.nameIdx = shape.GetNameIndex();
            this.numExtraData = shape.GetNumExtraData();
            this.extraData = shape.GetExtraData();
            this.controller = shape.GetController();
            this.flags = shape.GetFlags();
            this.flags2 = shape.GetFlags2();
            this.translation = shape.GetTranslation();
            this.rotation = shape.GetRotation();
            this.scale = shape.GetScale();
            this.collisionObject = shape.GetCollisionObject();

            this.center = new Vector3(0, 0, 0);
            this.radius = 0;

            this.skinInstance = shape.GetSkinInstance();
            this.bsProperties = new int[2];
            for (int index = 0; index < 2; ++index)
            {
                this.bsProperties[index] = shape.GetBSProperty(index);
            }

            this.vertexSize = 8;
            this.floatSize = 4;
            this.vertexFlag3 = 101;
            this.vertexFlag4 = 7;
            this.vertexFlag5 = 0;
            this.vertexFlags = 0;
            this.vertexFlag8 = 0;
            this.numTriangles = 0;
            this.numVertices = 0;
            this.dataSize = 0;
            this.vertexData = new List<BSVertexData>();
            this.triangles = new List<Triangle>();
            this.vertices = new List<Vector3>();
            this.normals = new List<Vector3>();
            this.tangents = new List<Vector3>();
            this.bitangents = new List<Vector3>();
            this.vertexColors = new List<Color4>();
            this.uvCoords = new List<UVCoord>();
            particleDataSize = 0;
       }

        public override void Read(NiHeader header, BinaryReader reader)
        {
            base.Read(header, reader);
            this.center = Utils.ReadVector3(reader);
            this.radius = reader.ReadSingle();
            this.skinInstance = reader.ReadInt32();
            this.bsProperties[0] = reader.ReadInt32();
            this.bsProperties[1] = reader.ReadInt32();
            this.vertexSize = reader.ReadByte();
            this.floatSize = reader.ReadByte();
            this.vertexFlag3 = reader.ReadByte();
            this.vertexFlag4 = reader.ReadByte();
            this.vertexFlag5 = reader.ReadByte();
            this.vertexFlags = reader.ReadUInt16();
            // 16 = vertex
            // 32 = uv
            // 128 = normals
            // 256 = tangents
            // 512 = vertex colors
            // 1024 = skinned
            // 4096 = male eyes
            // 16384 = full precision

            this.vertexFlag8 = reader.ReadByte();
            if (header.GetUserVersion2() == 100)
            {
                this.numTriangles = reader.ReadUInt16();
            }
            else
            {
                this.numTriangles = reader.ReadUInt32();
            }
            this.numVertices = reader.ReadUInt16();
            this.dataSize = reader.ReadUInt32();

            if (this.dataSize > 0)
            {
                for (int index = 0; index < numVertices; index++)
                {
                    BSVertexData vd = new BSVertexData();
                    if ((this.vertexFlags & 16) == 16)
                    {
                        if (header.GetUserVersion2() == 100 || (this.vertexFlags & 16384) == 16384)
                        {
                            vd.vertex = Utils.ReadVector3(reader);
                            vd.bitangentX = reader.ReadSingle();
                        }
                        else
                        {
                            vd.vertex = new Vector3(Utils.ShortToFloat(reader.ReadInt16()), Utils.ShortToFloat(reader.ReadInt16()), Utils.ShortToFloat(reader.ReadInt16()));
                            vd.bitangentX = Utils.ShortToFloat(reader.ReadInt16());
                        }
                    }
                    if ((this.vertexFlags & 32) == 32)
                    {
                        vd.uvcoords = new UVCoord(Utils.ShortToFloat(reader.ReadInt16()), Utils.ShortToFloat(reader.ReadInt16()));
                    }
                    if ((this.vertexFlags & 128) == 128)
                    {
                        vd.normal = new Vector3(Utils.ByteToFloat(reader.ReadByte()), Utils.ByteToFloat(reader.ReadByte()), Utils.ByteToFloat(reader.ReadByte()));
                        vd.bitangentY = Utils.ByteToFloat(reader.ReadByte());
                    }
                    if ((this.vertexFlags & 256) == 256)
                    {
                        vd.tangent = new Vector3(Utils.ByteToFloat(reader.ReadByte()), Utils.ByteToFloat(reader.ReadByte()), Utils.ByteToFloat(reader.ReadByte()));
                        vd.bitangentZ = Utils.ByteToFloat(reader.ReadByte());
                    }
                    if ((this.vertexFlags & 512) == 512)
                    {
                        vd.vertexColors = new Color4((float)reader.ReadByte() / 255, (float)reader.ReadByte() / 255, (float)reader.ReadByte() / 255, (float)reader.ReadByte() / 255);
                    }
                    if ((this.vertexFlags & 1024) == 1024)
                    {
                        vd.boneWeights[0] = Utils.ShortToFloat(reader.ReadInt16());
                        vd.boneWeights[1] = Utils.ShortToFloat(reader.ReadInt16());
                        vd.boneWeights[2] = Utils.ShortToFloat(reader.ReadInt16());
                        vd.boneWeights[3] = Utils.ShortToFloat(reader.ReadInt16());
                        vd.boneIndices[0] = reader.ReadByte();
                        vd.boneIndices[1] = reader.ReadByte();
                        vd.boneIndices[2] = reader.ReadByte();
                        vd.boneIndices[3] = reader.ReadByte();
                    }
                    if ((this.vertexFlags & 4096) == 4096)
                    {
                        reader.ReadInt32();
                    }

                    this.vertexData.Add(vd);
                    this.vertices.Add(vd.vertex);
                    this.normals.Add(vd.normal);
                    this.tangents.Add(vd.tangent);
                    this.bitangents.Add(new Vector3(vd.bitangentX, vd.bitangentY, vd.bitangentZ));
                    this.vertexColors.Add(vd.vertexColors);
                    this.uvCoords.Add(vd.uvcoords);
                }
                for (int index = 0; index < numTriangles; index++)
                {
                    this.triangles.Add(Utils.ReadTriangle(reader));
                }

                if (header.GetUserVersion2() == 100)
                {
                    particleDataSize = reader.ReadUInt32();
                }

                /*for (int index = 0; index < numVertices; index++)
                {
                    Console.WriteLine("Vertex ["+ index + "] = " + vertices[index]);
                }*/
                /*for (int index = 0; index < normals.Count; index++)
                {
                    Console.WriteLine("Normal ["+ index + "] = " + normals[index]);
                }
                for (int index = 0; index < bitangents.Count; index++)
                {
                    Console.WriteLine("Bitang [" + index + "] = " + bitangents[index]);
                }
                for (int index = 0; index < vertexColors.Count; index++)
                {
                    Console.WriteLine("vertexColors [" + index + "] = " + vertexColors[index]);
                }
                for (int index = 0; index < numTriangles; index++)
                {
                    Console.WriteLine("Triangle [" + index + "] = " + triangles[index]);
                }*/
            }
        }

        public override void Write(NiHeader header, BinaryWriter writer)
        {
            List<int> blockReferences = header.GetBlockReferences();
            if (blockReferences.Count > 0)
            {
                if (this.skinInstance != -1)
                {
                    this.skinInstance = blockReferences[this.skinInstance];
                }
                if (this.bsProperties[0] != -1)
                {
                    this.bsProperties[0] = blockReferences[this.bsProperties[0]];
                }
                if (this.bsProperties[1] != -1)
                {
                    this.bsProperties[1] = blockReferences[this.bsProperties[1]];
                }
            }
            base.Write(header, writer);
            Utils.WriteVector3(writer, this.center);
            writer.Write(this.radius);
            writer.Write(this.skinInstance);
            writer.Write(this.bsProperties[0]);
            writer.Write(this.bsProperties[1]);
            this.vertexSize = 0;
            this.vertexFlags = 0;
            // 16 = vertex
            // 32 = uv
            // 128 = normals
            // 256 = tangents
            // 512 = vertex colors
            // 1024 = skinned
            // 4096 = male eyes
            // 16384 = full precision
            if (this.vertices.Count > 0)
            {
                this.vertexFlags |= 16;
                this.vertexSize += 2;
            }
            if (this.uvCoords.Count >= 0)
            {
                this.vertexFlags |= 32;
                this.vertexSize++;
            }
            if (this.normals.Count > 0)
            {
                this.vertexFlags |= 128;
                this.vertexSize += 2;
            }
            if (this.tangents.Count > 0)
            {
                this.vertexFlags |= 256;
                this.vertexSize += 2;
            }
            if (this.vertexColors.Count > 0)
            {
                this.vertexFlags |= 512;
                this.vertexSize++;
                this.vertexFlag4 = 7;
            }
            writer.Write(this.vertexSize);
            if (((this.vertexFlags & 16384) == 16384) || (header.GetUserVersion2() == 100))
            {
                this.floatSize = 4;
            }
            writer.Write(this.floatSize);
            writer.Write(this.vertexFlag3);
            writer.Write(this.vertexFlag4);
            writer.Write(this.vertexFlag5);
            writer.Write(this.vertexFlags);
            writer.Write(this.vertexFlag8);
            if (header.GetUserVersion2() == 100)
            {
                writer.Write((ushort)this.numTriangles);
            }
            else
            {
                writer.Write(this.numTriangles);
            }
            writer.Write(this.numVertices);
            writer.Write(GetDataSize(header));

            if (this.dataSize > 0)
            {
                for (int index = 0; index < this.numVertices; index++)
                {
                    if ((this.vertexFlags & 16) == 16)
                    {
                        if (header.GetUserVersion2() == 100 || (this.vertexFlags & 16384) == 16384)
                        {
                            writer.Write(this.vertices[index][0]);
                            writer.Write(this.vertices[index][1]);
                            writer.Write(this.vertices[index][2]);
                            if (this.bitangents.Count > 0)
                            {
                                writer.Write(this.bitangents[index][0]);
                            }
                            else
                            {
                                writer.Write((uint)0);
                            }

                        }
                        else
                        {
                            writer.Write(Utils.FloatToShort(this.vertices[index][0]));
                            writer.Write(Utils.FloatToShort(this.vertices[index][1]));
                            writer.Write(Utils.FloatToShort(this.vertices[index][2]));
                            if (this.bitangents.Count > 0)
                            {
                                writer.Write(Utils.FloatToShort(this.bitangents[index][0]));
                            }
                            else
                            {
                                writer.Write((ushort)0);
                            }

                        }
                    }
                    if ((this.vertexFlags & 32) == 32)
                    {
                        writer.Write(Utils.FloatToShort(this.uvCoords[index][0]));
                        writer.Write(Utils.FloatToShort(this.uvCoords[index][1]));
                    }
                    if ((this.vertexFlags & 128) == 128)
                    {
                        writer.Write(Utils.FloatToByte(this.normals[index][0]));
                        writer.Write(Utils.FloatToByte(this.normals[index][1]));
                        writer.Write(Utils.FloatToByte(this.normals[index][2]));
                        if (this.bitangents.Count > 0)
                        {
                            writer.Write(Utils.FloatToByte(this.bitangents[index][1]));
                        }
                        else
                        {
                            writer.Write((byte)0);
                        }
                    }
                    if ((this.vertexFlags & 256) == 256)
                    {
                        writer.Write(Utils.FloatToByte(this.tangents[index][0]));
                        writer.Write(Utils.FloatToByte(this.tangents[index][1]));
                        writer.Write(Utils.FloatToByte(this.tangents[index][2]));
                        if (this.bitangents.Count > 0)
                        {
                            writer.Write(Utils.FloatToByte(this.bitangents[index][2]));
                        }
                        else
                        {
                            writer.Write((byte)0);
                        }
                    }
                    if ((this.vertexFlags & 512) == 512)
                    {
                        writer.Write((byte)(this.vertexColors[index][0] * 255));
                        writer.Write((byte)(this.vertexColors[index][1] * 255));
                        writer.Write((byte)(this.vertexColors[index][2] * 255));
                        writer.Write((byte)(this.vertexColors[index][3] * 255));
                    }
                    if ((this.vertexFlags & 1024) == 1024)
                    {
                        Console.WriteLine("boneweight not supported yet");
                        System.Environment.Exit(667);
                    }
                    if ((this.vertexFlags & 4096) == 4096)
                    {
                        writer.Write((uint)0);
                    }
                }
            }
            for (int index = 0; index < this.triangles.Count; ++index)
            {
                Utils.WriteTriangle(writer, this.triangles[index]);
            }
            if (header.GetUserVersion2() == 100)
            {
                writer.Write(particleDataSize);
            }
        }

        private uint GetDataSize(NiHeader header)
        {
            uint numV = 0;
            if (this.vertices.Count > 0)
            {
                if (header.GetUserVersion2() == 100 || (this.vertexFlags & 16384) == 16384)
                {
                    numV += 16;
                }
                else
                {
                    numV += 8;
                }
            }
            if (this.uvCoords.Count >= 0)
            {
                numV += 4;
            }
            if (this.normals.Count > 0)
            {
                numV += 4;
            }
            if (this.tangents.Count > 0)
            {
                numV += 4;
            }
            if (this.vertexColors.Count > 0)
            {
                numV += 4;
            }
            if ((this.vertexFlags & 1024) == 1024)
            {
                numV += 12;
            }
            if ((this.vertexFlags & 4096) == 4096)
            {
                numV += 4;
            }
            return (uint)(this.numVertices * numV) + (this.numTriangles * 6);
        }

        public override uint GetSize(NiHeader header)
        {
            uint numH = base.GetSize(header) + 28;
            if (header.GetUserVersion2() == 100)
            {
                numH += 20;
            }
            else
            {
                numH += 18;
            }
            return (uint) numH + GetDataSize(header);
        }

        public void UpdateVertexData()
        {
            this.vertexData.Clear();
            for (int index = 0; index < this.vertices.Count; index++)
            {
                BSVertexData vd = new BSVertexData();
                vd.vertex = new Vector3(this.vertices[index][0], this.vertices[index][1], this.vertices[index][2]);
                if (this.uvCoords.Count != 0)
                {
                    vd.uvcoords = new UVCoord(this.uvCoords[index][0], this.uvCoords[index][1]);
                }
                if (this.normals.Count != 0)
                {
                    vd.normal = new Vector3(this.normals[index][0], this.normals[index][1], this.normals[index][2]);
                }
                if (this.tangents.Count != 0)
                {
                    vd.tangent = new Vector3(this.tangents[index][0], this.tangents[index][1], this.tangents[index][2]);
                    vd.bitangentY = this.bitangents[index][1];
                    vd.bitangentX = this.bitangents[index][0];
                    vd.bitangentZ = this.bitangents[index][2];
                }
                if (this.vertexColors.Count != 0)
                {
                    vd.vertexColors = new Color4(this.vertexColors[index][0], this.vertexColors[index][1], this.vertexColors[index][2], this.vertexColors[index][3]);
                }
                this.vertexData.Add(vd);
                this.numVertices = (ushort)this.vertices.Count;
                this.numTriangles = (uint)this.triangles.Count;
            }
        }

        public void SetCenter(Vector3 value)
        {
            this.center = value;
        }

        public Vector3 GetCenter()
        {
            return this.center;
        }

        public void SetRadius(float value)
        {
            this.radius = value;
        }

        public int GetBSProperty(int index)
        {
            return this.bsProperties[index];
        }

        public void SetBSProperty(int index, int value)
        {
            this.bsProperties[index] = value;
        }

        public void SetVertexSize(byte value)
        {
            this.vertexSize = value;
        }

        public void SetFloatSize(byte value)
        {
            this.floatSize = value;
        }

        public void SetVF3(byte value)
        {
            this.vertexFlag3 = value;
        }

        public void SetVF4(byte value)
        {
            this.vertexFlag4 = value;
        }

        public void SetVF5(byte value)
        {
            this.vertexFlag5 = value;
        }

        public void SetVF8(byte value)
        {
            this.vertexFlag8 = value;
        }

        public void SetVertexFlags(ushort value)
        {
            this.vertexFlags = value;
        }

        public void SetNumTriangles(uint value)
        {
            this.numTriangles = value;
        }

        public uint GetNumTriangles()
        {
            return this.numTriangles;
        }

        public void SetNumVertices(ushort value)
        {
            this.numVertices = value;
        }

        public void SetDataSize(uint value)
        {
            this.dataSize = value;
        }

        public void SetVertices(List<Vector3> v)
        {
            this.vertices = v;
        }

        public List<Vector3> GetVertices()
        {
            return this.vertices;
        }

        public void SetNormals(List<Vector3> v)
        {
            this.normals = v;
        }

        public List<Vector3> GetNormals()
        {
            return this.normals;
        }

        public void SetTangents(List<Vector3> v)
        {
            this.tangents = v;
        }

        public List<Vector3> GetTangents()
        {
            return this.tangents;
        }

        public void SetBitangents(List<Vector3> v)
        {
            this.bitangents = v;
        }

        public List<Vector3> GetBitangents()
        {
            return this.bitangents;
        }

        public void SetVertexColors(List<Color4> v)
        {
            this.vertexColors = v;
        }

        public List<Color4> GetVertexColors()
        {
            return this.vertexColors;
        }

        public void SetUVCoords(List<UVCoord> v)
        {
            this.uvCoords = v;
        }

        public List<UVCoord> GetUVCoords()
        {
            return this.uvCoords;
        }

        public void SetTriangles(List<Triangle> tris)
        {
            this.triangles = tris;
        }

        public List<Triangle> GetTriangles()
        {
            return this.triangles;
        }

        public void SetGeom(Geometry geom)
        {
            this.triangles = geom.GetTriangles();
            this.vertices = geom.GetVertices();
            this.normals = geom.GetNormals();
            this.tangents = geom.GetTangents();
            this.bitangents = geom.GetBitangents();
            this.vertexColors = geom.GetVertexColors();
            this.uvCoords = geom.GetUVCoords();
        }

        public Geometry GetGeom()
        {
            Geometry geom = new Geometry();
            geom.SetTriangles(this.triangles);
            geom.SetVertices(this.vertices);
            geom.SetNormals(this.normals);
            geom.SetTangents(this.tangents);
            geom.SetBitangents(this.bitangents);
            geom.SetVertexColors(this.vertexColors);
            geom.SetUVCoords(this.uvCoords);
            return geom;
        }

        public override bool IsDerivedType(string type)
        {
            bool flag = base.IsDerivedType(type);
            if (!flag)
                flag = type == "BSTriShape";
            return flag;
        }

        public override string GetClassName()
        {
            return "BSTriShape";
        }
    }
}

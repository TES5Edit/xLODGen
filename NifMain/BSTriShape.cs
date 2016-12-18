using LODGenerator.Common;
using System;
using System.Collections.Generic;
using System.IO;

namespace LODGenerator.NifMain
{
    public class BSTriShape : NiTriBasedGeom
    {
        protected byte vertexFlag1;
        protected byte vertexFlag2;
        protected byte vertexFlag3;
        protected byte vertexFlag4;
        protected byte vertexFlag5;
        protected byte vertexFlag6;
        protected byte vertexFlag7;
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
        protected List<UVCoord> uvcoords;

        public BSTriShape()
        {
            this.center = new Vector3(0, 0, 0);
            this.radius = 0;
            this.skinInstance = -1;
            this.bsProperties = new int[2];
            this.bsProperties[0] = -1;
            this.bsProperties[1] = -1;
            this.vertexFlag1 = 5;
            this.vertexFlag2 = 2;
            this.vertexFlag3 = 67;
            this.vertexFlag4 = 0;
            this.vertexFlag5 = 0;
            this.vertexFlag6 = 176;
            this.vertexFlag7 = 1;
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
            this.uvcoords = new List<UVCoord>();
        }

        public override void Read(NiHeader header, BinaryReader reader)
        {
            base.Read(header, reader);
            this.vertexFlag1 = reader.ReadByte();
            this.vertexFlag2 = reader.ReadByte();
            this.vertexFlag3 = reader.ReadByte();
            this.vertexFlag4 = reader.ReadByte();
            this.vertexFlag5 = reader.ReadByte();
            this.vertexFlag6 = reader.ReadByte();
            this.vertexFlag7 = reader.ReadByte();
            this.vertexFlag8 = reader.ReadByte();
            this.numTriangles = reader.ReadUInt32();
            this.numVertices = reader.ReadUInt16();
            this.dataSize = reader.ReadUInt32();

            if (this.dataSize > 0)
            {
                for (int index = 0; index < numVertices; index++)
                {
                    BSVertexData vd = new BSVertexData();
                    vd.vertex = new Vector3(Utils.ShortToFloat(reader.ReadInt16()), Utils.ShortToFloat(reader.ReadInt16()), Utils.ShortToFloat(reader.ReadInt16()));
                    //BSVertexDataNoNormals
                    if ((this.vertexFlag7 & 64) == 0 && this.vertexFlag6 == 48)
                    {
                        vd.unknownShort1 = reader.ReadUInt16();
                        vd.uvcoords = new UVCoord(Utils.ShortToFloat(reader.ReadInt16()), Utils.ShortToFloat(reader.ReadInt16()));
                        //Vertex Colors
                        if ((this.vertexFlag7 & 2) != 0)
                        {
                            vd.vertexColors = new Color4(reader.ReadByte() / 255, reader.ReadByte() / 255, reader.ReadByte() / 255, reader.ReadByte() / 255);
                        }
                        if ((this.vertexFlag7 & 4) != 0)
                        {
                            vd.boneWeights[0] = Utils.ShortToFloat(reader.ReadInt16());
                            vd.boneIndices[0] = reader.ReadByte();
                        }
                    }
                    //BSVertexDataRigid + BSVertexDataSkinned
                    else if ((this.vertexFlag7 & 1) != 0)
                    {
                        vd.bitangentX = Utils.ShortToFloat(reader.ReadInt16());
                        vd.uvcoords = new UVCoord(Utils.ShortToFloat(reader.ReadInt16()), Utils.ShortToFloat(reader.ReadInt16()));
                        vd.normal = new Vector3(Utils.ByteToFloat(reader.ReadByte()), Utils.ByteToFloat(reader.ReadByte()), Utils.ByteToFloat(reader.ReadByte()));
                        vd.bitangentY = Utils.ByteToFloat(reader.ReadByte());
                        vd.tangent = new Vector3(Utils.ByteToFloat(reader.ReadByte()), Utils.ByteToFloat(reader.ReadByte()), Utils.ByteToFloat(reader.ReadByte()));
                        vd.bitangentZ = Utils.ByteToFloat(reader.ReadByte());

                        //Vertex Colors
                        if ((this.vertexFlag7 & 2) != 0)
                        {
                            vd.vertexColors = new Color4(reader.ReadByte() / 255, reader.ReadByte() / 255, reader.ReadByte() / 255, reader.ReadByte() / 255);
                        }
                        //BSVertexDataSkinned
                        if ((this.vertexFlag7 & 4) != 0 && this.vertexFlag5 == 0)
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
                    }
                    //BSVertexData
                    else if ((this.vertexFlag7 & 1) == 0 || this.vertexFlag5 > 0)
                    {
                        if (this.vertexFlag1 != 6 && this.vertexFlag1 != 3)
                        {
                            vd.bitangentX = Utils.ShortToFloat(reader.ReadInt16());
                        }
                        if (this.vertexFlag1 == 6 || this.vertexFlag1 == 3)
                        {
                            vd.unknownShort1 = reader.ReadUInt16();
                        }
                        if (this.vertexFlag1 == 3)
                        {
                            vd.unknownInt1 = reader.ReadUInt32();
                        }
                        if (this.vertexFlag1 > 4)
                        {
                            vd.uvcoords = new UVCoord(Utils.ShortToFloat(reader.ReadInt16()), Utils.ShortToFloat(reader.ReadInt16()));
                        }
                        if (this.vertexFlag1 > 3 && this.vertexFlag1 != 7)
                        {
                            vd.normal = new Vector3(Utils.ByteToFloat(reader.ReadByte()), Utils.ByteToFloat(reader.ReadByte()), Utils.ByteToFloat(reader.ReadByte()));
                            vd.bitangentY = Utils.ByteToFloat(reader.ReadByte());
                            vd.tangent = new Vector3(Utils.ByteToFloat(reader.ReadByte()), Utils.ByteToFloat(reader.ReadByte()), Utils.ByteToFloat(reader.ReadByte()));
                            vd.bitangentZ = Utils.ByteToFloat(reader.ReadByte());
                        }
                        if (this.vertexFlag1 == 6 || this.vertexFlag1 == 7 || this.vertexFlag1 == 9 || this.vertexFlag1 == 10)
                        {
                            vd.vertexColors = new Color4(reader.ReadByte() / 255, reader.ReadByte() / 255, reader.ReadByte() / 255, reader.ReadByte() / 255);
                        }
                        if (this.vertexFlag1 > 6)
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
                        if (this.vertexFlag1 == 10)
                        {
                            vd.unknownInt2 = reader.ReadUInt32();
                        }
                    }
                    this.vertexData.Add(vd);
                    this.vertices.Add(vd.vertex);
                    this.normals.Add(vd.normal);
                    this.tangents.Add(vd.tangent);
                    this.bitangents.Add(new Vector3(vd.bitangentX, vd.bitangentY, vd.bitangentZ));
                    this.vertexColors.Add(vd.vertexColors);
                    this.uvcoords.Add(vd.uvcoords);
                }
                for (int index = 0; index < numTriangles; index++)
                {
                    this.triangles.Add(Utils.ReadTriangle(reader));
                }
                /*for (int index = 0; index < numVertices; index++)
                {
                    Console.WriteLine("Vertex ["+ index + "] = " + vertices[index]);
                }*/
                /*for (int index = 0; index < normals.Count; index++)
                {
                    Console.WriteLine("Normal ["+ index + "] = " + normals[index]);
                }*/
                /*for (int index = 0; index < bitangents.Count; index++)
                {
                    Console.WriteLine("Bitang [" + index + "] = " + bitangents[index]);
                }*/
                /*for (int index = 0; index < numTriangles; index++)
                {
                    Console.WriteLine("Triangle [" + index + "] = " + triangles[index]);
                }*/
            }
        }

        public override void Write(NiHeader header, BinaryWriter writer)
        {
            base.Write(header, writer);
            writer.Write(this.vertexFlag1);
            writer.Write(this.vertexFlag2);
            writer.Write(this.vertexFlag3);
            writer.Write(this.vertexFlag4);
            writer.Write(this.vertexFlag5);
            writer.Write(this.vertexFlag6);
            writer.Write(this.vertexFlag7);
            writer.Write(this.vertexFlag8);
            writer.Write(this.numTriangles);
            writer.Write(this.numVertices);
            writer.Write((uint)(this.numVertices * 20 + this.numTriangles * 6));

            for (int index = 0; index < this.vertices.Count; index++)
            {
                writer.Write(Utils.FloatToShort(this.vertices[index][0]));
                writer.Write(Utils.FloatToShort(this.vertices[index][1]));
                writer.Write(Utils.FloatToShort(this.vertices[index][2]));
                writer.Write(Utils.FloatToShort(this.bitangents[index][0]));
                writer.Write(Utils.FloatToShort(this.uvcoords[index][0]));
                writer.Write(Utils.FloatToShort(this.uvcoords[index][1]));
                writer.Write(Utils.FloatToByte(this.normals[index][0]));
                writer.Write(Utils.FloatToByte(this.normals[index][1]));
                writer.Write(Utils.FloatToByte(this.normals[index][2]));
                writer.Write(Utils.FloatToByte(this.bitangents[index][1]));
                writer.Write(Utils.FloatToByte(this.tangents[index][0]));
                writer.Write(Utils.FloatToByte(this.tangents[index][1]));
                writer.Write(Utils.FloatToByte(this.tangents[index][2]));
                writer.Write(Utils.FloatToByte(this.bitangents[index][2]));
            }
            for (int index = 0; index < this.triangles.Count; ++index)
            {
                Utils.WriteTriangle(writer, this.triangles[index]);
            }
        }

        public override uint GetSize(NiHeader header)
        {
            return (uint)((int)base.GetSize(header) + 18 + this.numVertices * 20 + this.numTriangles * 6);
        }

        public void UpdateVertexData()
        {
            this.vertexData.Clear();
            for (int index = 0; index < this.vertices.Count; index++)
            {
                BSVertexData vd = new BSVertexData();
                vd.vertex = new Vector3(this.vertices[index][0], this.vertices[index][1], this.vertices[index][2]);
                if (this.uvcoords.Count != 0)
                {
                    vd.uvcoords = new UVCoord(this.uvcoords[index][0], this.uvcoords[index][1]);
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
            this.uvcoords = v;
        }

        public List<UVCoord> GetUVCoords()
        {
            return this.uvcoords;
        }

        public void SetTriangles(List<Triangle> tris)
        {
            this.triangles = tris;
        }

        public List<Triangle> GetTriangles()
        {
            return this.triangles;
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

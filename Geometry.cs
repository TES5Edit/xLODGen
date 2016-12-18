using LODGenerator.Common;
using LODGenerator.NifMain;
using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Linq;
using DelaunayTriangulator;

namespace LODGenerator
{
    static class GeometryList
    {
        private static ConcurrentDictionary<string, Geometry> _list;

        static GeometryList()
        {
            _list = new ConcurrentDictionary<string, Geometry>();
        }

        public static void Set(string key, Geometry value)
        {
            if (!Contains(key))
            {
                _list.TryAdd(key, value);
            }
        }

        public static Geometry Get(string key)
        {
            return _list[key];
        }

        public static bool Contains(string key)
        {
            return _list.ContainsKey(key);
        }
    }

    [Serializable]
    public class Geometry
    {
        public List<Triangle> triangles;
        public List<Vector3> vertices;
        public List<Vector3> normals;
        public List<Vector3> tangents;
        public List<Vector3> bitangents;
        public List<Color4> vertexColors;
        public List<UVCoord> uvcoords;

        public Geometry()
        {
            this.triangles = new List<Triangle>();
            this.vertices = new List<Vector3>();
            this.normals = new List<Vector3>();
            this.tangents = new List<Vector3>();
            this.bitangents = new List<Vector3>();
            this.vertexColors = new List<Color4>();
            this.uvcoords = new List<UVCoord>();
        }

        public Geometry(NiTriShapeData data)
        {
            this.triangles = data.GetTriangles();
            this.vertices = data.GetVertices();
            this.normals = data.GetNormals();
            this.tangents = data.GetTangents();
            this.bitangents = data.GetBitangents();
            this.vertexColors = data.GetVertexColors();
            this.uvcoords = data.GetUVCoords();
        }

        public Geometry(BSTriShape data)
        {
            this.triangles = data.GetTriangles();
            this.vertices = data.GetVertices();
            this.normals = data.GetNormals();
            this.tangents = data.GetTangents();
            this.bitangents = data.GetBitangents();
            this.vertexColors = data.GetVertexColors();
            this.uvcoords = data.GetUVCoords();
        }

        public NiTriShapeData ToNiTriShapeData(bool generateVertexColors)
        {
            NiTriShapeData data = new NiTriShapeData();
            data.SetNumVertices(this.GetNumVertices());
            data.SetKeepFlags(51);
            data.SetHasVertices(this.HasVertices());
            data.SetVertices(this.GetVertices());
            if (this.HasNormals())
            {
                bool b = false;
                List<Vector3> normals = this.GetNormals();
                for (int index = 0; index < normals.Count; ++index)
                {
                    if (normals[index][0] != 0f || normals[index][1] != 0f || normals[index][2] != 0f)
                    {
                        b = true;
                        break;
                    }
                }
                if (b)
                {
                    data.SetHasNormals(this.HasNormals());
                    data.SetNormals(this.GetNormals());
                }
            }
            data.SetExtraVertexFlag(0);
            if (this.HasTangents() || this.HasBitangents())
            {
                bool b = false;
                List<Vector3> tangents = this.GetTangents();
                List<Vector3> bitangents = this.GetBitangents();
                for (int index = 0; index < tangents.Count; ++index)
                {
                    if (tangents[index][0] != 0f || tangents[index][1] != 0f || tangents[index][2] != 0f || bitangents[index][0] != 0f || bitangents[index][1] != 0f || bitangents[index][2] != 0f)
                    {
                        b = true;
                        break;
                    }
                }
                if (b)
                {
                    data.SetExtraVertexFlag(16);
                    data.SetTangents(this.GetTangents());
                    data.SetBitangents(this.GetBitangents());
                }
            }
            BBox bbox = new BBox(float.MaxValue, float.MinValue, float.MaxValue, float.MinValue, float.MaxValue, float.MinValue);
            for (int index = 0; index < (int)this.GetNumVertices(); ++index)
            {
                bbox.GrowByVertex(this.vertices[index]);
            }
            data.SetCenter(bbox.GetCenter());

            float num1 = float.MinValue;
            for (int index = 0; index < (int)this.GetNumVertices(); ++index)
            {
                Vector3 vector3_1 = this.GetVertices()[index];
                Vector3 vector3_2 = data.GetCenter() - vector3_1;
                float num2 = (float)((double)vector3_2[0] * (double)vector3_2[0] + (double)vector3_2[1] * (double)vector3_2[1] + (double)vector3_2[2] * (double)vector3_2[2]);
                if ((double)num2 > (double)num1)
                    num1 = num2;
            }
            data.SetRadius((float)Math.Sqrt((double)num1));

            if (generateVertexColors && this.HasVertexColors())
            {
                data.SetHasVertexColors(this.HasVertexColors());
                data.SetVertexColors(this.GetVertexColors());
            }
            else
            {
                data.SetHasVertexColors(false);
            }
            data.SetUVCoords(this.GetUVCoords());
            if (this.HasUVCoords())
            {
                data.SetNumUVSets(1);
            }
            else
            {
                data.SetNumUVSets(0);
            }
            data.SetNumTriangles(this.GetNumTriangles());
            data.SetNumTrianglePoints((uint)this.GetNumTriangles() * 3);
            data.SetHasTriangles(this.HasTriangles());
            data.SetTriangles(this.GetTriangles());

            return data;
        }

        public BSSubIndexTriShape ToBSSubIndexTriShape(bool generateVertexColors)
        {
            BSSubIndexTriShape data = new BSSubIndexTriShape();
            BBox bbox = new BBox(float.MaxValue, float.MinValue, float.MaxValue, float.MinValue, float.MaxValue, float.MinValue);
            for (int index = 0; index < (int)this.GetNumVertices(); ++index)
            {
                bbox.GrowByVertex(this.vertices[index]);
            }
            data.SetCenter(bbox.GetCenter());

            float num1 = float.MinValue;
            for (int index = 0; index < (int)this.GetNumVertices(); ++index)
            {
                Vector3 vector3_1 = this.GetVertices()[index];
                Vector3 vector3_2 = data.GetCenter() - vector3_1;
                float num2 = (float)((double)vector3_2[0] * (double)vector3_2[0] + (double)vector3_2[1] * (double)vector3_2[1] + (double)vector3_2[2] * (double)vector3_2[2]);
                if ((double)num2 > (double)num1)
                    num1 = num2;
            }
            data.SetRadius((float)Math.Sqrt((double)num1));

            data.SetNumTriangles(this.GetNumTriangles());
            data.SetNumVertices(this.GetNumVertices());
            data.SetDataSize((uint)this.GetNumVertices() * 20 + (uint)GetNumTriangles() * 6);

            ushort vflag = 0;

            if (this.HasVertices())
            {
                data.SetVertices(this.GetVertices());
                vflag |= 16;
            }
            if (this.HasUVCoords())
            {
                data.SetUVCoords(this.GetUVCoords());
                vflag |= 32;
            }
            if (this.HasNormals())
            {
                data.SetNormals(this.GetNormals());
                vflag |= 128;
            }
            if (this.HasTangents())
            {
                data.SetTangents(this.GetTangents());
                data.SetBitangents(this.GetBitangents());
                vflag |= 256;
            }
            if (generateVertexColors && this.HasVertexColors())
            {
                data.SetVertexColors(this.GetVertexColors());
                vflag |= 512;
            }
            //data.SetVertexFlags(vflag);
            data.SetTriangles(this.GetTriangles());
            data.UpdateVertexData();

            return data;
        }

        public BSTriShape ToBSTriShape(BSTriShape data, bool generateVertexColors)
        {
            BBox bbox = new BBox(float.MaxValue, float.MinValue, float.MaxValue, float.MinValue, float.MaxValue, float.MinValue);
            for (int index = 0; index < (int)this.GetNumVertices(); ++index)
            {
                bbox.GrowByVertex(this.vertices[index]);
            }
            data.SetCenter(bbox.GetCenter());

            float num1 = float.MinValue;
            for (int index = 0; index < (int)this.GetNumVertices(); ++index)
            {
                Vector3 vector3_1 = this.GetVertices()[index];
                Vector3 vector3_2 = data.GetCenter() - vector3_1;
                float num2 = (float)((double)vector3_2[0] * (double)vector3_2[0] + (double)vector3_2[1] * (double)vector3_2[1] + (double)vector3_2[2] * (double)vector3_2[2]);
                if ((double)num2 > (double)num1)
                    num1 = num2;
            }
            data.SetRadius((float)Math.Sqrt((double)num1));

            data.SetNumTriangles(this.GetNumTriangles());
            data.SetNumVertices(this.GetNumVertices());
            data.SetDataSize((uint)this.GetNumVertices() * 20 + (uint)GetNumTriangles() * 6);

            ushort vflag = 0;

            if (this.HasVertices())
            {
                data.SetVertices(this.GetVertices());
                vflag |= 16;
            }
            if (this.HasUVCoords())
            {
                data.SetUVCoords(this.GetUVCoords());
                vflag |= 32;
            }
            if (this.HasNormals())
            {
                data.SetNormals(this.GetNormals());
                vflag |= 128;
            }
            if (this.HasTangents())
            {
                data.SetTangents(this.GetTangents());
                data.SetBitangents(this.GetBitangents());
                vflag |= 256;
            }
            if (generateVertexColors && this.HasVertexColors())
            {
                data.SetVertexColors(this.GetVertexColors());
                vflag |= 512;
            }
            //data.SetVertexFlags(vflag);
            data.SetTriangles(this.GetTriangles());
            data.UpdateVertexData();

            return data;
        }

        public void FaceNormals()
        {
            this.normals = new List<Vector3>();
            for (int index = 0; index < this.vertices.Count; index++)
            {
                this.normals.Add(new Vector3(0, 0, 0));
            }
            for (int index = 0; index < this.triangles.Count; index++)
            {
                Vector3 a = this.vertices[this.triangles[index][0]];
                Vector3 b = this.vertices[this.triangles[index][1]];
                Vector3 c = this.vertices[this.triangles[index][2]];
                Vector3 u = b - a;
                Vector3 v = c - a;
                Vector3 normal = Vector3.Cross(u, v);
                this.normals[this.triangles[index][0]] += normal;
                this.normals[this.triangles[index][1]] += normal;
                this.normals[this.triangles[index][2]] += normal;
            }
            for (int index = 0; index < this.normals.Count; index++)
            {
                this.normals[index].Normalize();
            }
        }

        public void SmoothNormals(float angle, float maxd)
        {
            float maxa = (float)(angle / 180 * Math.PI);
            List<Vector3> snormals = new List<Vector3>();
            for (int index = 0; index < this.vertices.Count; index++)
            {
                snormals.Add(normals[index]);
            }
            for (int index = 0; index < this.vertices.Count; index++)
            {
                Vector3 a = this.vertices[index];
                Vector3 an = this.normals[index];

                for (int index2 = index + 1; index2 < this.vertices.Count; index2++)
                {
                    Vector3 b = this.vertices[index2];
                    if ((a - b).squaredLength < maxd)
                    {
                        Vector3 bn = this.normals[index2];
                        if (Vector3.Angle(an, bn) < maxa)
                        {
                            snormals[index] += bn;
                            snormals[index2] += an;
                        }
                    }
                }
            }
            for (int index = 0; index < this.normals.Count; index++)
            {
                this.normals[index] = snormals[index];
                this.normals[index].Normalize();
            }
        }

        public void UpdateTangents(bool generateTangents)
        {
            if (generateTangents && tangents.Count == 0 && bitangents.Count == 0)
            {
                for (int index = 0; index < this.vertices.Count; index++)
                {
                    this.tangents.Add(new Vector3(0, 0, 0));
                    this.bitangents.Add(new Vector3(0, 0, 0));
                }
            }
            else if (tangents.Count > 0 && bitangents.Count > 0)
            {
                for (int index = 0; index < this.vertices.Count; index++)
                {
                    this.tangents[index] = new Vector3(0, 0, 0);
                    this.bitangents[index] = new Vector3(0, 0, 0);
                }
            }
            if (triangles.Count > 0 && tangents.Count > 0 && bitangents.Count > 0)
            {
                for (int index = 0; index < triangles.Count; index++)
                {
                    Triangle tri = triangles[index];
                    ushort i1 = tri[0];
                    ushort i2 = tri[1];
                    ushort i3 = tri[2];
                    Vector3 v1 = vertices[i1];
                    Vector3 v2 = vertices[i2];
                    Vector3 v3 = vertices[i3];
                    Vector2 w1 = new Vector2(uvcoords[i1][0], uvcoords[i1][1]);
                    Vector2 w2 = new Vector2(uvcoords[i2][0], uvcoords[i2][1]);
                    Vector2 w3 = new Vector2(uvcoords[i3][0], uvcoords[i3][1]);
                    Vector2 w2w1 = w2 - w1;
                    Vector2 w3w1 = w3 - w1;
                    //float r = 1f / w2w1[0] * w3w1[1] - w3w1[0] * w2w1[1];
                    // niftools says this is better
                    float r = w2w1[0] * w3w1[1] - w3w1[0] * w2w1[1];
                    r = (r >= 0 ? +1 : -1);
                    Vector3 v2v1 = v2 - v1;
                    Vector3 v3v1 = v3 - v1;
                    Vector3 sdir = new Vector3((w3w1[1] * v2v1[0] - w2w1[1] * v3v1[0]) * r, (w3w1[1] * v2v1[1] - w2w1[1] * v3v1[1]) * r, (w3w1[1] * v2v1[2] - w2w1[1] * v3v1[2]) * r);
                    Vector3 tdir = new Vector3((w2w1[0] * v3v1[0] - w3w1[0] * v2v1[0]) * r, (w2w1[0] * v3v1[1] - w3w1[0] * v2v1[1]) * r, (w2w1[0] * v3v1[2] - w3w1[0] * v2v1[2]) * r);
                    sdir.Normalize();
                    tdir.Normalize();
                    for (int index2 = 0; index2 < 3; index2++)
                    {
                        int i = tri[index2];
                        tangents[i] += tdir;
                        bitangents[i] += sdir;
                    }
                }
                for (int index = 0; index < vertices.Count; index++)
                {
                    Vector3 n = normals[index];
                    Vector3 t = tangents[index];
                    Vector3 b = bitangents[index];
                    if ((t[0] == 0 && t[1] == 0 && t[2] == 0) || (b[0] == 0 && b[1] == 0 && b[2] == 0))
                    {
                        t[0] = n[1]; t[1] = n[2]; t[2] = n[1];
                        b = Vector3.Cross(n, t);
                    }
                    else {
                        t.Normalize();
                        t = (t - n * Vector3.Dot(n, t));
                        t.Normalize();

                        b.Normalize();
                        b = (b - n * Vector3.Dot(n, b));
                        b = (b - t * Vector3.Dot(t, b));
                        b.Normalize();
                    }
                    tangents[index] = t;
                    bitangents[index] = b;
                }
            }
        }

        public int CreateDuplicate(int index)
        {
            this.vertices.Add(this.vertices[index]);
            this.uvcoords.Add(this.uvcoords[index]);
            if (this.normals.Count > 0)
            {
                this.normals.Add(this.normals[index]);
            }
            if (this.tangents.Count > 0)
            {
                this.tangents.Add(this.tangents[index]);
            }
            if (this.bitangents.Count > 0)
            {
                this.bitangents.Add(this.bitangents[index]);
            }
            if (this.vertexColors.Count > 0)
            {
                this.vertexColors.Add(this.vertexColors[index]);
            }
            return this.vertices.Count - 1;
        }

        public int CreateDuplicate(int index, int index2)
        {
            this.vertices.Add(this.vertices[index]);
            this.uvcoords.Add(this.uvcoords[index]);
            if (this.normals.Count > 0)
            {
                this.normals.Add(this.normals[index]);
            }
            if (this.tangents.Count > 0)
            {
                this.tangents.Add(this.tangents[index]);
            }
            if (this.bitangents.Count > 0)
            {
                this.bitangents.Add(this.bitangents[index]);
            }
            if (this.vertexColors.Count > 0)
            {
                this.vertexColors.Add(this.vertexColors[index] + this.vertexColors[index2] / 2);
            }
            return this.vertices.Count - 1;
        }

        public int CreateDuplicate(int index, int index2, int index3)
        {
            this.vertices.Add(this.vertices[index]);
            this.uvcoords.Add(this.uvcoords[index]);
            if (this.normals.Count > 0)
            {
                this.normals.Add(this.normals[index]);
            }
            if (this.tangents.Count > 0)
            {
                this.tangents.Add(this.tangents[index]);
            }
            if (this.bitangents.Count > 0)
            {
                this.bitangents.Add(this.bitangents[index]);
            }
            if (this.vertexColors.Count > 0)
            {
                this.vertexColors.Add(this.vertexColors[index] + this.vertexColors[index2] + this.vertexColors[index3] / 3);
            }
            return this.vertices.Count - 1;
        }

        public int CreateDuplicate(int p0, int p1, int p2, UVCoord uv)
        {
            this.vertices.Add(this.vertices[p0]);
            this.uvcoords.Add(uv);
            if (this.normals.Count > 0)
            {
                this.normals.Add(this.normals[p0]);
            }
            if (this.tangents.Count > 0)
            {
                this.tangents.Add(this.tangents[p0]);
            }
            if (this.bitangents.Count > 0)
            {
                this.bitangents.Add(this.bitangents[p0]);
            }
            if (this.vertexColors.Count > 0)
            {
                double[] c = new double[3] { 0, 0, 0 };
                double[] s = new double[3] { 0, 0, 0 };

                c[0] = Math.Sqrt(Math.Pow(this.uvcoords[p1][0] - this.uvcoords[p0][0], 2) + Math.Pow(this.uvcoords[p1][1] - this.uvcoords[p0][1], 2));
                c[1] = Math.Sqrt(Math.Pow(this.uvcoords[p2][0] - this.uvcoords[p1][0], 2) + Math.Pow(this.uvcoords[p2][1] - this.uvcoords[p1][1], 2));
                c[2] = Math.Sqrt(Math.Pow(this.uvcoords[p2][0] - this.uvcoords[p0][0], 2) + Math.Pow(this.uvcoords[p2][1] - this.uvcoords[p0][1], 2));

                s[0] = 1 - Math.Sqrt(Math.Pow(uv[0] - this.uvcoords[p0][0], 2) + Math.Pow(uv[1] - this.uvcoords[p0][1], 2)) / c[0];
                s[1] = 1 - Math.Sqrt(Math.Pow(uv[0] - this.uvcoords[p1][0], 2) + Math.Pow(uv[1] - this.uvcoords[p1][1], 2)) / c[1];
                if (p0 != p2)
                {
                    s[2] = 1 - (s[0] + s[1]);
                }
                this.vertexColors.Add(this.vertexColors[p0] * s[0] + this.vertexColors[p1] * s[1] + this.vertexColors[p2] * s[2]);
            }
            return this.vertices.Count - 1;
        }

        public void RemoveDuplicate(bool high)
        {
            Dictionary<int, int> dictionary = new Dictionary<int, int>();
            for (int index = 0; index < vertices.Count; index++)
            {
                Vector3 vertex = this.vertices[index];
                for (int index2 = 0; index2 < index; index2++)
                {
                    if (Utils.QHigh(vertex[0]) != Utils.QHigh(vertices[index2][0]) || Utils.QHigh(vertex[1]) != Utils.QHigh(vertices[index2][1]) || Utils.QHigh(vertex[2]) != Utils.QHigh(vertices[index2][2]))
                    {
                        continue;
                    }
                    if (this.normals.Count > 0)
                    {
                        if (Utils.QHigh(this.normals[index][0]) != Utils.QHigh(this.normals[index2][0]) || Utils.QHigh(this.normals[index][1]) != Utils.QHigh(this.normals[index2][1]) || Utils.QHigh(this.normals[index][2]) != Utils.QHigh(this.normals[index2][2]))
                        {
                            continue;
                        }
                    }
                    if (this.tangents.Count > 0)
                    {
                        if (Utils.QHigh(tangents[index][0]) != Utils.QHigh(tangents[index2][0]) || Utils.QHigh(tangents[index][1]) != Utils.QHigh(tangents[index2][1]) || Utils.QHigh(tangents[index][2]) != Utils.QHigh(tangents[index2][2]))
                        {
                            continue;
                        }
                    }
                    if (this.bitangents.Count > 0)
                    {
                        if (Utils.QHigh(bitangents[index][0]) != Utils.QHigh(bitangents[index2][0]) || Utils.QHigh(bitangents[index][1]) != Utils.QHigh(bitangents[index2][1]) || Utils.QHigh(bitangents[index][2]) != Utils.QHigh(bitangents[index2][2]))
                        {
                            continue;
                        }
                    }
                    if (this.vertexColors.Count > 0)
                    {
                        if (Utils.FloatToUByte(vertexColors[index][0]) != Utils.FloatToUByte(vertexColors[index2][0]) || Utils.FloatToUByte(vertexColors[index][1]) != Utils.FloatToUByte(vertexColors[index2][1]) || Utils.FloatToUByte(vertexColors[index][2]) != Utils.FloatToUByte(vertexColors[index2][2]) || Utils.FloatToUByte(vertexColors[index][3]) != Utils.FloatToUByte(vertexColors[index2][3]))
                        {
                            continue;
                        }
                    }
                    if (this.uvcoords.Count > 0)
                    {
                        if (high)
                        {
                            if (Utils.QUV(uvcoords[index][0]) != Utils.QUV(uvcoords[index2][0]) || Utils.QUV(this.uvcoords[index][1]) != Utils.QUV(this.uvcoords[index2][1]))
                            {
                                continue;
                            }
                        }
                        else
                        {
                            if (Utils.FloatToUByte(uvcoords[index][0]) != Utils.FloatToUByte(uvcoords[index2][0]) || Utils.FloatToUByte(this.uvcoords[index][1]) != Utils.FloatToUByte(this.uvcoords[index2][1]))
                            {
                                continue;
                            }
                        }
                    }
                    if (!dictionary.ContainsKey(index))
                    {
                        dictionary.Add(index, index2);
                    }
                }
            }
            for (int index = 0; index < this.triangles.Count; index++)
            {
                for (int index2 = 0; index2 < 3; index2++)
                {
                    if (dictionary.ContainsKey(this.triangles[index][index2]))
                    {
                        this.triangles[index][index2] = (ushort)dictionary[this.triangles[index][index2]];
                    }
                }
            }
            this.RemoveUnused();
        }

        public void RemoveUnused()
        {
            Dictionary<ushort, ushort> dictionary = new Dictionary<ushort, ushort>();
            List<Vector3> vertices2 = new List<Vector3>();
            List<UVCoord> uvcoords2 = new List<UVCoord>();
            List<Color4> vertexColors2 = new List<Color4>();
            List<Vector3> normals2 = new List<Vector3>();
            List<Vector3> tangents2 = new List<Vector3>();
            List<Vector3> bitangents2 = new List<Vector3>();
            ushort index3 = 0;

            if (vertices.Count != uvcoords.Count || 
                (vertexColors.Count != 0 && vertices.Count != vertexColors.Count) || 
                (normals.Count != 0 && vertices.Count != normals.Count) || 
                (tangents.Count != 0 && vertices.Count != tangents.Count) || 
                (bitangents.Count != 0 && vertices.Count != bitangents.Count))
            {
                Console.WriteLine("Oh crap " + vertices.Count + " = " + uvcoords.Count + " = " + vertexColors.Count + " = " + normals.Count + " = " + tangents.Count + " = " + bitangents.Count);
            }

            for (int index = 0; index < triangles.Count; ++index)
            {
                for (int index2 = 0; index2 < 3; ++index2)
                {
                    if (!dictionary.ContainsKey(triangles[index][index2]))
                    {
                        dictionary.Add(triangles[index][index2], index3);
                        ++index3;
                        if (this.vertices.Count != 0)
                        {
                            vertices2.Add(this.vertices[triangles[index][index2]]);
                        }
                        if (this.uvcoords.Count != 0)
                        {
                            uvcoords2.Add(this.uvcoords[triangles[index][index2]]);
                        }
                        if (this.vertexColors.Count != 0)
                        {
                            vertexColors2.Add(this.vertexColors[triangles[index][index2]]);
                        }
                        if (this.normals.Count != 0)
                        {
                            normals2.Add(this.normals[triangles[index][index2]]);
                        }
                        if (this.tangents.Count != 0)
                        {
                            tangents2.Add(this.tangents[triangles[index][index2]]);
                        }
                        if (this.bitangents.Count != 0)
                        {
                            bitangents2.Add(this.bitangents[triangles[index][index2]]);
                        }
                    }
                }
            }
            //Console.WriteLine("dat tri count 2 " + triangles.Count);
            for (int index = 0; index < this.triangles.Count; ++index)
            {
                for (int index2 = 0; index2 < 3; ++index2)
                {
                    this.triangles[index][index2] = dictionary[this.triangles[index][index2]];
                }
            }
            this.vertices = vertices2;
            this.uvcoords = uvcoords2;
            this.vertexColors = vertexColors2;
            this.normals = normals2;
            this.tangents = tangents2;
            this.bitangents = bitangents2;
        }

        public void AppendVertices(List<Vector3> v)
        {
            this.vertices.AddRange((IEnumerable<Vector3>)v);
        }

        public void SetVertices(List<Vector3> v)
        {
            this.vertices = v;
        }

        public List<Vector3> GetVertices()
        {
            return this.vertices;
        }

        public ushort GetNumVertices()
        {
            return (ushort)this.vertices.Count;
        }

        public bool HasVertices()
        {
            return Convert.ToBoolean(this.vertices.Count);
        }

        public void AppendNormals(List<Vector3> v)
        {
            this.normals.AddRange((IEnumerable<Vector3>)v);
        }

        public void SetNormals(List<Vector3> v)
        {
            this.normals = v;
        }

        public List<Vector3> GetNormals()
        {
            return this.normals;
        }

        public bool HasNormals()
        {
            return Convert.ToBoolean(this.normals.Count);
        }

        public void AppendTangents(List<Vector3> v)
        {
            this.tangents.AddRange((IEnumerable<Vector3>)v);
        }

        public void SetTangents(List<Vector3> v)
        {
            this.tangents = v;
        }

        public List<Vector3> GetTangents()
        {
            return this.tangents;
        }

        public bool HasTangents()
        {
            return Convert.ToBoolean(this.tangents.Count);
        }

        public void AppendBitangents(List<Vector3> v)
        {
            this.bitangents.AddRange((IEnumerable<Vector3>)v);
        }

        public void SetBitangents(List<Vector3> v)
        {
            this.bitangents = v;
        }

        public List<Vector3> GetBitangents()
        {
            return this.bitangents;
        }

        public bool HasBitangents()
        {
            return Convert.ToBoolean(this.bitangents.Count);
        }

        public void AppendVertexColors(List<Color4> v)
        {
            this.vertexColors.AddRange((IEnumerable<Color4>)v);
        }

        public void SetVertexColors(List<Color4> v)
        {
            this.vertexColors = v;
        }

        public List<Color4> GetVertexColors()
        {
            return this.vertexColors;
        }

        public bool HasVertexColors()
        {
            return Convert.ToBoolean(this.vertexColors.Count);
        }

        public ushort GetNumVertexColors()
        {
            return (ushort)this.vertexColors.Count;
        }

        public void AppendUVCoords(List<UVCoord> v)
        {
            this.uvcoords.AddRange((IEnumerable<UVCoord>)v);
        }

        public void SetUVCoords(List<UVCoord> v)
        {
            this.uvcoords = v;
        }

        public List<UVCoord> GetUVCoords()
        {
            return this.uvcoords;
        }

        public bool HasUVCoords()
        {
            return Convert.ToBoolean(this.uvcoords.Count);
        }

        public void AppendTriangles(List<Triangle> tris)
        {
            this.triangles.AddRange((IEnumerable<Triangle>)tris);
        }

        public void SetTriangles(List<Triangle> tris)
        {
            this.triangles = tris;
        }

        public List<Triangle> GetTriangles()
        {
            return this.triangles;
        }

        public ushort GetNumTriangles()
        {
            return (ushort)this.triangles.Count;
        }

        public bool HasTriangles()
        {
            return Convert.ToBoolean(this.triangles.Count);
        }

        public Geometry ReUV(ShapeDesc shapedesc, string texture, LogFile logFile, bool verbose)
        {
            string key = shapedesc.staticModel + "_" + shapedesc.staticName + "_" + shapedesc.name + "_" + texture + "_" + Utils.GetHash(Utils.ObjectToByteArray(this));
            if (GeometryList.Contains(key))
            {
                return GeometryList.Get(key).Copy();
            }
            Geometry geometry = this.Copy();
            if (AtlasList.Contains(texture) && AtlasList.Get(texture).miniatlas)
            {
                //logFile.WriteLog("converting " + shapedesc.staticName + " for " + texture);
                if (verbose)
                {
                    logFile.WriteLog("Re UV " + key);
                }
                bool hadtangents = geometry.HasTangents();
                string before = texture;
                float scaleU = 1f;
                float scaleV = 1f;
                float posU = 0f;
                float posV = 0f;
                string basetexture = texture;
                AtlasList.BeforeAdd(texture, geometry.triangles.Count);
                scaleU = AtlasList.Get(basetexture).scaleU;
                scaleV = AtlasList.Get(basetexture).scaleV;
                posU = AtlasList.Get(basetexture).posU;
                posV = AtlasList.Get(basetexture).posV;

                for (int index = 0; index < geometry.uvcoords.Count; index++)
                {
                    geometry.uvcoords[index] = new UVCoord(Utils.xQUV(geometry.uvcoords[index][0]), Utils.xQUV(geometry.uvcoords[index][1]));
                    for (int index2 = 0; index2 < 2; index2++)
                    {
                        if (float.IsNaN(geometry.uvcoords[index][index2]) || float.IsNegativeInfinity(geometry.uvcoords[index][index2]))
                        {
                            geometry.uvcoords[index][index2] = 0;
                        }
                        if (float.IsPositiveInfinity(geometry.uvcoords[index][index2]))
                        {
                            geometry.uvcoords[index][index2] = 1;
                        }
                    }
                }

                // break apart all triangles
                Geometry face = new Geometry();
                for (int index = 0; index < geometry.triangles.Count; index++)
                {
                    Triangle triangle = geometry.triangles[index];
                    for (int index2 = 0; index2 < 3; index2++)
                    {
                        face.uvcoords.Add(geometry.uvcoords[triangle[index2]]);
                        face.vertices.Add(geometry.vertices[triangle[index2]]);
                        if (geometry.normals.Count > 0)
                        {
                            face.normals.Add(geometry.normals[triangle[index2]]);
                        }
                        if (geometry.tangents.Count > 0)
                        {
                            face.tangents.Add(geometry.tangents[triangle[index2]]);
                        }
                        if (geometry.bitangents.Count > 0)
                        {
                            face.bitangents.Add(geometry.bitangents[triangle[index2]]);
                        }
                        if (geometry.vertexColors.Count > 0)
                        {
                            face.vertexColors.Add(geometry.vertexColors[triangle[index2]]);
                        }
                    }
                    face.AppendTriangles(new List<Triangle>() { new Triangle((ushort)(face.vertices.Count - 3), (ushort)(face.vertices.Count - 2), (ushort)(face.vertices.Count - 1)) });
                }
                geometry.SetTriangles(face.GetTriangles());
                geometry.SetVertices(face.GetVertices());
                geometry.SetNormals(face.GetNormals());
                geometry.SetTangents(face.GetTangents());
                geometry.SetBitangents(face.GetBitangents());
                geometry.SetVertexColors(face.GetVertexColors());
                geometry.SetUVCoords(face.GetUVCoords());

                face = new Geometry();
                bool split = false;
                while (geometry.triangles.Count > 0)
                {
                    Triangle triangle = geometry.triangles[0];
                    geometry.triangles.RemoveAt(0);

                    // find triangle UV position
                    float minU = float.MaxValue;
                    float minV = float.MaxValue;
                    float maxU = float.MinValue;
                    float maxV = float.MinValue;
                    for (int index2 = 0; index2 < 3; index2++)
                    {
                        float u = geometry.uvcoords[triangle[index2]][0];
                        float v = geometry.uvcoords[triangle[index2]][1];
                        minU = Math.Min(minU, u);
                        minV = Math.Min(minV, v);
                        maxU = Math.Max(maxU, u);
                        maxV = Math.Max(maxV, v);
                    }
                    float shiftU = 0;
                    float shiftV = 0;
                    if (Math.Floor(minU) != 0)
                    {
                        shiftU = -(float)Math.Floor(minU);
                    }
                    if (Math.Floor(minV) != 0)
                    {
                        shiftV = -(float)Math.Floor(minV);
                    }
                    // move triangle UV to 0, 0 of this texture
                    minU = float.MaxValue;
                    minV = float.MaxValue;
                    maxU = float.MinValue;
                    maxV = float.MinValue;
                    float area = UVCoord.Area(geometry.uvcoords[triangle[0]], geometry.uvcoords[triangle[1]], geometry.uvcoords[triangle[2]]);

                    for (int index2 = 0; index2 < 3; index2++)
                    {
                        //float u = Utils.Q(geometry.uvcoords[triangle[index2]][0] + shiftU);
                        //float v = Utils.Q(geometry.uvcoords[triangle[index2]][1] + shiftV);
                        float u = (geometry.uvcoords[triangle[index2]][0] + shiftU) * scaleU;
                        float v = (geometry.uvcoords[triangle[index2]][1] + shiftV) * scaleV;

                        // force flat / large UV into 0, 1
                        if (area == 0 || area > 100 || u > 100 || v > 100)
                        {
                            if (verbose && (area > 100 || u > 100 || v > 100))
                            {
                                logFile.WriteLog(shapedesc.staticModel + " " + shapedesc.name + " " + texture + " has huge UV " + (maxU - minU) + " x " + (maxV - minV) + " = " + u + ", " + v + " (" + area + ")");
                            }
                            if (u > 1)
                            {
                                u = 1;
                            }
                            if (v > 1)
                            {
                                v = 1;
                            }
                            if (u < 0)
                            {
                                u = 0;
                            }
                            if (v < 0)
                            {
                                v = 0;
                            }
                        }
                        geometry.uvcoords[triangle[index2]] = new UVCoord(u, v);
                        minU = Math.Min(minU, u);
                        minV = Math.Min(minV, v);
                        maxU = Math.Max(maxU, u);
                        maxV = Math.Max(maxV, v);
                    }

                    // check if triangle UV > 1
                    bool front = true;
                    if (true && (Math.Ceiling(maxU) - Math.Floor(minU) > 1 || Math.Ceiling(maxV) - Math.Floor(minV) > 1))
                    {
                        List<int> points = new List<int>();
                        points.Add(triangle[0]);
                        points.Add(triangle[1]);
                        points.Add(triangle[2]);

                        front = UVCoord.Clockwise(geometry.uvcoords[points[0]], geometry.uvcoords[points[1]], geometry.uvcoords[points[2]]);

                        int p = 0;
                        while (p < points.Count)
                        {
                            int p1 = p;
                            int p2 = p + 1;
                            if (p2 >= points.Count)
                            {
                                p2 = 0;
                            }

                            // split line at UV borders
                            if (geometry.uvcoords[points[p1]][0] != geometry.uvcoords[points[p2]][0] || geometry.uvcoords[points[p1]][1] != geometry.uvcoords[points[p2]][1])
                            {
                                Vector3 psplit = SplitUV(geometry.uvcoords[points[p1]], geometry.uvcoords[points[p2]]);
                                if (psplit[2] != 0)
                                {
                                    split = true;
                                    if (psplit[0] == 1)
                                    {
                                        AtlasList.SetAverageU(before, (AtlasList.GetAverageU(before) + 1));
                                    }
                                    if (psplit[1] == 1)
                                    {
                                        AtlasList.SetAverageV(before, (AtlasList.GetAverageV(before) + 1));
                                    }
                                    Vector3 vertex = geometry.vertices[points[p1]] + ((geometry.vertices[points[p1]] - geometry.vertices[points[p2]]) * psplit[2]);
                                    int vert = geometry.CreateDuplicate(points[p1], points[p2], points[p1], new UVCoord(psplit[0], psplit[1]));
                                    geometry.vertices[vert] = geometry.vertices[points[p1]] + ((geometry.vertices[points[p2]] - geometry.vertices[points[p1]]) * psplit[2]);
                                    if (p2 == 0)
                                    {
                                        points.Add(vert);
                                    }
                                    else
                                    {
                                        points.Insert(p2, vert);
                                    }
                                }
                            }
                            p++;
                        }

                        // create missing points at intersections
                        float u1;
                        float v1;
                        for (float u = (float)Math.Ceiling(minU); u < Math.Ceiling(maxU); u++)
                        {
                            for (float v = (float)Math.Ceiling(minV); v < Math.Ceiling(maxV); v++)
                            {
                                if (Utils.PointInTriangle(new UVCoord(u, v), geometry.uvcoords[triangle[0]], geometry.uvcoords[triangle[1]], geometry.uvcoords[triangle[2]], out u1, out v1))
                                {
                                    int vert = geometry.CreateDuplicate(triangle[0], triangle[1], triangle[2], new UVCoord(u, v));
                                    geometry.vertices[vert] = geometry.vertices[triangle[0]] + ((geometry.vertices[triangle[1]] - geometry.vertices[triangle[0]]) * v1) + ((geometry.vertices[triangle[2]] - geometry.vertices[triangle[0]]) * u1);
                                    points.Add(vert);
                                }
                            }
                        }

                        // create triangles for each quadrant
                        for (float u = (float)Math.Floor(minU); u < Math.Ceiling(maxU); u++)
                        {
                            for (float v = (float)Math.Floor(minV); v < Math.Ceiling(maxV); v++)
                            {
                                List<int> idx = new List<int>();
                                List<Vertex> pts = new List<Vertex>();
                                bool duplicate = false;
                                for (int index = 0; index < points.Count; index++)
                                {
                                    if (((float)Math.Floor(geometry.uvcoords[points[index]][0]) == (float)u || (float)(Math.Ceiling(geometry.uvcoords[points[index]][0]) - 1) == (float)u) && ((float)Math.Floor(geometry.uvcoords[points[index]][1]) == (float)v || (float)(Math.Ceiling(geometry.uvcoords[points[index]][1]) - 1) == (float)v))
                                    {
                                        duplicate = false;
                                        for (int index2 = 0; index2 < idx.Count; index2++)
                                        {
                                            if ((float)geometry.uvcoords[points[index]][0] == (float)geometry.uvcoords[idx[index2]][0] && (float)geometry.uvcoords[points[index]][1] == (float)geometry.uvcoords[idx[index2]][1])
                                            {
                                                duplicate = true;
                                                break;
                                            }
                                        }
                                        if (!duplicate)
                                        {
                                            pts.Add(new Vertex(geometry.uvcoords[points[index]][0], geometry.uvcoords[points[index]][1]));
                                            idx.Add(points[index]);
                                        }
                                    }
                                }

                                // create new triangles
                                if (pts.Count >= 3)
                                {
                                    geometry.CreateTriangles(pts, idx, true, front, scaleU, scaleV);
                                }
                            }
                        }
                        continue;
                    }

                    minU = float.MaxValue;
                    minV = float.MaxValue;
                    maxU = float.MinValue;
                    maxV = float.MinValue;
                    for (int index2 = 0; index2 < 3; index2++)
                    {
                        float u = posU + geometry.uvcoords[triangle[index2]][0];
                        float v = posV + geometry.uvcoords[triangle[index2]][1];

                        minU = Math.Min(minU, u);
                        minV = Math.Min(minV, v);
                        maxU = Math.Max(maxU, u);
                        maxV = Math.Max(maxV, v);
                        face.uvcoords.Add(new UVCoord(u, v));
                        face.vertices.Add(geometry.vertices[triangle[index2]]);
                        // imagine calculating these based on the split, hell no, using spells afterwards
                        //if (geometry.normals.Count > 0)
                        //{
                        //    face.normals.Add(geometry.normals[triangle[index2]]);
                        //}
                        //if (geometry.tangents.Count > 0)
                        //{
                        //    face.tangents.Add(geometry.tangents[triangle[index2]]);
                        //}
                        //if (geometry.bitangents.Count > 0)
                        //{
                        //    face.bitangents.Add(geometry.bitangents[triangle[index2]]);
                        //}
                        if (geometry.vertexColors.Count > 0)
                        {
                            face.vertexColors.Add(geometry.vertexColors[triangle[index2]]);
                        }
                    }
                    face.AppendTriangles(new List<Triangle>() { new Triangle((ushort)(face.vertices.Count - 3), (ushort)(face.vertices.Count - 2), (ushort)(face.vertices.Count - 1)) });
                }
                if (face.tangents.Count != 0 && face.vertices.Count != face.tangents.Count)
                {
                    Console.WriteLine("1different! " + shapedesc.name);
                }
                face.RemoveDuplicate(false);

                geometry = new Geometry();
                geometry.SetTriangles(face.GetTriangles());
                geometry.SetVertices(face.GetVertices());
                geometry.SetNormals(face.GetNormals());
                geometry.SetTangents(face.GetTangents());
                geometry.SetBitangents(face.GetBitangents());
                geometry.SetVertexColors(face.GetVertexColors());
                geometry.SetUVCoords(face.GetUVCoords());

                face = new Geometry();

                if (split)
                {
                    float minU = float.MaxValue;
                    float minV = float.MaxValue;
                    float maxU = float.MinValue;
                    float maxV = float.MinValue;
                    List<Triad> triads = new List<Triad>();
                    for (int index = 0; index < geometry.triangles.Count; index++)
                    {
                        for (int index2 = 0; index2 < 3; index2++)
                        {
                            float u = geometry.uvcoords[geometry.triangles[index][index2]][0];
                            float v = geometry.uvcoords[geometry.triangles[index][index2]][1];
                            minU = Math.Min(minU, u);
                            minV = Math.Min(minV, v);
                            maxU = Math.Max(maxU, u);
                            maxV = Math.Max(maxV, v);
                        }
                        triads.Add(new Triad(geometry.triangles[index][0], geometry.triangles[index][1], geometry.triangles[index][2]));
                        triads[triads.Count - 1].FindNeighbours(triads, false, geometry);
                    }

                    if (minU == 0 || minV == 0 || maxU == 1 || maxV == 1)
                    {
                        for (int index = 0; index < triads.Count; index++)
                        {
                            triads[index].FindNeighbours(triads, false, geometry);
                            if (Convert.ToByte(triads[index].ab >= 0) + Convert.ToByte(triads[index].bc >= 0) + Convert.ToByte(triads[index].ac >= 0) > 1)
                            {

                                List<int> plist = new List<int>();
                                List<int> tlist = new List<int>();
                                List<int> vlist = new List<int>();
                                int edges = 0;
                                tlist.Add(index);
                                int current = 0;
                                while (current < tlist.Count)
                                {
                                    int t = tlist[current];
                                    triads[t].FindNeighbours(triads, false, geometry);
                                    for (int index2 = 0; index2 < 3; index2++)
                                    {
                                        if (triads[t][index2] != -1)
                                        {
                                            if (!vlist.Contains(triads[t][index2]))
                                            {
                                                vlist.Add(triads[t][index2]);
                                            }
                                            if (triads[t][index2 + 3] == -1)
                                            {
                                                edges++;
                                                if (!plist.Contains(triads[t][index2]))
                                                {
                                                    plist.Add(triads[t][index2]);
                                                }
                                            }
                                            else
                                            {
                                                if (!tlist.Contains(triads[t][index2 + 3]))
                                                {
                                                    tlist.Add(triads[t][index2 + 3]);
                                                    edges++;
                                                }
                                            }
                                        }
                                    }
                                    current++;
                                }
                                List<Vertex> pts = new List<Vertex>();
                                for (int index2 = 0; index2 < plist.Count; index2++)
                                {
                                    Vertex uv = new Vertex(geometry.uvcoords[plist[index2]][0], geometry.uvcoords[plist[index2]][1]);
                                    if (!pts.Contains(uv))
                                    {
                                        pts.Add(uv);
                                    }
                                }
                                if (pts.Count > 4)
                                {
                                    Vertex center = new Vertex(pts.Average(ppp => ppp.x), pts.Average(ppp => ppp.y));
                                    SortedDictionary<Vertex, int> uvlist = new SortedDictionary<Vertex, int>(new Vertex.SortCounterClockwise());
                                    for (int index2 = 0; index2 < pts.Count; index2++)
                                    {
                                        if (!uvlist.ContainsKey(pts[index2]))
                                        {
                                            pts[index2].cx = center.x;
                                            pts[index2].cy = center.y;
                                            uvlist.Add(pts[index2], index2);
                                        }
                                    }

                                    List<int> xlist = new List<int>();
                                    foreach (KeyValuePair<Vertex, int> keyValuePair in uvlist)
                                    {
                                        xlist.Add(plist[keyValuePair.Value]);
                                    }

                                    for (int index2 = 0; index2 < xlist.Count; index2++)
                                    {
                                        int x0 = index2;
                                        int x1 = x0 + 1;
                                        if (x1 >= xlist.Count)
                                        {
                                            x1 = 0;
                                        }
                                        int x2 = x1 + 1;
                                        if (x2 >= xlist.Count)
                                        {
                                            x2 = 0;
                                        }
                                        int p0 = xlist[x0];
                                        int p1 = xlist[x1];
                                        int p2 = xlist[x2];

                                        if ((geometry.uvcoords[p1][0] == 0 || geometry.uvcoords[p1][0] == 1 || geometry.uvcoords[p1][1] == 0 || geometry.uvcoords[p1][1] == 1) && PointOnLine(geometry.uvcoords[p1], geometry.uvcoords[p0], geometry.uvcoords[p2]))
                                        {
                                            Vector3 v0 = new Vector3(geometry.vertices[p0][0], geometry.vertices[p0][1], geometry.vertices[p0][2]);
                                            Vector3 v1 = new Vector3(geometry.vertices[p1][0], geometry.vertices[p1][1], geometry.vertices[p1][2]);
                                            Vector3 v2 = new Vector3(geometry.vertices[p2][0], geometry.vertices[p2][1], geometry.vertices[p2][2]);
                                            if (Vector3.OnLine(v1, v0, v2))
                                            {
                                                int pc = 0;
                                                float areaOld = 0;
                                                float areaNew = 0;
                                                // trying to prevent errors because of holes
                                                for (int index3 = 0; index3 < tlist.Count; index3++)
                                                {
                                                    pc = 0;
                                                    for (int index4 = 0; index4 < 3; index4++)
                                                    {
                                                        if (triads[tlist[index3]][index4] == p0)
                                                        {
                                                            //pc++;
                                                        }
                                                        if (triads[tlist[index3]][index4] == p1)
                                                        {
                                                            List<UVCoord> uv = new List<UVCoord>();
                                                            uv.Add(geometry.uvcoords[triads[tlist[index3]][0]]);
                                                            uv.Add(geometry.uvcoords[triads[tlist[index3]][1]]);
                                                            uv.Add(geometry.uvcoords[triads[tlist[index3]][2]]);
                                                            areaOld += UVCoord.Area(uv[0], uv[1], uv[2]);
                                                            uv[index4] = geometry.uvcoords[p0];
                                                            areaNew += UVCoord.Area(uv[0], uv[1], uv[2]);
                                                        }
                                                    }
                                                }
                                                if (Utils.QHigh(areaOld) == Utils.QHigh(areaNew))
                                                {
                                                    int min = triads.Count;
                                                    for (int index3 = 0; index3 < triads.Count; index3++)
                                                    {
                                                        pc = 0;
                                                        for (int index4 = 0; index4 < 3; index4++)
                                                        {
                                                            if (triads[index3][index4] == p0)
                                                            {
                                                                pc++;
                                                                min = Math.Min(min, index3);
                                                            }
                                                            else if (triads[index3][index4] == p1)
                                                            {
                                                                triads[index3][index4] = p0;
                                                                pc++;
                                                                min = Math.Min(min, index3);
                                                            }
                                                        }
                                                        if (pc >= 2)
                                                        {
                                                            triads[index3] = new Triad(-1, -1, -1);
                                                        }
                                                    }
                                                    if (v1[0] != v0[0] || v1[1] != v0[1] || v1[2] != v0[2])
                                                    {
                                                        for (int index3 = 0; index3 < geometry.vertices.Count; index3++)
                                                        {
                                                            if (geometry.vertices[index3][0] == v1[0] && geometry.vertices[index3][1] == v1[1] && geometry.vertices[index3][2] == v1[2])
                                                            {
                                                                geometry.vertices[index3] = Vector3.MoveToLine(v1, v0, v2);
                                                            }
                                                        }
                                                    }
                                                    index = min;
                                                    break;
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }

                    geometry.triangles = new List<Triangle>();
                    for (int index = 0; index < triads.Count; index++)
                    {
                        if (triads[index].a != -1 && triads[index].b != -1 && triads[index].c != -1)
                        {
                            geometry.triangles.Add(new Triangle(triads[index].a, triads[index].b, triads[index].c));
                        }
                    }
                }

                if (geometry.tangents.Count != 0 && geometry.vertices.Count != geometry.tangents.Count)
                {
                    Console.WriteLine("2different! " + shapedesc.name);
                }
                geometry.RemoveDuplicate(false);
                geometry.FaceNormals();
                geometry.SmoothNormals(60f, 0.001f);
                geometry.UpdateTangents(hadtangents);

                if (AtlasList.Contains(before))
                {
                    AtlasList.AfterAdd(before, geometry.triangles.Count);
                }

                if (verbose)
                {
                    float minU = float.MaxValue;
                    float minV = float.MaxValue;
                    float maxU = float.MinValue;
                    float maxV = float.MinValue;
                    for (int index = 0; index < geometry.uvcoords.Count; index++)
                    {
                        minU = Math.Min(minU, geometry.uvcoords[index][0]);
                        minV = Math.Min(minV, geometry.uvcoords[index][1]);
                        maxU = Math.Max(maxU, geometry.uvcoords[index][0]);
                        maxV = Math.Max(maxV, geometry.uvcoords[index][1]);
                    }
                    if (maxU > 1 || minU < 0 || maxV > 1 || minV < 0)
                    {
                        logFile.WriteLog("UV outside " + shapedesc.staticName + " " + shapedesc.name + " " + shapedesc.textures[0] + " (" + (maxU - minU) + ", " + (maxV - minV) + ")");
                    }
                }
            }
            GeometryList.Set(key, geometry);
            return geometry;
        }

        void CreateTriangles(List<Vertex> pts, List<int> idx, bool createnew, bool front, float scaleU, float scaleV)
        {
            Vertex center = new Vertex(pts.Average(ppp => ppp.x), pts.Average(ppp => ppp.y));
            SortedDictionary<Vertex, int> uvlist = new SortedDictionary<Vertex, int>(new Vertex.SortCounterClockwise());
            for (int index = 0; index < pts.Count; index++)
            {
                pts[index].cx = center.x;
                pts[index].cy = center.y;
                uvlist.Add(pts[index], idx[index]);
            }

            idx = new List<int>();
            pts = new List<Vertex>();
            foreach (KeyValuePair<Vertex, int> keyValuePair in uvlist)
            {
                pts.Add(keyValuePair.Key);
                idx.Add(keyValuePair.Value);
            }

            if (pts.Count >= 3)
            {
                Triangulator angulator = new Triangulator();
                List<Triad> triangles = angulator.Triangulation(pts, true);
                for (int index = 0; index < triangles.Count; index++)
                {
                    List<ushort> t = new List<ushort>();
                    if (createnew)
                    {
                        t.Add((ushort)this.CreateDuplicate(idx[triangles[index].c]));
                        t.Add((ushort)this.CreateDuplicate(idx[triangles[index].b]));
                        t.Add((ushort)this.CreateDuplicate(idx[triangles[index].a]));
                    }
                    else
                    {
                        t.Add((ushort)idx[triangles[index].c]);
                        t.Add((ushort)idx[triangles[index].b]);
                        t.Add((ushort)idx[triangles[index].a]);
                    }
                    if (front == UVCoord.Clockwise(this.uvcoords[t[0]], this.uvcoords[t[1]], this.uvcoords[t[2]]))
                    {
                        this.triangles.Add(new Triangle(t[0], t[1], t[2]));
                    }
                    else
                    {
                        this.triangles.Add(new Triangle(t[0], t[2], t[1]));
                    }
                    if (createnew)
                    {
                        this.uvcoords[t[0]] = new UVCoord(this.uvcoords[t[0]][0] / scaleU, this.uvcoords[t[0]][1] / scaleV);
                        this.uvcoords[t[1]] = new UVCoord(this.uvcoords[t[1]][0] / scaleU, this.uvcoords[t[1]][1] / scaleV);
                        this.uvcoords[t[2]] = new UVCoord(this.uvcoords[t[2]][0] / scaleU, this.uvcoords[t[2]][1] / scaleV);
                    }
                }
            }
        }

        private bool PointOnLine(UVCoord p, UVCoord a, UVCoord b)
        {
            double dxc = p[0] - a[0];
            double dyc = p[1] - a[1];
            double dxl = b[0] - a[0];
            double dyl = b[1] - a[1];
            double cross = dxc * dyl - dyc * dxl;
            if (cross != 0)
            {
                return false;
            }
            if (Math.Abs(dxl) >= Math.Abs(dyl))
            {
                return dxl > 0 ? a[0] <= p[0] && p[0] <= b[0] : b[0] <= p[0] && p[0] <= a[0];
            }
            else
            {
                return dyl > 0 ? a[1] <= p[1] && p[1] <= b[1] : b[1] <= p[1] && p[1] <= a[1];
            }
        }

        private Vector3 SplitUV(UVCoord p1, UVCoord p2)
        {
            double a = p2[0] - p1[0];
            double b = p2[1] - p1[1];
            double c = Math.Sqrt(a * a + b * b);
            int du = Math.Sign(a);
            int dv = Math.Sign(b);
            double a1 = 0;
            if (Math.Floor(p1[0]) != Math.Floor(p2[0]) && Math.Ceiling(p1[0]) != Math.Ceiling(p2[0]))
            {
                if (du == 1)
                {
                    a1 = Math.Ceiling(p1[0]) - p1[0];
                }
                else
                {
                    a1 = Math.Floor(p1[0]) - p1[0];
                }
                if (a1 == 0)
                {
                    a1 = 1;
                }
            }
            double b1 = 0;
            if (Math.Floor(p1[1]) != Math.Floor(p2[1]) && Math.Ceiling(p1[1]) != Math.Ceiling(p2[1]))
            {
                if (dv == 1)
                {
                    b1 = Math.Ceiling(p1[1]) - p1[1];
                }
                else
                {
                    b1 = Math.Floor(p1[1]) - p1[1];
                }
                if (b1 == 0)
                {
                    b1 = 1;
                }
            }
            a1 = Math.Abs(a1) * du;
            b1 = Math.Abs(b1) * dv;
            double cu = 0;
            double cv = 0;
            if (a == 0)
            {
                cu = 0;
            }
            else
            {
                cu = a1 * c / a;
            }
            if (b == 0)
            {
                cv = 0;
            }
            else
            {
                cv = b1 * c / b;
            }
            double u = 0;
            double v = 0;
            if (cu != 0 || cv != 0)
            {
                if ((cu != 0 && cu < cv) || cv == 0)
                {
                    b1 = (a1 * b / a);
                    u = p1[0] + a1;
                    v = p1[1] + b1;
                }
                else if (cv != 0)
                {
                    a1 = (b1 * a / b);
                    u = p1[0] + a1;
                    v = p1[1] + b1;
                }
            }
            double d = Math.Sqrt(a1 * a1 + b1 * b1) / c;
            return new Vector3((float)u, (float)v, (float)d);
        }
    }
}
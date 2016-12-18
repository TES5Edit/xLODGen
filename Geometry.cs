using LODGenerator.Common;
using LODGenerator.NifMain;
using System;
using System.Collections.Generic;

namespace LODGenerator
{
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

        public BSSubIndexTriShape ToBSSubIndexTriShape()
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
            data.SetVertices(this.GetVertices());
            data.SetUVCoords(this.GetUVCoords());
            data.SetNormals(this.GetNormals());
            data.SetTangents(this.GetTangents());
            data.SetBitangents(this.GetBitangents());
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
    }
}
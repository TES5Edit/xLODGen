using LODGenerator.Common;
using LODGenerator.NifMain;
using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using StringList = System.Collections.Generic.List<string>;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Linq;
using System.Globalization;
using System.Text.RegularExpressions;

namespace LODGenerator
{
    public class LODApp
    {
        private string worldspaceName;
        private string outputDir;
        private string terrainDir;
        private string gameDir;
        private LogFile logFile;
        public List<QuadDesc> quadList;
        public bool verbose;
        public string texturesListFile;
        public bool fixTangents;
        public bool generateTangents;
        public bool generateVertexColors;
        public bool mergeShapes;
        public bool useHDFlag;
        public bool useOptimizer;
        public bool useFadeNode;
        public bool removeUnseenFaces;
        public bool removeUnderwaterFaces;
        public float removeUnseenZShift;
        public bool ignoreMaterial;
        public bool alphaDoublesided;
        public bool useAlphaThreshold;
        public bool useBacklightPower;
        public bool useDecalFlag;
        public bool dontGroup;
        public bool skyblivionTexPath;
        public bool removeBlocks;
        public int flatLODLevelLODFlag;
        public float globalScale;
        public float eliminateSize;
        public int southWestX;
        public int southWestY;
        public float atlasToleranceMin;
        public float atlasToleranceMax;
        public int lodLevelToGenerate;
        public int lodX;
        public int lodY;
        public StringList ignoreTransRot;
        public StringList HDTextureList;
        public StringList notHDTextureList;
        public StringList HDMeshList;
        public StringList notHDMeshList;
        public StringList PassThruMeshList;
        private int quadLevel;
        private int quadIndex;
        private float quadOffset;

        private static readonly Random getrandom = new Random();
        private static readonly object syncLock = new object();

        public static float GetRandomNumber()
        {
            lock (syncLock)
            {
                return (float)getrandom.NextDouble();
            }
        }


        public LODApp(string wsn, string od, string sd, LogFile lf)
        {
            this.worldspaceName = wsn;
            this.outputDir = od;
            this.terrainDir = od + "\\..";
            this.gameDir = sd;
            this.logFile = lf;
            this.quadList = new List<QuadDesc>();
            this.verbose = false;
            this.fixTangents = false;
            this.generateTangents = true;
            this.generateVertexColors = true;
            this.mergeShapes = true;
            this.useHDFlag = false;
            this.useFadeNode = false;
            this.removeUnseenFaces = false;
            this.removeUnseenZShift = 0;
            this.globalScale = 1f;
            this.eliminateSize = 0.0f;
            this.lodLevelToGenerate = Int32.MinValue;
        }

        private int cellquad(float pos, int southWest)
        {
            // properly address offset world coordinates to match CK
            double xoffset = pos - ((southWest % this.quadLevel) * 4096);
            int num1 = (int)((double)xoffset / (double)this.quadOffset);
            if ((double)xoffset < 0.0)
            {
                --num1;
            }
            int num3 = num1 * this.quadLevel;
            if (southWest % this.quadLevel != 0)
            {
                num3 += southWest % this.quadLevel;
            }
            return num3;
        }

        private List<QuadDesc> SortMeshesIntoQuads(List<StaticDesc> statics)
        {
            List<QuadDesc> list = new List<QuadDesc>();
            bool flag = false;
            foreach (StaticDesc staticDesc in statics)
            {
                int num3 = cellquad(staticDesc.x, this.southWestX);
                int num4 = cellquad(staticDesc.y, this.southWestY);
                for (int index = 0; index < list.Count; ++index)
                {
                    if (num3 == list[index].x && num4 == list[index].y)
                    {
                        list[index].statics.Add(staticDesc);
                        flag = true;
                        break;
                    }
                }
                if (!flag)
                {
                    QuadDesc quadDesc = new QuadDesc(true);
                    quadDesc.x = num3;
                    quadDesc.y = num4;
                    //quadDesc.statics = new List<StaticDesc>();
                    quadDesc.statics.Add(staticDesc);
                    //quadDesc.outValues = new OutDesc();
                    quadDesc.outValues.totalTriCount = 0;
                    quadDesc.outValues.reducedTriCount = 0;
                    //quadDesc.textureBlockIndex = new Dictionary<string, int>();
                    //quadDesc.textureBlockIndexPassThru = new Dictionary<string, int>();
                    //quadDesc.dataBlockIndex = new Dictionary<string, int>();
                    //
                    quadDesc.shaderBlockIndex = new Dictionary<string, int>();
                    list.Add(quadDesc);
                }
                flag = false;
            }
            return list;
        }

        private void GenerateMultibound(NiFile file, BSMultiBoundNode node, QuadDesc curQuad, BBox bb)
        {
            if ((Game.Mode != "fo3") || (Game.Mode == "fo3" && node.GetMultiBound() == -1))
            {
                BSMultiBound bsMultiBound = new BSMultiBound();
                node.SetMultiBound(file.AddBlock((NiObject)bsMultiBound));
                BSMultiBoundAABB bsMultiBoundAabb = new BSMultiBoundAABB();
                bsMultiBound.SetData(file.AddBlock((NiObject)bsMultiBoundAabb));
                float num1 = (float)curQuad.x * 4096f;
                float num2 = (float)curQuad.y * 4096f;
                bsMultiBoundAabb.SetPosition(new Vector3((float)(((double)num1 + (double)bb.px1 + ((double)num1 + (double)bb.px2)) / 2.0), (float)(((double)num2 + (double)bb.py1 + ((double)num2 + (double)bb.py2)) / 2.0), (float)(((double)bb.pz1 + (double)bb.pz2) / 2.0)));
                bsMultiBoundAabb.SetExtent(new Vector3((float)(((double)bb.px2 - (double)bb.px1) / 2.0), (float)(((double)bb.py2 - (double)bb.py1) / 2.0), (float)(((double)bb.pz2 - (double)bb.pz1) / 2.0)));
            }
        }

        private void GenerateSegments(QuadDesc curQuad, ref ShapeDesc shape)
        {
            // use x, y of object instead of boundingbox center to determine segment
            SegmentDesc segmentDesc = new SegmentDesc();
            if (this.quadLevel == 4)
            {
                int num1 = (int)((double)shape.x / ((double)this.quadOffset / 4.0));
                int num2 = (int)((double)shape.y / ((double)this.quadOffset / 4.0));
                if (num1 > 3)
                    num1 = 3;
                if (num1 < 0)
                    num1 = 0;
                if (num2 > 3)
                    num2 = 3;
                if (num2 < 0)
                    num2 = 0;
                segmentDesc.id = 4 * num1 + num2;
            }
            else
            {
                segmentDesc.id = 0;
            }
            segmentDesc.startTriangle = 0U;
            segmentDesc.numTriangles = shape.geometry.GetNumTriangles();
            shape.segments = new List<SegmentDesc>();
            shape.segments.Add(segmentDesc);
        }

        private List<ShapeDesc> IterateNodes(QuadDesc quad, StaticDesc stat, int level, NiFile file, NiNode parentNode, Matrix44 parentTransform, float parentScale)
        {
            List<ShapeDesc> list = new List<ShapeDesc>();
            if (parentNode == null || parentNode.IsHidden())
            {
                return list;
            }
            int nameIndex = parentNode.GetNameIndex();
            string str = nameIndex != -1 ? file.GetStringAtIndex(nameIndex) : parentNode.GetName();
            if (str != null && str.ToLower(CultureInfo.InvariantCulture).Contains("editormarker"))
            {
                return list;
            }
            Matrix44 parentTransform1 = parentNode.GetTransform() * parentTransform;
            if (ignoreTransRot.Any(stat.staticModels[level].Contains) || Game.Mode.Contains("convert"))
            {
                parentTransform1 = parentTransform;
            }
            float parentScale1 = parentNode.GetScale() * parentScale;
            uint numChildren = parentNode.GetNumChildren();
            int numGeom = -1;
            for (int index = 0; (long)index < (long)numChildren; ++index)
            {
                if (parentNode.GetChildAtIndex(index) != -1)
                {
                    NiObject blockAtIndex = file.GetBlockAtIndex(parentNode.GetChildAtIndex(index));
                    if (blockAtIndex != null)
                    {
                        if (blockAtIndex.IsDerivedType("NiNode"))
                        {
                            list.AddRange((IEnumerable<ShapeDesc>)this.IterateNodes(quad, stat, level, file, (NiNode)blockAtIndex, parentTransform1, parentScale1));
                        }
                        else if (blockAtIndex.IsDerivedType("NiTriBasedGeom"))
                        {
                            numGeom++;
                            NiTriBasedGeom geom = (NiTriBasedGeom)blockAtIndex;
                            if (geom.GetSkinInstance() == -1)
                            {
                                ShapeDesc shapeDesc = new ShapeDesc();
                                if (!dontGroup && (stat.staticFlags & 1) == 1)
                                {
                                    shapeDesc = this.GroupShape(quad, stat, file, geom, parentTransform1, parentScale1, numGeom);
                                }
                                else
                                {
                                    shapeDesc = this.TransformShape(quad, stat, file, geom, parentTransform1, parentScale1, numGeom);
                                }
                                if (shapeDesc != null && shapeDesc.geometry != null)
                                {
                                    //logFile.WriteLog(shapeDesc.material + shapeDesc.shaderType + shapeDesc.shaderHash);
                                    list.Add(shapeDesc);
                                }
                            }
                        }
                        else if (blockAtIndex.IsDerivedType("BSTriShape"))
                        {
                            numGeom++;
                            BSTriShape geomOld = (BSTriShape)blockAtIndex;
                            NiTriBasedGeom geomNew = new NiTriBasedGeom();
                            if (index < numChildren)
                            {
                                geomNew.SetFlags(14);
                                geomNew.SetTranslation(geomOld.GetTranslation());
                                geomNew.SetRotation(geomOld.GetRotation());
                                geomNew.SetScale(geomOld.GetScale());
                                geomNew.SetNumProperties(geomOld.GetNumProperties());
                                for (int index2 = 0; (long)index2 < (long)geomOld.GetNumProperties(); ++index2)
                                {
                                    geomNew.SetProperties(geomOld.GetProperty(index2));
                                }
                                geomNew.SetData(parentNode.GetChildAtIndex(index));
                                geomNew.SetBSProperty(0, geomOld.GetBSProperty(0));
                                geomNew.SetBSProperty(1, geomOld.GetBSProperty(1));
                                //geomNew.SetGeo(geomOld.GetGeom());
                            }
                            ShapeDesc shapeDesc = new ShapeDesc();
                            if (!dontGroup && (stat.staticFlags & 1) == 1)
                            {
                                shapeDesc = this.GroupShape(quad, stat, file, geomNew, parentTransform1, parentScale1, numGeom);
                            }
                            else
                            {
                                shapeDesc = this.TransformShape(quad, stat, file, geomNew, parentTransform1, parentScale1, numGeom);
                            }
                            if (shapeDesc != null && shapeDesc.geometry != null)
                            {
                                //logFile.WriteLog(shapeDesc.material + shapeDesc.shaderType + shapeDesc.shaderHash);
                                list.Add(shapeDesc);
                            }
                        }
                    }
                    else
                    {
                        //nothing
                    }
                }
            }
            return list;
        }

        private ShapeDesc GroupShape(QuadDesc quad, StaticDesc stat, NiFile file, NiTriBasedGeom geom, Matrix44 parentTransform, float parentScale, int numGeom)
        {
            BBox bbox = new BBox(float.MaxValue, float.MinValue, float.MaxValue, float.MinValue, float.MaxValue, float.MinValue);
            ShapeDesc shapedesc = new ShapeDesc(this.gameDir, file, geom, stat, this.quadIndex, PassThruMeshList, skyblivionTexPath, useOptimizer, fixTangents, useDecalFlag, false, verbose, this.logFile);

            parentTransform *= stat.transrot;
            parentScale *= stat.transscale;

            Matrix33 matrix33_1 = new Matrix33(true);
            Matrix33 matrix33_2 = new Matrix33(true);
            Matrix33 matrix33_3 = new Matrix33(true);
            matrix33_1.SetRotationX(Utils.ToRadians(-stat.rotX));
            matrix33_2.SetRotationY(Utils.ToRadians(-stat.rotY));
            matrix33_3.SetRotationZ(Utils.ToRadians(-stat.rotZ));
            Matrix44 matrix44 = new Matrix44(new Matrix33(true) * matrix33_1 * matrix33_2 * matrix33_3, new Vector3(stat.x, stat.y, stat.z), 1f);

            List<Vector3> vertices = new List<Vector3>(shapedesc.geometry.GetVertices());
            if (geom.GetClassName() == "NiTriStrips")
            {
                List<int> extradatalist = geom.GetExtraData();
                if (extradatalist.Count == 1)
                {
                    NiBinaryExtraData extradata = (NiBinaryExtraData)file.GetBlockAtIndex(extradatalist[0]);
                    shapedesc.geometry.SetTangents(extradata.GetTangents());
                    shapedesc.geometry.SetBitangents(extradata.GetBitangents());
                }
            }
            for (int index = 0; index < vertices.Count; ++index)
            {
                bbox.GrowByVertex(vertices[index]);
                if (Game.Mode != "merge4" && Game.Mode != "merge5")
                {
                    vertices[index] /= (float)this.quadLevel;
                }
            }
            shapedesc.geometry.SetVertices(vertices);
            shapedesc.x = stat.x;
            shapedesc.y = stat.y;
            shapedesc.boundingBox = bbox;
            shapedesc.enableParent = stat.enableParent;
            shapedesc.isHighDetail = false;
            // only snow/ash shader care about HD flag -> use vertex color alpha for shader
            if (stat.materialName != "" && useHDFlag)
            {
                if ((stat.staticFlags & 131072) == 131072)
                {
                    if (HDMeshList.Any(stat.staticModels[this.quadIndex].ToLower(CultureInfo.InvariantCulture).Contains))
                    {
                        shapedesc.isHighDetail = true;
                    }
                    else if (notHDMeshList.Any(stat.staticModels[this.quadIndex].ToLower(CultureInfo.InvariantCulture).Contains))
                    {
                        shapedesc.isHighDetail = false;
                    }
                    else if (this.quadLevel == 4)
                    {
                        shapedesc.isHighDetail = true;
                    }
                }
            }
            if (shapedesc.name != "")
            {
                string key = (stat.refID + "_" + numGeom + "_" + shapedesc.name).ToLower(CultureInfo.InvariantCulture);
                if (AltTextureList.Contains(key))
                {
                    AltTextureDesc altTexDesc = new AltTextureDesc();
                    altTexDesc = AltTextureList.Get(key);
                    if ((shapedesc.textures[0].Contains("\\lod") && altTexDesc.textures[0].Contains("\\lod")) || ((!shapedesc.textures[0].Contains("\\lod") && !altTexDesc.textures[0].Contains("\\lod"))))
                    {
                        string s = "";
                        for (int index = 0; index < altTexDesc.textures.Count(); index++)
                        {
                            shapedesc.textures[index] = altTexDesc.textures[index];
                            s += altTexDesc.textures[index];
                        }
                        if (s == "")
                        {
                            return (ShapeDesc)null;
                        }
                    }
                }
            }
            if (shapedesc.isPassThru)
            {
                shapedesc.hasVertexColor = shapedesc.geometry.HasVertexColors();
            }
            else
            {
                //shapedesc.geometry = shapedesc.geometry.ReUV(shapedesc, shapedesc.textures[0], logFile, verbose);
                //shapedesc = shapedesc.ReUV(logFile, verbose);

                if (AtlasList.Contains(shapedesc.textures[0]))
                {
                    if (this.UVAtlas(shapedesc.geometry, shapedesc.textures[0], stat))
                    {
                        string[] strArray = new string[10] { "", "", "", "", "", "", "", "", "", "" };
                        strArray[0] = AtlasList.Get(shapedesc.textures[0]).AtlasTexture;
                        strArray[1] = AtlasList.Get(shapedesc.textures[0]).AtlasTextureN;
                        if (shapedesc.textures[2] != "")
                        {
                            strArray[2] = AtlasList.Get(shapedesc.textures[2]).AtlasTexture;
                        }
                        if (Game.Mode == "fo4")
                        {
                            strArray[7] = AtlasList.Get(shapedesc.textures[0]).AtlasTextureS;
                        }
                        shapedesc.textures = strArray;
                        shapedesc.TextureClampMode = 0U;
                        shapedesc.isHighDetail = false;
                    }
                }
                else
                {
                    if (useOptimizer && quadLevel != 4 && shapedesc.textures[0].ToLower(CultureInfo.InvariantCulture).Contains("mountainslab01"))
                    {
                        string[] strArray = new string[10] { "", "", "", "", "", "", "", "", "", "" };
                        strArray[0] = "textures\\landscape\\mountains\\mountainslab02.dds";
                        strArray[1] = "textures\\landscape\\mountains\\mountainslab02_n.dds";
                        shapedesc.textures = strArray;
                    }
                    if (notHDTextureList.Any(shapedesc.textures[0].Contains))
                    {
                        if (this.verbose && shapedesc.textures[0].Contains("dyndolodtreelod"))
                        {
                            logFile.WriteLog("No atlas for " + shapedesc.textures[0] + " in " + stat.staticModels[this.quadIndex]);
                        }
                    }
                    else
                    {
                        if (!useHDFlag && stat.materialName != "")
                        {
                            shapedesc.isHighDetail = true;
                        }
                    }
                }
                if (notHDTextureList.Any(shapedesc.textures[0].Contains))
                {
                    shapedesc.isHighDetail = false;
                }
                if (HDTextureList.Any(shapedesc.textures[0].Contains) && stat.materialName != "")
                {
                    shapedesc.isHighDetail = true;
                }
                if (shapedesc.textures[0].Contains("dyndolod\\lod"))
                {
                    shapedesc.TextureClampMode = 0U;
                    shapedesc.isHighDetail = false;
                }
            }
            if (!shapedesc.isPassThru && (!this.generateVertexColors || (this.quadLevel != 4 && !shapedesc.isHighDetail)))
            {
                shapedesc.hasVertexColor = false;
            }
            quad.outValues.totalTriCount += shapedesc.geometry.GetNumTriangles();
            quad.outValues.reducedTriCount += shapedesc.geometry.GetNumTriangles();
            this.GenerateSegments(quad, ref shapedesc);
            return shapedesc;
        }

        private ShapeDesc TransformShape(QuadDesc quad, StaticDesc stat, NiFile file, NiTriBasedGeom geom, Matrix44 parentTransform, float parentScale, int numGeom)
        {
            BBox bbox = new BBox(float.MaxValue, float.MinValue, float.MaxValue, float.MinValue, float.MaxValue, float.MinValue);
            ShapeDesc shapedesc = new ShapeDesc(this.gameDir, file, geom, stat, this.quadIndex, PassThruMeshList, skyblivionTexPath, useOptimizer, fixTangents, useDecalFlag, false, verbose, this.logFile);

            if (shapedesc.name != "")
            {
                string key = (stat.refID + "_" + numGeom + "_" + shapedesc.name).ToLower(CultureInfo.InvariantCulture);
                if (AltTextureList.Contains(key))
                {
                    AltTextureDesc altTexDesc = new AltTextureDesc();
                    altTexDesc = AltTextureList.Get(key);
                    if ((shapedesc.textures[0].Contains("\\lod") && altTexDesc.textures[0].Contains("\\lod")) || ((!shapedesc.textures[0].Contains("\\lod") && !altTexDesc.textures[0].Contains("\\lod"))))
                    {
                        string s = "";
                        for (int index = 0; index < altTexDesc.textures.Count(); index++)
                        {
                            shapedesc.textures[index] = altTexDesc.textures[index];
                            s += altTexDesc.textures[index];
                        }
                        if (s == "")
                        {
                            return (ShapeDesc)null;
                        }
                    }
                }
            }

            if (AtlasList.Contains(shapedesc.textures[0]))
            {
                bool ok = false;
                for (int index = 0; index < shapedesc.textures.Count(); index++)
                {
                    if (Game.Mode == "tes5" || Game.Mode == "sse" || Game.Mode == "fo4")
                    {
                        if (Game.Mode != "fo4" && (shapedesc.textures[index] == "" || shapedesc.textures[index] == shapedesc.textures[0] || shapedesc.textures[index] == shapedesc.textures[1]))
                        {
                            ok = true;
                        }
                        else if (Game.Mode == "fo4" && (shapedesc.textures[index] == "" || shapedesc.textures[index] == shapedesc.textures[0] || shapedesc.textures[index] == shapedesc.textures[1] || shapedesc.textures[index] == shapedesc.textures[7]))
                        {
                            ok = true;
                        }
                        else
                        {
                            ok = false;
                            break;
                        }
                    }
                    else
                    {
                        ok = true;
                    }
                }

                if (Game.Mode == "tes5" || Game.Mode == "sse")
                {
                    shapedesc = shapedesc.ReUV(logFile, verbose);
                    if (!AtlasList.Contains(shapedesc.textures[0]))
                    {
                        ok = false;
                    }
                }

                if (ok && this.UVAtlas(shapedesc.geometry, shapedesc.textures[0], stat))
                {
                    string[] strArray = new string[10] { "", "", "", "", "", "", "", "", "", "" };
                    for (int index = 0; index < shapedesc.textures.Count(); index++)
                    {
                        if (shapedesc.textures[index] == shapedesc.textures[0])
                        {
                            strArray[index] = AtlasList.Get(shapedesc.textures[0]).AtlasTexture;
                        }
                        if (shapedesc.textures[index] == shapedesc.textures[1])
                        {
                            strArray[index] = AtlasList.Get(shapedesc.textures[0]).AtlasTextureN;
                        }
                        if (Game.Mode == "fo4" && shapedesc.textures[index] == shapedesc.textures[7])
                        {
                            strArray[index] = AtlasList.Get(shapedesc.textures[0]).AtlasTextureS;
                        }
                    }
                    shapedesc.textures = strArray;
                    shapedesc.TextureClampMode = 0U;
                    shapedesc.isHighDetail = false;
                }
            }

            if (verbose && !shapedesc.geometry.HasVertexColors() && shapedesc.hasVertexColor)
            {
                if (!stat.staticModels[this.quadIndex].Contains("glacierrubbletrim0"))
                {
                    logFile.WriteLog("Vertex Colors Flag, but no vertex colors in " + stat.staticModels[this.quadIndex]);
                }
            }
            parentTransform *= stat.transrot;
            parentScale *= stat.transscale;

            float _x = stat.x - (float)quad.x * 4096f;
            float _y = stat.y - (float)quad.y * 4096f;
            Matrix33 matrix33_1 = new Matrix33(true);
            Matrix33 matrix33_2 = new Matrix33(true);
            Matrix33 matrix33_3 = new Matrix33(true);
            matrix33_1.SetRotationX(Utils.ToRadians(-stat.rotX));
            matrix33_2.SetRotationY(Utils.ToRadians(-stat.rotY));
            matrix33_3.SetRotationZ(Utils.ToRadians(-stat.rotZ));
            Matrix44 matrix44 = new Matrix44(new Matrix33(true) * matrix33_1 * matrix33_2 * matrix33_3, new Vector3(_x, _y, stat.z), 1f);

            List<Triangle> triangles = new List<Triangle>(shapedesc.geometry.GetTriangles());
            List<Vector3> vertices = new List<Vector3>(shapedesc.geometry.GetVertices());
            List<Vector3> normals = new List<Vector3>(shapedesc.geometry.GetNormals());
            List<Vector3> tangents = new List<Vector3>(shapedesc.geometry.GetTangents());
            List<Vector3> bitangents = new List<Vector3>(shapedesc.geometry.GetBitangents());
            List<UVCoord> uvcoords = new List<UVCoord>(shapedesc.geometry.GetUVCoords());

            for (int index = 0; index < vertices.Count; ++index)
            {
                vertices[index] *= geom.GetScale() * parentScale;
                vertices[index] *= geom.GetTransform() * parentTransform;
                vertices[index] *= stat.scale;
                vertices[index] *= matrix44;
                if (shapedesc.geometry.HasNormals())
                {
                    normals[index] *= parentTransform.RemoveTranslation() * geom.GetTransform().RemoveTranslation();
                    normals[index] *= matrix44.RemoveTranslation();
                    if (this.generateTangents)
                    {
                        // adjust tangents as well
                        if (tangents.Count != 0)
                        {
                            tangents[index] *= parentTransform.RemoveTranslation() * geom.GetTransform().RemoveTranslation();
                            tangents[index] *= matrix44.RemoveTranslation();
                        }
                        // adjust bitangents as well
                        if (bitangents.Count != 0)
                        {
                            bitangents[index] *= parentTransform.RemoveTranslation() * geom.GetTransform().RemoveTranslation();
                            bitangents[index] *= matrix44.RemoveTranslation();
                        }
                    }
                }
                bbox.GrowByVertex(vertices[index]);
                if (Game.Mode != "merge4" && Game.Mode != "merge5")
                {
                    vertices[index] /= (float)this.quadLevel;
                }
            }

            shapedesc.geometry.SetVertices(vertices);
            if (shapedesc.geometry.HasNormals())
            {
                shapedesc.geometry.SetNormals(normals);
            }
            //if (shapedesc.geometry.HasTangents())
            //{
            //    shapedesc.geometry.SetTangents(tangents);
            //}
            //if (shapedesc.geometry.HasBitangents())
            //{
            //    shapedesc.geometry.SetBitangents(bitangents);
            //}

            // relative x, y for segment
            shapedesc.x = _x;
            shapedesc.y = _y;
            shapedesc.boundingBox = bbox;

            if (shapedesc.material == "")
            {
                shapedesc.material = stat.materialName;
            }
            shapedesc.enableParent = stat.enableParent;
            shapedesc.isHighDetail = false;
            if (useHDFlag && stat.materialName != "")
            {
                if ((stat.staticFlags & 131072) == 131072)
                {
                    if (HDMeshList.Any(stat.staticModels[this.quadIndex].ToLower(CultureInfo.InvariantCulture).Contains))
                    {
                        shapedesc.isHighDetail = true;
                    }
                    else if (notHDMeshList.Any(stat.staticModels[this.quadIndex].ToLower(CultureInfo.InvariantCulture).Contains))
                    {
                        shapedesc.isHighDetail = false;
                    }
                    else
                    {
                        shapedesc.isHighDetail = true;
                    }
                }
            }

            if (shapedesc.isPassThru)
            {
                shapedesc.hasVertexColor = shapedesc.geometry.HasVertexColors();
            }
            else
            {
                if (useOptimizer && quadLevel != 4 && shapedesc.textures[0].ToLower(CultureInfo.InvariantCulture).Contains("mountainslab01"))
                {
                    string[] strArray = new string[10] { "", "", "", "", "", "", "", "", "", "" };
                    strArray[0] = "textures\\landscape\\mountains\\mountainslab02.dds";
                    strArray[1] = "textures\\landscape\\mountains\\mountainslab02_n.dds";
                    shapedesc.textures = strArray;
                }
                if (notHDTextureList.Any(shapedesc.textures[0].Contains))
                {
                    if (this.verbose && shapedesc.textures[0].Contains("dyndolodtreelod"))
                    {
                        logFile.WriteLog("No atlas for " + shapedesc.textures[0] + " in " + stat.staticModels[this.quadIndex]);
                    }
                }
                else
                {
                    if (!useHDFlag && stat.materialName != "")
                    {
                        shapedesc.isHighDetail = true;
                    }
                }

                if (notHDTextureList.Any(shapedesc.textures[0].Contains))
                {
                    shapedesc.isHighDetail = false;
                }
                if (HDTextureList.Any(shapedesc.textures[0].Contains) && stat.materialName != "")
                {
                    shapedesc.isHighDetail = true;
                }
                if (shapedesc.textures[0].Contains("dyndolod\\lod"))
                {
                    shapedesc.TextureClampMode = 0U;
                    shapedesc.isHighDetail = false;
                }
            }
            if (this.generateTangents && (!useOptimizer || (useOptimizer && (this.quadLevel == 4 || shapedesc.isHighDetail || shapedesc.isPassThru))))
            {
                shapedesc.geometry.SetTangents(tangents);
                shapedesc.geometry.SetBitangents(bitangents);
            }
            else
            {
                if (!Game.Mode.Contains("merge") && !Game.Mode.Contains("convert"))
                {
                    shapedesc.geometry.SetTangents(new List<Vector3>());
                    shapedesc.geometry.SetBitangents(new List<Vector3>());
                }
            }
            if (!shapedesc.isPassThru && (!this.generateVertexColors || (this.quadLevel != 4 && !shapedesc.isHighDetail)))
            {
                shapedesc.hasVertexColor = false;
            }
            if (this.removeUnseenFaces && quad.hasTerrainVertices)
            {
                this.RemoveUnseenFaces(quad, shapedesc);
                if ((int)shapedesc.geometry.GetNumTriangles() == 0)
                {
                    return (ShapeDesc)null;
                }
            }
            else
            {
                quad.outValues.totalTriCount += shapedesc.geometry.GetNumTriangles();
                quad.outValues.reducedTriCount += shapedesc.geometry.GetNumTriangles();
            }
            this.GenerateSegments(quad, ref shapedesc);
            return shapedesc;
        }

        private bool UVAtlas(Geometry geometry, string texture, StaticDesc stat)
        {
            List<UVCoord> uvcoords = geometry.GetUVCoords();
            List<UVCoord> uvcoords2 = new List<UVCoord>();
            for (int index = 0; index < uvcoords.Count; ++index)
            {
                float u = uvcoords[index][0];
                float v = uvcoords[index][1];
                //logFile.WriteLog("B " + u + ", " + v);
                if (u < atlasToleranceMin || u > atlasToleranceMax || v < atlasToleranceMin || u > atlasToleranceMax)
                {
                    if (this.verbose && !texture.Contains("glacierslablod"))
                    {
                        logFile.WriteLog("Out of range " + atlasToleranceMin + " <= " + u + ", " + v + " <= " + atlasToleranceMax + " for " + texture + " in " + stat.staticModels[this.quadIndex]);
                    }
                    return false;
                }
                u = Math.Max(u, AtlasList.Get(texture).minU);
                v = Math.Max(v, AtlasList.Get(texture).minV);
                u = Math.Min(u, AtlasList.Get(texture).maxU);
                v = Math.Min(v, AtlasList.Get(texture).maxV);
                u *= AtlasList.Get(texture).scaleU;
                v *= AtlasList.Get(texture).scaleV;
                u += AtlasList.Get(texture).posU;
                v += AtlasList.Get(texture).posV;
                UVCoord coords = new UVCoord(u, v);
                uvcoords2.Add(coords);
            }
            geometry.SetUVCoords(uvcoords2);
            return true;
        }

        private float GetTriangleHeight(List<Vector3> verts, List<Triangle> tris, Vector3 pt)
        {
            float result = float.MinValue;
            //Parallel.For(0, tris.Count - 1, (index, state) => {
            for (int index = 0; index < tris.Count; index++)
            {
                Triangle triangle = tris[index];
                float u;
                float v;
                if (Utils.PointInTriangle(new Vector2(pt[0], pt[1]),
                   new Vector2(verts[(int)triangle[0]][0], verts[(int)triangle[0]][1]),
                   new Vector2(verts[(int)triangle[1]][0], verts[(int)triangle[1]][1]),
                   new Vector2(verts[(int)triangle[2]][0], verts[(int)triangle[2]][1]), out u, out v))
                {
                    Vector3 vector3_1 = verts[(int)triangle[0]];
                    Vector3 vector3_2 = verts[(int)triangle[1]];
                    Vector3 vector3_3 = verts[(int)triangle[2]];
                    result = (vector3_2[2] - vector3_1[2]) * v + vector3_1[2] + ((vector3_3[2] - vector3_1[2]) * u + vector3_1[2]) - vector3_1[2];
                    break;  //state.Stop();
                }
            };
            return result;
        }

        private void RemoveUnseenFaces(QuadDesc quad, ShapeDesc shapedesc)
        {
            List<Triangle> triangles = shapedesc.geometry.GetTriangles();
            List<Vector3> vertices = shapedesc.geometry.GetVertices();
            Dictionary<ushort, bool> whatever = new Dictionary<ushort, bool>();
            float zShift = removeUnseenZShift / this.quadLevel;
            int count = triangles.Count;
            quad.outValues.totalTriCount += count;
            int loops = 1;
            if (this.removeUnderwaterFaces)
            {
                loops = 0;
            }
            for (int loop = loops; loop < 2; loop++)
            {
                for (int index = 0; index < triangles.Count; ++index)
                {
                    QuadDesc quadCurrent = new QuadDesc(true);
                    //List<Triangle> trianglesCompare = new List<Triangle>();
                    //List<Vector3> verticesCompare = new List<Vector3>();
                    bool[] vertexBelow = new bool[3];
                    for (int index1 = 0; index1 < 3; index1++)
                    {
                        ushort tri = triangles[index][index1];
                        if (whatever.ContainsKey(tri))
                        {
                            if (!whatever[tri])
                            {
                                break;
                            }
                            else
                            {
                                vertexBelow[index1] = true;
                            }
                        }
                        else
                        {
                            //logFile.WriteLog(shapedesc.name + " - " + triangles[index][index1] + " = " + verticesUnder[triangles[index][index1]]);
                            Vector3 vertex1 = vertices[triangles[index][index1]];
                            //float x = vertex[0];
                            //float y = vertex[1];
                            /*int vertexQuadx = quad.x + cellquad(x * quadLevel, southWestX);
                            int vertexQuady = quad.y + cellquad(y * quadLevel, southWestY);
                            if (quad.x != vertexQuadx || quad.y != vertexQuady)
                            {
                                for (int index3 = 0; index3 < this.quadList.Count; ++index3)
                                {
                                    if (vertexQuadx == this.quadList[index3].x && vertexQuady == this.quadList[index3].y)
                                    {
                                        quadCurrent = this.quadList[index3];
                                        x -= (vertexQuadx - quad.x) / quadLevel * 4096;
                                        y -= (vertexQuady - quad.y) / quadLevel * 4096;
                                        break;
                                    }
                                }
                            }
                            else*/
                            {
                                quadCurrent = quad;
                            }
                            if (!quadCurrent.hasTerrainVertices)
                            {
                                continue;
                            }
                            //Vector3 vertex1 = new Vector3(x, y, vertex[2]);
                            List<Triangle> trianglesTerrain = new List<Triangle>();
                            List<Vector3> verticesTerrain = new List<Vector3>();
                            if (loop == 0)
                            {
                                if (quadCurrent.waterQuadTree != null)
                                {
                                    //logFile.WriteLog("w " + shapedesc.boundingBox.pz1 + " = " + shapedesc.boundingBox.pz2 + " === " + quadCurrent.boundingBox.pz1 + " = " + quadCurrent.boundingBox.pz2);
                                    if (vertex1[2] < quadCurrent.boundingBox.pz2)
                                    {
                                        trianglesTerrain = quadCurrent.waterQuadTree.entirequad.triangles;
                                        verticesTerrain = quadCurrent.waterQuadTree.vertices;
                                    }
                                    else
                                    {
                                        vertexBelow[index1] = false;
                                        break;
                                    }
                                }
                            }
                            else
                            {
                                //logFile.WriteLog("t " + shapedesc.boundingBox.pz1 + " = " + shapedesc.boundingBox.pz2 + " === " + quadCurrent.boundingBox.pz1 + " = " + quadCurrent.boundingBox.pz2);
                                if (vertex1[2] < quadCurrent.boundingBox.pz2)
                                {
                                    trianglesTerrain = quadCurrent.terrainQuadTree.GetSegment(vertex1, quadLevel);
                                    verticesTerrain = quadCurrent.terrainQuadTree.vertices;
                                }
                                else
                                {
                                    vertexBelow[index1] = false;
                                    whatever.Add(tri, false);
                                    break;
                                }
                            }
                            if (trianglesTerrain == null)
                            {
                                vertexBelow[index1] = true;
                                whatever.Add(tri, true);
                            }
                            else
                            {
                                if (trianglesTerrain.Count != 0)
                                {
                                    float num1 = GetTriangleHeight(verticesTerrain, trianglesTerrain, vertex1) - zShift;
                                    if (vertex1[2] < num1)
                                    {
                                        vertexBelow[index1] = true;
                                        whatever.Add(tri, true);
                                    }
                                    else
                                    {
                                        vertexBelow[index1] = false;
                                        if (loop == 1)
                                        {
                                            whatever.Add(tri, false);
                                        }
                                        break;
                                    }
                                }
                                else
                                {
                                    if (loop == 1)
                                    {
                                        vertexBelow[index1] = false;
                                        whatever.Add(tri, false);
                                        break;
                                    }
                                }
                            }
                        }
                    }
                    if (vertexBelow[0] && vertexBelow[1] && vertexBelow[2])
                    {
                        triangles[index] = new Triangle(0, 0, 0);
                        triangles.RemoveAt(index);
                        --index;
                    }
                }
            }
            shapedesc.geometry.SetTriangles(triangles);
            if (useOptimizer)
            {
                shapedesc.geometry.RemoveDuplicate(true);
            }
            else
            {
                shapedesc.geometry.RemoveUnused();
            }
            quad.outValues.reducedTriCount += triangles.Count;
        }

        private List<ShapeDesc> ParseNif(QuadDesc quad, StaticDesc curStat, int level)
        {
            NiFile file1 = new NiFile();
            if (curStat.staticModels[level].Contains(".nif"))
            {
                try
                {
                    file1.Read(this.gameDir, curStat.staticModels[level], this.logFile);
                }
                catch
                {
                    Console.WriteLine(curStat.staticModels[level] + " can not read file");
                }
                return this.IterateNodes(quad, curStat, level, file1, (NiNode)file1.GetBlockAtIndex(0), new Matrix44(true), 1f);
            }

            if (curStat.staticModels[level].Contains(".dds"))
            {
                if (FlatList.Contains(curStat.staticModels[level]))
                {
                    //this.logFile.WriteLog(curStat.staticModels[level] + " = " + FlatList.Get(curStat.staticModels[level]).width + ", " + FlatList.Get(curStat.staticModels[level]).height);
                    BSFadeNode rootFadeNode = new BSFadeNode();
                    rootFadeNode.SetNameIndex(file1.AddString("LODGenPassThru"));

                    NiTriShape nitrishape = new NiTriShape();
                    nitrishape.SetNameIndex(file1.AddString("LODGenPassThru"));
                    nitrishape.SetFlags(14);
                    nitrishape.SetFlags2(8);
                    nitrishape.SetScale(FlatList.Get(curStat.staticModels[level]).scale);

                    NiTriShapeData data = new NiTriShapeData();
                    List<Vector3> vertices = new List<Vector3>();
                    vertices.Add(new Vector3(-FlatList.Get(curStat.staticModels[level]).width / 2, 0, FlatList.Get(curStat.staticModels[level]).shiftZ - 5));
                    vertices.Add(new Vector3(+FlatList.Get(curStat.staticModels[level]).width / 2, 0, FlatList.Get(curStat.staticModels[level]).shiftZ - 5));
                    vertices.Add(new Vector3(+FlatList.Get(curStat.staticModels[level]).width / 2, 0, FlatList.Get(curStat.staticModels[level]).height + FlatList.Get(curStat.staticModels[level]).shiftZ - 5));
                    vertices.Add(new Vector3(-FlatList.Get(curStat.staticModels[level]).width / 2, 0, FlatList.Get(curStat.staticModels[level]).height + FlatList.Get(curStat.staticModels[level]).shiftZ - 5));
                    vertices.Add(new Vector3(0, -FlatList.Get(curStat.staticModels[level]).width / 2, FlatList.Get(curStat.staticModels[level]).shiftZ + 5));
                    vertices.Add(new Vector3(0, +FlatList.Get(curStat.staticModels[level]).width / 2, FlatList.Get(curStat.staticModels[level]).shiftZ + 5));
                    vertices.Add(new Vector3(0, +FlatList.Get(curStat.staticModels[level]).width / 2, FlatList.Get(curStat.staticModels[level]).height + FlatList.Get(curStat.staticModels[level]).shiftZ + 5));
                    vertices.Add(new Vector3(0, -FlatList.Get(curStat.staticModels[level]).width / 2, FlatList.Get(curStat.staticModels[level]).height + FlatList.Get(curStat.staticModels[level]).shiftZ + 5));

                    List<UVCoord> uv = new List<UVCoord>();
                    uv.Add(new UVCoord(0.0f, 1.0f));
                    uv.Add(new UVCoord(1.0f, 1.0f));
                    uv.Add(new UVCoord(1.0f, 0.0f));
                    uv.Add(new UVCoord(0.0f, 0.0f));
                    uv.Add(new UVCoord(0.0f, 1.0f));
                    uv.Add(new UVCoord(1.0f, 1.0f));
                    uv.Add(new UVCoord(1.0f, 0.0f));
                    uv.Add(new UVCoord(0.0f, 0.0f));
                    List<Triangle> triangles = new List<Triangle>();
                    triangles.Add(new Triangle(0, 1, 2));
                    triangles.Add(new Triangle(2, 3, 0));
                    triangles.Add(new Triangle(4, 5, 6));
                    triangles.Add(new Triangle(6, 7, 4));
                    data.SetNumVertices(8);
                    data.SetHasVertices(true);
                    data.SetVertices(vertices);
                    data.SetNumUVSets(1);
                    data.SetHasTangents(true);
                    data.SetHasNormals(true);
                    data.SetNormals(FlatList.Get(curStat.staticModels[level]).normals);
                    data.SetTangents(FlatList.Get(curStat.staticModels[level]).tangents);
                    data.SetBitangents(FlatList.Get(curStat.staticModels[level]).bitangents);
                    data.SetCenter(new Vector3(0.0f, 0.0f, (FlatList.Get(curStat.staticModels[level]).height + FlatList.Get(curStat.staticModels[level]).shiftZ) / 2));
                    data.SetRadius(FlatList.Get(curStat.staticModels[level]).width);
                    data.SetHasVertexColors(false);
                    data.SetUVCoords(uv);
                    data.SetNumTriangles(4);
                    data.SetNumTrianglePoints(12);
                    data.SetHasTriangles(true);
                    data.SetTriangles(triangles);

                    BSLightingShaderProperty lightingShaderProperty = new BSLightingShaderProperty();
                    // SLSF1_ZBuffer_Test
                    lightingShaderProperty.SetShaderFlags1(2147483648);
                    // SLSF2_ZBuffer_Write SLSF2_Double_Sided SLSF2_Soft_Lighting
                    lightingShaderProperty.SetShaderFlags2(33554449);
                    if (level >= flatLODLevelLODFlag)
                    {
                        // SLSF2_ZBuffer_Write SLSF2_LOD_Objects SLSF2_Double_Sided SLSF2_Soft_Lighting
                        lightingShaderProperty.SetShaderFlags2(33554453);
                    }
                    lightingShaderProperty.SetTextureClampMode(0);
                    //lightingShaderProperty.SetLightingEffect1(2f);
                    lightingShaderProperty.SetLightingEffect1(FlatList.Get(curStat.staticModels[level]).effect1);
                    // no visual change
                    lightingShaderProperty.SetLightingEffect2(1.0f);

                    BSShaderTextureSet shaderTextureSet = new BSShaderTextureSet();
                    shaderTextureSet.SetNumTextures(10);
                    shaderTextureSet.SetTexture(0, curStat.staticModels[level]);
                    shaderTextureSet.SetTexture(1, Utils.GetDiffuseTextureName(curStat.staticModels[level]));
                    //shaderTextureSet.SetTexture(2, curStat.staticModels[level]);
                    shaderTextureSet.SetTexture(2, FlatList.Get(curStat.staticModels[level]).GlowTexture);
                    shaderTextureSet.SetTexture(3, "");
                    shaderTextureSet.SetTexture(4, "");
                    shaderTextureSet.SetTexture(5, "");
                    shaderTextureSet.SetTexture(6, "");
                    if (Game.Mode == "fo4")
                    {
                        shaderTextureSet.SetTexture(7, Utils.GetSpecularTextureName(curStat.staticModels[level]));
                    }
                    else
                    {
                        shaderTextureSet.SetTexture(7, "");
                    }
                    shaderTextureSet.SetTexture(8, "");
                    shaderTextureSet.SetTexture(9, "");

                    file1.AddBlock((NiObject)rootFadeNode);
                    rootFadeNode.AddChild(file1.AddBlock((NiObject)nitrishape));
                    nitrishape.SetData(file1.AddBlock((NiObject)data));
                    nitrishape.SetBSProperty(0, file1.AddBlock((NiObject)lightingShaderProperty));
                    lightingShaderProperty.SetTextureSet(file1.AddBlock((NiObject)shaderTextureSet));

                    if (AtlasList.Contains(curStat.staticModels[level]))
                    {
                        Geometry geom = new Geometry(data);
                        if (this.UVAtlas(geom, curStat.staticModels[level], new StaticDesc()))
                        {
                            data.SetUVCoords(geom.GetUVCoords());
                            string[] strArray = new string[10] { "", "", "", "", "", "", "", "", "", "" };
                            shaderTextureSet.SetTexture(0, AtlasList.Get(curStat.staticModels[level]).AtlasTexture);
                            shaderTextureSet.SetTexture(1, AtlasList.Get(curStat.staticModels[level]).AtlasTextureN);
                            shaderTextureSet.SetTexture(2, FlatList.Get(curStat.staticModels[level]).GlowTexture);
                            //shaderTextureSet.SetTexture(2, AtlasList.Get(curStat.staticModels[level]).AtlasTextureI);
                        }
                    }

                    //file1.Write(this.outputDir + (object)"\\" + this.worldspaceName + ".Level4.X" + quad.x.ToString() + ".Y" + quad.y.ToString() + curStat.staticModels[level].Replace("\\","") + ".nif", logFile);
                    Matrix44 transform = new Matrix44(true);
                    transform.SetRotationZ((GetRandomNumber() * 90) - 45);
                    return this.IterateNodes(quad, curStat, level, file1, (NiNode)file1.GetBlockAtIndex(0), transform, 1f);
                }
            }
            return new List<ShapeDesc>();
        }

        private void CreateLODNodes(NiFile file, NiNode rootNode, QuadDesc quad, List<ShapeDesc> shapes)
        {
            foreach (ShapeDesc shapeDesc in shapes)
            {
                //logFile.WriteLog("shape " + shapeDesc.staticModel + " = " + shapeDesc.name + " = " + shapeDesc.textures[0]);
                BSMultiBoundNode node = new BSMultiBoundNode();
                rootNode.AddChild(file.AddBlock((NiObject)node));
                BSSegmentedTriShape segmentedTriShape = new BSSegmentedTriShape();

                node.AddChild(file.AddBlock((NiObject)segmentedTriShape));
                node.SetCullMode(1U);
                string str = "obj";
                // use material name from list file, Snow/Ash
                if (!this.ignoreMaterial && shapeDesc.material != "")
                {
                    str = str + shapeDesc.material;
                }
                // only level 4 should have HD - there seems to be no visual difference if no shader is used
                if (shapeDesc.isHighDetail && shapeDesc.material != "" && (this.quadLevel == 4 || this.useHDFlag))
                {
                    str = str + "HD";
                }
                segmentedTriShape.SetNameIndex(file.AddString(str));
                segmentedTriShape.SetFlags((ushort)14);
                segmentedTriShape.SetFlags2((ushort)8320);
                int dataBlock = -1;
                if (!dontGroup && shapeDesc.isGroup)
                {
                    if (quad.dataBlockIndex.ContainsKey(shapeDesc.shapeHash))
                    {
                        dataBlock = quad.dataBlockIndex[shapeDesc.shapeHash];
                    }
                    else
                    {
                        dataBlock = (file.AddBlock((NiObject)shapeDesc.geometry.ToNiTriShapeData(generateVertexColors)));
                        quad.dataBlockIndex.Add(shapeDesc.shapeHash, dataBlock);
                    }
                }
                else
                {
                    dataBlock = (file.AddBlock((NiObject)shapeDesc.geometry.ToNiTriShapeData(generateVertexColors)));
                }
                segmentedTriShape.SetData(dataBlock);
                segmentedTriShape.SetBSProperty(1, -1);
                segmentedTriShape.SetScale((float)this.quadLevel);
                if (!dontGroup && shapeDesc.isGroup)
                {
                    segmentedTriShape.SetRotation(shapeDesc.rotation);
                    segmentedTriShape.SetTranslation(shapeDesc.translation);
                }
                else
                {
                    segmentedTriShape.SetRotation(new Matrix33(true));
                    segmentedTriShape.SetTranslation(new Vector3((float)quad.x * 4096f, (float)quad.y * 4096f, 0.0f));
                }
                for (int index = 0; index < 16; ++index)
                {
                    segmentedTriShape.AddSegment(new BSSegment(0U, (ushort)0));
                }
                for (int index = 0; index < shapeDesc.segments.Count; ++index)
                {
                    BSSegment segmentAtIndex = segmentedTriShape.GetSegmentAtIndex(shapeDesc.segments[index].id);
                    segmentAtIndex.startTriangle = shapeDesc.segments[index].startTriangle;
                    segmentAtIndex.numTriangles = shapeDesc.segments[index].numTriangles;
                    segmentedTriShape.SetSegment(shapeDesc.segments[index].id, segmentAtIndex);
                }
                for (int index = 15; index >= 0 && (int)segmentedTriShape.GetSegmentAtIndex(index).numTriangles == 0; --index)
                {
                    segmentedTriShape.RemoveSegment(index);
                }
                if (shapeDesc.isPassThru)
                {
                    if (shapeDesc.shaderType == "bseffectshaderproperty")
                    {
                        BSEffectShaderProperty effectShaderProperty = new BSEffectShaderProperty();
                        effectShaderProperty = shapeDesc.effectShader;
                        effectShaderProperty.SetTextureClampMode(shapeDesc.TextureClampMode);
                        effectShaderProperty.SetSourceTexture(shapeDesc.textures[0]);
                        segmentedTriShape.SetBSProperty(0, file.AddBlock((NiObject)effectShaderProperty));
                    }
                    else if (shapeDesc.shaderType == "bslightingshaderproperty")
                    {
                        BSLightingShaderProperty lightingShaderProperty = new BSLightingShaderProperty();
                        int shaderBlock = -1;
                        // no batching of shaders :(
                        //if (quad.shaderBlockIndex.ContainsKey(shapeDesc.shaderHash))
                        //{
                        //    shaderBlock = quad.shaderBlockIndex[shapeDesc.shaderHash];
                        //}
                        //else
                        {
                            lightingShaderProperty = shapeDesc.lightingShader;

                            if (lightingShaderProperty.GetShaderType() != 0 && lightingShaderProperty.GetShaderType() != 2)
                            {
                                logFile.WriteLog("Shader Type " + lightingShaderProperty.GetShaderType() + " = " + shapeDesc.name + " = " + shapeDesc.staticName + ", Cell " + quad.ToString());
                            }

                            lightingShaderProperty.SetTextureClampMode(shapeDesc.TextureClampMode);
                            shaderBlock = file.AddBlock((NiObject)lightingShaderProperty);
                            //quad.shaderBlockIndex.Add(shapeDesc.shaderHash, shaderBlock);
                            int textureBlock = -1;
                            BSShaderTextureSet shaderTextureSet = new BSShaderTextureSet();
                            shaderTextureSet.SetNumTextures(9);
                            shaderTextureSet.SetTexture(0, shapeDesc.textures[0]);
                            shaderTextureSet.SetTexture(1, shapeDesc.textures[1]);
                            shaderTextureSet.SetTexture(2, shapeDesc.textures[2]);
                            shaderTextureSet.SetTexture(3, shapeDesc.textures[3]);
                            shaderTextureSet.SetTexture(4, shapeDesc.textures[4]);
                            shaderTextureSet.SetTexture(5, shapeDesc.textures[5]);
                            shaderTextureSet.SetTexture(6, shapeDesc.textures[6]);
                            shaderTextureSet.SetTexture(7, shapeDesc.textures[7]);
                            shaderTextureSet.SetTexture(8, shapeDesc.textures[8]);
                            if (quad.textureBlockIndexPassThru.ContainsKey(Utils.GetHash(Utils.ObjectToByteArray(shaderTextureSet))))
                            {
                                textureBlock = quad.textureBlockIndexPassThru[Utils.GetHash(Utils.ObjectToByteArray(shaderTextureSet))];
                            }
                            else
                            {
                                textureBlock = file.AddBlock(shaderTextureSet);
                                quad.textureBlockIndexPassThru.Add(Utils.GetHash(Utils.ObjectToByteArray(shaderTextureSet)), textureBlock);
                            }
                            lightingShaderProperty.SetTextureSet(textureBlock);
                        }
                        segmentedTriShape.SetBSProperty(0, shaderBlock);
                    }
                    else
                    {
                        if (verbose)
                        {
                            logFile.WriteLog(shapeDesc.staticModel + " " + shapeDesc.name + "unknown shader " + shapeDesc.shaderType);
                        }
                    }
                }
                else
                {
                    BSLightingShaderProperty lightingShaderProperty = new BSLightingShaderProperty();
                    segmentedTriShape.SetBSProperty(0, file.AddBlock((NiObject)lightingShaderProperty));
                    int textureBlock = -1;
                    BSShaderTextureSet shaderTextureSet = new BSShaderTextureSet();
                    shaderTextureSet.SetNumTextures(9);
                    shaderTextureSet.SetTexture(0, shapeDesc.textures[0]);
                    shaderTextureSet.SetTexture(1, shapeDesc.textures[1]);
                    shaderTextureSet.SetTexture(2, shapeDesc.textures[2]);
                    shaderTextureSet.SetTexture(3, shapeDesc.textures[3]);
                    shaderTextureSet.SetTexture(4, shapeDesc.textures[4]);
                    shaderTextureSet.SetTexture(5, shapeDesc.textures[5]);
                    shaderTextureSet.SetTexture(6, shapeDesc.textures[6]);
                    shaderTextureSet.SetTexture(7, shapeDesc.textures[7]);
                    shaderTextureSet.SetTexture(8, shapeDesc.textures[8]);
                    if (quad.textureBlockIndex.ContainsKey(Utils.GetHash(Utils.ObjectToByteArray(shaderTextureSet))))
                    {
                        textureBlock = quad.textureBlockIndex[Utils.GetHash(Utils.ObjectToByteArray(shaderTextureSet))];
                    }
                    else
                    {
                        textureBlock = file.AddBlock((NiObject)shaderTextureSet);
                        quad.textureBlockIndex.Add(Utils.GetHash(Utils.ObjectToByteArray(shaderTextureSet)), textureBlock);
                    }
                    lightingShaderProperty.SetTextureSet(textureBlock);
                    lightingShaderProperty.SetLightingEffect1(0.0f);
                    lightingShaderProperty.SetLightingEffect2(0.0f);
                    lightingShaderProperty.SetGlossiness(1f);
                    lightingShaderProperty.SetTextureClampMode(shapeDesc.TextureClampMode);
                    // SLSF1_ZBuffer_Test
                    uint num1 = 2147483648U;
                    // SLSF2_ZBuffer_Write
                    uint num2 = 1U;
                    if (useDecalFlag && shapeDesc.isDecal)
                    {
                        // SLSF1_Decal
                        num1 |= 67108864U;
                        // SLSF1_Dynamic_Decal
                        num1 |= 134217728U;
                        // SLSF2_No_Fade
                        num2 |= 8U;
                    }
                    if (shapeDesc.hasVertexColor)
                    {
                        // SLSF2_Vertex_Colors
                        num2 |= 32U;
                    }
                    if (shapeDesc.isDoubleSided || (this.alphaDoublesided && shapeDesc.isAlpha))
                    {
                        // SLSF2_Double_Sided
                        num2 |= 16U;
                    }
                    if (shapeDesc.isHighDetail && (this.quadLevel == 4 || this.useHDFlag))
                    {
                        // SLSF2_HD_LOD_Objects
                        num2 |= 2147483648U;
                    }
                    else
                    {
                        // SLSF2_LOD_Objects - doesn't seem to be required!? If omitted LOD can use specular, vertex alpha on/off
                        num2 |= 4U;
                    }
                    lightingShaderProperty.SetShaderFlags1(num1);
                    lightingShaderProperty.SetShaderFlags2(num2);
                }
                this.GenerateMultibound(file, node, quad, shapeDesc.boundingBox);
            }
        }

        // Skyrim SEE
        private void CreateLODNodesSSE(NiFile file, NiNode rootNode, QuadDesc quad, List<ShapeDesc> shapes)
        {
            //Console.WriteLine("SSSSSSSSSSSEEEEEEEEEEEEEE");
            foreach (ShapeDesc shapeDesc in shapes)
            {
                //logFile.WriteLog("shape " + shapeDesc.staticModel + " = " + shapeDesc.name + " = " + shapeDesc.textures[0]);
                BSMultiBoundNode node = new BSMultiBoundNode();
                node.SetCullMode(1);
                rootNode.AddChild(file.AddBlock((NiObject)node));
                BSSubIndexTriShape subIndexTriShape = new BSSubIndexTriShape();
                string str = "obj";
                // use material name from list file, Snow/Ash
                if (!this.ignoreMaterial && shapeDesc.material != "")
                {
                    str = str + shapeDesc.material;
                }
                if (this.quadLevel != 4 && str.ToLower().Contains("largeref"))
                {
                    str = Regex.Replace(str, "-largeref", "", RegexOptions.IgnoreCase);
                }
                if (this.quadLevel != 4 && str.ToLower().Contains("hd"))
                {
                    str = Regex.Replace(str, "hd", "", RegexOptions.IgnoreCase);
                }
                // only level 4 should have HD - there seems to be no visual difference if no shader is used
                if (shapeDesc.isHighDetail && shapeDesc.material != "" && (this.quadLevel == 4 || this.useHDFlag))
                {
                    if (str.ToLower().Contains("largeref"))
                    {
                        str = Regex.Replace(str, "-largeref", "HD-LargeRef", RegexOptions.IgnoreCase);
                    }
                    else
                    {
                        str = str + "HD";
                    }
                }
                // lets be stricked for now
                if (this.quadLevel != 4 && str.ToLower().Contains("hd"))
                {
                    shapeDesc.isHighDetail = false;
                    str = Regex.Replace(str, "hd", "", RegexOptions.IgnoreCase);
                }

                subIndexTriShape = shapeDesc.geometry.ToBSSubIndexTriShape(generateVertexColors);
                subIndexTriShape.UpdateVertexData();
                subIndexTriShape.SetNameIndex(file.AddString(str));
                subIndexTriShape.SetFlags((ushort)14);
                subIndexTriShape.SetFlags2((ushort)0);
                subIndexTriShape.SetVF3(101);
                subIndexTriShape.SetVF4(0);
                subIndexTriShape.SetVF5(0);
                subIndexTriShape.SetVF8(0);
                subIndexTriShape.SetScale((float)this.quadLevel);
                subIndexTriShape.SetRotation(new Matrix33(true));
                subIndexTriShape.SetTranslation(new Vector3((float)quad.x * 4096f, (float)quad.y * 4096f, 0.0f));
                subIndexTriShape.SetNumTriangles2(subIndexTriShape.GetNumTriangles() * 2);

                for (int index = 0; index < 16; ++index)
                {
                    subIndexTriShape.AddSegment(new BSSegment(0U, (ushort)0));
                }
                for (int index = 0; index < shapeDesc.segments.Count; ++index)
                {
                    BSSegment segmentAtIndex = subIndexTriShape.GetBSSegmentAtIndex(shapeDesc.segments[index].id);
                    segmentAtIndex.startTriangle = shapeDesc.segments[index].startTriangle;
                    segmentAtIndex.numTriangles = shapeDesc.segments[index].numTriangles;
                    subIndexTriShape.SetSegment(shapeDesc.segments[index].id, segmentAtIndex);
                }
                for (int index = 15; index >= 0 && (int)subIndexTriShape.GetBSSegmentAtIndex(index).numTriangles == 0; --index)
                {
                    subIndexTriShape.RemoveBSSegment(index);
                }

                node.AddChild(file.AddBlock(subIndexTriShape));
                if (str.ToLower().Contains("largeref"))
                {
                    BSDistantObjectLargeRefExtraData largeRefExtraData = new BSDistantObjectLargeRefExtraData();
                    largeRefExtraData.SetNameIndex(file.AddString("DOLRED"));
                    largeRefExtraData.SetByte(1);
                    List<int> idx = new List<int>();
                    idx.Add(file.AddBlock(largeRefExtraData));
                    subIndexTriShape.SetExtraData(idx);
                }

                if (shapeDesc.isPassThru)
                {
                    if (shapeDesc.shaderType == "bseffectshaderproperty")
                    {
                        BSEffectShaderProperty effectShaderProperty = new BSEffectShaderProperty();
                        effectShaderProperty = shapeDesc.effectShader;
                        effectShaderProperty.SetTextureClampMode(shapeDesc.TextureClampMode);
                        effectShaderProperty.SetSourceTexture(shapeDesc.textures[0]);
                        subIndexTriShape.SetBSProperty(0, file.AddBlock((NiObject)effectShaderProperty));
                    }
                    else if (shapeDesc.shaderType == "bslightingshaderproperty")
                    {
                        BSLightingShaderProperty lightingShaderProperty = new BSLightingShaderProperty();
                        int shaderBlock = -1;
                        // no batching of shaders :(
                        //if (quad.shaderBlockIndex.ContainsKey(shapeDesc.shaderHash))
                        //{
                        //    shaderBlock = quad.shaderBlockIndex[shapeDesc.shaderHash];
                        //}
                        //else
                        {
                            lightingShaderProperty = shapeDesc.lightingShader;

                            if (lightingShaderProperty.GetShaderType() != 0 && lightingShaderProperty.GetShaderType() != 2)
                            {
                                logFile.WriteLog("Shader Type " + lightingShaderProperty.GetShaderType() + " = " + shapeDesc.name + " = " + shapeDesc.staticName + ", Cell " + quad.ToString());
                            }

                            lightingShaderProperty.SetTextureClampMode(shapeDesc.TextureClampMode);
                            shaderBlock = file.AddBlock((NiObject)lightingShaderProperty);
                            //quad.shaderBlockIndex.Add(shapeDesc.shaderHash, shaderBlock);
                            int textureBlock = -1;
                            BSShaderTextureSet shaderTextureSet = new BSShaderTextureSet();
                            shaderTextureSet.SetNumTextures(9);
                            /*for (int index = 0; index < shaderTextureSet.GetNumTextures(); index++)
                            {
                                if (shapeDesc.textures[index] != "")
                                {
                                    shapeDesc.textures[index] = "data\\" + shapeDesc.textures[index];
                                }
                            }*/
                            shaderTextureSet.SetTexture(0, shapeDesc.textures[0]);
                            shaderTextureSet.SetTexture(1, shapeDesc.textures[1]);
                            shaderTextureSet.SetTexture(2, shapeDesc.textures[2]);
                            shaderTextureSet.SetTexture(3, shapeDesc.textures[3]);
                            shaderTextureSet.SetTexture(4, shapeDesc.textures[4]);
                            shaderTextureSet.SetTexture(5, shapeDesc.textures[5]);
                            shaderTextureSet.SetTexture(6, shapeDesc.textures[6]);
                            shaderTextureSet.SetTexture(7, shapeDesc.textures[7]);
                            shaderTextureSet.SetTexture(8, shapeDesc.textures[8]);
                            if (quad.textureBlockIndexPassThru.ContainsKey(Utils.GetHash(Utils.ObjectToByteArray(shaderTextureSet))))
                            {
                                textureBlock = quad.textureBlockIndexPassThru[Utils.GetHash(Utils.ObjectToByteArray(shaderTextureSet))];
                            }
                            else
                            {
                                textureBlock = file.AddBlock(shaderTextureSet);
                                quad.textureBlockIndexPassThru.Add(Utils.GetHash(Utils.ObjectToByteArray(shaderTextureSet)), textureBlock);
                            }
                            lightingShaderProperty.SetTextureSet(textureBlock);
                        }
                        subIndexTriShape.SetBSProperty(0, shaderBlock);
                    }
                    else
                    {
                        if (verbose)
                        {
                            logFile.WriteLog(shapeDesc.staticModel + " " + shapeDesc.name + "unknown shader " + shapeDesc.shaderType);
                        }
                    }
                }
                else
                {
                    BSLightingShaderProperty lightingShaderProperty = new BSLightingShaderProperty();
                    subIndexTriShape.SetBSProperty(0, file.AddBlock((NiObject)lightingShaderProperty));
                    int textureBlock = -1;
                    BSShaderTextureSet shaderTextureSet = new BSShaderTextureSet();
                    shaderTextureSet.SetNumTextures(9);
                    /*for (int index = 0; index < shaderTextureSet.GetNumTextures(); index++)
                    {
                        if (shapeDesc.textures[index] != "")
                        {
                            shapeDesc.textures[index] = "data\\" + shapeDesc.textures[index];
                        }
                    }*/
                    shaderTextureSet.SetTexture(0, shapeDesc.textures[0]);
                    shaderTextureSet.SetTexture(1, shapeDesc.textures[1]);
                    shaderTextureSet.SetTexture(2, shapeDesc.textures[2]);
                    shaderTextureSet.SetTexture(3, shapeDesc.textures[3]);
                    shaderTextureSet.SetTexture(4, shapeDesc.textures[4]);
                    shaderTextureSet.SetTexture(5, shapeDesc.textures[5]);
                    shaderTextureSet.SetTexture(6, shapeDesc.textures[6]);
                    shaderTextureSet.SetTexture(7, shapeDesc.textures[7]);
                    shaderTextureSet.SetTexture(8, shapeDesc.textures[8]);
                    if (quad.textureBlockIndex.ContainsKey(Utils.GetHash(Utils.ObjectToByteArray(shaderTextureSet))))
                    {
                        textureBlock = quad.textureBlockIndex[Utils.GetHash(Utils.ObjectToByteArray(shaderTextureSet))];
                    }
                    else
                    {
                        textureBlock = file.AddBlock((NiObject)shaderTextureSet);
                        quad.textureBlockIndex.Add(Utils.GetHash(Utils.ObjectToByteArray(shaderTextureSet)), textureBlock);
                    }
                    lightingShaderProperty.SetTextureSet(textureBlock);
                    lightingShaderProperty.SetLightingEffect1(0.0f);
                    lightingShaderProperty.SetLightingEffect2(0.0f);
                    lightingShaderProperty.SetGlossiness(1f);
                    lightingShaderProperty.SetTextureClampMode(shapeDesc.TextureClampMode);
                    // SLSF1_ZBuffer_Test
                    uint num1 = 2147483648U;
                    // SLSF2_ZBuffer_Write
                    uint num2 = 1U;
                    if (useDecalFlag && shapeDesc.isDecal)
                    {
                        // SLSF1_Decal
                        num1 |= 67108864U;
                        // SLSF1_Dynamic_Decal
                        num1 |= 134217728U;
                        // SLSF2_No_Fade
                        num2 |= 8U;
                    }
                    if (shapeDesc.hasVertexColor)
                    {
                        // SLSF2_Vertex_Colors
                        num2 |= 32U;
                    }
                    if (shapeDesc.isDoubleSided || (this.alphaDoublesided && shapeDesc.isAlpha))
                    {
                        // SLSF2_Double_Sided
                        num2 |= 16U;
                    }
                    if (shapeDesc.isHighDetail && (this.quadLevel == 4 || this.useHDFlag))
                    {
                        // SLSF2_HD_LOD_Objects
                        num2 |= 2147483648U;
                    }
                    else
                    {
                        // SLSF2_LOD_Objects - doesn't seem to be required!? If omitted LOD can use specular, vertex alpha on/off
                        num2 |= 4U;
                    }
                    lightingShaderProperty.SetShaderFlags1(num1);
                    lightingShaderProperty.SetShaderFlags2(num2);
                }
                this.GenerateMultibound(file, node, quad, shapeDesc.boundingBox);
            }
        }

        // oblivion format
        private void CreateMerge4Nodes(NiFile file, NiNode rootNode, QuadDesc quad, List<ShapeDesc> shapes)
        {
            foreach (ShapeDesc shapeDesc in shapes)
            {
                if (shapeDesc.geometry.GetVertices() == null || shapeDesc.geometry.GetNumVertices() == 0 || shapeDesc.shaderType != "nitexturingproperty")
                {
                    continue;
                }
                NiTriShape triShape = new NiTriShape();
                rootNode.AddChild(file.AddBlock(triShape));
                triShape.SetNameIndex(file.AddString(shapeDesc.name));
                triShape.SetFlags(0);
                triShape.SetData(file.AddBlock((NiObject)shapeDesc.geometry.ToNiTriShapeData(generateVertexColors)));
                /*if (shapeDesc.isAlpha)
                {
                    NiAlphaProperty alphaProperty = new NiAlphaProperty();
                    alphaProperty.SetFlags(4844);
                    alphaProperty.SetThreshold(shapeDesc.alphaThreshold);
                    triShape.SetBSProperty(1, file.AddBlock(alphaProperty));
                }
                else
                {
                    triShape.SetBSProperty(1, -1);
                }*/
                if (shapeDesc.shaderType == "nitexturingproperty")
                {
                    if (shapeDesc.materialProperty != null)
                    {
                        int materialBlock = -1;
                        if (quad.dataBlockIndex.ContainsKey(Utils.GetHash(Utils.ObjectToByteArray(shapeDesc.materialProperty))))
                        {
                            materialBlock = quad.dataBlockIndex[Utils.GetHash(Utils.ObjectToByteArray(shapeDesc.materialProperty))];
                        }
                        else
                        {
                            materialBlock = file.AddBlock(shapeDesc.materialProperty);
                            triShape.SetProperties(materialBlock);
                            quad.dataBlockIndex.Add(Utils.GetHash(Utils.ObjectToByteArray(shapeDesc.materialProperty)), materialBlock);
                        }
                        triShape.SetProperties(materialBlock);
                    }
                    NiTexturingProperty texturingProperty = new NiTexturingProperty();
                    texturingProperty = shapeDesc.texturingProperty;
                    //texturingProperty.SetHasBaseTexture(false);
                    if (texturingProperty.HasBaseTexture())
                    {
                        if (shapeDesc.texturingProperty.GetBaseTexture().source != -1)
                        {
                            TexDesc texDesc = texturingProperty.GetBaseTexture();
                            shapeDesc.sourceTextureBase.SetFileName(shapeDesc.textures[0]);
                            if (!quad.textureBlockIndex.ContainsKey(Utils.GetHash(Utils.ObjectToByteArray(shapeDesc.sourceTextureBase))))
                            {
                                texDesc.source = file.AddBlock(shapeDesc.sourceTextureBase);
                                quad.textureBlockIndex.Add(Utils.GetHash(Utils.ObjectToByteArray(shapeDesc.sourceTextureBase)), texDesc.source);
                            }
                            else
                            {
                                texDesc.source = quad.textureBlockIndex[Utils.GetHash(Utils.ObjectToByteArray(shapeDesc.sourceTextureBase))];
                            }
                            texturingProperty.SetBaseTexture(texDesc);
                        }
                    }
                    if (texturingProperty.HasDetailTexture())
                    {
                        if (shapeDesc.texturingProperty.GetDetailTexture().source != -1)
                        {
                            TexDesc texDesc = texturingProperty.GetDetailTexture();
                            shapeDesc.sourceTextureDetail.SetFileName(shapeDesc.textures[0]);
                            if (!quad.textureBlockIndex.ContainsKey(Utils.GetHash(Utils.ObjectToByteArray(shapeDesc.sourceTextureDetail))))
                            {
                                texDesc.source = file.AddBlock(shapeDesc.sourceTextureDetail);
                                quad.textureBlockIndex.Add(Utils.GetHash(Utils.ObjectToByteArray(shapeDesc.sourceTextureDetail)), texDesc.source);
                            }
                            else
                            {
                                texDesc.source = quad.textureBlockIndex[Utils.GetHash(Utils.ObjectToByteArray(shapeDesc.sourceTextureDetail))];
                            }
                            texturingProperty.SetDetailTexture(texDesc);
                        }
                    }
                    if (texturingProperty.HasGlowTexture())
                    {
                        if (shapeDesc.texturingProperty.GetGlowTexture().source != -1)
                        {
                            TexDesc texDesc = texturingProperty.GetGlowTexture();
                            shapeDesc.sourceTextureGlow.SetFileName(shapeDesc.textures[0]);
                            if (!quad.textureBlockIndex.ContainsKey(Utils.GetHash(Utils.ObjectToByteArray(shapeDesc.sourceTextureGlow))))
                            {
                                texDesc.source = file.AddBlock(shapeDesc.sourceTextureGlow);
                                quad.textureBlockIndex.Add(Utils.GetHash(Utils.ObjectToByteArray(shapeDesc.sourceTextureGlow)), texDesc.source);
                            }
                            else
                            {
                                texDesc.source = quad.textureBlockIndex[Utils.GetHash(Utils.ObjectToByteArray(shapeDesc.sourceTextureGlow))];
                            }
                            texturingProperty.SetGlowTexture(texDesc);
                        }
                    }
                    if (texturingProperty.HasBumpMapTexture())
                    {
                        if (shapeDesc.texturingProperty.GetBumpMapTexture().source != -1)
                        {
                            TexDesc texDesc = texturingProperty.GetBumpMapTexture();
                            shapeDesc.sourceTextureBump.SetFileName(shapeDesc.textures[0]);
                            if (!quad.textureBlockIndex.ContainsKey(Utils.GetHash(Utils.ObjectToByteArray(shapeDesc.sourceTextureBump))))
                            {
                                texDesc.source = file.AddBlock(shapeDesc.sourceTextureBump);
                                quad.textureBlockIndex.Add(Utils.GetHash(Utils.ObjectToByteArray(shapeDesc.sourceTextureBump)), texDesc.source);
                            }
                            else
                            {
                                texDesc.source = quad.textureBlockIndex[Utils.GetHash(Utils.ObjectToByteArray(shapeDesc.sourceTextureBump))];
                            }
                            texturingProperty.SetBumpMapTexture(texDesc);
                        }
                    }
                    triShape.SetProperties(file.AddBlock(shapeDesc.texturingProperty));
                }
                /*else if (shapeDesc.shaderType == "bseffectshaderproperty")
                {
                    BSEffectShaderProperty effectShaderProperty = new BSEffectShaderProperty();
                    effectShaderProperty = shapeDesc.effectShader;
                    effectShaderProperty.SetSourceTexture(shapeDesc.textures[0]);
                    triShape.SetBSProperty(0, file.AddBlock((NiObject)effectShaderProperty));
                }
                else if (shapeDesc.shaderType == "bslightingshaderproperty")
                {
                    BSLightingShaderProperty lightingShaderProperty = new BSLightingShaderProperty();
                    lightingShaderProperty = shapeDesc.lightingShader;
                    triShape.SetBSProperty(0, file.AddBlock((NiObject)lightingShaderProperty));
                    BSShaderTextureSet shaderTextureSet = new BSShaderTextureSet();
                    lightingShaderProperty.SetTextureSet(file.AddBlock((NiObject)shaderTextureSet));
                    shaderTextureSet.SetNumTextures(9);
                    shaderTextureSet.SetTexture(0, shapeDesc.textures[0]);
                    shaderTextureSet.SetTexture(1, shapeDesc.textures[1]);
                    shaderTextureSet.SetTexture(2, shapeDesc.textures[2]);
                    shaderTextureSet.SetTexture(3, shapeDesc.textures[3]);
                    shaderTextureSet.SetTexture(4, shapeDesc.textures[4]);
                    shaderTextureSet.SetTexture(5, shapeDesc.textures[5]);
                    shaderTextureSet.SetTexture(6, shapeDesc.textures[6]);
                    shaderTextureSet.SetTexture(7, shapeDesc.textures[7]);
                    shaderTextureSet.SetTexture(8, shapeDesc.textures[8]);
                }*/
                else
                {
                    if (verbose)
                    {
                        logFile.WriteLog(shapeDesc.staticModel + " " + shapeDesc.name + "unknown shader " + shapeDesc.shaderType);
                    }
                }
            }
        }

        // skyrim format
        private void CreateMergeNodes(NiFile file, NiNode rootNode, QuadDesc quad, List<ShapeDesc> shapes)
        {
            foreach (ShapeDesc shapeDesc in shapes)
            {
                rootNode.SetNameIndex(file.AddString("Scene Root"));
                rootNode.SetFlags(14);
                rootNode.SetFlags2(8);
                //file.AddString("BSX");
                NiTriShape triShape = new NiTriShape();
                rootNode.AddChild(file.AddBlock((NiObject)triShape));
                string str = shapeDesc.name;
                triShape.SetNameIndex(file.AddString(str));
                triShape.SetFlags(14);
                triShape.SetFlags2(8);
                triShape.SetData(file.AddBlock((NiObject)shapeDesc.geometry.ToNiTriShapeData(generateVertexColors)));
                if (shapeDesc.isAlpha)
                {
                    NiAlphaProperty alphaProperty = new NiAlphaProperty();
                    alphaProperty.SetFlags(4844);
                    alphaProperty.SetThreshold(shapeDesc.alphaThreshold);
                    triShape.SetBSProperty(1, file.AddBlock(alphaProperty));
                }
                else
                {
                    triShape.SetBSProperty(1, -1);
                }
                if (shapeDesc.shaderType == "bseffectshaderproperty")
                {
                    BSEffectShaderProperty effectShaderProperty = new BSEffectShaderProperty();
                    effectShaderProperty = shapeDesc.effectShader;
                    effectShaderProperty.SetSourceTexture(shapeDesc.textures[0]);
                    triShape.SetBSProperty(0, file.AddBlock((NiObject)effectShaderProperty));
                }
                else if (shapeDesc.shaderType == "bslightingshaderproperty")
                {
                    BSLightingShaderProperty lightingShaderProperty = new BSLightingShaderProperty();
                    lightingShaderProperty = shapeDesc.lightingShader;
                    triShape.SetBSProperty(0, file.AddBlock((NiObject)lightingShaderProperty));
                    BSShaderTextureSet shaderTextureSet = new BSShaderTextureSet();
                    lightingShaderProperty.SetTextureSet(file.AddBlock((NiObject)shaderTextureSet));
                    shaderTextureSet.SetNumTextures(9);
                    shaderTextureSet.SetTexture(0, shapeDesc.textures[0]);
                    shaderTextureSet.SetTexture(1, shapeDesc.textures[1]);
                    shaderTextureSet.SetTexture(2, shapeDesc.textures[2]);
                    shaderTextureSet.SetTexture(3, shapeDesc.textures[3]);
                    shaderTextureSet.SetTexture(4, shapeDesc.textures[4]);
                    shaderTextureSet.SetTexture(5, shapeDesc.textures[5]);
                    shaderTextureSet.SetTexture(6, shapeDesc.textures[6]);
                    shaderTextureSet.SetTexture(7, shapeDesc.textures[7]);
                    shaderTextureSet.SetTexture(8, shapeDesc.textures[8]);
                }
                else if (shapeDesc.shaderType == "nitexturingproperty")
                {
                    BSLightingShaderProperty lightingShaderProperty = new BSLightingShaderProperty();
                    if (generateVertexColors && shapeDesc.geometry.HasVertexColors())
                    {
                        lightingShaderProperty.SetShaderFlags2(lightingShaderProperty.GetShaderFlags2() | 32);
                    }
                    else
                    {
                        lightingShaderProperty.SetShaderFlags2(lightingShaderProperty.GetShaderFlags2() & 4294967263);
                    }
                    if (shapeDesc.materialProperty != null)
                    {
                        lightingShaderProperty.SetEmissiveColor(shapeDesc.materialProperty.GetEmissiveColor());
                        lightingShaderProperty.SetEmissiveMultiple(shapeDesc.materialProperty.GetEmissiveMultiple());
                        lightingShaderProperty.SetSpecularColor(shapeDesc.materialProperty.GetSpecularColor());
                        lightingShaderProperty.SetGlossiness(shapeDesc.materialProperty.GetGlossiness());
                        lightingShaderProperty.SetAlpha(shapeDesc.materialProperty.GetAlpha());
                    }

                    triShape.SetBSProperty(0, file.AddBlock((NiObject)lightingShaderProperty));
                    BSShaderTextureSet shaderTextureSet = new BSShaderTextureSet();
                    lightingShaderProperty.SetTextureSet(file.AddBlock((NiObject)shaderTextureSet));
                    shaderTextureSet.SetNumTextures(9);
                    shaderTextureSet.SetTexture(0, shapeDesc.textures[0]);
                    shaderTextureSet.SetTexture(1, shapeDesc.textures[1]);
                    shaderTextureSet.SetTexture(2, shapeDesc.textures[2]);
                    shaderTextureSet.SetTexture(3, shapeDesc.textures[3]);
                    shaderTextureSet.SetTexture(4, shapeDesc.textures[4]);
                    shaderTextureSet.SetTexture(5, shapeDesc.textures[5]);
                    shaderTextureSet.SetTexture(6, shapeDesc.textures[6]);
                    shaderTextureSet.SetTexture(7, shapeDesc.textures[7]);
                    shaderTextureSet.SetTexture(8, shapeDesc.textures[8]);
                }
                else
                {
                    if (verbose)
                    {
                        logFile.WriteLog(shapeDesc.staticModel + " " + shapeDesc.name + "unknown shader " + shapeDesc.shaderType);
                    }
                }
            }
        }

        // skyrim special edition format
        private void CreateMergeSSENodes(NiFile file, NiNode rootNode, QuadDesc quad, List<ShapeDesc> shapes)
        {
            foreach (ShapeDesc shapeDesc in shapes)
            {
                //rootNode.SetNameIndex(file.Get);
                rootNode.SetFlags(14);
                rootNode.SetFlags2(8);
                //file.AddString("BSX");
                NiTriShape triShape = new NiTriShape();
                rootNode.AddChild(file.AddBlock((NiObject)triShape));
                string str = shapeDesc.name;
                triShape.SetNameIndex(file.AddString(str));
                triShape.SetFlags(14);
                triShape.SetFlags2(8);
                triShape.SetData(file.AddBlock((NiObject)shapeDesc.geometry.ToNiTriShapeData(generateVertexColors)));
                if (shapeDesc.isAlpha)
                {
                    NiAlphaProperty alphaProperty = new NiAlphaProperty();
                    alphaProperty.SetFlags(4844);
                    alphaProperty.SetThreshold(shapeDesc.alphaThreshold);
                    triShape.SetBSProperty(1, file.AddBlock(alphaProperty));
                }
                else
                {
                    triShape.SetBSProperty(1, -1);
                }
                if (shapeDesc.shaderType == "bseffectshaderproperty")
                {
                    BSEffectShaderProperty effectShaderProperty = new BSEffectShaderProperty();
                    effectShaderProperty = shapeDesc.effectShader;
                    effectShaderProperty.SetSourceTexture(shapeDesc.textures[0]);
                    triShape.SetBSProperty(0, file.AddBlock((NiObject)effectShaderProperty));
                }
                else if (shapeDesc.shaderType == "bslightingshaderproperty")
                {
                    BSLightingShaderProperty lightingShaderProperty = new BSLightingShaderProperty();
                    lightingShaderProperty = shapeDesc.lightingShader;
                    triShape.SetBSProperty(0, file.AddBlock((NiObject)lightingShaderProperty));
                    BSShaderTextureSet shaderTextureSet = new BSShaderTextureSet();
                    lightingShaderProperty.SetTextureSet(file.AddBlock((NiObject)shaderTextureSet));
                    shaderTextureSet.SetNumTextures(9);
                    shaderTextureSet.SetTexture(0, shapeDesc.textures[0]);
                    shaderTextureSet.SetTexture(1, shapeDesc.textures[1]);
                    shaderTextureSet.SetTexture(2, shapeDesc.textures[2]);
                    shaderTextureSet.SetTexture(3, shapeDesc.textures[3]);
                    shaderTextureSet.SetTexture(4, shapeDesc.textures[4]);
                    shaderTextureSet.SetTexture(5, shapeDesc.textures[5]);
                    shaderTextureSet.SetTexture(6, shapeDesc.textures[6]);
                    shaderTextureSet.SetTexture(7, shapeDesc.textures[7]);
                    shaderTextureSet.SetTexture(8, shapeDesc.textures[8]);
                }
                else if (shapeDesc.shaderType == "nitexturingproperty")
                {
                    BSLightingShaderProperty lightingShaderProperty = new BSLightingShaderProperty();
                    if (generateVertexColors && shapeDesc.geometry.HasVertexColors())
                    {
                        lightingShaderProperty.SetShaderFlags2(lightingShaderProperty.GetShaderFlags2() | 32);
                    }
                    else
                    {
                        lightingShaderProperty.SetShaderFlags2(lightingShaderProperty.GetShaderFlags2() & 4294967263);
                    }
                    if (shapeDesc.materialProperty != null)
                    {
                        lightingShaderProperty.SetEmissiveColor(shapeDesc.materialProperty.GetEmissiveColor());
                        lightingShaderProperty.SetEmissiveMultiple(shapeDesc.materialProperty.GetEmissiveMultiple());
                        lightingShaderProperty.SetSpecularColor(shapeDesc.materialProperty.GetSpecularColor());
                        lightingShaderProperty.SetGlossiness(shapeDesc.materialProperty.GetGlossiness());
                        lightingShaderProperty.SetAlpha(shapeDesc.materialProperty.GetAlpha());
                    }

                    triShape.SetBSProperty(0, file.AddBlock((NiObject)lightingShaderProperty));
                    BSShaderTextureSet shaderTextureSet = new BSShaderTextureSet();
                    lightingShaderProperty.SetTextureSet(file.AddBlock((NiObject)shaderTextureSet));
                    shaderTextureSet.SetNumTextures(9);
                    shaderTextureSet.SetTexture(0, shapeDesc.textures[0]);
                    shaderTextureSet.SetTexture(1, shapeDesc.textures[1]);
                    shaderTextureSet.SetTexture(2, shapeDesc.textures[2]);
                    shaderTextureSet.SetTexture(3, shapeDesc.textures[3]);
                    shaderTextureSet.SetTexture(4, shapeDesc.textures[4]);
                    shaderTextureSet.SetTexture(5, shapeDesc.textures[5]);
                    shaderTextureSet.SetTexture(6, shapeDesc.textures[6]);
                    shaderTextureSet.SetTexture(7, shapeDesc.textures[7]);
                    shaderTextureSet.SetTexture(8, shapeDesc.textures[8]);
                }
                else
                {
                    if (verbose)
                    {
                        logFile.WriteLog(shapeDesc.staticModel + " " + shapeDesc.name + "unknown shader " + shapeDesc.shaderType);
                    }
                }
            }
        }

        private void CreateLODNodesFO3(NiFile file, BSMultiBoundNode node, QuadDesc quad, List<ShapeDesc> shapes)
        {
            foreach (ShapeDesc shapeDesc in shapes)
            {
                BSSegmentedTriShape segmentedTriShape = new BSSegmentedTriShape();
                node.AddChild(file.AddBlock((NiObject)segmentedTriShape));
                node.SetCullMode(1U);
                string str = "obj";
                // use material name from list file, Snow/Ash
                if (!this.ignoreMaterial && shapeDesc.material != "")
                    str = str + shapeDesc.material;
                // only level 4 should have HD
                if (shapeDesc.isHighDetail && (this.quadLevel == 4 || useHDFlag))
                {
                    str = str + "HD";
                }
                segmentedTriShape.SetFlags((ushort)14);
                segmentedTriShape.SetFlags2((ushort)8);
                segmentedTriShape.SetTranslation(new Vector3((float)quad.x * 4096f, (float)quad.y * 4096f, 0.0f));
                segmentedTriShape.SetRotation(new Matrix33(true));
                segmentedTriShape.SetScale((float)this.quadLevel);
                BSShaderTextureSet shaderTextureSet = new BSShaderTextureSet();
                BSShaderPPLightingProperty lightingShaderProperty = new BSShaderPPLightingProperty();
                segmentedTriShape.SetProperties(file.AddBlock((NiObject)lightingShaderProperty));
                lightingShaderProperty.SetTextureSet(file.AddBlock((NiObject)shaderTextureSet));
                shaderTextureSet.SetNumTextures(6);
                shaderTextureSet.SetTexture(0, shapeDesc.textures[0]);
                shaderTextureSet.SetTexture(1, shapeDesc.textures[1]);
                segmentedTriShape.SetData(file.AddBlock((NiObject)shapeDesc.geometry.ToNiTriShapeData(generateVertexColors)));
                for (int index = 0; index < 16; ++index)
                {
                    segmentedTriShape.AddSegment(new BSSegment(0U, (ushort)0));
                }
                for (int index = 0; index < shapeDesc.segments.Count; ++index)
                {
                    BSSegment segmentAtIndex = segmentedTriShape.GetSegmentAtIndex(shapeDesc.segments[index].id);
                    segmentAtIndex.startTriangle = shapeDesc.segments[index].startTriangle;
                    segmentAtIndex.numTriangles = shapeDesc.segments[index].numTriangles;
                    segmentedTriShape.SetSegment(shapeDesc.segments[index].id, segmentAtIndex);
                }
                for (int index = 15; index >= 0 && (int)segmentedTriShape.GetSegmentAtIndex(index).numTriangles == 0; --index)
                {
                    segmentedTriShape.RemoveSegment(index);
                }
                this.GenerateMultibound(file, node, quad, shapeDesc.boundingBox);
            }
        }

        private void CreateLODNodesFO4(NiFile file, NiNode rootNode, QuadDesc quad, List<ShapeDesc> shapes)
        {
            foreach (ShapeDesc shapeDesc in shapes)
            {
                BSMultiBoundNode node = new BSMultiBoundNode();
                rootNode.AddChild(file.AddBlock((NiObject)node));
                BSSubIndexTriShape subIndexTriShape = new BSSubIndexTriShape();
                int str0 = file.AddString("obj");
                int str1 = file.AddString("");
                int str2 = 0;
                if (shapeDesc.isAlpha)
                {
                    str0 = file.AddString("obj-at");
                }
                node.SetNameIndex(str1);
                subIndexTriShape = shapeDesc.geometry.ToBSSubIndexTriShape(generateVertexColors);
                subIndexTriShape.UpdateVertexData();
                subIndexTriShape.SetNameIndex(str0);
                subIndexTriShape.SetFlags((ushort)14);
                subIndexTriShape.SetFlags2((ushort)0);
                subIndexTriShape.SetScale((float)this.quadLevel);
                subIndexTriShape.SetRotation(new Matrix33(true));
                subIndexTriShape.SetTranslation(new Vector3((float)quad.x * 4096f, (float)quad.y * 4096f, 0.0f));
                subIndexTriShape.SetNumTriangles2(subIndexTriShape.GetNumTriangles() * 2);
                for (int index = 0; index < 16; ++index)
                {
                    subIndexTriShape.AddSegment(new BSSITSSegment(0U, (ushort)0));
                }
                for (int index = 0; index < shapeDesc.segments.Count; ++index)
                {
                    BSSITSSegment segmentAtIndex = subIndexTriShape.GetBSSITSSegmentAtIndex(shapeDesc.segments[index].id);
                    segmentAtIndex.triangleOffset = shapeDesc.segments[index].startTriangle;
                    segmentAtIndex.triangleCount = shapeDesc.segments[index].numTriangles;
                    subIndexTriShape.SetSegment(shapeDesc.segments[index].id, segmentAtIndex);
                }
                for (int index = 15; index >= 0 && (int)subIndexTriShape.GetBSSITSSegmentAtIndex(index).triangleCount == 0; --index)
                {
                    subIndexTriShape.RemoveBSSITSSegment(index);
                }
                node.AddChild(file.AddBlock(subIndexTriShape));
                if (shapeDesc.enableParent != 0)
                {
                    str2 = file.AddString("ToggleRefID");
                    NiIntegerExtraData integerExtraData = new NiIntegerExtraData();
                    integerExtraData.SetNameIndex(str2);
                    integerExtraData.SetIntegerData(shapeDesc.enableParent);
                    List<int> idx = new List<int>();
                    idx.Add(file.AddBlock(integerExtraData));
                    subIndexTriShape.SetExtraData(idx);
                }
                if (shapeDesc.isPassThru)
                {
                    if (shapeDesc.shaderType == "effectshader")
                    {
                        BSEffectShaderProperty effectShaderProperty = new BSEffectShaderProperty();
                        effectShaderProperty = shapeDesc.effectShader;
                        effectShaderProperty.SetTextureClampMode(shapeDesc.TextureClampMode);
                        effectShaderProperty.SetSourceTexture(shapeDesc.textures[0]);
                        subIndexTriShape.SetBSProperty(0, file.AddBlock((NiObject)effectShaderProperty));
                    }
                    else
                    {
                        BSLightingShaderProperty lightingShaderProperty = new BSLightingShaderProperty();
                        lightingShaderProperty = shapeDesc.lightingShader;
                        lightingShaderProperty.SetTextureClampMode(shapeDesc.TextureClampMode);
                        subIndexTriShape.SetBSProperty(0, file.AddBlock((NiObject)lightingShaderProperty));
                        BSShaderTextureSet shaderTextureSet = new BSShaderTextureSet();
                        lightingShaderProperty.SetTextureSet(file.AddBlock((NiObject)shaderTextureSet));
                        lightingShaderProperty.SetWetMaterialIndex(str1);
                        shaderTextureSet.SetNumTextures(10);
                        shaderTextureSet.SetTexture(0, shapeDesc.textures[0]);
                        shaderTextureSet.SetTexture(1, shapeDesc.textures[1]);
                        shaderTextureSet.SetTexture(2, shapeDesc.textures[2]);
                        shaderTextureSet.SetTexture(3, shapeDesc.textures[3]);
                        shaderTextureSet.SetTexture(4, shapeDesc.textures[4]);
                        shaderTextureSet.SetTexture(5, shapeDesc.textures[5]);
                        shaderTextureSet.SetTexture(6, shapeDesc.textures[6]);
                        shaderTextureSet.SetTexture(7, shapeDesc.textures[7]);
                        shaderTextureSet.SetTexture(8, shapeDesc.textures[8]);
                        shaderTextureSet.SetTexture(9, shapeDesc.textures[9]);
                    }
                }
                else
                {
                    BSLightingShaderProperty lightingShaderProperty = new BSLightingShaderProperty();
                    subIndexTriShape.SetBSProperty(0, file.AddBlock((NiObject)lightingShaderProperty));
                    lightingShaderProperty.SetNameIndex(str1);
                    int textureBlock = -1;
                    if (quad.textureBlockIndex.ContainsKey(shapeDesc.textures[0]))
                    {
                        textureBlock = quad.textureBlockIndex[shapeDesc.textures[0]];
                    }
                    else
                    {
                        BSShaderTextureSet shaderTextureSet = new BSShaderTextureSet();
                        shaderTextureSet.SetNumTextures(10);
                        shaderTextureSet.SetTexture(0, "data\\" + shapeDesc.textures[0]);
                        shaderTextureSet.SetTexture(1, "data\\" + shapeDesc.textures[1]);
                        shaderTextureSet.SetTexture(7, "data\\" + shapeDesc.textures[7]);
                        textureBlock = file.AddBlock((NiObject)shaderTextureSet);
                        quad.textureBlockIndex.Add(shapeDesc.textures[0], textureBlock);
                    }
                    lightingShaderProperty.SetTextureSet(textureBlock);
                    lightingShaderProperty.SetWetMaterialIndex(str1);
                    lightingShaderProperty.SetTextureClampMode(shapeDesc.TextureClampMode);
                    lightingShaderProperty.SetGlossiness(1f);
                    if (this.useBacklightPower)
                    {
                        lightingShaderProperty.SetBacklightPower(shapeDesc.backlightPower);
                    }
                    uint num1 = 2151677953U;
                    uint num2 = 5U;
                    /*if (shapeDesc.hasVertexColor)
                    {
                        num2 |= 32U;
                    }*/
                    if (shapeDesc.isDoubleSided || (this.alphaDoublesided && shapeDesc.isAlpha))
                    {
                        num2 |= 16U;
                    }
                    lightingShaderProperty.SetShaderFlags1(num1);
                    lightingShaderProperty.SetShaderFlags2(num2);
                }
                if (shapeDesc.isAlpha)
                {
                    NiAlphaProperty alphaProperty = new NiAlphaProperty();
                    alphaProperty.SetNameIndex(str1);
                    alphaProperty.SetFlags(4844);
                    if (this.useAlphaThreshold)
                    {
                        alphaProperty.SetThreshold(shapeDesc.alphaThreshold);
                    }
                    else
                    {
                        alphaProperty.SetThreshold(128);
                    }
                    subIndexTriShape.SetBSProperty(1, file.AddBlock(alphaProperty));
                }
                else
                {
                    subIndexTriShape.SetBSProperty(1, -1);
                }
                this.GenerateMultibound(file, node, quad, shapeDesc.boundingBox);
                node.SetCullMode(1U);
            }
        }

        private void MergeShapes(List<ShapeDesc> shapes)
        {
            int count = shapes.Count;
            Dictionary<string, List<ShapeDesc>> dictionary = new Dictionary<string, List<ShapeDesc>>();
            for (int index = 0; index < shapes.Count; ++index)
            {
                ShapeDesc shape = new ShapeDesc();
                shape = shapes[index];
                string key = shape.textures[0].ToLower(CultureInfo.InvariantCulture);

                if (this.quadLevel != 4 && shape.material.ToLower().Contains("largeref"))
                {
                    shape.material = Regex.Replace(shape.material, "-largeref", "", RegexOptions.IgnoreCase);
                }
                if (shape.geometry.HasTangents() && shape.geometry.HasBitangents())
                {
                    key = key + "_TB";
                }
                // use material name from list file, Snow/Ash
                if (!this.ignoreMaterial && shape.material != "")
                {
                    key = key + "_" + shape.material;
                }
                if (shape.isPassThru && shape.shaderHash != "")
                {
                    key = key + "_" + shape.shaderHash;
                }
                // only level 4 should have HD - only snow/ash shader care about HD flag
                if (shape.isHighDetail && (this.quadLevel == 4 || (useHDFlag && Game.Mode != "sse")))
                {
                    key = key + "_HD";
                }
                if ((Game.Mode == "tes5" || Game.Mode == "sse") && useDecalFlag && shape.isDecal)
                {
                    key = key + "_DC";
                }
                // double-sided
                if (shape.isDoubleSided || (this.alphaDoublesided && shape.isAlpha))
                {
                    key = key + "_DS";
                }
                if (shape.isAlpha)
                {
                    key = key + "_AT";
                }
                if (this.useAlphaThreshold)
                {
                    key = key + "_" + shape.alphaThreshold;
                }
                if (this.useBacklightPower)
                {
                    key = key + "_" + shape.backlightPower;
                }
                if (shape.enableParent != 0)
                {
                    key = key + "_" + shape.enableParent;
                }
                // clamp mode
                key = key + "_" + shape.TextureClampMode;
                if (!dontGroup && shape.isGroup)
                {
                    key = key + "_" + index;
                }
                // drawcalls over saving bytes
                if (Game.Mode == "sse" && (!shape.hasVertexColor || shape.allWhite) && dictionary.ContainsKey(key + "_VC"))
                {
                    dictionary[key + "_VC"].Add(shape);
                }
                else if (Game.Mode == "sse" && (shape.hasVertexColor || !shape.allWhite) && dictionary.ContainsKey(key))
                {
                    dictionary[key].Insert(0, shape);
                }
                else
                {
                    // vanilla has VC only in level 4, but do for HD and PassThru in higher level - no point in doing VC if they are all white
                    if ((this.quadLevel == 4 || (shape.isHighDetail && !useHDFlag) || shape.isPassThru) && shape.hasVertexColor && !shape.allWhite)
                    {
                        key = key + "_VC";
                    }
                    //logFile.WriteLog(index + " = " + shape.name + " = " + key);

                    if (dictionary.ContainsKey(key))
                    {
                        dictionary[key].Add(shape);
                    }
                    else
                    {
                        dictionary.Add(key, new List<ShapeDesc>());
                        dictionary[key].Add(shape);
                    }
                }

            }
            shapes.Clear();
            foreach (KeyValuePair<string, List<ShapeDesc>> keyValuePair in dictionary)
            {
                if (keyValuePair.Value.Count == 1)
                {
                    shapes.Add(keyValuePair.Value[0]);
                }
                else
                {
                    SegmentDesc segmentDesc = (SegmentDesc)null;
                    uint num1 = 0U;
                    uint num2 = 0U;
                    int num3 = -1;
                    //NiTriShape niTriShape = new NiTriShape();
                    //NiTriShapeData data = new NiTriShapeData();
                    Geometry geometry = new Geometry();
                    List<ShapeDesc> list = keyValuePair.Value;
                    ShapeDesc baseShapeDesc = new ShapeDesc(list[0]);
                    bool hasVertexColors = false;
                    if ((this.quadLevel == 4 || baseShapeDesc.isHighDetail || baseShapeDesc.isPassThru) && baseShapeDesc.hasVertexColor && !baseShapeDesc.allWhite)
                    {
                        hasVertexColors = true;
                    }
                    list.Sort((Comparison<ShapeDesc>)((a, b) =>
                    {
                        if (a.segments[0].id > b.segments[0].id)
                        {
                            return 1;
                        }
                        return a.segments[0].id < b.segments[0].id ? -1 : 0;
                    }));
                    ShapeDesc shapeDesc = new ShapeDesc(baseShapeDesc);
                    shapeDesc.boundingBox.Set(float.MaxValue, float.MinValue, float.MaxValue, float.MinValue, float.MaxValue, float.MinValue);
                    for (int index1 = 0; index1 < list.Count; ++index1)
                    {
                        if ((Game.Mode == "merge4" || Game.Mode == "merge5") && index1 > 0 && list[0].shapeHash == list[index1].shapeHash)
                        {
                            logFile.WriteLog("Ignoring duplicate shape with name #" + index1 + ": " + list[index1].name + ", #0: " + list[0].name);
                            continue;
                        }
                        Geometry geometrySegment = list[index1].geometry;
                        List<Vector3> vertices = geometrySegment.GetVertices();
                        List<Triangle> triangles = new List<Triangle>(geometrySegment.GetTriangles());
                        if ((uint)geometry.GetNumVertices() + (uint)vertices.Count > (uint)ushort.MaxValue || (uint)geometry.GetNumTriangles() + (uint)triangles.Count > (uint)ushort.MaxValue)
                        {
                            shapeDesc.segments.Add(segmentDesc);
                            shapeDesc.geometry = geometry;
                            shapes.Add(shapeDesc);

                            shapeDesc = new ShapeDesc(baseShapeDesc);
                            shapeDesc.boundingBox.Set(float.MaxValue, float.MinValue, float.MaxValue, float.MinValue, float.MaxValue, float.MinValue);
                            geometry = new Geometry();
                            num1 = 0U;
                            num2 = 0U;
                            num3 = -1;
                        }
                        for (int index2 = 0; index2 < (int)geometrySegment.GetNumVertices(); ++index2)
                        {
                            Vector3 vector3 = vertices[index2] * (float)this.quadLevel;
                            shapeDesc.boundingBox.GrowByVertex(vector3);
                        }
                        geometry.AppendVertices(geometrySegment.GetVertices());
                        geometry.AppendNormals(geometrySegment.GetNormals());
                        geometry.AppendUVCoords(geometrySegment.GetUVCoords());
                        if (this.generateTangents)
                        {
                            if (!useOptimizer || (useOptimizer && (this.quadLevel == 4 || baseShapeDesc.isHighDetail || baseShapeDesc.isPassThru)))
                            {
                                geometry.AppendTangents(geometrySegment.GetTangents());
                                geometry.AppendBitangents(geometrySegment.GetBitangents());
                            }
                        }
                        // genenerate missing vertex colors
                        List<Color4> colors = geometrySegment.GetVertexColors();
                        if (hasVertexColors && colors.Count == 0)
                        {
                            for (int index2 = 0; index2 < vertices.Count; ++index2)
                            {
                                colors.Add(new Color4(1f, 1f, 1f, 1f));
                            }
                        }
                        if (hasVertexColors)
                        {
                            geometry.AppendVertexColors(colors);
                        }
                        List<Triangle> trianglesNew = new List<Triangle>();
                        for (int index2 = 0; index2 < (int)geometrySegment.GetNumTriangles(); ++index2)
                        {
                            trianglesNew.Add(new Triangle((ushort)(triangles[index2][0] + num1), (ushort)(triangles[index2][1] + num1), (ushort)(triangles[index2][2] + num1)));
                        }
                        geometry.AppendTriangles(trianglesNew);
                        num1 += (uint)geometrySegment.GetNumVertices();
                        if (list[index1].segments[0].id != num3)
                        {
                            if (num3 != -1)
                            {
                                num2 += (uint)segmentDesc.numTriangles * 3U;
                                shapeDesc.segments.Add(segmentDesc);
                            }
                            segmentDesc = new SegmentDesc();
                            segmentDesc.id = list[index1].segments[0].id;
                            segmentDesc.startTriangle = num2;
                            segmentDesc.numTriangles = list[index1].segments[0].numTriangles;
                            num3 = list[index1].segments[0].id;
                        }
                        else
                        {
                            segmentDesc.numTriangles += list[index1].geometry.GetNumTriangles();
                        }
                    }
                    //data.SetCenter(shapeDesc.boundingBox.GetCenter() / (float)this.quadLevel);
                    //data.SetRadius(this.CalcRadius(data));
                    shapeDesc.segments.Add(segmentDesc);
                    shapeDesc.hasVertexColor = hasVertexColors;
                    if (!this.generateVertexColors)
                    {
                        geometry.SetVertexColors(new List<Color4>());
                        shapeDesc.hasVertexColor = false;
                    }
                    shapeDesc.geometry = geometry;
                    shapes.Add(shapeDesc);
                }
            }
        }

        private float CalcRadius(NiTriShapeData data)
        {
            float num1 = float.MinValue;
            for (int index = 0; index < (int)data.GetNumVertices(); ++index)
            {
                Vector3 vector3_1 = data.GetVertices()[index];
                Vector3 vector3_2 = data.GetCenter() - vector3_1;
                float num2 = (float)((double)vector3_2[0] * (double)vector3_2[0] + (double)vector3_2[1] * (double)vector3_2[1] + (double)vector3_2[2] * (double)vector3_2[2]);
                if ((double)num2 > (double)num1)
                    num1 = num2;
            }
            return (float)Math.Sqrt((double)num1);
        }

        private bool LoadTerrainQuad(QuadDesc quad, out QuadTree qt, out QuadTree waterQt, out BBox boundingBox)
        {
            bool flag = false;
            string str = "";
            if (Game.Mode == "fo3")
            {
                str = "meshes\\landscape\\lod\\" + this.worldspaceName + "\\" + this.worldspaceName + ".Level" + this.quadLevel.ToString() + ".X" + quad.x.ToString() + ".Y" + quad.y.ToString() + ".nif";
            }
            else
            {
                str = "meshes\\terrain\\" + this.worldspaceName + "\\" + this.worldspaceName + "." + this.quadLevel.ToString() + "." + quad.x.ToString() + "." + quad.y.ToString() + ".btr";
            }
            boundingBox = new BBox();
            qt = (QuadTree)null;
            waterQt = (QuadTree)null;
            if (!File.Exists(this.gameDir + str) && !BSAArchive.FileExists(str))
            {
                if (this.verbose)
                {
                    this.logFile.WriteLog("terrain file not found " + str);
                }
            }
            else
            {
                //logFile.WriteLog("Loading " + str);
                NiFile niFile = new NiFile();
                niFile.Read(this.gameDir, str, logFile);
                BSMultiBoundNode bsMultiBoundNode1 = (BSMultiBoundNode)niFile.GetBlockAtIndex(0);
                if (bsMultiBoundNode1 != null && bsMultiBoundNode1.GetNumChildren() > 0U)
                {
                    BSMultiBound bsMultiBound1 = (BSMultiBound)niFile.GetBlockAtIndex(bsMultiBoundNode1.GetMultiBound());
                    BSMultiBoundAABB bsMultiBoundAabb1 = bsMultiBound1 != null ? (BSMultiBoundAABB)niFile.GetBlockAtIndex(bsMultiBound1.GetData()) : (BSMultiBoundAABB)null;
                    if (bsMultiBound1 != null && bsMultiBoundAabb1 != null)
                    {
                        NiTriBasedGeom geom = new NiTriBasedGeom();
                        NiObject block = niFile.GetBlockAtIndex(bsMultiBoundNode1.GetChildAtIndex(0));
                        if (block.IsDerivedType("BSTriShape"))
                        {
                            BSTriShape geomOld = (BSTriShape)block;
                            geom.SetFlags(14);
                            geom.SetTranslation(geomOld.GetTranslation());
                            geom.SetRotation(geomOld.GetRotation());
                            geom.SetScale(geomOld.GetScale());
                            geom.SetData(bsMultiBoundNode1.GetChildAtIndex(0));
                        }
                        else
                        {
                            geom = (NiTriBasedGeom)block;
                        }
                        if (geom != null)
                        {
                            ShapeDesc shapedesc = new ShapeDesc(this.gameDir, niFile, geom, new StaticDesc(), this.quadIndex, PassThruMeshList, skyblivionTexPath, false, false, false, true, verbose, this.logFile);
                            Vector3 vector3_1 = bsMultiBoundAabb1.GetPosition() / 4;
                            Vector3 vector3_2 = bsMultiBoundAabb1.GetExtent() / 4;
                            boundingBox.Set(vector3_1[0] - vector3_2[0], vector3_1[0] + vector3_2[0], vector3_1[1] - vector3_2[1], vector3_1[1] + vector3_2[1], vector3_1[2] - vector3_2[2], vector3_1[2] + vector3_2[2]);
                            List<Vector3> vertices = shapedesc.geometry.GetVertices();
                            if (Game.Mode == "fo3")
                            {
                                for (int index = 0; index < vertices.Count; index++)
                                {
                                    vertices[index] = vertices[index] / quadLevel;
                                }
                            }
                            List<Triangle> triangles = shapedesc.geometry.GetTriangles();
                            for (int index = 0; index < triangles.Count; index++)
                            {
                                Vector3 vertA = vertices[triangles[index][0]];
                                Vector3 vertB = vertices[triangles[index][1]];
                                Vector3 vertC = vertices[triangles[index][2]];
                                if ((vertA[0] == 0 && vertB[0] == 0 && vertC[0] == 0) || (vertA[0] == 4096 && vertB[0] == 4096 && vertC[0] == 4096) || (vertA[1] == 0 && vertB[1] == 0 && vertC[1] == 0) || (vertA[1] == 4096 && vertB[1] == 4096 && vertC[1] == 4096))
                                {
                                    triangles.RemoveAt(index);
                                    --index;
                                }
                            }
                            shapedesc.geometry.SetVertices(vertices);
                            shapedesc.geometry.SetTriangles(triangles);
                            qt = new QuadTree(shapedesc, quadLevel);
                            if (qt != null)
                            {
                                flag = true;
                            }
                        }
                    }
                    if (Game.Mode != "fo3" && this.removeUnderwaterFaces && bsMultiBoundNode1.GetNumChildren() > 1U)
                    {
                        BSMultiBoundNode bsMultiBoundNode2 = (BSMultiBoundNode)niFile.GetBlockAtIndex(bsMultiBoundNode1.GetChildAtIndex(1));
                        if (bsMultiBoundNode2 != null)
                        {
                            BSMultiBound bsMultiBound2 = (BSMultiBound)niFile.GetBlockAtIndex(bsMultiBoundNode2.GetMultiBound());
                            BSMultiBoundAABB bsMultiBoundAabb2 = (BSMultiBoundAABB)niFile.GetBlockAtIndex(bsMultiBound2.GetData());
                            //NiTriShapeData datacombined = new NiTriShapeData();
                            //NiTriBasedGeom geom = new NiTriBasedGeom();
                            ShapeDesc shapecombined = new ShapeDesc();
                            ushort numVertices = 0;
                            for (int index = 0; index < bsMultiBoundNode2.GetNumChildren(); index++)
                            {
                                NiTriBasedGeom geom = new NiTriBasedGeom();
                                NiObject block = niFile.GetBlockAtIndex(bsMultiBoundNode2.GetChildAtIndex(index));
                                if (block.IsDerivedType("BSTriShape"))
                                {
                                    BSTriShape geomOld = (BSTriShape)block;
                                    geom.SetFlags(14);
                                    geom.SetTranslation(geomOld.GetTranslation());
                                    geom.SetRotation(geomOld.GetRotation());
                                    geom.SetScale(geomOld.GetScale());
                                    geom.SetData(bsMultiBoundNode2.GetChildAtIndex(index));
                                }
                                else
                                {
                                    geom = (NiTriBasedGeom)block;
                                }
                                if (geom != null)
                                {
                                    ShapeDesc shapedesc = new ShapeDesc(this.gameDir, niFile, geom, new StaticDesc(), this.quadIndex, PassThruMeshList, skyblivionTexPath, false, false, false, true, verbose, this.logFile);
                                    if (index == 0)
                                    {
                                        shapecombined = shapedesc;
                                        numVertices += shapedesc.geometry.GetNumVertices();
                                    }
                                    else
                                    {
                                        shapecombined.geometry.AppendVertices(shapedesc.geometry.GetVertices());
                                        List<Triangle> triangles = new List<Triangle>(shapedesc.geometry.GetTriangles());
                                        for (int index2 = 0; index2 < triangles.Count; ++index2)
                                        {
                                            triangles[index2][0] += numVertices;
                                            triangles[index2][1] += numVertices;
                                            triangles[index2][2] += numVertices;
                                        }
                                        shapecombined.geometry.AppendTriangles(triangles);
                                        numVertices += shapedesc.geometry.GetNumVertices();
                                    }
                                }
                            }
                            Vector3 vector3_1 = bsMultiBoundAabb2.GetPosition() / 4;
                            Vector3 vector3_2 = bsMultiBoundAabb2.GetExtent() / 4;
                            BBox boundingBox1 = new BBox(vector3_1[0] - vector3_2[0], vector3_1[0] + vector3_2[0], vector3_1[1] - vector3_2[1], vector3_1[1] + vector3_2[1], vector3_1[2] - vector3_2[2], vector3_1[2] + vector3_2[2]);
                            waterQt = new QuadTree(shapecombined, quadLevel);
                        }
                    }
                }
            }
            return flag;
        }

        public void GenerateLOD(int index, int level, List<StaticDesc> statics)
        {
            int workerThreads;
            int portThreads;
            ThreadPool.GetMinThreads(out workerThreads, out portThreads);
            if (Game.Mode.Contains("convert"))
            {
                if (level > 1)
                {
                    return;
                }
                this.quadLevel = 1;
                this.quadOffset = 0;
                List<Thread> threads = new List<Thread>();
                for (int index1 = 0; index1 < statics.Count; index1++)
                {
                    StaticDesc stat = statics[index1];
                    while (threads.Count == workerThreads)
                    {
                        for (int index2 = 0; index2 < threads.Count; ++index2)
                        {
                            if (!threads[index2].IsAlive)
                            {
                                threads.RemoveAt(index2);
                                --index2;
                            }
                        }
                        Thread.Sleep(50);
                    }
                    threads.Add(new Thread((ParameterizedThreadStart)(state =>
                    {
                        List<ShapeDesc> shapes = new List<ShapeDesc>();
                        QuadDesc quad = new QuadDesc(true);
                        quad.x = 0;
                        quad.y = 0;
                        quad.statics = new List<StaticDesc>();
                        quad.statics.Add((StaticDesc)state);
                        string fileName = Path.Combine(this.outputDir, stat.staticModels[index]);
                        DateTime now = DateTime.Now;
                        //if (verbose)
                        //{
                        //    logFile.WriteLog("Started " + fileName);
                        //}
                        shapes.AddRange(this.ParseNif(quad, quad.statics[index], index));
                        if (this.mergeShapes)
                        {
                            this.MergeShapes(shapes);
                        }
                        NiFile file = new NiFile();
                        NiNode rootNiNode = new NiNode();
                        BSFadeNode rootBSFadeNode = new BSFadeNode();
                        if (Game.Mode == "convert4")
                        {
                            file.SetHeaderString("Gamebryo File Format, Version 20.2.0.7");
                            file.SetVersion(335544325U);
                            file.SetUserVersion(11U);
                            file.SetUserVersion2(11U);
                            rootNiNode.SetFlags(0);
                            rootNiNode.SetNameIndex(file.AddString("Scene Root"));
                        }
                        else if (Game.Mode == "convertsse")
                        {
                            file.SetUserVersion(12U);
                            file.SetUserVersion2(100U);

                            int shapeindex = 0;
                            List<int> blocklist = new List<int>();
                            NiFile srcFile = new NiFile();
                            string srcFileName = stat.staticModels[index];
                            if (srcFileName.ToLower().Contains(".nif"))
                            {
                                try
                                {
                                    srcFile.Read(this.gameDir, srcFileName, this.logFile);
                                    logFile.WriteLog(srcFileName + " Processing file");
                                }
                                catch
                                {
                                    logFile.WriteLog(srcFileName + " Can not process file");
                                }
                            }
                            else
                            {
                                logFile.WriteLog("Not a nif " + srcFileName);
                            }

                            for (int index2 = 0; index2 < srcFile.GetNumStrings(); index2++)
                            {
                                file.AddString(srcFile.GetStringAtIndex(index2));
                            }

                            List<int> datablocks = new List<int>();
                            List<int> refEx = Enumerable.Repeat(-1, srcFile.GetNumBlocks()).ToList();

                            for (int index2 = 0; index2 < srcFile.GetNumBlocks(); index2++)
                            {
                                try
                                {
                                    NiObject block = new NiObject();
                                    try
                                    {
                                        block = srcFile.GetBlockAtIndex(index2);
                                    }
                                    catch
                                    {
                                        logFile.WriteLog(srcFileName + ": " + index2 + " can not read block");
                                        refEx[index2] = file.GetNumBlocks();
                                        file.AddBlock(new NiNode());
                                        continue;
                                    }
                                    if (block == null)
                                    {
                                        if (verbose)
                                        {
                                            logFile.WriteLog(srcFileName + ": " + index2 + " " + srcFile.GetBlockTypeAtIndex(index2) + " -> raw data copy");
                                        }
                                        refEx[index2] = file.GetNumBlocks();
                                        file.AddRawBlock(srcFile.GetRawBlockAtIndex(index2), srcFile.GetBlockTypeAtIndex(index2));
                                        continue;
                                    }
                                    if (datablocks.Contains(index2))
                                    {
                                        if (!removeBlocks)
                                        {
                                            if (verbose)
                                            {
                                                logFile.WriteLog(srcFileName + ": " + index2 + " " + srcFile.GetBlockTypeAtIndex(index2) + " -> replaced");
                                            }
                                            refEx[index2] = file.GetNumBlocks();
                                            file.AddBlock(new NiNode());
                                        }
                                        else
                                        {
                                            if (verbose)
                                            {
                                                logFile.WriteLog(srcFileName + ": " + index2 + " " + srcFile.GetBlockTypeAtIndex(index2) + " -> removed");
                                            }
                                        }
                                        continue;
                                    }
                                    if (verbose)
                                    {
                                        logFile.WriteLog(srcFileName + ": " + index2 + " " + srcFile.GetBlockTypeAtIndex(index2));
                                    }
                                    if (block.GetType() == typeof(NiTriShape) || block.GetType() == typeof(BSLODTriShape))
                                    {
                                        NiTriBasedGeom shape = (NiTriBasedGeom)block;
                                        BSTriShape newshape = new BSTriShape(shape);
                                        if (shapeindex >= shapes.Count)
                                        {
                                            shapeindex = shapes.Count - 1;
                                        }
                                        Geometry geom = new Geometry((NiTriShapeData)srcFile.GetBlockAtIndex(shape.GetData()));
                                        newshape = geom.ToBSTriShape(newshape, generateVertexColors);
                                        shapeindex++;
                                        newshape.UpdateVertexData();
                                        refEx[index2] = file.GetNumBlocks();
                                        file.AddBlock(newshape);
                                        datablocks.Add(shape.GetData());
                                        /*if (removeBlocks)
                                        {
                                            for (int index3 = 0; index3 < 2; index3++)
                                            {
                                                if (shape.GetBSProperty(index3) != -1)
                                                {
                                                    int propertyindex = shape.GetBSProperty(index3);
                                                    datablocks.Add(propertyindex);
                                                    NiProperty propertyblock = (NiProperty)srcFile.GetBlockAtIndex(propertyindex);
                                                    if (verbose)
                                                    {
                                                        logFile.WriteLog(srcFileName + ": " + propertyindex + " " + propertyblock.GetClassName());
                                                    }
                                                    if (propertyblock.GetType() == typeof(BSLightingShaderProperty))
                                                    {
                                                        BSLightingShaderProperty shader = (BSLightingShaderProperty)propertyblock;
                                                        datablocks.Add(shader.GetTextureSet());
                                                        BSShaderTextureSet textureblock = (BSShaderTextureSet)srcFile.GetBlockAtIndex(shader.GetTextureSet());
                                                        refEx[propertyindex] = file.GetNumBlocks();
                                                        newshape.SetBSProperty(index3, file.AddBlock(shader));
                                                        refEx[shader.GetTextureSet()] = file.GetNumBlocks();
                                                        shader.SetTextureSet(file.AddBlock(textureblock));
                                                    }
                                                    else if (propertyblock.GetType() == typeof(BSShaderPPLightingProperty))
                                                    {
                                                        BSShaderPPLightingProperty shader = (BSShaderPPLightingProperty)propertyblock;
                                                        datablocks.Add(shader.GetTextureSet());
                                                        BSShaderTextureSet textureblock = (BSShaderTextureSet)srcFile.GetBlockAtIndex(shader.GetTextureSet());
                                                        refEx[propertyindex] = file.GetNumBlocks();
                                                        newshape.SetBSProperty(index3, file.AddBlock(shader));
                                                        refEx[shader.GetTextureSet()] = file.GetNumBlocks();
                                                        shader.SetTextureSet(file.AddBlock(textureblock));
                                                    }
                                                    else
                                                    {
                                                        refEx[propertyindex] = file.GetNumBlocks();
                                                        newshape.SetBSProperty(index3, file.AddBlock(propertyblock));
                                                    }
                                                }
                                            }
                                        }*/
                                    }
                                    else
                                    {
                                        refEx[index2] = file.GetNumBlocks();
                                        file.AddBlock(block);
                                    }
                                }
                                finally
                                {
                                    //Console.WriteLine(index2 + " .!!!. " + _file.GetNumBlocks()-1);
                                }
                            }
                            for (int index2 = 0; index2 < refEx.Count; index2++)
                            {
                                //Console.WriteLine("ref " + index2 + " = " + refEx[index2]);
                                file.AddBlockReference(refEx[index2]);
                            }
                        }
                        else if (Game.Mode != "convert5")
                        {
                            rootNiNode.SetNameIndex(file.AddString("obj"));
                        }
                        if (Game.Mode == "convertsse")
                        {
                            //file.AddBlock(rootBSFadeNode);
                        }
                        else
                        {
                            file.AddBlock(rootNiNode);
                        }
                        if (Game.Mode == "convert5")
                        {
                            this.CreateMergeNodes(file, rootNiNode, quad, shapes);
                            if (rootNiNode.GetNumChildren() == 0)
                            {
                                return;
                            }
                        }
                        else if (Game.Mode == "convertsse")
                        {
                            //this.CreateMergeSSENodes(file, rootBSFadeNode, quad, shapes);
                            //if (rootNiNode.GetNumChildren() == 0)
                            //{
                            //    return;
                            //}
                        }
                        else if (Game.Mode == "convert4")
                        {
                            this.CreateMerge4Nodes(file, rootNiNode, quad, shapes);
                            if (rootNiNode.GetNumChildren() == 0)
                            {
                                return;
                            }
                        }
                        if (!Directory.Exists(Path.GetDirectoryName(fileName)))
                        {
                            Directory.CreateDirectory(Path.GetDirectoryName(fileName));
                        }
                        file.Write(fileName, logFile);
                        if (verbose)
                        {
                            this.logFile.WriteLog("Created " + fileName + " " + (DateTime.Now - now).ToString());
                        }
                    })));
                    threads[threads.Count - 1].Start((object)stat);
                }
                while (threads.Count > 0)
                {
                    for (int index1 = 0; index1 < threads.Count; ++index1)
                    {
                        if (!threads[index1].IsAlive)
                        {
                            threads.RemoveAt(index1);
                            --index1;
                        }
                    }
                    Thread.Sleep(50);
                }
                return;
            }
            this.quadIndex = index;
            if (Game.Mode == "fo3")
            {
                this.quadLevel = 4;
                this.quadOffset = 16384f;
                if (level == 4)
                {
                    return;
                }
            }
            else if (Game.Mode == "merge4" || Game.Mode == "merge5")
            {
                this.quadLevel = 4;
                this.quadOffset = 0;
                if (level > 1)
                {
                    return;
                }
            }
            else if (Game.Mode == "textureslist")
            {
                if (level > 1)
                {
                    return;
                }
                List<string> models = new List<string>();
                List<string> textures = new List<string>();
                logFile.WriteLog("Tolerance: " + atlasToleranceMin + ", " + atlasToleranceMax);
                Parallel.For(0, statics.Count, index1 =>
                {
                    List<ShapeDesc> shapes = new List<ShapeDesc>();
                    QuadDesc quad = new QuadDesc(true);
                    quad.x = 0;
                    quad.y = 0;
                    quad.statics = new List<StaticDesc>();
                    StaticDesc stat = statics[index1];
                    quad.statics.Add(stat);
                    for (int index2 = 0; index2 < 3; index2++)
                    {
                        string model = stat.staticModels[index2];
                        if (!models.Contains(model))
                        {
                            models.Add(model);
                            shapes.AddRange(this.ParseNif(quad, stat, index2));
                        }
                    }
                    for (int index2 = 0; index2 < shapes.Count; index2++)
                    {
                        bool ok = true;
                        ShapeDesc shape = shapes[index2];
                        string texture = shape.textures[0];
                        if (!textures.Contains(texture) && !HDTextureList.Any(texture.Contains))
                        {
                            if (verbose)
                            {
                                logFile.WriteLog("Checking " + shape.staticModel + " : " + shape.staticName);
                            }
                            List<UVCoord> uvcoords = shape.geometry.GetUVCoords();
                            for (int index3 = 0; index3 < uvcoords.Count; ++index3)
                            {
                                float u = uvcoords[index3][0];
                                float v = uvcoords[index3][1];
                                if (u < atlasToleranceMin || u > atlasToleranceMax || v < atlasToleranceMin || u > atlasToleranceMax)
                                {
                                    if (verbose)
                                    {
                                        logFile.WriteLog("UV outside " + shape.staticModel + " " + texture);
                                    }
                                    ok = false;
                                    break;
                                }
                            }
                            if (ok && !textures.Contains(texture))
                            {
                                textures.Add(texture);
                            }
                        }
                    }
                });
                logFile.WriteLog("Writing " + texturesListFile);
                File.WriteAllLines(texturesListFile, textures);
                return;
            }
            else
            {
                this.quadLevel = level * 4;
                this.quadOffset = (float)level * 16384f;
                if (Game.Mode != "fo4" && level > 4)
                {
                    return;
                }
            }
            if (this.lodLevelToGenerate != Int32.MinValue && this.lodLevelToGenerate != this.quadLevel)
            {
                return;
            }
            List<QuadDesc> list1 = this.SortMeshesIntoQuads(statics);
            if (this.removeUnseenFaces)
            {
                for (int index1 = 0; index1 < list1.Count; index1++)
                {
                    QuadDesc quad = list1[index1];
                    if (this.LoadTerrainQuad(quad, out quad.terrainQuadTree, out quad.waterQuadTree, out quad.boundingBox))
                    {
                        quad.hasTerrainVertices = true;
                        list1[index1] = quad;
                    }
                }
            }
            //list1.Sort((x, y) => x.statics.Count().CompareTo(y.statics.Count()));
            this.quadList = list1;
            List<Thread> list2 = new List<Thread>();
            for (int index1 = 0; index1 < list1.Count; ++index1)
            {
                QuadDesc quadDesc = list1[index1];
                if ((this.lodX == Int32.MinValue || this.lodX == quadDesc.x) && (this.lodY == Int32.MinValue || this.lodY == quadDesc.y))
                {
                    while (list2.Count == workerThreads)
                    {
                        for (int index2 = 0; index2 < list2.Count; ++index2)
                        {
                            if (!list2[index2].IsAlive)
                            {
                                list2.RemoveAt(index2);
                                --index2;
                            }
                        }
                        Thread.Sleep(50);
                    }
                    list2.Add(new Thread((ParameterizedThreadStart)(state =>
                    {
                        QuadDesc quad = (QuadDesc)state;
                        string fileName = "";
                        if (Game.Mode == "fo3")
                        {
                            if (level == 1)
                            {
                                fileName = this.outputDir + "\\" + this.worldspaceName + ".Level4.X" + quad.x.ToString() + ".Y" + quad.y.ToString() + ".nif";
                            }
                            else
                            {
                                fileName = this.outputDir + "\\" + this.worldspaceName + ".Level4.High.X" + quad.x.ToString() + ".Y" + quad.y.ToString() + ".nif";
                            }
                        }
                        else if (Game.Mode == "merge4" || Game.Mode == "merge5")
                        {
                            fileName = this.outputDir + "\\" + this.worldspaceName + ".nif";
                        }
                        else
                        {
                            fileName = this.outputDir + "\\" + this.worldspaceName + "." + this.quadLevel.ToString() + "." + quad.x.ToString() + "." + quad.y.ToString() + ".bto";
                        }

                        /*if (File.Exists(fileName))
                        {
                            logFile.WriteLog("File already exists: " + fileName);
                            return;
                        }*/

                        if (this.verbose)
                        {
                            this.logFile.WriteLog("Started LOD level " + this.quadLevel.ToString() + " coord [" + quad.x.ToString() + ", " + quad.y.ToString() + "]");
                        }
                        NiFile file = new NiFile();
                        NiNode rootNiNode = new NiNode();
                        BSMultiBoundNode rootBSMultiBoundNode = new BSMultiBoundNode();

                        // defaults are for TES5
                        if (Game.Mode == "sse")
                        {
                            file.SetUserVersion(12U);
                            file.SetUserVersion2(100U);
                        }
                        if (Game.Mode == "fo3")
                        {
                            file.SetUserVersion(11U);
                            file.SetUserVersion2(34U);
                            rootBSMultiBoundNode.SetFlags(2062);
                            rootBSMultiBoundNode.SetFlags2(8);
                        }
                        else if (Game.Mode == "fo4")
                        {
                            file.SetUserVersion(12U);
                            file.SetUserVersion2(130U);
                        }
                        else if (Game.Mode == "merge4")
                        {
                            file.SetHeaderString("Gamebryo File Format, Version 20.0.0.5");
                            file.SetVersion(335544325U);
                            file.SetUserVersion(11U);
                            file.SetUserVersion2(11U);
                            rootNiNode.SetFlags(0);
                        }

                        if (Game.Mode != "fo3")
                        {
                            file.AddBlock((NiObject)rootNiNode);
                        }
                        else
                        {
                            file.AddBlock((NiObject)rootBSMultiBoundNode);
                        }
                        if (Game.Mode == "merge4")
                        {
                            rootNiNode.SetNameIndex(file.AddString("Scene Root"));
                        }
                        else if (Game.Mode != "fo3" && Game.Mode != "merge5")
                        {
                            rootNiNode.SetNameIndex(file.AddString("obj"));
                        }
                        List<ShapeDesc> shapes = new List<ShapeDesc>();
                        for (int index11 = 0; index11 < quad.statics.Count; ++index11)
                        {
                            if (!(quad.statics[index11].staticModels[index] == ""))
                            {
                                shapes.AddRange(this.ParseNif(quad, quad.statics[index11], index));
                            }
                        }
                        if (this.mergeShapes)
                        {
                            this.MergeShapes(shapes);
                        }
                        if (Game.Mode == "fo3")
                        {
                            this.CreateLODNodesFO3(file, rootBSMultiBoundNode, quad, shapes);
                            if (verbose)
                            {
                                this.logFile.WriteLog("Finished LOD level " + this.quadLevel.ToString() + " coord [" + quad.x.ToString() + ", " + quad.y.ToString() + "] [" + quad.outValues.totalTriCount.ToString() + "/" + quad.outValues.reducedTriCount.ToString() + "]");
                            }
                            if ((int)rootBSMultiBoundNode.GetNumChildren() == 0)
                            {
                                return;
                            }
                        }
                        else if (Game.Mode == "fo4")
                        {
                            this.CreateLODNodesFO4(file, rootNiNode, quad, shapes);
                            if (verbose)
                            {
                                this.logFile.WriteLog("Finished LOD level " + this.quadLevel.ToString() + " coord [" + quad.x.ToString() + ", " + quad.y.ToString() + "] [" + quad.outValues.totalTriCount.ToString() + "/" + quad.outValues.reducedTriCount.ToString() + "]");
                            }
                            if ((int)rootNiNode.GetNumChildren() == 0)
                            {
                                return;
                            }
                        }
                        else if (Game.Mode == "merge5")
                        {
                            this.CreateMergeNodes(file, rootNiNode, quad, shapes);
                            if ((int)rootNiNode.GetNumChildren() == 0)
                            {
                                return;
                            }
                        }
                        else if (Game.Mode == "merge4")
                        {
                            this.CreateMerge4Nodes(file, rootNiNode, quad, shapes);
                            if ((int)rootNiNode.GetNumChildren() == 0)
                            {
                                return;
                            }
                        }
                        else if (Game.Mode == "sse")
                        {
                            this.CreateLODNodesSSE(file, rootNiNode, quad, shapes);
                            if (verbose)
                            {
                                this.logFile.WriteLog("Finished LOD level " + this.quadLevel.ToString() + " coord [" + quad.x.ToString() + ", " + quad.y.ToString() + "] [" + quad.outValues.totalTriCount.ToString() + "/" + quad.outValues.reducedTriCount.ToString() + "]");
                            }
                            if ((int)rootNiNode.GetNumChildren() == 0)
                            {
                                //return;
                            }
                        }
                        else
                        {
                            this.CreateLODNodes(file, rootNiNode, quad, shapes);
                            if (verbose)
                            {
                                this.logFile.WriteLog("Finished LOD level " + this.quadLevel.ToString() + " coord [" + quad.x.ToString() + ", " + quad.y.ToString() + "] [" + quad.outValues.totalTriCount.ToString() + "/" + quad.outValues.reducedTriCount.ToString() + "]");
                            }
                            if ((int)rootNiNode.GetNumChildren() == 0)
                            {
                                //return;
                            }
                        }
                        file.Write(fileName, logFile);
                        if (!verbose)
                        {
                            this.logFile.WriteLog("Finished LOD level " + (object)this.quadLevel.ToString() + " coord [" + quad.x.ToString() + ", " + quad.y.ToString() + "] [" + quad.outValues.totalTriCount.ToString() + "/" + quad.outValues.reducedTriCount.ToString() + "]");
                        }
                    })));
                    list2[list2.Count - 1].Start((object)quadDesc);
                }
            }
            while (list2.Count > 0)
            {
                for (int index1 = 0; index1 < list2.Count; ++index1)
                {
                    if (!list2[index1].IsAlive)
                    {
                        list2.RemoveAt(index1);
                        --index1;
                    }
                }
                Thread.Sleep(50);
            }
            int num1 = 0;
            int num2 = 0;
            foreach (QuadDesc quadDesc in list1)
            {
                num1 += quadDesc.outValues.totalTriCount;
                num2 += quadDesc.outValues.reducedTriCount;
            }
            this.logFile.WriteLog("LOD level " + this.quadLevel.ToString() + " total triangles " + num1.ToString() + " reduced to " + num2.ToString());
        }

        public void GenerateTest(string strfile, LogFile logFile)
        {
            NiFile file = new NiFile();
            NiNode rootNode = new NiNode();
            file.AddBlock((NiObject)rootNode);
            this.quadLevel = 4;
            this.quadOffset = 16384f;
            QuadDesc quad = new QuadDesc(true);
            quad.x = 0;
            quad.y = 0;
            quad.statics = new List<StaticDesc>();
            StaticDesc curStat = new StaticDesc();
            curStat.refID = "0";
            curStat.rotX = 0.0f;
            curStat.rotY = 0.0f;
            curStat.rotZ = 0.0f;
            curStat.scale = 1f;
            curStat.x = 0.0f;
            curStat.y = 0.0f;
            curStat.z = 0.0f;
            curStat.staticName = "Test";
            curStat.staticFullModel = strfile;
            curStat.staticModels = new string[3];
            curStat.staticModels[0] = strfile;
            curStat.staticModels[1] = strfile;
            curStat.staticModels[2] = strfile;
            quad.statics.Add(curStat);
            List<ShapeDesc> shapes = this.ParseNif(quad, curStat, 0);
            this.MergeShapes(shapes);
            for (int index = 0; index < shapes.Count; ++index)
            {
                BSSegmentedTriShape segmentedTriShape = new BSSegmentedTriShape();
                segmentedTriShape.SetRotation(new Matrix33(true));
                segmentedTriShape.SetTranslation(new Vector3(0.0f, 0.0f, 0.0f));
                rootNode.AddChild(file.AddBlock((NiObject)segmentedTriShape));
                //segmentedTriShape.SetData(file.AddBlock((NiObject)shapes[index].geometry));
            }
            file.Write(this.outputDir + "\\" + this.worldspaceName + ".test.nif", logFile);
        }
    }
}
using LODGenerator.Common;
using LODGenerator.NifMain;
using System;
using System.IO;
using System.Collections.Generic;
using StringList = System.Collections.Generic.List<string>;
using System.Linq;
using System.Globalization;
using DelaunayTriangulator;

namespace LODGenerator
{
    public class ShapeDesc
    {
        public string name;
        public string staticName;
        public string staticModel;
        public Geometry geometry;
        public string shaderType;
        public BSEffectShaderProperty effectShader;
        public BSLightingShaderProperty lightingShader;
        public NiTexturingProperty texturingProperty;
        public NiMaterialProperty materialProperty;
        public NiSourceTexture sourceTextureBase;
        public NiSourceTexture sourceTextureDetail;
        public NiSourceTexture sourceTextureGlow;
        public NiSourceTexture sourceTextureBump;
        public string shaderHash;
        public string shapeHash;
        public string material;
        public string[] textures;
        public bool isPassThru;
        public bool isGroup;
        public bool isHighDetail;
        public bool hasVertexColor;
        public bool allWhite;
        public bool isDoubleSided;
        public bool isAlpha;
        public byte alphaThreshold;
        public float backlightPower;
        public uint enableParent;
        public uint TextureClampMode;
        public BBox boundingBox;
        public float x;
        public float y;
        public List<SegmentDesc> segments;
        public Vector3 translation;
        public Matrix33 rotation;
        public float scale;


        public ShapeDesc()
        {
            this.name = "";
            this.staticName = "";
            this.staticModel = "";
            this.geometry = null;
            this.shaderType = "";
            this.effectShader = null;
            this.lightingShader = null;
            this.texturingProperty = null;
            this.materialProperty = null;
            this.sourceTextureBase = null;
            this.sourceTextureDetail = null;
            this.sourceTextureGlow = null;
            this.sourceTextureBump = null;
            this.shaderHash = "";
            this.shapeHash = "";
            this.material = "";
            this.textures = new string[10] { "", "", "", "", "", "", "", "", "", "" };
            this.textures[0] = "textures\\default.dds";
            this.textures[1] = "textures\\default_n.dds";
            this.isPassThru = false;
            this.isGroup = false;
            this.isHighDetail = false;
            this.hasVertexColor = false;
            this.allWhite = false;
            this.isDoubleSided = false;
            this.isAlpha = false;
            this.enableParent = 0;
            this.TextureClampMode = 0;
            this.boundingBox = new BBox();
            this.x = new float();
            this.y = new float();
            this.segments = new List<SegmentDesc>();
            this.translation = new Vector3(0f, 0f, 0f);
            this.rotation = new Matrix33(true);
            this.scale = 1f;

        }

        public ShapeDesc(ShapeDesc shapeDesc)
        {
            this.name = shapeDesc.name;
            this.staticName = shapeDesc.name;
            this.staticModel = shapeDesc.name;
            this.geometry = null;
            this.shaderType = shapeDesc.shaderType;
            this.effectShader = shapeDesc.effectShader;
            this.lightingShader = shapeDesc.lightingShader;
            this.texturingProperty = shapeDesc.texturingProperty;
            this.materialProperty = shapeDesc.materialProperty;
            this.sourceTextureBase = shapeDesc.sourceTextureBase;
            this.sourceTextureDetail = shapeDesc.sourceTextureDetail;
            this.sourceTextureGlow = shapeDesc.sourceTextureGlow;
            this.sourceTextureBump = shapeDesc.sourceTextureBump;
            this.shaderHash = shapeDesc.shaderHash;
            this.shapeHash = shapeDesc.shapeHash;
            this.material = shapeDesc.material;
            this.textures = shapeDesc.textures;
            this.isPassThru = shapeDesc.isPassThru;
            this.isGroup = shapeDesc.isGroup;
            this.isHighDetail = shapeDesc.isHighDetail;
            this.hasVertexColor = shapeDesc.hasVertexColor;
            this.allWhite = shapeDesc.allWhite;
            this.isDoubleSided = shapeDesc.isDoubleSided;
            this.isAlpha = shapeDesc.isAlpha;
            this.enableParent = shapeDesc.enableParent;
            this.TextureClampMode = shapeDesc.TextureClampMode;
            this.boundingBox = new BBox();
            this.x = shapeDesc.x;
            this.y = shapeDesc.y;
            this.segments = new List<SegmentDesc>();
            this.translation = shapeDesc.translation;
            this.rotation = shapeDesc.rotation;
            this.scale = shapeDesc.scale;

        }

        public ShapeDesc(String gameDir, NiFile file, NiTriBasedGeom geom, StaticDesc stat, int quadIndex, StringList PassThruMeshList, bool skyblivionTexPath, bool useOptimizer, bool fixTangents, bool terrain, bool verbose, LogFile logFile)
        {
            this.name = "";
            if (geom.GetNameIndex() != -1)
            {
                this.name = file.GetStringAtIndex(geom.GetNameIndex());
            }
            else
            {
                this.name = geom.GetName();
            }
            if (this.name == null)
            {
                this.name = "";
            }
            this.staticName = stat.staticName;
            if (stat.staticModels != null && quadIndex < stat.staticModels.Count())
            {
                this.staticModel = stat.staticModels[quadIndex].ToLower(CultureInfo.InvariantCulture);
            }
            else
            {
                this.staticModel = "";
            }
            this.geometry = null;
            this.shaderType = "";
            this.effectShader = null;
            this.lightingShader = null;
            this.texturingProperty = null;
            this.materialProperty = null;
            this.sourceTextureBase = null;
            this.sourceTextureDetail = null;
            this.sourceTextureGlow = null;
            this.sourceTextureBump = null;
            this.shaderHash = "";
            this.shapeHash = "";
            if (stat.materialName != null)
            {
                this.material = stat.materialName;
            }
            else
            {
                this.material = "";
            }
            this.textures = new string[10] { "", "", "", "", "", "", "", "", "", "" };
            this.textures[0] = "textures\\default.dds";
            this.textures[1] = "textures\\default_n.dds";

            if (this.name == "LODGenPassThru" || this.material.ToLower(CultureInfo.InvariantCulture) == "passthru" || (PassThruMeshList != null && staticModel != "" && PassThruMeshList.Any(staticModel.Contains)))
            {
                this.isPassThru = true;
                this.material = "passthru";
            }
            else
            {
                this.isPassThru = false;
            }
            if ((stat.staticFlags & 1) == 1)
            {
                this.isGroup = true;
            }
            this.isHighDetail = false;
            this.hasVertexColor = false;
            this.allWhite = false;
            this.isDoubleSided = false;
            this.isAlpha = false;
            this.enableParent = 0;
            this.TextureClampMode = 0;
            this.boundingBox = new BBox();
            this.x = new float();
            this.y = new float();
            this.segments = new List<SegmentDesc>();
            this.translation = new Vector3(stat.x, stat.y, stat.z);

            this.scale = stat.scale;

            Matrix33 matrix33_1 = new Matrix33(true);
            Matrix33 matrix33_2 = new Matrix33(true);
            Matrix33 matrix33_3 = new Matrix33(true);
            matrix33_1.SetRotationX(Utils.ToRadians(-stat.rotX));
            matrix33_2.SetRotationY(Utils.ToRadians(-stat.rotY));
            matrix33_3.SetRotationZ(Utils.ToRadians(-stat.rotZ));
            this.rotation = new Matrix33(true) * matrix33_1 * matrix33_2 * matrix33_3;

            try
            {
                if (geom.GetClassName() == "NiTriStrips")
                {
                    int index = geom.GetData();
                    if (index == -1)
                    {
                        geometry = new Geometry();
                    }
                    else
                    {
                        geometry = new Geometry(new NiTriShapeData((NiTriStripsData)file.GetBlockAtIndex(index)));
                    }
                    List<int> extradatalist = geom.GetExtraData();
                    if (extradatalist.Count == 1)
                    {
                        NiBinaryExtraData extradata = (NiBinaryExtraData)file.GetBlockAtIndex(extradatalist[0]);
                        this.geometry.SetTangents(extradata.GetTangents());
                        this.geometry.SetBitangents(extradata.GetBitangents());
                    }
                }
                else if (geom.IsDerivedType("BSTriShape"))
                {
                    geometry = new Geometry((BSTriShape)geom);
                }
                else
                {
                    int index = geom.GetData();
                    if (index == -1)
                    {
                        geometry = new Geometry();
                    }
                    else
                    {
                        geometry = new Geometry((NiTriShapeData)file.GetBlockAtIndex(index));
                    }
                }

            }
            catch
            {
                logFile.WriteLog("Skipping non supported data " + this.staticModel + " " + this.name);
                geometry = new Geometry();
                return;
            }

            if (terrain)
            {
                return;
            }

            if (geometry.uvcoords.Count == 0)
            {
                logFile.WriteLog("Skipping no UV " + this.staticModel + " " + this.name);
                geometry = new Geometry();
                return;
            }

            if (geometry.HasVertexColors())
            {
                this.allWhite = true;
                List<Color4> vertexColors = geometry.GetVertexColors();
                for (int index = 0; index < vertexColors.Count; index++)
                {
                    float r = vertexColors[index][0];
                    float g = vertexColors[index][1];
                    float b = vertexColors[index][2];
                    float a = vertexColors[index][3];
                    if (r < 0.9f || b < 0.9f || g < 0.9f)
                    {
                        this.allWhite = false;
                    }
                    if (this.isPassThru && Game.Mode != "merge5")
                    {
                        // if neither LOD flag is set alpha is used on/off at 0.5f, nobody wants that
                        // HD snow shader uses alpha for something else
                        a = 1f;
                    }
                    vertexColors[index] = new Color4(r, g, b, a);
                }
                if (this.allWhite)
                {
                    geometry.SetVertexColors(new List<Color4>());
                }
                else if (this.isPassThru && Game.Mode != "merge5")
                {
                    geometry.SetVertexColors(vertexColors);
                }
            }
            this.hasVertexColor = geometry.HasVertexColors();
            this.shapeHash = Utils.GetHash(Utils.ObjectToByteArray(geometry.vertices));

            if ((file.GetVersion() > 335544325U) && (file.GetUserVersion() > 11U))
            {
                for (int index = 0; index < 2; index++)
                {
                    if (geom.GetBSProperty(index) != -1)
                    {
                        string type = file.GetBlockAtIndex(geom.GetBSProperty(index)).GetClassName().ToLower(CultureInfo.InvariantCulture);
                        if (type == "nialphaproperty")
                        {
                            if (Game.Mode == "fo4" || Game.Mode == "merge5")
                            {
                                this.isAlpha = true;
                                NiAlphaProperty alphaProperty = (NiAlphaProperty)file.GetBlockAtIndex(geom.GetBSProperty(index));
                                this.alphaThreshold = alphaProperty.GetThreshold();
                            }
                        }
                        else if (type == "bseffectshaderproperty")
                        {
                            this.shaderType = type;
                            BSEffectShaderProperty shader = (BSEffectShaderProperty)file.GetBlockAtIndex(geom.GetBSProperty(index));
                            this.textures[0] = shader.GetSourceTexture().ToLower(CultureInfo.InvariantCulture);
                            // disable non supported flags
                            if (Game.Mode != "merge5")
                            {
                                shader.SetShaderFlags1(shader.GetShaderFlags1() & 3724541045);
                                shader.SetShaderFlags2(shader.GetShaderFlags2() & 3758063615);
                            }
                            shader.SetSourceTexture("");
                            this.effectShader = shader;
                            if (this.hasVertexColor)
                            {
                                shader.SetShaderFlags2(shader.GetShaderFlags2() | 32);
                            }
                            else
                            {
                                shader.SetShaderFlags2(shader.GetShaderFlags2() & 4294967263);
                            }
                            this.isDoubleSided = (shader.GetShaderFlags2() & 16) == 16;
                            this.TextureClampMode = shader.GetTextureClampMode();
                            shader.SetTextureClampMode(3);
                            this.shaderHash = Utils.GetHash(Utils.ObjectToByteArray(shader));
                        }
                        else if (type == "bslightingshaderproperty")
                        {
                            this.shaderType = type;
                            BSLightingShaderProperty shader = (BSLightingShaderProperty)file.GetBlockAtIndex(geom.GetBSProperty(index));
                            // disable non supported flags
                            if (Game.Mode != "merge5")
                            {
                                //enviroment shader
                                if (shader.GetShaderType() == 1)
                                {
                                    if ((shader.GetShaderFlags2() & 64) == 64)
                                    {
                                        //glow shader
                                        shader.SetShaderType(2);
                                    }
                                    shader.SetShaderType(0);
                                }
                                //3695270121
                                shader.SetShaderFlags1(shader.GetShaderFlags1() & 3724541045);
                                shader.SetShaderFlags2(shader.GetShaderFlags2() & 3758063615);
                            }
                            this.TextureClampMode = shader.GetTextureClampMode();
                            shader.SetTextureClampMode(3);
                            this.isDoubleSided = (shader.GetShaderFlags2() & 16) == 16;
                            if (this.hasVertexColor)
                            {
                                shader.SetShaderFlags2(shader.GetShaderFlags2() | 32);
                            }
                            else
                            {
                                shader.SetShaderFlags2(shader.GetShaderFlags2() & 4294967263);
                            }
                            // no specular flag -> reset specular strength, color, glossiness
                            if ((shader.GetShaderFlags1() & 1) == 0 || shader.GetSpecularStrength() == 0f || (shader.GetGlossiness() == 80f && shader.GetSpecularColor() == new Color3(0f, 0f, 0f) && shader.GetSpecularStrength() == 1f))
                            {
                                shader.SetShaderFlags1(shader.GetShaderFlags1() & 4294967294);
                                shader.SetGlossiness(80f);
                                shader.SetSpecularColor(new Color3(0f, 0f, 0f));
                                shader.SetSpecularStrength(1f);
                            }
                            // no soft/rim/back/effect lighting -> reset effect lighting 1 and 2 
                            if ((shader.GetShaderFlags2() & 1308622848) == 0 || (shader.GetLightingEffect1() < 0.1f && shader.GetLightingEffect2() < 0.1f))
                            {
                                shader.SetShaderFlags2(shader.GetShaderFlags2() & 2986344447);
                                shader.SetLightingEffect1(0f);
                                shader.SetLightingEffect2(0f);
                            }
                            // no own emit -> reset own emit, emissive color, muliplier
                            if ((shader.GetShaderFlags1() & 4194304) == 0 || (shader.GetEmissiveColor()[0] < 0.1f && shader.GetEmissiveColor()[1] < 0.1f && shader.GetEmissiveColor()[2] < 0.1f) || shader.GetEmissiveMultiple() == 0f)
                            {
                                shader.SetShaderFlags1(shader.GetShaderFlags1() & 4290772991);
                                shader.SetEmissiveColor(new Color3(0f, 0f, 0f));
                                shader.SetEmissiveMultiple(1f);
                            }
                            this.backlightPower = shader.GetBacklightPower();
                            if (shader.GetTextureSet() != -1 && this.textures[0] == "textures\\default.dds")
                            {
                                BSShaderTextureSet shaderTextureSet = (BSShaderTextureSet)file.GetBlockAtIndex(shader.GetTextureSet());
                                for (int index2 = 0; index2 < shaderTextureSet.GetNumTextures(); ++index2)
                                {
                                    this.textures[index2] = shaderTextureSet.GetTexture(index2).ToLower(CultureInfo.InvariantCulture);
                                }
                            }
                            shader.SetTextureSet(-1);
                            //BGSM takes priority and overwrite everything in nif
                            if (shader.GetNameIndex() != -1)
                            {
                                string bgsmFileName = file.GetStringAtIndex(shader.GetNameIndex()).ToLower(CultureInfo.InvariantCulture);
                                if (bgsmFileName.Contains(".bgsm"))
                                {
                                    int i = bgsmFileName.IndexOf("\\data\\");
                                    if (i > 0)
                                    {
                                        i += 6;
                                        bgsmFileName = bgsmFileName.Substring(i, bgsmFileName.Length - i);
                                    }
                                    if (stat.materialSwap.ContainsKey(bgsmFileName))
                                    {
                                        bgsmFileName = stat.materialSwap[bgsmFileName];
                                    }
                                    BGSMFile bgsmdata = new BGSMFile();
                                    bgsmdata.Read(gameDir, bgsmFileName, logFile);
                                    this.textures[0] = "textures\\" + bgsmdata.textures[0];
                                    this.textures[1] = "textures\\" + bgsmdata.textures[1];
                                    this.textures[7] = "textures\\" + bgsmdata.textures[2];
                                    this.TextureClampMode = bgsmdata.textureClampMode;
                                    shader.SetTextureClampMode(this.TextureClampMode);
                                    this.isAlpha = Convert.ToBoolean(bgsmdata.alphaFlag);
                                    this.isDoubleSided = Convert.ToBoolean(bgsmdata.doubleSided);
                                    shader.SetShaderFlags2(shader.GetShaderFlags2() | 16);
                                    this.alphaThreshold = bgsmdata.alphaThreshold;
                                    this.backlightPower = bgsmdata.backlightPower;
                                    shader.SetBacklightPower(this.backlightPower);
                                }
                            }
                            this.shaderHash = Utils.GetHash(Utils.ObjectToByteArray(shader));
                            this.lightingShader = shader;
                        }
                        else
                        {
                            if (this.shaderType == "")
                            {
                                this.shaderType = type;
                            }
                        }
                    }
                }
            }
            else
            {
                this.shaderType = "none";
                for (int index = 0; index < geom.GetNumProperties(); ++index)
                {
                    NiProperty niProperty = (NiProperty)file.GetBlockAtIndex(geom.GetProperty(index));
                    string type = niProperty.GetClassName().ToLower(CultureInfo.InvariantCulture);
                    if (niProperty.GetType() == typeof(BSShaderPPLightingProperty))
                    {
                        BSShaderPPLightingProperty shader = (BSShaderPPLightingProperty)file.GetBlockAtIndex(geom.GetProperty(index));
                        BSShaderTextureSet shaderTextureSet = (BSShaderTextureSet)file.GetBlockAtIndex(shader.GetTextureSet());
                        for (int index2 = 0; index2 < shaderTextureSet.GetNumTextures(); ++index2)
                        {
                            this.textures[index2] = shaderTextureSet.GetTexture(index2).ToLower(CultureInfo.InvariantCulture);
                        }
                        //this.hasVertexColor = (shader.GetShaderFlags2() & 32) == 32;
                        //this.isDoubleSided = (shader.GetShaderFlags2() & 16) == 16;
                        //this.TextureClampMode = shader.GetTextureClampMode();
                        this.shaderType = type;
                        break;
                    }
                    if (niProperty.GetType() == typeof(NiMaterialProperty))
                    {
                        this.materialProperty = (NiMaterialProperty)niProperty;
                        this.hasVertexColor = this.geometry.HasVertexColors();
                    }
                    if (niProperty.GetType() == typeof(NiTexturingProperty))
                    {
                        this.texturingProperty = (NiTexturingProperty)niProperty;
                        string str1 = "textures\\defaultdiffuse.dds";
                        string str2 = "textures\\default_n.dds";
                        if (this.texturingProperty != null)
                        {
                            TexDesc baseTexture = this.texturingProperty.GetBaseTexture();
                            if (this.texturingProperty.HasBaseTexture() && this.texturingProperty.GetBaseTexture().source != -1)
                            {
                                this.sourceTextureBase = (NiSourceTexture)file.GetBlockAtIndex(this.texturingProperty.GetBaseTexture().source);
                                str1 = this.sourceTextureBase.GetFileName().ToLower(CultureInfo.InvariantCulture);
                            }
                            if (this.texturingProperty.HasDarkTexture() && this.texturingProperty.GetDarkTexture().source != -1)
                            {
                                if (verbose)
                                {
                                    logFile.WriteLog("dark texture " + this.staticName);
                                }
                            }
                            if (this.texturingProperty.HasDetailTexture() && this.texturingProperty.GetDetailTexture().source != -1)
                            {
                                if (verbose)
                                {
                                    logFile.WriteLog("detail texture " + this.staticName);
                                }
                                this.sourceTextureDetail = (NiSourceTexture)file.GetBlockAtIndex(this.texturingProperty.GetDetailTexture().source);
                                str1 = this.sourceTextureDetail.GetFileName().ToLower(CultureInfo.InvariantCulture);
                            }
                            if (this.texturingProperty.HasGlossTexture() && this.texturingProperty.GetGlossTexture().source != -1)
                            {
                                if (verbose)
                                {
                                    logFile.WriteLog("gloss texture " + this.staticName);
                                }
                            }
                            if (this.texturingProperty.HasGlowTexture() && this.texturingProperty.GetGlowTexture().source != -1)
                            {
                                if (verbose)
                                {
                                    logFile.WriteLog("glow texture " + this.staticName);
                                }
                                this.sourceTextureGlow = (NiSourceTexture)file.GetBlockAtIndex(this.texturingProperty.GetGlowTexture().source);
                            }
                            if (this.texturingProperty.HasBumpMapTexture() && this.texturingProperty.GetBumpMapTexture().source != -1)
                            {
                                if (verbose)
                                {
                                    logFile.WriteLog("bump texture " + this.staticName);
                                }
                                this.sourceTextureBump = (NiSourceTexture)file.GetBlockAtIndex(this.texturingProperty.GetBumpMapTexture().source);
                            }
                            if (this.texturingProperty.HasDecalTexture0() && this.texturingProperty.GetDecalTexture0().source != -1)
                            {
                                if (verbose)
                                {
                                    logFile.WriteLog("decal texture " + this.staticName);
                                }
                            }
                            str2 = Utils.GetNormalTextureName(str1);
                            if (skyblivionTexPath && !str1.Contains("textures\\tes4"))
                            {
                                str1 = str1.ToLower().Replace("textures", "textures\\tes4");
                                str2 = str2.ToLower().Replace("textures", "textures\\tes4");
                            }
                        }
                        this.textures[0] = str1.ToLower(CultureInfo.InvariantCulture);
                        this.textures[1] = str2.ToLower(CultureInfo.InvariantCulture);
                        if (this.texturingProperty.HasBaseTexture())
                        {
                            this.TextureClampMode = this.texturingProperty.GetBaseTexture().clampMode;
                        }
                        this.shaderType = type;
                        break;
                    }
                }
            }

            for (int index = 0; index < this.textures.Length; index++)
            {
                if (this.textures[index].Contains(".dds") && !this.textures[index].Contains("textures\\"))
                {
                    this.textures[index] = Path.Combine("textures\\", this.textures[index]);
                }
            }

            if ((Game.Mode == "convert4" || Game.Mode == "convert5") && useOptimizer && AtlasList.Contains(this.textures[0]) || this.textures[0] == "textures\\grid.dds")
            {
                string before = this.textures[0];
                float scaleU = 1f;
                float scaleV = 1f;
                float posU = 0f;
                float posV = 0f;
                string basetexture = this.textures[0];
                if (AtlasList.Contains(this.textures[0]))
                {
                    AtlasList.BeforeAdd(this.textures[0], geometry.triangles.Count);
                    scaleU = AtlasList.Get(basetexture).scaleU;
                    scaleV = AtlasList.Get(basetexture).scaleV;
                    posU = AtlasList.Get(basetexture).posU;
                    posV = AtlasList.Get(basetexture).posV;
                    this.textures[0] = AtlasList.Get(basetexture).AtlasTexture;
                    this.textures[1] = AtlasList.Get(basetexture).AtlasTextureN;
                    if (this.textures[2] != "")
                    {
                        this.textures[2] = AtlasList.Get(basetexture).AtlasTexture;
                    }
                    if (Game.Mode == "fo4")
                    {
                        this.textures[7] = AtlasList.Get(basetexture).AtlasTextureS;
                    }
                    this.TextureClampMode = 0U;
                    this.isHighDetail = false;
                }

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
                                logFile.WriteLog(this.staticModel + " " + this.name + " " + this.textures[0] + " has huge UV " + (maxU - minU) + " x " + (maxV - minV) + " = " + u + ", " + v + " (" + area + ")");
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
                                    CreateTriangles(pts, idx, true, front, scaleU, scaleV);
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

                geometry.RemoveDuplicate(false);
                geometry.FaceNormals();
                geometry.SmoothNormals(60f, 0.001f);
                geometry.UpdateTangents(fixTangents);

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
                        logFile.WriteLog("UV outside " + this.staticName + " " + this.name + " " + this.textures[0] + " (" + (maxU - minU) + ", " + (maxV - minV) + ")");
                    }
                }
            }
            else if ((Game.Mode == "convert4" || Game.Mode == "convert5") && useOptimizer && verbose)
            {
                logFile.WriteLog(this.staticModel + " " + this.name + " " + this.textures[0] + " not in atlas file");
            }
            if (!geometry.HasNormals())
            {
                geometry.FaceNormals();
                geometry.SmoothNormals(60f, 0.001f);
            }
            if (fixTangents && !geometry.HasTangents())
            {
                geometry.UpdateTangents(fixTangents);
            }
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
                        t.Add((ushort)geometry.CreateDuplicate(idx[triangles[index].c]));
                        t.Add((ushort)geometry.CreateDuplicate(idx[triangles[index].b]));
                        t.Add((ushort)geometry.CreateDuplicate(idx[triangles[index].a]));
                    }
                    else
                    {
                        t.Add((ushort)idx[triangles[index].c]);
                        t.Add((ushort)idx[triangles[index].b]);
                        t.Add((ushort)idx[triangles[index].a]);
                    }
                    if (front == UVCoord.Clockwise(geometry.uvcoords[t[0]], geometry.uvcoords[t[1]], geometry.uvcoords[t[2]]))
                    {
                        geometry.triangles.Add(new Triangle(t[0], t[1], t[2]));
                    }
                    else
                    {
                        geometry.triangles.Add(new Triangle(t[0], t[2], t[1]));
                    }
                    if (createnew)
                    {
                        geometry.uvcoords[t[0]] = new UVCoord(geometry.uvcoords[t[0]][0] / scaleU, geometry.uvcoords[t[0]][1] / scaleV);
                        geometry.uvcoords[t[1]] = new UVCoord(geometry.uvcoords[t[1]][0] / scaleU, geometry.uvcoords[t[1]][1] / scaleV);
                        geometry.uvcoords[t[2]] = new UVCoord(geometry.uvcoords[t[2]][0] / scaleU, geometry.uvcoords[t[2]][1] / scaleV);
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
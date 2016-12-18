using LODGenerator.Common;
using LODGenerator.NifMain;
using System;
using System.IO;
using System.Collections.Generic;
using System.Collections.Concurrent;
using StringList = System.Collections.Generic.List<string>;
using System.Linq;
using System.Globalization;
using DelaunayTriangulator;
using System.Runtime.Serialization.Formatters.Binary;

namespace LODGenerator
{
    static class ShapeList
    {
        private static ConcurrentDictionary<string, ShapeDesc> _list;

        static ShapeList()
        {
            _list = new ConcurrentDictionary<string, ShapeDesc>();
        }

        public static void Set(string key, ShapeDesc value)
        {
            if (!Contains(key))
            {
                _list.TryAdd(key, value);
            }
        }

        public static ShapeDesc Get(string key)
        {
            return _list[key];
        }

        public static bool Contains(string key)
        {
            return _list.ContainsKey(key);
        }
    }

    [Serializable]
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
        public bool isDecal;
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
            this.isDecal = false;
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
            this.isDecal = shapeDesc.isDecal;
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

        public ShapeDesc(String gameDir, NiFile file, NiTriBasedGeom geom, StaticDesc stat, int quadIndex, StringList PassThruMeshList, bool skyblivionTexPath, bool useOptimizer, bool fixTangents, bool useDecalFlag, bool terrain, bool verbose, LogFile logFile)
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
            this.isDecal = false;
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
                        // if neither LOD flag is set, alpha is used on/off at 0.5f, nobody wants that
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
            try
            {
                this.shapeHash = Utils.GetHash(Utils.ObjectToByteArray(geometry.vertices));
            }
            catch
            {
                logFile.WriteLog("Can not get hash for vertices in " + this.staticModel + " block " + this.name);
                logFile.Close();
                System.Environment.Exit(3003);
            }
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
                            try
                            {
                                this.shaderHash = Utils.GetHash(Utils.ObjectToByteArray(shader));
                            }
                            catch
                            {
                                logFile.WriteLog("Can not get hash for shader in " + this.staticModel + " block " + this.name);
                                logFile.Close();
                                System.Environment.Exit(3004);
                            }
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
                            if (Game.Mode == "tes5" && useDecalFlag && !this.isPassThru && ((shader.GetShaderFlags1() & 67108864) == 67108864 || (shader.GetShaderFlags1() & 134217728) == 134217728))
                            {
                                this.isDecal = true;
                                // SLSF1_Decal
                                shader.SetShaderFlags1(shader.GetShaderFlags1() | 67108864);
                                // SLSF1_Dynamic_Decal
                                shader.SetShaderFlags1(shader.GetShaderFlags1() | 134217728);
                                // SLSF2_No_Fade
                                shader.SetShaderFlags2(shader.GetShaderFlags2() | 8);
                            }
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
                            try
                            {
                                this.shaderHash = Utils.GetHash(Utils.ObjectToByteArray(shader));
                            }
                            catch
                            {
                                logFile.WriteLog("Can not get hash for shader in " + this.staticModel + " block " + this.name);
                                logFile.Close();
                                System.Environment.Exit(3005);
                            }
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
                this.textures[index] = this.textures[index].Trim();
                if (this.textures[index].Contains(".dds") && !this.textures[index].Contains("textures\\"))
                {
                    this.textures[index] = Path.Combine("textures\\", this.textures[index]);
                }
            }

            if (((Game.Mode == "convert4" || Game.Mode == "convert5") && useOptimizer && AtlasList.Contains(this.textures[0])) || this.textures[0] == "textures\\grid.dds")
            {
                string texture = this.textures[0];
                geometry = geometry.ReUV(this, texture, logFile, verbose);
                this.textures = new string[10] { "", "", "", "", "", "", "", "", "", "" };
                this.textures[0] = AtlasList.Get(texture).AtlasTexture;
                this.textures[1] = AtlasList.Get(texture).AtlasTextureN;
                if (this.textures[2] != "")
                {
                    this.textures[2] = AtlasList.Get(texture).AtlasTexture;
                }
                if (Game.Mode == "fo4")
                {
                    this.textures[7] = AtlasList.Get(texture).AtlasTextureS;
                }
                this.TextureClampMode = 0U;
                this.isHighDetail = false;
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

        public ShapeDesc ReUV(LogFile logFile, bool verbose)
        {
            string texture = this.textures[0];
            if (AtlasList.Contains(texture) && AtlasList.Get(texture).miniatlas)
            {
                this.geometry = this.geometry.ReUV(this, texture, logFile, verbose).Copy();
                this.textures = new string[10] { "", "", "", "", "", "", "", "", "", "" };
                this.textures[0] = AtlasList.Get(texture).AtlasTexture;
                this.textures[1] = AtlasList.Get(texture).AtlasTextureN;
                if (this.textures[2] != "")
                {
                    this.textures[2] = AtlasList.Get(texture).AtlasTexture;
                }
                if (Game.Mode == "fo4")
                {
                    this.textures[7] = AtlasList.Get(texture).AtlasTextureS;
                }
                this.TextureClampMode = 0U;
                this.isHighDetail = false;
            }
            return this;
        }
    }
}
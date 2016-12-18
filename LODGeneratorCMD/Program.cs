using LODGenerator;
using LODGenerator.Common;
using System;
using System.Collections.Generic;
using StringList = System.Collections.Generic.List<string>;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading;
using Ini;

namespace LODGeneratorCMD
{

    internal class Program
    {
        private static Dictionary<string, string> CollectCmdArgs(string[] args)
        {
            Dictionary<string, string> dictionary = new Dictionary<string, string>();
            for (int index = 1; index < Enumerable.Count<string>((IEnumerable<string>)args); ++index)
            {
                string str = "";
                string key = args[index].Remove(0, 2).ToLower(CultureInfo.InvariantCulture);
                if (index + 1 < Enumerable.Count<string>((IEnumerable<string>)args) && !args[index + 1].StartsWith("--"))
                    str = args[++index];
                dictionary.Add(key, str);
            }
            return dictionary;
        }

        private static int Main(string[] args)
        {
            LogFile theLog = new LogFile();
            if (Enumerable.Count<string>((IEnumerable<string>)args) < 1)
            {
                theLog.WriteLog("Nothing to do");
                theLog.Close();
                return -1;
            }
            string path = args[0];
            if (!File.Exists(path))
            {
                theLog.WriteLog("No input file " + path);
                theLog.Close();
                return -1;
            }
            CultureInfo cultureInfo = CmdArgs.ci;
            StreamReader streamReader = new StreamReader(path, System.Text.Encoding.Default, true);
            List<StaticDesc> statics = new List<StaticDesc>();
            StringList BSAFiles = new StringList();
            StringList ignoreList = new StringList();
            StringList HDTexture = new StringList();
            StringList notHDTexture = new StringList();
            StringList HDMesh = new StringList();
            StringList notHDMesh = new StringList();
            Game.Mode = "tes5";
            // 1 = 1 cell seems best
            Game.sampleSize = 1f;
            int counter = 0;
            string worldspaceName = "";
            string gameDir = "";
            string outputDir = "";
            string uvfile = "";
            string altfile = "";
            float southWestX = 0;
            float southWestY = 0;
            float atlasTolerance = 0.2f;
            bool generateVertexColors = true;
            bool dontFixTangents = false;
            bool dontGenerateTangents = false;
            bool mergeShapes = true;
            bool removeUnseenFaces = false;
            bool ignoreWater = false;
            bool useHDFlag = true;
            bool useOptimizer = false;
            bool ignoreMaterial = false;
            float globalScale = 1f;
            while (!streamReader.EndOfStream)
            {
                string[] strArray2 = streamReader.ReadLine().Split('=');
                if (strArray2.Length == 2)
                {
                    ++counter;
                    if (strArray2[0].ToLower(CultureInfo.InvariantCulture) == "worldspace")
                    {
                        worldspaceName = strArray2[1];
                    }
                    if (strArray2[0].ToLower(CultureInfo.InvariantCulture) == "cellsw")
                    {
                        string[] strArray1 = strArray2[1].Split(' ');
                        southWestX = float.Parse(strArray1[0], CultureInfo.InvariantCulture);
                        southWestY = float.Parse(strArray1[1], CultureInfo.InvariantCulture);
                    }
                    if (strArray2[0].ToLower(CultureInfo.InvariantCulture) == "pathdata")
                    {
                        gameDir = strArray2[1].ToLower(CultureInfo.InvariantCulture);
                        if (!Directory.Exists(gameDir))
                        {
                            theLog.WriteLog("No Data directory " + gameDir);
                            theLog.Close();
                            return -1;
                        }
                    }
                    if (strArray2[0].ToLower(CultureInfo.InvariantCulture) == "pathoutput")
                    {
                        outputDir = strArray2[1];
                        if (!Directory.Exists(outputDir))
                        {
                            Directory.CreateDirectory(outputDir);
                        }
                    }
                    if (strArray2[0].ToLower(CultureInfo.InvariantCulture) == "textureatlasmap")
                    {
                        uvfile = strArray2[1];
                    }
                    if (strArray2[0].ToLower(CultureInfo.InvariantCulture) == "alttextures")
                    {
                        altfile = strArray2[1];
                    }
                    if (strArray2[0].ToLower(CultureInfo.InvariantCulture) == "resource")
                    {
                        if (File.Exists(strArray2[1]))
                        {
                            BSAFiles.Add(strArray2[1].ToLower(CultureInfo.InvariantCulture));
                        }
                    }
                    if (strArray2[0].ToLower(CultureInfo.InvariantCulture) == "ishdmeshmask")
                    {
                        HDMesh.Add(strArray2[1].ToLower(CultureInfo.InvariantCulture));
                    }
                    if (strArray2[0].ToLower(CultureInfo.InvariantCulture) == "nothdmeshmask")
                    {
                        notHDMesh.Add(strArray2[1].ToLower(CultureInfo.InvariantCulture));
                    }
                    if (strArray2[0].ToLower(CultureInfo.InvariantCulture) == "ishdtexturemask")
                    {
                        HDTexture.Add(strArray2[1].ToLower(CultureInfo.InvariantCulture));
                    }
                    if (strArray2[0].ToLower(CultureInfo.InvariantCulture) == "nothdtexturemask")
                    {
                        notHDTexture.Add(strArray2[1].ToLower(CultureInfo.InvariantCulture));
                    }
                    if (strArray2[0].ToLower(CultureInfo.InvariantCulture) == "atlastolerance")
                    {
                        atlasTolerance = float.Parse(strArray2[1], CultureInfo.InvariantCulture);
                    }
                    if (strArray2[0].ToLower(CultureInfo.InvariantCulture) == "ignoretranslation")
                    {
                        ignoreList.Add(strArray2[1].ToLower(CultureInfo.InvariantCulture));
                    }
                    if (strArray2[0].ToLower(CultureInfo.InvariantCulture) == "gamemode")
                    {
                        Game.Mode = ((strArray2[1].ToLower(CultureInfo.InvariantCulture)));
                        if (Game.Mode == "fo3")
                        {
                            Game.Mode = "fnv";
                        }
                        //generateVertexColors = false;
                    }
                    if (strArray2[0].ToLower(CultureInfo.InvariantCulture) == "dontgeneratevertexcolors")
                    {
                        generateVertexColors = !Boolean.Parse(strArray2[1]);
                    }
                    if (strArray2[0].ToLower(CultureInfo.InvariantCulture) == "dontfixtangents")
                    {
                        dontFixTangents = Boolean.Parse(strArray2[1]);
                    }
                    if (strArray2[0].ToLower(CultureInfo.InvariantCulture) == "dontgeneratetangents")
                    {
                        dontGenerateTangents = Boolean.Parse(strArray2[1]);
                    }
                    if (strArray2[0].ToLower(CultureInfo.InvariantCulture) == "dontmergeshapes")
                    {
                        mergeShapes = !Boolean.Parse(strArray2[1]);
                    }
                    if (strArray2[0].ToLower(CultureInfo.InvariantCulture) == "removeunseenfaces")
                    {
                        removeUnseenFaces = Boolean.Parse(strArray2[1]);
                    }
                    if (strArray2[0].ToLower(CultureInfo.InvariantCulture) == "ignorewater")
                    {
                        ignoreWater = Boolean.Parse(strArray2[1]);
                    }
                    if (strArray2[0].ToLower(CultureInfo.InvariantCulture) == "usehdflag")
                    {
                        useHDFlag = Boolean.Parse(strArray2[1]);
                    }
                    if (strArray2[0].ToLower(CultureInfo.InvariantCulture) == "useoptimizer")
                    {
                        useOptimizer = Boolean.Parse(strArray2[1]);
                    }
                    if (strArray2[0].ToLower(CultureInfo.InvariantCulture) == "ignorematerial")
                    {
                        ignoreMaterial = Boolean.Parse(strArray2[1]);
                    }
                    if (strArray2[0].ToLower(CultureInfo.InvariantCulture) == "globalscale")
                    {
                        globalScale = float.Parse(strArray2[1], CultureInfo.InvariantCulture);
                    }
                }
                else
                {
                    break;
                }
            }

            Dictionary<string, string> cmdArgs = Program.CollectCmdArgs(args);
            if (CmdArgs.GetBool(cmdArgs, "dontGenerateVertexColors", false))
            {
                generateVertexColors = false;
            }
            if (CmdArgs.GetBool(cmdArgs, "dontFixTangents", false))
            {
                dontFixTangents = true;
            }
            if (CmdArgs.GetBool(cmdArgs, "dontGenerateTangents", false))
            {
                dontGenerateTangents = true;
            }
            if (CmdArgs.GetBool(cmdArgs, "dontMergeShapes", false))
            {
                mergeShapes = false;
            }
            if (CmdArgs.GetBool(cmdArgs, "removeUnseenFaces", false))
            {
                removeUnseenFaces = true;
            }
            if (CmdArgs.GetBool(cmdArgs, "ignoreWater", false))
            {
                ignoreWater = true;
            }
            if (CmdArgs.GetBool(cmdArgs, "ignoreMaterial", false))
            {
                ignoreMaterial = true;
            }
            if (CmdArgs.GetBool(cmdArgs, "usehdlod", false))
            {
                useHDFlag = true;
            }
            if (CmdArgs.GetBool(cmdArgs, "globalScale", false))
            {
                globalScale = CmdArgs.GetFloat(cmdArgs, "globalScale", 1f);
            }

            int int1 = CmdArgs.GetInt(cmdArgs, "lodLevel", -1);
            int int2 = CmdArgs.GetInt(cmdArgs, "x", -1);
            int int3 = CmdArgs.GetInt(cmdArgs, "y", -1);
            theLog.WriteLog("Game Mode: " + Game.Mode.ToUpper());
            theLog.WriteLog("Fix Tangents: " + (!dontFixTangents ? "True" : "False"));
            theLog.WriteLog("Generate Tangents: " + (!dontGenerateTangents ? "True" : "False"));
            theLog.WriteLog("Generate Vertex Colors: " + (generateVertexColors ? "True" : "False"));
            theLog.WriteLog("Merge Meshes: " + (mergeShapes ? "True" : "False"));
            theLog.WriteLog("Remove Faces under Terrain: " + (removeUnseenFaces ? "True" : "False"));
            theLog.WriteLog("Remove Faces under Water: " + (!ignoreWater ? "True" : "False"));
            theLog.WriteLog("Use HD Flag: " + (useHDFlag ? "True" : "False"));
            //theLog.WriteLog("Use Optimizer: " + (useOptimizer ? "True" : "False"));
            theLog.WriteLog("Ignore Materials: " + (ignoreMaterial ? "True" : "False"));
            theLog.WriteLog("Global scale: " + string.Format("{0:0.00}", globalScale));
            theLog.WriteLog("Specific level: " + (int1 != -1 ? int1.ToString() : "No"));
            if (int2 != -1 && int3 == -1)
                theLog.WriteLog("Specific quad: [" + (object)int2.ToString() + ", X]");
            else if (int2 == -1 && int3 != -1)
                theLog.WriteLog("Specific quad: [X, " + (object)int3.ToString() + "]");
            else if (int2 != -1 && int3 != -1)
                theLog.WriteLog("Specific quad: [" + (object)int2.ToString() + ", " + (string)(object)int3.ToString() + "]");
            else
                theLog.WriteLog("Specific quad: No");

            streamReader.Close();
            if (worldspaceName == "")
            {
                theLog.WriteLog("No Worldspace");
                theLog.Close();
                return -1;
            }
            if (outputDir == "")
            {
                theLog.WriteLog("No PathOutput");
                theLog.Close();
                return -1;
            }
            /*IniFile ini = new IniFile(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "My games\\skyrim\\skyrim.ini"));
            if (ini.IniReadValue("Archive", "sResourceArchiveList").ToLower(CultureInfo.InvariantCulture).Contains("aaa"))
            {
                string archiveList1 = ini.IniReadValue("Archive", "sResourceArchiveList").ToLower(CultureInfo.InvariantCulture);
                string archiveList2 = ini.IniReadValue("Archive", "sResourceArchiveList2").ToLower(CultureInfo.InvariantCulture);
                if (archiveList2.Length > 0)
                {
                    archiveList1 += "," + archiveList2;
                }
                BSAFiles.Clear();
                BSAFiles.AddRange(archiveList1.Split(','));
                for (int index = 0; index < BSAFiles.Count; ++index)
                {
                    if (File.Exists(Path.Combine(gameDir, BSAFiles[index].Trim())))
                    {
                        BSAFiles[index] = Path.Combine(gameDir, BSAFiles[index].Trim());
                    }
                }
            }*/
            BSAArchive.Load(BSAFiles, theLog, (CmdArgs.GetBool(cmdArgs, "verbose", false)));
            streamReader = new StreamReader(path);
            for (int index = 0; index < counter; ++index)
            {
                streamReader.ReadLine();
            }
            while (!streamReader.EndOfStream)
            {
                StaticDesc staticDesc = new StaticDesc();
                string[] strArray2 = streamReader.ReadLine().Split('\t');
                counter++;
                if (strArray2.Count() != 16)
                {
                    theLog.WriteLog("Input file " + path + " has wrong data on line " + counter);
                    theLog.Close();
                    return -1;
                }
                staticDesc.refID = strArray2[0];
                //theLog.WriteLog(staticDesc.refID);
                staticDesc.refFlags = int.Parse(strArray2[1], NumberStyles.HexNumber, CultureInfo.InvariantCulture);
                staticDesc.x = float.Parse(strArray2[2], CultureInfo.InvariantCulture);
                staticDesc.y = float.Parse(strArray2[3], CultureInfo.InvariantCulture);
                staticDesc.z = float.Parse(strArray2[4], CultureInfo.InvariantCulture);
                staticDesc.rotX = float.Parse(strArray2[5], CultureInfo.InvariantCulture);
                staticDesc.rotY = float.Parse(strArray2[6], CultureInfo.InvariantCulture);
                staticDesc.rotZ = float.Parse(strArray2[7], CultureInfo.InvariantCulture);
                staticDesc.scale = float.Parse(strArray2[8], CultureInfo.InvariantCulture);
                staticDesc.staticName = strArray2[9];
                staticDesc.staticFlags = int.Parse(strArray2[10], NumberStyles.HexNumber, CultureInfo.InvariantCulture);
                staticDesc.materialName = strArray2[11];
                staticDesc.staticFullModel = strArray2[12];
                if (strArray2.Length >= 16)
                {
                    staticDesc.staticModels = new string[3];
                    for (int index = 0; index < 3; ++index)
                    {
                        string str = strArray2[13 + index];
                        staticDesc.staticModels[index] = str.ToLower(CultureInfo.InvariantCulture);
                        if (str.Length > 0 && !File.Exists(gameDir + str))
                        {
                            if (!BSAArchive.FileExists(str))
                            {
                                theLog.WriteLog("file not found " + gameDir + str);
                                theLog.Close();
                                System.Environment.Exit(404);
                            }
                        }
                    }
                    statics.Add(staticDesc);
                }
            }
            streamReader.Close();
            if (File.Exists(uvfile))
            {
                theLog.WriteLog("Using UV Atlas: " + uvfile);
                streamReader = new StreamReader(uvfile);
                while (!streamReader.EndOfStream)
                {
                    string[] strArray2 = streamReader.ReadLine().Split('\t');
                    AtlasDesc atlasDesc = new AtlasDesc();
                    if (strArray2.Length >= 8)
                    {
                        atlasDesc.SourceTexture = strArray2[0].ToLower(CultureInfo.InvariantCulture);
                        int textureWidth = int.Parse(strArray2[1], CultureInfo.InvariantCulture);
                        int textureHeight = int.Parse(strArray2[2], CultureInfo.InvariantCulture);
                        int textureX = int.Parse(strArray2[3], CultureInfo.InvariantCulture);
                        int textureY = int.Parse(strArray2[4], CultureInfo.InvariantCulture);
                        int atlasWidth = int.Parse(strArray2[6], CultureInfo.InvariantCulture);
                        int atlasHeight = int.Parse(strArray2[7], CultureInfo.InvariantCulture);
                        atlasDesc.scaleU = (float)textureWidth / (float)atlasWidth;
                        atlasDesc.scaleV = (float)textureHeight / (float)atlasHeight;
                        atlasDesc.posU = (float)textureX / (float)atlasWidth;
                        atlasDesc.posV = (float)textureY / (float)atlasHeight;
                        atlasDesc.AtlasTexture = strArray2[5].ToLower(CultureInfo.InvariantCulture);
                        atlasDesc.AtlasTextureN = strArray2[5].ToLower(CultureInfo.InvariantCulture).Replace(".dds", "_n.dds");
                        //theLog.WriteLog(atlasDesc.SourceTexture + "\t" + atlasDesc.AtlasTexture + "\t" + atlasDesc.scaleU + "\t" + atlasDesc.scaleV + "\t" + atlasDesc.posU + "\t" + atlasDesc.posV);
                        AtlasList.Set(atlasDesc.SourceTexture, atlasDesc);
                    }
                }
                streamReader.Close();
            }
            if (File.Exists(altfile))
            {
                theLog.WriteLog("Using Alt Textures: " + altfile);
                streamReader = new StreamReader(altfile);
                while (!streamReader.EndOfStream)
                {
                    string[] strArray2 = streamReader.ReadLine().ToLower(CultureInfo.InvariantCulture).Split('\t');
                    AltTextureDesc altTexDesc = new AltTextureDesc();
                    if (strArray2.Length > 0)
                    {
                        for (int index = 1; index < strArray2.Length; index++)
                        {
                            string[] strArray3 = strArray2[index].Split('=');
                            string key = strArray2[0] + "_" + strArray3[0] + "_" + strArray3[1];
                            altTexDesc.textures = strArray3[2].Split(',');
                            if (!AltTextureList.Contains(key))
                            {
                                //theLog.WriteLog(key + " = " + altTexDesc.textures[0] + ", " + altTexDesc.textures[1]);
                                AltTextureList.Set(key, altTexDesc);
                            }
                        }
                    }
                }
                streamReader.Close();
            }
            theLog.WriteLog("Output: " + outputDir);
            theLog.WriteLog("Generating LOD for worldspace " + worldspaceName);
            List<Thread> list1 = new List<Thread>();
            int num = 1;
            int index1 = 0;
            while (num <= 4)
            {
                list1.Add(new Thread((ParameterizedThreadStart)(state =>
                {
                    List<int> list2 = (List<int>)state;
                    new LODApp(worldspaceName, outputDir, gameDir, theLog)
                    {
                        verbose = (CmdArgs.GetBool(cmdArgs, "verbose", false)),
                        fixTangents = dontFixTangents,
                        generateTangents = !dontGenerateTangents,
                        generateVertexColors = generateVertexColors,
                        mergeShapes = mergeShapes,
                        useHDFlag = useHDFlag,
                        useOptimizer = useOptimizer,
                        useFadeNode = CmdArgs.GetBool(cmdArgs, "useFadeNode", false),
                        removeUnseenFaces = removeUnseenFaces,
                        globalScale = globalScale,
                        lodLevelToGenerate = CmdArgs.GetInt(cmdArgs, "lodLevel", -1),
                        lodX = CmdArgs.GetInt(cmdArgs, "x", -1),
                        lodY = CmdArgs.GetInt(cmdArgs, "y", -1),
                        southWestX = (int)southWestX,
                        southWestY = (int)southWestY,
                        atlasToleranceMin = atlasTolerance * -1f,
                        atlasToleranceMax = atlasTolerance + 1f,
                        removeUnderwaterFaces = !ignoreWater,
                        ignoreMaterial = ignoreMaterial,
                        skyblivionTexPath = CmdArgs.GetBool(cmdArgs, "skyblivionTexPath", false),
                        ignoreTransRot = ignoreList,
                        HDTextureList = HDTexture,
                        notHDTextureList = notHDTexture,
                        HDMeshList = HDMesh,
                        notHDMeshList = notHDMesh
                    }.GenerateLOD(list2[1], list2[0], statics);
                })));
                list1[index1].Start((object)new List<int>()
                {
                    num,
                    index1
                });
                num <<= 1;
                ++index1;
            }
            while (list1.Count > 0)
            {
                for (int index2 = 0; index2 < list1.Count; ++index2)
                {
                    if (!list1[index2].IsAlive)
                    {
                        list1.RemoveAt(index2);
                        --index2;
                    }
                }
                Thread.Sleep(100);
            }
            theLog.Close();
            return 0;
        }
    }
}
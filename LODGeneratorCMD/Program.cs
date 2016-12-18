using LODGenerator;
using LODGenerator.Common;
using System;
using System.Collections.Generic;
using StringList = System.Collections.Generic.List<string>;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading;
using System.Runtime.InteropServices;
using Ini;

namespace LODGeneratorCMD
{

    internal class Program
    {
        [DllImport("shell32.dll", SetLastError = true)]
        static extern IntPtr CommandLineToArgvW([MarshalAs(UnmanagedType.LPWStr)] string lpCmdLine, out int pNumArgs);

        public static string[] CommandLineToArgs(string commandLine)
        {
            int argc;
            var argv = CommandLineToArgvW(commandLine, out argc);
            if (argv == IntPtr.Zero)
                throw new System.ComponentModel.Win32Exception();
            try
            {
                var args = new string[argc];
                for (var i = 0; i < args.Length; i++)
                {
                    var p = Marshal.ReadIntPtr(argv, i * IntPtr.Size);
                    args[i] = Marshal.PtrToStringUni(p);
                }

                return args;
            }
            finally
            {
                Marshal.FreeHGlobal(argv);
            }
        }

        private static Dictionary<string, string> CollectCmdArgs(string[] args)
        {
            Dictionary<string, string> dictionary = new Dictionary<string, string>();
            for (int index = 0; index < Enumerable.Count<string>((IEnumerable<string>)args); ++index)
            {
                string str = "";
                string key = str;
                try
                {
                    key = args[index].Remove(0, 2).ToLower(CultureInfo.InvariantCulture);
                }
                catch
                {
                    Console.WriteLine("Command line paramter error");
                    System.Environment.Exit(666);
                }
                if (index + 1 < Enumerable.Count<string>((IEnumerable<string>)args) && !args[index + 1].StartsWith("--"))
                    str = args[++index];
                dictionary.Add(key, str);
            }
            return dictionary;
        }

        private static int Main(string[] args)
        {
            if (Environment.CommandLine.Contains("\\\""))
            {
                args = CommandLineToArgs(Environment.CommandLine.Replace("\\\"", "\""));
            }
            Dictionary<string, string> cmdArgs = Program.CollectCmdArgs(args);
            string logFileName = CmdArgs.GetString(cmdArgs, "logfile", Directory.GetCurrentDirectory() + "\\LODGen_log.txt");
            LogFile theLog = new LogFile(logFileName);

            if (Enumerable.Count<string>((IEnumerable<string>)args) < 1)
            {
                theLog.WriteLog("Nothing to do");
                theLog.Close();
                return -1;
            }
            string path = CmdArgs.GetString(cmdArgs, "inputfile", args[0]);
            if (!File.Exists(path))
            {
                theLog.WriteLog("No input file " + path);
                theLog.Close();
                return -1;
            }
            CultureInfo cultureInfo = CmdArgs.ci;
            StreamReader streamReader = new StreamReader(path, System.Text.Encoding.Default, true);
            StreamReader streamReader2 = new StreamReader(path, System.Text.Encoding.Default, true);
            List<StaticDesc> statics = new List<StaticDesc>();
            StringList BSAFiles = new StringList();
            StringList ignoreList = new StringList();
            StringList HDTexture = new StringList();
            StringList notHDTexture = new StringList();
            StringList HDMesh = new StringList();
            StringList notHDMesh = new StringList();
            StringList PassThruMesh = new StringList();
            Game.Mode = "tes5";
            // 1 = 1 cell seems best
            Game.sampleSize = 1f;
            int counter = 0;
            string worldspaceName = "";
            string gameDir = "";
            string outputDir = "";
            string texturesListFile = "";
            string uvfile = "";
            string flatfile = "";
            string altfile = "";
            float southWestX = 0;
            float southWestY = 0;
            float atlasTolerance = 0.2f;
            bool verbose = false;
            bool generateVertexColors = true;
            bool dontFixTangents = false;
            bool dontGenerateTangents = false;
            bool mergeShapes = true;
            bool removeUnseenFaces = false;
            float removeUnseenZShift = 0;
            bool ignoreWater = false;
            bool useHDFlag = true;
            bool useOptimizer = false;
            bool ignoreMaterial = false;
            bool alphaDoubleSided = false;
            bool useAlphaThreshold = false;
            bool useBacklightPower = false;
            bool useDecalFlag = false;
            bool dontGroup = true;
            bool removeBlocks = false;
            int flatLODLevelLODFlag = -1;
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

                    if (strArray2[0].ToLower(CultureInfo.InvariantCulture) == "textureslistfile")
                    {
                        texturesListFile = strArray2[1];
                    }

                    if (strArray2[0].ToLower(CultureInfo.InvariantCulture) == "textureatlasmap")
                    {
                        uvfile = strArray2[1];
                    }
                    if (strArray2[0].ToLower(CultureInfo.InvariantCulture) == "flattextures")
                    {
                        flatfile = strArray2[1];
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
                        if (strArray2[1] != "")
                        {
                            HDMesh.Add(strArray2[1].ToLower(CultureInfo.InvariantCulture));
                        }
                    }
                    if (strArray2[0].ToLower(CultureInfo.InvariantCulture) == "nothdmeshmask")
                    {
                        if (strArray2[1] != "")
                        {
                            notHDMesh.Add(strArray2[1].ToLower(CultureInfo.InvariantCulture));
                        }
                    }
                    if (strArray2[0].ToLower(CultureInfo.InvariantCulture) == "passthrumeshmask")
                    {
                        if (strArray2[1] != "")
                        {
                            PassThruMesh.Add(strArray2[1].ToLower(CultureInfo.InvariantCulture));
                        }
                    }
                    if (strArray2[0].ToLower(CultureInfo.InvariantCulture) == "ishdtexturemask")
                    {
                        if (strArray2[1] != "")
                        {
                            HDTexture.Add(strArray2[1].ToLower(CultureInfo.InvariantCulture).Replace("data\\", ""));
                        }
                    }
                    if (strArray2[0].ToLower(CultureInfo.InvariantCulture) == "texturediffusehd")
                    {
                        if (strArray2[1] != "")
                        {
                            HDTexture.Add(strArray2[1].ToLower(CultureInfo.InvariantCulture).Replace("data\\", ""));
                        }
                    }
                    if (strArray2[0].ToLower(CultureInfo.InvariantCulture) == "nothdtexturemask")
                    {
                        if (strArray2[1] != "")
                        {
                            notHDTexture.Add(strArray2[1].ToLower(CultureInfo.InvariantCulture));
                        }
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
                        if (Game.Mode == "fnv")
                        {
                            Game.Mode = "fo3";
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
                    if (strArray2[0].ToLower(CultureInfo.InvariantCulture) == "removeunseenzshift")
                    {
                        removeUnseenZShift = float.Parse(strArray2[1]);
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
                    if (strArray2[0].ToLower(CultureInfo.InvariantCulture) == "alphadoublesided")
                    {
                        alphaDoubleSided = Boolean.Parse(strArray2[1]);
                    }
                    if (strArray2[0].ToLower(CultureInfo.InvariantCulture) == "usealphathreshold")
                    {
                        useAlphaThreshold = Boolean.Parse(strArray2[1]);
                    }
                    if (strArray2[0].ToLower(CultureInfo.InvariantCulture) == "usebacklightpower")
                    {
                        useBacklightPower = Boolean.Parse(strArray2[1]);
                    }
                    if (strArray2[0].ToLower(CultureInfo.InvariantCulture) == "usedecalflag")
                    {
                        useDecalFlag = Boolean.Parse(strArray2[1]);
                    }
                    if (strArray2[0].ToLower(CultureInfo.InvariantCulture) == "dontgroup")
                    {
                        dontGroup = Boolean.Parse(strArray2[1]);
                    }
                    if (strArray2[0].ToLower(CultureInfo.InvariantCulture) == "flatlodlevellodflag")
                    {
                        flatLODLevelLODFlag = int.Parse(strArray2[1]);
                        flatLODLevelLODFlag--;
                    }
                    if (strArray2[0].ToLower(CultureInfo.InvariantCulture) == "globalscale")
                    {
                        globalScale = float.Parse(strArray2[1], CultureInfo.InvariantCulture);
                    }
                    if (strArray2[0].ToLower(CultureInfo.InvariantCulture) == "removeblocks")
                    {
                        removeBlocks = Boolean.Parse(strArray2[1]);
                    }
                    if (strArray2[0].ToLower(CultureInfo.InvariantCulture) == "verbose")
                    {
                        verbose = Boolean.Parse(strArray2[1]);
                    }
                }
                else
                {
                    break;
                }
            }

            if (CmdArgs.GetBool(cmdArgs, "gamemode", false))
            {
                Game.Mode = CmdArgs.GetString(cmdArgs, "gamemode", Game.Mode).ToLower();
            }

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
            if (CmdArgs.GetBool(cmdArgs, "verbose", false))
            {
                verbose = true;
            }

            int int1 = CmdArgs.GetInt(cmdArgs, "lodLevel", Int32.MinValue);
            int int2 = CmdArgs.GetInt(cmdArgs, "x", Int32.MinValue);
            int int3 = CmdArgs.GetInt(cmdArgs, "y", Int32.MinValue);
            theLog.WriteLog("Game Mode: " + Game.Mode.ToUpper());
            if (Game.Mode != "textureslist")
            {
                theLog.WriteLog("Fix Tangents: " + (!dontFixTangents ? "True" : "False"));
                theLog.WriteLog("Generate Tangents: " + (!dontGenerateTangents ? "True" : "False"));
                theLog.WriteLog("Generate Vertex Colors: " + (generateVertexColors ? "True" : "False"));
                theLog.WriteLog("Merge Meshes: " + (mergeShapes ? "True" : "False"));
                theLog.WriteLog("Grouping: " + (dontGroup ? "False" : "True"));
                theLog.WriteLog("Remove Faces under Terrain: " + (removeUnseenFaces ? "True" : "False"));
                theLog.WriteLog("Remove Faces under Water: " + (!ignoreWater ? "True" : "False"));
                if (removeUnseenZShift != 0)
                {
                    theLog.WriteLog("Remove Faces Z-Shift: " + removeUnseenZShift);
                }
                theLog.WriteLog("Use HD Flag: " + (useHDFlag ? "True" : "False"));
                //theLog.WriteLog("Use Optimizer: " + (useOptimizer ? "True" : "False"));
                theLog.WriteLog("Ignore Materials: " + (ignoreMaterial ? "True" : "False"));
                theLog.WriteLog("Alpha DoubleSided: " + (alphaDoubleSided ? "True" : "False"));
                theLog.WriteLog("Use Alpha Threshold: " + (useAlphaThreshold ? "True" : "False"));
                theLog.WriteLog("Use Backlight Power: " + (useBacklightPower ? "True" : "False"));
                theLog.WriteLog("Use Decal Flag: " + (useDecalFlag ? "True" : "False"));
                theLog.WriteLog("Global scale: " + string.Format("{0:0.00}", globalScale));
                theLog.WriteLog("Specific level: " + (int1 != Int32.MinValue ? int1.ToString() : "No"));
                if (int2 != Int32.MinValue && int3 == Int32.MinValue)
                    theLog.WriteLog("Specific quad: [" + (object)int2.ToString() + ", X]");
                else if (int2 == Int32.MinValue && int3 != Int32.MinValue)
                    theLog.WriteLog("Specific quad: [X, " + (object)int3.ToString() + "]");
                else if (int2 != Int32.MinValue && int3 != Int32.MinValue)
                    theLog.WriteLog("Specific quad: [" + (object)int2.ToString() + ", " + (string)(object)int3.ToString() + "]");
                else
                    theLog.WriteLog("Specific quad: No");
                theLog.WriteLog("Output: " + outputDir);
            }

            streamReader.Close();
            if (worldspaceName == "")
            {
                theLog.WriteLog("No Worldspace, set Worldspace=");
                theLog.Close();
                return -1;
            }
            if (outputDir == "")
            {
                theLog.WriteLog("No PathOutput, set PathOutput=");
                theLog.Close();
                return -1;
            }
            if (Game.Mode == "textureslist" && texturesListFile == "")
            {
                theLog.WriteLog("No output file for textures list, set TexturesListFile=");
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
            BSAArchive.Load(BSAFiles, theLog, verbose);
            streamReader = new StreamReader(path);
            for (int index = 0; index < counter; ++index)
            {
                streamReader.ReadLine();
            }
            while (!streamReader.EndOfStream)
            {
                StaticDesc staticDesc = new StaticDesc();
                staticDesc.staticModels = new string[4];
                staticDesc.staticFlags = 0;
                staticDesc.materialName = "";
                staticDesc.staticModels[0] = "";
                staticDesc.staticModels[1] = "";
                staticDesc.staticModels[2] = "";
                staticDesc.staticModels[3] = "";
                staticDesc.enableParent = 0;
                staticDesc.materialSwap = new Dictionary<string, string>();
                staticDesc.transrot = new Matrix44(new Matrix33(true), new Vector3(0f, 0f, 0f), 1f);
                staticDesc.transscale = 1f;
                string line = streamReader.ReadLine();
                if (line.Contains('\t'))
                {
                    string[] strArray2 = line.Split('\t');
                    counter++;
                    if (strArray2.Count() >= 10)
                    {
                        staticDesc.refID = strArray2[0];
                        staticDesc.refFlags = int.Parse(strArray2[1], NumberStyles.HexNumber, CultureInfo.InvariantCulture);
                        staticDesc.x = float.Parse(strArray2[2], CultureInfo.InvariantCulture);
                        staticDesc.y = float.Parse(strArray2[3], CultureInfo.InvariantCulture);
                        staticDesc.z = float.Parse(strArray2[4], CultureInfo.InvariantCulture);
                        staticDesc.rotX = float.Parse(strArray2[5], CultureInfo.InvariantCulture);
                        staticDesc.rotY = float.Parse(strArray2[6], CultureInfo.InvariantCulture);
                        staticDesc.rotZ = float.Parse(strArray2[7], CultureInfo.InvariantCulture);
                        staticDesc.scale = float.Parse(strArray2[8], CultureInfo.InvariantCulture);
                        staticDesc.staticName = strArray2[9];
                        staticDesc.staticModels[0] = staticDesc.staticName;
                    }
                    if (strArray2.Count() >= 13)
                    {
                        staticDesc.staticName = strArray2[9];
                        staticDesc.staticFlags = int.Parse(strArray2[10], NumberStyles.HexNumber, CultureInfo.InvariantCulture);
                        staticDesc.materialName = strArray2[11];
                        staticDesc.staticFullModel = strArray2[12];
                        if (strArray2.Count() >= 16)
                        {
                            int noStatics = 3;
                            staticDesc.staticModels[3] = "";
                            if (strArray2.Count() >= 17)
                            {
                                noStatics = 4;
                            }
                            for (int index = 0; index < noStatics; ++index)
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
                            if (strArray2.Count() >= 20)
                            {
                                if (strArray2[17] != "")
                                {
                                    staticDesc.enableParent = uint.Parse(strArray2[17], NumberStyles.HexNumber, CultureInfo.InvariantCulture);
                                }
                                //staticDesc.enableParent = uint.Parse(strArray2[0], NumberStyles.HexNumber, CultureInfo.InvariantCulture);
                                string[] baseMat = strArray2[18].Split(',');
                                string[] swapMat = strArray2[19].Split(',');
                                if (baseMat.Count() == swapMat.Count())
                                {
                                    for (int index = 0; index < baseMat.Count(); index++)
                                    {
                                        if (baseMat[index] != "" && swapMat[index] != "")
                                        {
                                            staticDesc.materialSwap.Add("materials\\" + baseMat[index], "materials\\" + swapMat[index]);
                                        }
                                    }
                                }
                                else
                                {
                                    theLog.WriteLog(staticDesc.refID + " ignoring material swap data because item count is not equal");
                                }
                                if (strArray2.Count() == 27 && strArray2[20] != "" && strArray2[21] != "" && strArray2[22] != "" && strArray2[23] != "" && strArray2[24] != "" && strArray2[25] != "" && strArray2[26] != "")
                                {

                                    Matrix33 matrix33_1 = new Matrix33(true);
                                    Matrix33 matrix33_2 = new Matrix33(true);
                                    Matrix33 matrix33_3 = new Matrix33(true);
                                    matrix33_1.SetRotationX(Utils.ToRadians(-float.Parse(strArray2[23], CultureInfo.InvariantCulture)));
                                    matrix33_2.SetRotationY(Utils.ToRadians(-float.Parse(strArray2[24], CultureInfo.InvariantCulture)));
                                    matrix33_3.SetRotationZ(Utils.ToRadians(-float.Parse(strArray2[25], CultureInfo.InvariantCulture)));
                                    staticDesc.transrot = new Matrix44(new Matrix33(true) * matrix33_1 * matrix33_2 * matrix33_3, new Vector3(float.Parse(strArray2[20], CultureInfo.InvariantCulture), float.Parse(strArray2[21], CultureInfo.InvariantCulture), float.Parse(strArray2[22], CultureInfo.InvariantCulture)), 1f);
                                    staticDesc.transscale = float.Parse(strArray2[26], CultureInfo.InvariantCulture);
                                }
                            }
                        }
                    }
                    statics.Add(staticDesc);
                }
            }
            statics.Reverse();
            streamReader.Close();
            if (!File.Exists(uvfile))
            {
                uvfile = System.AppDomain.CurrentDomain.BaseDirectory + uvfile;
            }
            if (File.Exists(uvfile) && Game.Mode != "textureslist")
            {
                theLog.WriteLog("Using UV Atlas: " + uvfile);
                streamReader = new StreamReader(uvfile);
                int lineCount = 0;
                while (!streamReader.EndOfStream)
                {
                    lineCount++;
                    string[] strArray2 = streamReader.ReadLine().Split('\t');
                    AtlasDesc atlasDesc = new AtlasDesc();
                    if (strArray2.Length >= 8)
                    {
                        try {
                            atlasDesc.SourceTexture = strArray2[0].ToLower(CultureInfo.InvariantCulture);
                            int textureWidth = int.Parse(strArray2[1], CultureInfo.InvariantCulture);
                            int textureHeight = int.Parse(strArray2[2], CultureInfo.InvariantCulture);
                            int textureX = int.Parse(strArray2[3], CultureInfo.InvariantCulture);
                            int textureY = int.Parse(strArray2[4], CultureInfo.InvariantCulture);
                            int atlasWidth = int.Parse(strArray2[6], CultureInfo.InvariantCulture);
                            int atlasHeight = int.Parse(strArray2[7], CultureInfo.InvariantCulture);
                            float scaleU = 1f;
                            float scaleV = 1f;
                            if (strArray2.Length >= 10)
                            {
                                atlasDesc.miniatlas = true;
                                scaleU = float.Parse(strArray2[8], CultureInfo.InvariantCulture);
                                scaleV = float.Parse(strArray2[9], CultureInfo.InvariantCulture);
                            }
                            else
                            {
                                atlasDesc.miniatlas = false;
                            }
                            atlasDesc.scaleU = (float)textureWidth / atlasWidth * scaleU;
                            atlasDesc.scaleV = (float)textureHeight / atlasHeight * scaleV;
                            atlasDesc.posU = (float)textureX / atlasWidth;
                            atlasDesc.posV = (float)textureY / atlasHeight;
                            atlasDesc.minU = (float)0.5 / textureWidth;
                            atlasDesc.maxU = (float)(textureWidth - 0.5) / textureWidth;
                            atlasDesc.minV = (float)0.5 / textureHeight;
                            atlasDesc.maxV = (float)(textureHeight - 0.5) / textureHeight;
                            atlasDesc.before = 0;
                            atlasDesc.after = 0;
                            atlasDesc.averageU = 0;
                            atlasDesc.averageV = 0;
                            atlasDesc.AtlasTexture = strArray2[5].ToLower(CultureInfo.InvariantCulture);
                            atlasDesc.AtlasTextureN = Utils.GetNormalTextureName(strArray2[5].ToLower(CultureInfo.InvariantCulture));
                            if (Game.Mode == "fo4")
                            {
                                atlasDesc.AtlasTextureS = Utils.GetSpecularTextureName(strArray2[5].ToLower(CultureInfo.InvariantCulture));
                            }
                            else
                            {
                                atlasDesc.AtlasTextureS = "";
                            }
                        }
                        catch
                        {
                            theLog.WriteLog("Error reading line " + lineCount + " from " + uvfile);
                            theLog.Close();
                            System.Environment.Exit(1062);
                        }
                        //if (AtlasList.Contains(atlasDesc.SourceTexture) && AtlasList.Get(atlasDesc.SourceTexture).miniatlas)
                        //{
                        //    atlasDesc.SourceTexture = AtlasList.Get(atlasDesc.SourceTexture).AtlasTexture;
                        //}
                        if (AtlasList.Contains(atlasDesc.SourceTexture))
                        {
                            theLog.WriteLog("Texture name already defined in atlas file " + atlasDesc.SourceTexture);
                            theLog.Close();
                            System.Environment.Exit(1062);
                        }
                        AtlasList.Set(atlasDesc.SourceTexture, atlasDesc);
                    }
                    else
                    {
                        theLog.WriteLog("Line " + lineCount + " not enough data in " + uvfile);
                    }
                }
                streamReader.Close();
            }
            if (!File.Exists(flatfile))
            {
                flatfile = System.AppDomain.CurrentDomain.BaseDirectory + flatfile;
            }
            if (File.Exists(flatfile))
            {
                theLog.WriteLog("Using Flat Textures: " + flatfile);
                streamReader = new StreamReader(flatfile);
                while (!streamReader.EndOfStream)
                {
                    string[] strArray2 = streamReader.ReadLine().Split('\t');
                    FlatDesc flatDesc = new FlatDesc();
                    if (strArray2.Length >= 6)
                    {
                        flatDesc.SourceTexture = strArray2[0].ToLower(CultureInfo.InvariantCulture);
                        flatDesc.width = float.Parse(strArray2[1], CultureInfo.InvariantCulture);
                        flatDesc.height = float.Parse(strArray2[2], CultureInfo.InvariantCulture);
                        flatDesc.shiftZ = float.Parse(strArray2[3], CultureInfo.InvariantCulture);
                        flatDesc.scale = float.Parse(strArray2[4], CultureInfo.InvariantCulture);
                        flatDesc.effect1 = float.Parse(strArray2[5], CultureInfo.InvariantCulture);
                        if (strArray2.Length >= 8)
                        {
                            flatDesc.GlowTexture = strArray2[7].ToLower(CultureInfo.InvariantCulture);
                        }
                        else
                        {
                            flatDesc.GlowTexture = "textures\\white.dds";
                        }
                        flatDesc.normals = new List<Vector3>();
                        flatDesc.tangents = new List<Vector3>();
                        flatDesc.bitangents = new List<Vector3>();
                        if ((strArray2.Length >= 7) && (File.Exists(strArray2[6])))
                        {
                            streamReader2 = new StreamReader(strArray2[6]);
                            int index = 0;
                            while (!streamReader2.EndOfStream)
                            {
                                string[] strArray3 = streamReader2.ReadLine().Split(',');
                                if (strArray3.Length == 3)
                                {
                                    if (index < 8)
                                    {
                                        flatDesc.normals.Add(new Vector3(float.Parse(strArray3[0], CultureInfo.InvariantCulture), float.Parse(strArray3[1], CultureInfo.InvariantCulture), float.Parse(strArray3[2], CultureInfo.InvariantCulture)));
                                    }
                                    else if (index < 16)
                                    {
                                        flatDesc.tangents.Add(new Vector3(float.Parse(strArray3[0], CultureInfo.InvariantCulture), float.Parse(strArray3[1], CultureInfo.InvariantCulture), float.Parse(strArray3[2], CultureInfo.InvariantCulture)));
                                    }
                                    else if (index < 24)
                                    {
                                        flatDesc.bitangents.Add(new Vector3(float.Parse(strArray3[0], CultureInfo.InvariantCulture), float.Parse(strArray3[1], CultureInfo.InvariantCulture), float.Parse(strArray3[2], CultureInfo.InvariantCulture)));
                                    }
                                    index++;
                                }
                            }
                        }
                        if (flatDesc.normals.Count() < 8 || flatDesc.tangents.Count() < 8 || flatDesc.bitangents.Count() < 8)
                        {
                            for (int index = 0; index < 8; index++)
                            {
                                if (flatDesc.normals.Count() < 8)
                                {
                                    flatDesc.normals.Add(new Vector3(0.0f, 0.0f, 0.0f));
                                }
                                if (flatDesc.tangents.Count() < 8)
                                {
                                    flatDesc.tangents.Add(new Vector3(0.0f, 0.0f, 0.0f));
                                }
                                if (flatDesc.bitangents.Count() < 8)
                                {
                                    flatDesc.bitangents.Add(new Vector3(0.0f, 0.0f, 0.0f));
                                }
                            }
                        }
                        //theLog.WriteLog(flatDesc.SourceTexture + "\t" + flatDesc.width + "\t" + flatDesc.height + "\t" + flatDesc.shiftZ + "\t" + flatDesc.scale);
                        FlatList.Set(flatDesc.SourceTexture, flatDesc);
                    }
                }
                streamReader.Close();
            }
            if (!File.Exists(altfile))
            {
                altfile = System.AppDomain.CurrentDomain.BaseDirectory + altfile;
            }
            if (File.Exists(altfile))
            {
                theLog.WriteLog("Using Alt Textures: " + altfile);
                streamReader = new StreamReader(altfile);
                while (!streamReader.EndOfStream)
                {
                    string[] strArray2 = streamReader.ReadLine().ToLower(CultureInfo.InvariantCulture).Split('\t');
                    if (strArray2.Length > 0)
                    {
                        for (int index = 1; index < strArray2.Length; index++)
                        {
                            string[] strArray3 = strArray2[index].Split('=');
                            string key = strArray2[0] + "_" + strArray3[0] + "_" + strArray3[1];
                            AltTextureDesc altTexDesc = new AltTextureDesc();
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
            Game.atlasToleranceMin = atlasTolerance * -1f;
            Game.atlasToleranceMax = atlasTolerance + 1f;
            if (Game.Mode == "tes5" || Game.Mode == "sse" || Game.Mode == "fo3" || Game.Mode == "fo4")
            {
                theLog.WriteLog("Generating LOD for worldspace " + worldspaceName);
            }
            else if (Game.Mode.Contains("merge"))
            {
                theLog.WriteLog("Merging meshes");
            }
            else if (Game.Mode.Contains("convert"))
            {
                theLog.WriteLog("Converting meshes");
            }
            else if (Game.Mode.Contains("textures"))
            {
                theLog.WriteLog("Creating textures list");
            }
            theLog.WriteLog("");
            List<Thread> list1 = new List<Thread>();
            int num = 1;
            int index1 = 0;
            while (num <= 8)
            {
                list1.Add(new Thread((ParameterizedThreadStart)(state =>
                {
                    List<int> list2 = (List<int>)state;
                    new LODApp(worldspaceName, outputDir, gameDir, theLog)
                    {
                        verbose = verbose,
                        texturesListFile = texturesListFile,
                        fixTangents = !dontFixTangents,
                        generateTangents = !dontGenerateTangents,
                        generateVertexColors = generateVertexColors,
                        mergeShapes = mergeShapes,
                        useHDFlag = useHDFlag,
                        useOptimizer = useOptimizer,
                        useFadeNode = CmdArgs.GetBool(cmdArgs, "useFadeNode", false),
                        removeUnseenFaces = removeUnseenFaces,
                        globalScale = globalScale,
                        lodLevelToGenerate = CmdArgs.GetInt(cmdArgs, "lodLevel", Int32.MinValue),
                        lodX = CmdArgs.GetInt(cmdArgs, "x", Int32.MinValue),
                        lodY = CmdArgs.GetInt(cmdArgs, "y", Int32.MinValue),
                        southWestX = (int)southWestX,
                        southWestY = (int)southWestY,
                        atlasToleranceMin = Game.atlasToleranceMin,
                        atlasToleranceMax = Game.atlasToleranceMax,
                        removeUnderwaterFaces = !ignoreWater,
                        removeUnseenZShift = removeUnseenZShift,
                        ignoreMaterial = ignoreMaterial,
                        alphaDoublesided = alphaDoubleSided,
                        useAlphaThreshold = useAlphaThreshold,
                        useBacklightPower = useBacklightPower,
                        useDecalFlag = useDecalFlag,
                        dontGroup = dontGroup,
                        skyblivionTexPath = CmdArgs.GetBool(cmdArgs, "skyblivionTexPath", false),
                        removeBlocks = removeBlocks,
                        flatLODLevelLODFlag = flatLODLevelLODFlag,
                        ignoreTransRot = ignoreList,
                        HDTextureList = HDTexture,
                        notHDTextureList = notHDTexture,
                        HDMeshList = HDMesh,
                        notHDMeshList = notHDMesh,
                        PassThruMeshList = PassThruMesh
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
                Thread.Sleep(50);
            }
            //if (Game.Mode == "convert4" || Game.Mode == "convert5")
            {
                AtlasList.WriteStats(theLog);
            }
            theLog.Close();
            return 0;
        }
    }
}
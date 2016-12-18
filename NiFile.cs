using LODGenerator.Common;
using LODGenerator.NifMain;
using System;
using System.Collections.Generic;
using System.IO;

namespace LODGenerator
{
    public class NiFile
    {
        private static Dictionary<string, Type> classTypes = new Dictionary<string, Type>()
        {
            {
            "BSFadeNode",
            typeof (BSFadeNode)
            },
            {
                "NiTriShape",
                typeof (NiTriShape)
            },
            {
                "NiTriShapeData",
                typeof (NiTriShapeData)
            },
            {
                "BSTriShape",
                typeof (BSTriShape)
            },
            {
                "BSSubIndexTriShape",
                typeof (BSSubIndexTriShape)
            },
            {
                "BSMeshLODTriShape",
                typeof (BSMeshLODTriShape)
            },
            {
                "BSLightingShaderProperty",
                typeof (BSLightingShaderProperty)
            },
            {
                "BSShaderPPLightingProperty",
                typeof (BSShaderPPLightingProperty)
            },
            {
                "BSShaderTextureSet",
                typeof (BSShaderTextureSet)
            },
            {
                "NiNode",
                typeof (NiNode)
            },
            {
                "BSTreeNode",
                typeof (BSTreeNode)
            },
            {
                "NiSwitchNode",
                typeof (NiSwitchNode)
            },
            {
                "BSLeafAnimNode",
                typeof (NiNode)
            },
            {
                "NiBillboardNode",
                typeof (NiBillboardNode)
            },
            {
                "NiMaterialProperty",
                typeof (NiMaterialProperty)
            },
            {
                "NiTexturingProperty",
                typeof (NiTexturingProperty)
            },
            {
                "NiSourceTexture",
                typeof (NiSourceTexture)
            },
            {
                "NiTriStrips",
                typeof (NiTriStrips)
            },
            {
                "NiTriStripsData",
                typeof (NiTriStripsData)
            },
            {
                "NiStringExtraData",
                typeof (NiStringExtraData)
            },
            {
                "BSEffectShaderProperty",
                typeof (BSEffectShaderProperty)
            },
            {
                "NiStencilProperty",
                typeof (NiStencilProperty)
            },
            {
                "NiBinaryExtraData",
                typeof (NiBinaryExtraData)
            },
            {
                "NiVertexColorProperty",
                typeof (NiVertexColorProperty)
            },
            {
                "NiMaterialColorController",
                typeof (NiMaterialColorController)
            },
            {
                "NiPoint3Interpolator",
                typeof (NiPoint3Interpolator)
            },
            {
                "NiPosData",
                typeof (NiPosData)
            },
            {
                "NiIntegerExtraData",
                typeof (NiIntegerExtraData)
            },
            {
                "BSXFlags",
                typeof (BSXFlags)
            },
            {
                "NiAlphaProperty",
                typeof (NiAlphaProperty)
            },
            {
                "BSMultiBoundNode",
                typeof (BSMultiBoundNode)
            },
            {
                "BSLODTriShape",
                typeof (BSLODTriShape)
            },
            {
                "BSOrderedNode",
                typeof (BSOrderedNode)
            },
            {
                "BSMultiBound",
                typeof (BSMultiBound)
            },
            {
                "BSMultiBoundAABB",
                typeof (BSMultiBoundAABB)
            },
            {
                "BSSegmentedTriShape",
                typeof (BSSegmentedTriShape)
            },
            {
                "BSInvMarker",
                typeof (BSInvMarker)
            },
            {
                "NiCollisionObject",
                typeof (NiCollisionObject)
            },
            {
                "bhkNiCollisionObject",
                typeof (bhkNiCollisionObject)
            },
            {
                "bhkCollisionObject",
                typeof (bhkCollisionObject)
            },
            {
                "bhkRefObject",
                typeof (bhkRefObject)
            },
            {
                "bhkSerializable",
                typeof (bhkSerializable)
            },
            {
                "bhkWorldObject",
                typeof (bhkWorldObject)
            },
            {
                "bhkEntity",
                typeof (bhkEntity)
            },
            {
                "bhkRigidBody",
                typeof (bhkRigidBody)
            },
            {
                "bhkShape",
                typeof (bhkShape)
            },
            {
                "bhkCompressedMeshShape",
                typeof(bhkCompressedMeshShape)
            },
            {
                "bhkBvTreeShape",
                typeof (bhkBvTreeShape)
            },
            {
                "bhkMoppBvTreeShape",
                typeof (bhkMoppBvTreeShape)
            },
            {
                "BSWaterShaderProperty",
                typeof  (BSWaterShaderProperty)
            },
            {
                "NiInterpolator",
                typeof (NiInterpolator)
            },
            {
                "NiKeyBasedInterpolator",
                typeof (NiKeyBasedInterpolator)
            },
            {
                "NiFloatInterpolator",
                typeof (NiFloatInterpolator)
            },
            {
                "NiFloatData",
                typeof (NiFloatData)
            },
            {
                "NiTimeController",
                typeof (NiTimeController)
            },
            {
                "NiInterpController",
                typeof (NiInterpController)
            },
            {
                "NiSingleInterpController",
                typeof (NiSingleInterpController)
            },
            {
                "NiFloatInterpController",
                typeof (NiFloatInterpController)
            },
            {
                "BSEffectShaderPropertyColorController",
                typeof (BSEffectShaderPropertyColorController)
            },
            {
                "BSEffectShaderPropertyFloatController",
                typeof (BSEffectShaderPropertyFloatController)
            },
            {
                "NiKeyframeController",
                typeof (NiKeyframeController)
            },
            {
                "NiTransformController",
                typeof (NiTransformController)
            },
            {
                "NiTransformInterpolator",
                typeof (NiTransformInterpolator)
            },
            {
                "BSMultiBoundOBB",
                typeof (BSMultiBoundOBB)
            },
            {
                "NiSkinInstance",
                typeof (NiSkinInstance)
            }
        };
        private NiHeader header;
        private List<NiObject> blocks;
        private List<byte[]> rawBlocks;
        private List<string> blockType;
        private List<uint> blockSize;

        public NiFile()
        {
            this.header = new NiHeader();
            this.blocks = new List<NiObject>();
            this.rawBlocks = new List<byte[]>();
            this.blockType = new List<string>();
            this.blockSize = new List<uint>();
        }

        public void Read(string gameDir, string fileName, LogFile logFile)
        {
            MemoryStream memoryStream = new MemoryStream();
            try
            {
                if (File.Exists(gameDir + fileName))
                {
                    try
                    {
                        FileStream fileStream = (FileStream)null;
                        while (fileStream == null)
                        {
                            fileStream = Utils.GetFileStream(new FileInfo(gameDir + fileName), logFile);
                        }
                        BinaryReader binaryReader = new BinaryReader((Stream)fileStream);
                        long length = binaryReader.BaseStream.Length;
                        memoryStream.Write(binaryReader.ReadBytes((int)length), 0, (int)length);
                        binaryReader.Close();
                        //logFile.WriteLog(" read " + fileName + " " + length);
                    }
                    catch (Exception ex)
                    {
                        logFile.WriteLog("Error reading " + fileName + " " + ex.Message);
                        logFile.Close();
                        System.Environment.Exit(500);
                    }
                }
                else if (BSAArchive.FileExists(fileName))
                {
                    try
                    {
                        byte[] newfile = BSAArchive.GetFile(fileName);
                        int length = newfile.Length;
                        memoryStream.Write(newfile, 0, length);
                    }
                    catch (Exception ex)
                    {
                        logFile.WriteLog("Error reading " + fileName + " from BSA/BA2 " + ex.Message + ex.Source);
                        logFile.Close();
                        System.Environment.Exit(501);
                    }
                }
                else
                {
                    logFile.WriteLog(fileName + " not found");
                    logFile.Close();
                    System.Environment.Exit(404);
                }
            }
            catch (Exception ex)
            {
                logFile.WriteLog("Error accessing " + gameDir + fileName + " " + ex.Message);
                logFile.WriteLog("In case Mod Organizer is used, set output path outside of game and MO virtual file system directory");
                logFile.Close();
                System.Environment.Exit(502);
            }
            memoryStream.Position = 0L;
            BinaryReader reader = new BinaryReader((Stream)memoryStream);
            string error = "Read error " + fileName;
            try
            {
                this.header.Read(reader);
                var stream = reader.BaseStream;
                for (int index = 0; (long)index < (long)this.header.GetNumBlocks(); ++index)
                {
                    //Console.WriteLine("Reading block " + index + " of " + this.header.GetNumBlocks() + " = " + stream.Position + " = " + this.header.GetBlockTypeAtIndex(index));
                    if (NiFile.classTypes.ContainsKey(this.header.GetBlockTypeAtIndex(index)))
                    {
                        NiObject niObject = (NiObject)Activator.CreateInstance(NiFile.classTypes[this.header.GetBlockTypeAtIndex(index)]);
                        niObject.Read(this.header, reader);
                        this.blocks.Add(niObject);
                        this.rawBlocks.Add(new byte[0]);
                        this.blockType.Add(this.header.GetBlockTypeAtIndex(index));
                        this.blockSize.Add(this.header.GetBlockSizeAtIndex(index));
                    }
                    else
                    {
                        error = "Unsupported block " + index + " " + this.header.GetBlockTypeAtIndex(index) + " in " + gameDir + fileName;
                        //logFile.WriteLog("Unsupported block " + index + " " + this.header.GetBlockTypeAtIndex(index) + " in " + gameDir + fileName);
                        uint blockSizeAtIndex = this.header.GetBlockSizeAtIndex(index);
                        this.blocks.Add((NiObject)null);
                        this.rawBlocks.Add(reader.ReadBytes((int)blockSizeAtIndex));
                        this.blockType.Add(this.header.GetBlockTypeAtIndex(index));
                        this.blockSize.Add(this.header.GetBlockSizeAtIndex(index));
                    }
                }
            }
            catch
            {
                logFile.WriteLog(error);
                //logFile.Close();
                //System.Environment.Exit(501);
            }
            reader.Close();
        }

        public void Write(string fileName, LogFile logFile)
        {
            //try
            {
                BinaryWriter writer = new BinaryWriter((Stream)new FileStream(fileName, FileMode.Create));
                this.header.Update(header, this.blocks);
                this.header.Write(writer);
                for (int index = 0; (long)index < (long)this.header.GetNumBlocks(); ++index)
                {
                    if (this.blockSize[index] == 0)
                    {
                        this.blocks[index].Write(header, writer);
                    }
                    else
                    {
                        writer.Write(this.rawBlocks[index]);
                    }
                }
                writer.Write(1);
                writer.Write(0);
                writer.Close();
            }
            /*catch (Exception ex)
            {
                logFile.WriteLog("Error writing " + fileName + " " + ex.Message);
                logFile.WriteLog("In case Mod Organizer is used, set output path outside of game and MO virtual file system directory");
                logFile.Close();
                System.Environment.Exit(502);
            }*/
        }

        public void AddBlockReference(int value)
        {
            this.header.AddBlockReference(value);
        }

        public List<int> GetBlockReferences()
        {
            return this.header.GetBlockReferences();
        }

        public int GetNumBlocks()
        {
            return this.blocks.Count;
        }

        public NiObject GetBlockAtIndex(int index)
        {
            return this.blocks[index];
        }

        public byte[] GetRawBlockAtIndex(int index)
        {
            return this.rawBlocks[index];
        }

        public string GetBlockTypeAtIndex(int index)
        {
            return this.header.GetBlockTypeAtIndex(index);
        }

        public string GetRawBlockTypeAtIndex(int index)
        {
            return this.blockType[index];
        }

        public uint GetRawBlockSizeAtIndex(int index)
        {
            return this.blockSize[index];
        }

        public uint GetNumStrings()
        {
            return this.header.GetNumStrings();
        }

        public string GetStringAtIndex(int index)
        {
            return this.header.GetString((uint)index);
        }

        public void SetHeaderString(string value)
        {
            this.header.SetHeaderString(value);
        }

        public uint GetVersion()
        {
            return this.header.GetVersion();
        }

        public void SetVersion(uint value)
        {
            this.header.SetVersion(value);
        }

        public uint GetUserVersion()
        {
            return this.header.GetUserVersion();
        }

        public void SetUserVersion(uint value)
        {
            this.header.SetUserVersion(value);
        }

        public void SetUserVersion2(uint value)
        {
            this.header.SetUserVersion2(value);
        }

        public void SetCreator(string value)
        {
            this.header.SetCreator(value);
        }

        public int AddBlock(NiObject obj)
        {
            this.blocks.Add(obj);
            this.rawBlocks.Add(new byte[0]);
            this.blockType.Add("");
            this.blockSize.Add(0);
            this.header.AddBlock(this.header, obj);
            return this.blocks.Count - 1;
        }

        public int AddRawBlock(byte[] b, string s)
        {
            this.blocks.Add(new NiObject());
            this.rawBlocks.Add(b);
            this.blockType.Add(s);
            this.blockSize.Add((uint)b.Length);
            this.header.AddBlock(this.header, s, (uint)b.Length);
            return this.rawBlocks.Count - 1;
        }

        public int AddString(string str)
        {
            return this.header.AddString(str);
        }
    }
}

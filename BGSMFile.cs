using System;
using System.IO;
using System.Collections.Generic;
using System.Globalization;
using LODGenerator.Common;

namespace LODGenerator
{
    class BGSMFile
    {
        public string headerString;
        public uint version;
        public uint textureClampMode;
        public UVCoord uvOffset;
        public UVCoord uvScale;
        public float alpha;
        public byte alphamode0;
        public uint alphamode1;
        public uint alphamode2;
        public byte alphaThreshold;
        public byte alphaFlag;
        public byte zBufferWrite;
        public byte zBufferTest;
        public byte reflections;
        public byte wetreflections;
        public byte decal;
        public byte doubleSided;
        public byte decalnofade;
        public byte noocclude;
        public byte refraction;
        public byte refractionfalloff;
        public float refractionpower;
        public byte envmap;
        public float envmapscale;
        public byte graytocolor;
        public string[] textures;
        public byte enablealpha;
        public byte rimlighting;
        public float rimPower;
        public float backlightPower;
        // ... more
        
        public BGSMFile()
        {
            this.headerString = "BGSM";
            this.version = 3U;
            this.textureClampMode = 0U;
            this.uvOffset = new UVCoord(0.0f, 0.0f);
            this.uvScale = new UVCoord(1f, 1f);
            this.alpha = 1f;
            this.alphamode0 = 0;
            this.alphamode1 = 0;
            this.alphamode2 = 0;
            this.alphaThreshold = 0;
            this.alphaFlag = 0;
            this.zBufferWrite = 0;
            this.zBufferTest = 0;
            this.reflections = 0;
            this.wetreflections = 0;
            this.decal = 0;
            this.doubleSided = 0;
            this.decalnofade = 0;
            this.noocclude = 0;
            this.refraction = 0;
            this.refractionfalloff = 0;
            this.refractionpower = 0;
            this.envmap = 0;
            this.envmapscale = 0;
            this.graytocolor = 0;
            this.textures = new string[9];
            this.textures[0] = "";
            this.textures[1] = "";
            this.textures[2] = "";
            this.textures[3] = "";
            this.textures[4] = "";
            this.textures[5] = "";
            this.textures[6] = "";
            this.textures[7] = "";
            this.textures[8] = "";
            this.enablealpha = 0;
            this.rimlighting = 0;
            this.rimPower = 0;
            this.backlightPower = 0;
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
                        logFile.WriteLog("Error reading " + fileName + " from BSA/BA2 " + ex.Message);
                        logFile.Close();
                        System.Environment.Exit(501);
                    }
                }
                else
                {
                    logFile.WriteLog(fileName + " not found!");
                    return;
                    //logFile.Close();
                    //System.Environment.Exit(404);
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
            try
            {

                this.headerString = "";
                byte[] data = reader.ReadBytes(4);
                for (int index = 0; index < 4; index++)
                {
                    this.headerString += (object)(char)data[index];
                }
                this.version = reader.ReadUInt32();
                this.textureClampMode = reader.ReadUInt32();
                this.uvOffset = Utils.ReadUVCoord(reader);
                this.uvScale = Utils.ReadUVCoord(reader);
                this.alpha = reader.ReadSingle();
                this.alphamode0 = reader.ReadByte();
                this.alphamode1 = reader.ReadUInt32();
                this.alphamode2 = reader.ReadUInt32();
                this.alphaThreshold = reader.ReadByte();
                this.alphaFlag = reader.ReadByte();
                this.zBufferWrite = reader.ReadByte();
                this.zBufferTest = reader.ReadByte();
                this.reflections = reader.ReadByte();
                this.wetreflections = reader.ReadByte();
                this.decal = reader.ReadByte();
                this.doubleSided = reader.ReadByte();
                this.decalnofade = reader.ReadByte();
                this.noocclude = reader.ReadByte();
                this.refraction = reader.ReadByte();
                this.refractionfalloff = reader.ReadByte();
                this.refractionpower = reader.ReadSingle();
                this.envmap = reader.ReadByte();
                this.envmapscale = reader.ReadSingle();
                this.graytocolor = reader.ReadByte();
                for (int index = 0; index < 9; index++)
                {
                    this.textures[index] = Utils.ReadSizedString(reader).ToLower(CultureInfo.InvariantCulture).Replace("/", "\\").Replace("\0", string.Empty);
                }
                this.enablealpha = reader.ReadByte();
                this.rimlighting = reader.ReadByte();
                this.rimPower = reader.ReadSingle();
                this.backlightPower = reader.ReadSingle();
                // do no care what comes after this

            }
            catch (Exception ex)
            {
                logFile.WriteLog("Error BGSM " + fileName + " " + ex.Message);
                logFile.Close();
                System.Environment.Exit(501);
            }
            reader.Close();
        }
    }
}

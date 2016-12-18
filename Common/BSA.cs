using System;
using System.Collections.Generic;
using BSAList = System.Collections.Generic.List<LODGenerator.Common.BSAArchive>;
using StringList = System.Collections.Generic.List<string>;
using System.IO;
using System.IO.Compression;
using System.Globalization;

namespace LODGenerator.Common
{
    class BSAArchive
    {
        private string name;
        private bool defaultCompressed;
        private bool defaultFlag9;
        private static readonly BSAList LoadedArchives = new BSAList();
        private static readonly Dictionary<string, BSAArchiveFileInfo> FileList = new Dictionary<string, BSAArchiveFileInfo>();

        public struct BSAArchiveFileInfo
        {
            public readonly BSAArchive bsa;
            public readonly UInt64 offset;
            public readonly uint size;
            public readonly uint zsize;
            public readonly bool compressed;

            public BSAArchiveFileInfo(BSAArchive _bsa, UInt64 _offset, uint _size, uint _zsize)
            {
                bsa = _bsa;
                offset = _offset;
                size = _size;
                zsize = _zsize;

                if (_zsize == 0)
                {
                    if ((size & (1 << 30)) != 0)
                    {
                        size ^= 1 << 30;
                        compressed = !bsa.defaultCompressed;
                    }
                    else
                    {
                        compressed = bsa.defaultCompressed;
                    }
                }
                else
                {
                    if (_size != _zsize)
                    {
                        compressed = true;
                    }
                    else
                    {
                        compressed = false;
                    }
                }
            }

            public byte[] GetData()
            {
                FileInfo file = new FileInfo(bsa.name);
                BinaryReader binary = new BinaryReader(file.Open(FileMode.Open, FileAccess.Read, FileShare.ReadWrite), System.Text.Encoding.Default);
                binary.BaseStream.Seek((Int64)offset, SeekOrigin.Begin);
                byte start = 0;
                if (bsa.defaultFlag9)
                {
                    start = binary.ReadByte();
                    string str = new string(binary.ReadChars(start));
                }
                if (compressed)
                {
                    if (zsize != 0)
                    {
                        byte[] b = new byte[size - start];
                        byte[] output = new byte[zsize];
                        binary.Read(b, 0, (int)size - start);
                        ICSharpCode.SharpZipLib.Zip.Compression.Inflater inf = new ICSharpCode.SharpZipLib.Zip.Compression.Inflater();
                        inf.SetInput(b, 0, b.Length);
                        inf.Inflate(output);
                        binary.Close();
                        return output;
                    }
                    else
                    {
                        byte[] b = new byte[size - 4 - start];
                        byte[] output = new byte[binary.ReadUInt32()];
                        binary.Read(b, 0, (int)size - 4 - start);
                        ICSharpCode.SharpZipLib.Zip.Compression.Inflater inf = new ICSharpCode.SharpZipLib.Zip.Compression.Inflater();
                        inf.SetInput(b, 0, b.Length);
                        inf.Inflate(output);
                        binary.Close();
                        return output;
                    }
                }
                else
                {
                    byte[] output = binary.ReadBytes((int)size);
                    binary.Close();
                    return output;
                }
            }
        }

        private struct BSAHeader
        {
            public readonly uint bsaVersion;
            // .bsa = BSA, .ba2 = GNRL or DX10
            public readonly string bsaType;
            public readonly uint directorySize;
            public readonly uint archiveFlags;
            public readonly uint folderCount;
            public readonly uint fileCount;
            public readonly UInt64 nameTableOffset;
            public readonly uint totalFolderNameLength;
            public readonly uint totalFileNameLength;
            public readonly uint fileFlags;

            public BSAHeader(BinaryReader binary)
            {
                string id = "";
                bsaType = "";

                for (int index = 0; index < 4; index++)
                {
                    byte b = binary.ReadByte();
                    if (b != 0)
                    {
                        id += (object)(char)b;
                    }
                }

                bsaVersion = binary.ReadUInt32();

                if (id == "BTDX")
                {
                    for (int index = 0; index < 4; index++)
                    {
                        bsaType += (object)(char)binary.ReadByte();
                    }
                    directorySize = 0;
                    archiveFlags = 0;
                    folderCount = 0;
                    fileCount = binary.ReadUInt32();
                    nameTableOffset = binary.ReadUInt64();
                    totalFolderNameLength = 0;
                    totalFileNameLength = 0;
                    fileFlags = 0;
                }
                else
                {
                    bsaType = "BSA";
                    directorySize = binary.ReadUInt32();
                    archiveFlags = binary.ReadUInt32();
                    folderCount = binary.ReadUInt32();
                    fileCount = binary.ReadUInt32();
                    nameTableOffset = 0;
                    totalFolderNameLength = binary.ReadUInt32();
                    totalFileNameLength = binary.ReadUInt32();
                    fileFlags = binary.ReadUInt32();
                }
            }
        }

        private struct BSAFolderInfo
        {
            public string path;
            public readonly ulong hash;
            public readonly uint count;
            public uint offset;
            public BSAFolderInfo(BinaryReader binary)
            {
                path = null;
                offset = 0;
                hash = binary.ReadUInt64();
                count = binary.ReadUInt32();
                binary.BaseStream.Position += 4;
            }
        }

        private struct BSAFileInfo
        {
            public string path;
            public readonly ulong hash;
            public readonly uint size;
            public readonly uint offset;

            public BSAFileInfo(BinaryReader binary, bool iscompressed)
            {
                path = null;
                hash = binary.ReadUInt64();
                size = binary.ReadUInt32();
                offset = binary.ReadUInt32();
            }
        }

        private struct BA2FileInfo
        {
            public string path;
            public readonly uint hash;
            public readonly string ext;
            public readonly uint dhash;
            public readonly uint flags;
            public readonly UInt64 offset;
            public readonly uint zsize;
            public readonly uint size;
            public readonly uint unk20;
            public readonly bool compressed;

            public BA2FileInfo(BinaryReader binary)
            {
                path = null;
                hash = binary.ReadUInt32();
                ext = "";
                for (int index = 0; index < 4; index++)
                {
                    ext = ext + (char)binary.ReadByte();
                }
                ext = ext.ToLower(CultureInfo.InvariantCulture).Replace("\0", string.Empty);
                dhash = binary.ReadUInt32();
                flags = binary.ReadUInt32();
                offset = binary.ReadUInt64();
                zsize = binary.ReadUInt32();
                size = binary.ReadUInt32();
                unk20 = binary.ReadUInt32();
                if (size != zsize)
                {
                    compressed = true;
                }
                else
                {
                    compressed = false;
                }
            }
        }

        private BSAArchive(string archivepath, LogFile theLog, bool verbose)
        {
            if (!File.Exists(archivepath))
            {
                theLog.WriteLog("Archive not found " + archivepath);
                return;
            }
            if (verbose)
            {
                theLog.WriteLog("Using " + archivepath);
            }
            BSAHeader header;
            name = archivepath.ToLower(CultureInfo.InvariantCulture);
            FileInfo file = new FileInfo(archivepath);
            BinaryReader binary = new BinaryReader(file.Open(FileMode.Open, FileAccess.Read, FileShare.ReadWrite), System.Text.Encoding.Default);
            header = new BSAHeader(binary);
            if (header.bsaType == "GNRL")
            {
                if (header.bsaVersion != 0x01)
                {
                    theLog.WriteLog("Unknown BA2 version " + archivepath);
                    binary.Close();
                    return;
                }
                binary.BaseStream.Seek(24, SeekOrigin.Begin);
                BA2FileInfo[] fileInfo = new BA2FileInfo[header.fileCount];
                for (int index = 0; index < header.fileCount; index++)
                {
                    fileInfo[index] = new BA2FileInfo(binary);
                }
                binary.BaseStream.Seek((Int64)header.nameTableOffset, SeekOrigin.Begin);
                for (int index = 0; index < header.fileCount; index++)
                {
                    UInt16 size = binary.ReadUInt16();
                    fileInfo[index].path = new string(binary.ReadChars(size));
                    BSAArchiveFileInfo bsaArchiveFileInfo = new BSAArchiveFileInfo(this, fileInfo[index].offset, fileInfo[index].zsize, fileInfo[index].size);
                    //theLog.WriteLog("name=" + this.name + " file=" + fileInfo[index].path + " ext=" + fileInfo[index].ext + "< off=" + fileInfo[index].offset + " zsize=" + fileInfo[index].zsize + " size=" + fileInfo[index].size +  " compressed=" + fileInfo[index].compressed);
                    FileList[fileInfo[index].path.ToLower(CultureInfo.InvariantCulture)] = bsaArchiveFileInfo;
                }
            }
            if (header.bsaType == "BSA")
            {
                if (header.bsaVersion != 0x67 && header.bsaVersion != 0x68)
                {
                    theLog.WriteLog("Unknown BSA version " + archivepath);
                    binary.Close();
                    return;
                }
                if ((header.archiveFlags & 0x4) > 0)
                {
                    defaultCompressed = true;
                }
                else
                {
                    defaultCompressed = false;
                }
                if ((header.archiveFlags & 0x100) > 0)
                {
                    defaultFlag9 = true;
                }
                else
                {
                    defaultFlag9 = false;
                }
                //theLog.WriteLog(name + " is compressed? " + defaultCompressed + " flags " + header.archiveFlags);
                BSAFolderInfo[] folderInfo = new BSAFolderInfo[header.folderCount];
                BSAFileInfo[] fileInfo = new BSAFileInfo[header.fileCount];
                for (int index = 0; index < header.folderCount; index++)
                {
                    folderInfo[index] = new BSAFolderInfo(binary);
                }
                uint count = 0;
                for (uint index = 0; index < header.folderCount; index++)
                {
                    byte b = binary.ReadByte();
                    if (b > 0)
                    {
                        folderInfo[index].path = new string(binary.ReadChars(b - 1));
                    }
                    else
                    {
                        folderInfo[index].path = "";
                    }
                    binary.BaseStream.Position++;
                    folderInfo[index].offset = count;
                    for (int index2 = 0; index2 < folderInfo[index].count; index2++)
                    {
                        fileInfo[count + index2] = new BSAFileInfo(binary, defaultCompressed);
                    }
                    count += folderInfo[index].count;
                }
                for (uint index = 0; index < header.fileCount; index++)
                {
                    fileInfo[index].path = "";
                    char c;
                    while ((c = binary.ReadChar()) != '\0') fileInfo[index].path += c;
                }
                binary.Close();

                for (int index = 0; index < header.folderCount; index++)
                {
                    for (int index2 = 0; index2 < folderInfo[index].count; index2++)
                    {
                        BSAFileInfo bsaFileInfo = fileInfo[folderInfo[index].offset + index2];
                        BSAArchiveFileInfo bsaArchiveFileInfo = new BSAArchiveFileInfo(this, bsaFileInfo.offset, bsaFileInfo.size, 0);
                        string filepath = Path.Combine(folderInfo[index].path, bsaFileInfo.path);
                        //theLog.WriteLog(archivepath + " file = " + filepath + " off = " + bsaFileInfo.offset + " size = " + bsaFileInfo.size + " compressed = " + bsaArchiveFileInfo.compressed);
                        FileList[filepath.ToLower(CultureInfo.InvariantCulture)] = bsaArchiveFileInfo;
                    }
                }
            }
            LoadedArchives.Add(this);
        }

        public static void Load(StringList files, LogFile theLog, bool verbose)
        {
            foreach (string s in files)
            {
                new BSAArchive(s, theLog, verbose);
            }
        }

        public static bool FileExists(string path)
        {
            return FileList.ContainsKey(path.ToLower(CultureInfo.InvariantCulture));
        }

        public static byte[] GetFile(string path)
        {
            return FileList[path.ToLower(CultureInfo.InvariantCulture)].GetData();
        }
    }
}
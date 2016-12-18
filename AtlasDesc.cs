using System;
using System.Collections.Generic;
using LODGenerator.Common;

namespace LODGenerator
{
    public struct AtlasDesc
    {
        public string SourceTexture;
        public string AtlasTexture;
        public string AtlasTextureN;
        public string AtlasTextureS;
        public float posU;
        public float posV;
        public float scaleU;
        public float scaleV;
        public float minU;
        public float maxU;
        public float minV;
        public float maxV;
        public UInt64 before;
        public UInt64 after;
        public float averageU;
        public float averageV;
    }

    static class AtlasList
    {
        static Dictionary<string, AtlasDesc> _list;

        static AtlasList()
        {
            _list = new Dictionary<string, AtlasDesc>();
        }

        public static void Set(string key, AtlasDesc value)
        {
            _list.Add(key, value);
        }

        public static AtlasDesc Get(string key)
        {
            return _list[key];
        }

        public static bool Contains(string key)
        {
            return _list.ContainsKey(key);
        }

        public static void BeforeAdd(string key, int value)
        {
            AtlasDesc at = _list[key];
            at.before += (uint)value;
            _list[key] = at;
        }

        public static void AfterAdd(string key, int value)
        {
            AtlasDesc at = _list[key];
            at.after += (uint)value;
            _list[key] = at;
        }

        public static float GetAverageU(string key)
        {
            return _list[key].averageU;
        }

        public static void SetAverageU(string key, float value)
        {
            AtlasDesc at = _list[key];
            at.averageU = value;
            _list[key] = at;
        }

        public static float GetAverageV(string key)
        {
            return _list[key].averageV;
        }

        public static void SetAverageV(string key, float value)
        {
            AtlasDesc at = _list[key];
            at.averageV = value;
            _list[key] = at;
        }

        public static void WriteStats(LogFile logFile)
        {
            foreach (KeyValuePair<string, AtlasDesc> keyValuePair in _list)
            {
                if (keyValuePair.Value.before != 0 && keyValuePair.Value.before * 1.25 < keyValuePair.Value.after || keyValuePair.Value.averageU > 100 || keyValuePair.Value.averageV > 100)
                {
                    string texture = keyValuePair.Key.Replace("textures\\", "").Replace("lowres\\", "");
                    logFile.WriteLog(texture + ": " + keyValuePair.Value.before + " -> " + keyValuePair.Value.after + " | " + keyValuePair.Value.averageU + ", " +keyValuePair.Value.averageV);
                }
            }
        }
    }
}
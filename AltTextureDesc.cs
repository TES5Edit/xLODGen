using System;
using System.Collections.Generic;

namespace LODGenerator
{
    public class AltTextureDesc
    {
        public string[] textures;

        public AltTextureDesc()
        {
            this.textures = new string[9];
        }
    }

    static class AltTextureList
    {
        static Dictionary<string, AltTextureDesc> _list;

        static AltTextureList()
        {
            _list = new Dictionary<string, AltTextureDesc>();
        }

        public static void Set(string key, AltTextureDesc value)
        {
            _list.Add(key, value);
        }

        public static AltTextureDesc Get(string key)
        {
            return _list[key];
        }

        public static bool Contains(string key)
        {
            return _list.ContainsKey(key);
        }
    }
}
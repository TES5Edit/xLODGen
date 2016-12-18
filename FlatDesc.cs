using LODGenerator.Common;
using System;
using System.Collections.Generic;

namespace LODGenerator
{
    public struct FlatDesc
    {
        public string SourceTexture;
        public float width;
        public float height;
        public float shiftZ;
        public float scale;
        public float effect1;
        public List<Vector3> normals;
        public List<Vector3> tangents;
        public List<Vector3> bitangents;
    }

    static class FlatList
    {
        static Dictionary<string, FlatDesc> _list;

        static FlatList()
        {
            _list = new Dictionary<string, FlatDesc>();
        }

        public static void Set(string key, FlatDesc value)
        {
            _list.Add(key, value);
        }

        public static FlatDesc Get(string key)
        {
            return _list[key];
        }

        public static bool Contains(string key)
        {
            return _list.ContainsKey(key);
        }
    }
}
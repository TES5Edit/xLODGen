using System;
using System.Collections.Generic;
using System.Globalization;

namespace LODGenerator.Common
{
    public static class CmdArgs
    {
        public static CultureInfo ci = new CultureInfo("en-US");

        public static string GetString(Dictionary<string, string> args, string key, string defValue = "")
        {
            string result = defValue;
            if (!args.ContainsKey(key.ToLower(CultureInfo.InvariantCulture)))
                return defValue;
            else
                return args[key.ToLower(CultureInfo.InvariantCulture)];
        }

        public static bool GetBool(Dictionary<string, string> args, string key, bool defValue = false)
        {
            if (!args.ContainsKey(key.ToLower(CultureInfo.InvariantCulture)))
                return defValue;
            else
                return true;
        }

        public static float GetFloat(Dictionary<string, string> args, string key, float defValue = 0.0f)
        {
            float result = defValue;
            if (!args.ContainsKey(key.ToLower(CultureInfo.InvariantCulture)) || !float.TryParse(args[key.ToLower(CultureInfo.InvariantCulture)], NumberStyles.Any, (IFormatProvider)CmdArgs.ci, out result))
                return defValue;
            else
                return result;
        }

        public static int GetInt(Dictionary<string, string> args, string key, int defValue = -1)
        {
            int result = defValue;
            if (!args.ContainsKey(key.ToLower(CultureInfo.InvariantCulture)) || !int.TryParse(args[key.ToLower(CultureInfo.InvariantCulture)], NumberStyles.Integer, (IFormatProvider)CmdArgs.ci, out result))
                return defValue;
            else
                return result;
        }
    }
}

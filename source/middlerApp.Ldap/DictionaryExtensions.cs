using System;
using System.Collections.Generic;

namespace MiddlerApp.Ldap
{
    public static class DictionaryExtensions
    {
        public static object GetValue(this Dictionary<string, object> dict, string key)
        {
            return dict.TryGetValue(key, out var value) ? value : null;
        }

        public static string GetStringValue(this Dictionary<string, object> dict, string key)
        {
            return dict.TryGetValue(key, out var value) ? value as string : null;
        }

        public static int? GetIntegerValue(this Dictionary<string, object> dict, string key)
        {
            var val = GetStringValue(dict, key);
            var numb = 0;
            if (Int32.TryParse(val, out numb))
            {
                return numb;
            }

            return null;
        }

        public static string[] GetStringArrayValue(this Dictionary<string, object> dict, string key)
        {
            return dict.TryGetValue(key, out var value) ? (value as string[]) : null;
        }

        public static bool GetBooleaValue(this Dictionary<string, object> dict, string key)
        {
            var val = dict.TryGetValue(key, out var value) ? value as string : null;
            bool.TryParse(val, out var result);

            return result;
        }
    }
}

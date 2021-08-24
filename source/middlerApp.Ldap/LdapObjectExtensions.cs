using System;

namespace MiddlerApp.Ldap
{
    public static class LdapObjectExtensions
    {
        public static object GetValue(this LdapObject dict, string key)
        {
            return dict.TryGetValue(key, out var value) ? value : null;
        }

        public static string GetStringValue(this LdapObject dict, string key)
        {
            return dict.TryGetValue(key, out var value) ? value as string : null;
        }

        public static int? GetIntegerValue(this LdapObject dict, string key)
        {
            var val = GetStringValue(dict, key);
            var numb = 0;
            if (Int32.TryParse(val, out numb))
            {
                return numb;
            }

            return null;
        }

        public static string[] GetStringArrayValue(this LdapObject dict, string key)
        {
            return dict.TryGetValue(key, out var value) ? (value as string[]) : null;
        }

        public static bool GetBoolenValue(this LdapObject dict, string key)
        {
            var val = dict.TryGetValue(key, out var value) ? value as string : null;
            bool.TryParse(val, out var result);

            return result;
        }
    }
}

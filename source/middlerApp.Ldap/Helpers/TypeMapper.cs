using System;
using System.Collections.Generic;
using System.DirectoryServices.Protocols;
using System.Globalization;
using System.Linq;
using System.Text;
using doob.Reflectensions.Common;

namespace MiddlerApp.Ldap.Helpers
{
    public static class TypeMapper
    {
        private static Dictionary<string, Func<DirectoryAttribute, object>> AttributeToProperty =
            new Dictionary<string, Func<DirectoryAttribute, object>>(StringComparer.CurrentCultureIgnoreCase)
            {
                {"samAccountName", ToStringValue},
                {"memberof", ToStringArrayValue},
                {"member", ToStringArrayValue},
                {"objectguid", (attr) => ToGuidValue(attr)},
                {"objectsid", (attr) => DecodeSID(attr)},
                {"whenCreated", (attr) => FromGeneralizeDateTime(attr)},
                {"whenChanged", (attr) => FromGeneralizeDateTime(attr)},
                {"lastlogon", (attr) => FromTicks(attr)},
                {"lastLogonTimestamp", (attr) => FromTicks(attr)},
                {"pwdLastSet", (attr) => FromTicks(attr)},
                {"msTsExpireDate", (attr) => FromGeneralizeDateTime(attr)},
                {"msExchWhenMailboxCreated", (attr) => FromGeneralizeDateTime(attr)},
                {"dSCorePropagationData", (attr) => FromGeneralizeDateTime(attr)},
                {"badPasswordTime", (attr) => FromTicks(attr)},
                {"accountExpires", (attr) => FromTicks(attr)},
                {"objectclass", (attr) => ToStringArrayValue(attr)},
                {"oMObjectClass", (attribute => ToReadableBytesAsHex(attribute))},
                {"thumbnailPhoto", (attribute => ToByteArray(attribute))},
                {"userAccountControl", (attribute => ToUserAccountControl(attribute))},
                {"groupType", (attribute => ToGroupType(attribute))}

            };

        private static GroupType? ToGroupType(DirectoryAttribute attribute)
        {
            if (attribute == null)
                return null;

            var ui = ToLongValue(attribute);
            if (ui.HasValue)
            {
                var en = (GroupType)Enum.ToObject(typeof(GroupType), ui);
                //var enu = en.GetName().Split(",").Select(v => v.TrimToNull()).Where(v => v != null);

                return en;
            }

            return null;
        }


        public static object GetAttributeValue(DirectoryAttribute attribute)
        {
            if (attribute == null)
                return null;

            if (AttributeToProperty.ContainsKey(attribute.Name))
            {
                return AttributeToProperty[attribute.Name](attribute);
            }

            if (attribute.Count > 1)
            {
                return ToStringArrayValue(attribute);
            }
            return ToStringValue(attribute);
        }

        private static IEnumerable<T> GetEnumerableValue<T>(DirectoryAttribute attribute)
        {
            for (int ic = 0; ic < attribute.Count; ic++)
            {
                //T val = default;

                if (attribute[ic] is T t)
                {
                    yield return t;
                }
                else
                {
                    yield return default;
                }

                //try
                //{
                    
                    
                //    //Attribute Sub Value below
                //    val = (T)attribute[ic];
                //}
                //catch (Exception e)
                //{
                    
                //}

                //yield return val;


            }
        }

        private static IEnumerable<object> GetValue(DirectoryAttribute attribute)
        {
            return GetEnumerableValue<object>(attribute);
        }

        public static string ToStringValue(DirectoryAttribute attribute)
        {
            return attribute[0] as string;
        }

        public static int ToIntValue(DirectoryAttribute attribute)
        {
            return ToStringValue(attribute).ToInt();
        }

        public static long? ToLongValue(DirectoryAttribute attribute)
        {
            var str = ToStringValue(attribute);

            return str.ToNullableLong();
        }

        public static string[] ToStringArrayValue(DirectoryAttribute attribute)
        {
            return GetEnumerableValue<string>(attribute).ToArray();
            //return attribute.Value as string[];
        }

        public static Guid? ToGuidValue(DirectoryAttribute attribute)
        {
            var byteArray = GetEnumerableValue<byte[]>(attribute).ToList();
            if (!byteArray.Any())
                return null;


            var val = byteArray.First();
            return new Guid(val);
        }

        public static byte[] ToByteArray(DirectoryAttribute attribute, string format = "X2")
        {
            return GetEnumerableValue<byte[]>(attribute).FirstOrDefault();
            
        }
        public static string ToReadableBytesAsHex(DirectoryAttribute attribute, string format = "X2")
        {
            var byteArray = GetEnumerableValue<byte[]>(attribute).ToList();
            if (!byteArray.Any())
                return null;


            var val = byteArray.First();
            var res = val?.Select(b => $"0x{b.ToString(format)}") ?? new List<string>();
            return String.Join(" ", res.ToArray());
        }

        public static DateTime? FromGeneralizeDateTime(DirectoryAttribute attribute)
        {
            string format = "yyyyMMddHHmmss.f'Z'";
            return DateTime.ParseExact(ToStringValue(attribute), format, CultureInfo.InvariantCulture);
        }

        public static DateTime? FromTicks(DirectoryAttribute attribute)
        {
            var val = ToStringValue(attribute);
            if (String.IsNullOrEmpty(val))
                return null;

            long l;
            if (long.TryParse(val, out l))
            {
                if (!l.Equals(Int64.MaxValue))
                    return DateTime.FromFileTimeUtc(l);
            }

            return DateTime.MaxValue;


        }

        public static String DecodeSID(DirectoryAttribute attribute)
        {
            var byteArray = GetEnumerableValue<byte[]>(attribute).ToList();
            if (!byteArray.Any())
                return null;


            var sid = byteArray.First();

            if (sid == null)
                return null;

            StringBuilder strSid = new StringBuilder("S-");
            //  get byte(0) - revision level
            int revision = sid[0];
            strSid.Append(revision);
            // next byte byte(1) - count of sub-authorities
            int countSubAuths = (sid[1] & 255);
            // byte(2-7) - 48 bit authority ([Big-Endian])
            long authority = 0;
            // String rid = "";
            for (int i = 2; (i <= 7); i++)
            {
                authority = (authority
                             | (((long)(sid[i])) + (8 * (5
                                                         - (i - 2)))));
            }

            strSid.Append("-");
            strSid.Append(authority.ToString("x4"));
            // iterate all the sub-auths and then countSubAuths x 32 bit sub authorities ([Little-Endian])
            int offset = 8;
            int size = 4;
            // 4 bytes for each sub auth
            for (int j = 0; (j < countSubAuths); j++)
            {
                long subAuthority = 0;
                for (int k = 0; (k < size); k++)
                {
                    subAuthority = (subAuthority
                                    | (((long)((sid[(offset + k)] & 255))) + (8 * k)));
                }

                //  format it
                strSid.Append("-");
                strSid.Append(subAuthority);
                offset = (offset + size);
            }

            return strSid.ToString();
        }

        private static UserAccountControl ToUserAccountControl(DirectoryAttribute attribute)
        {
            return (UserAccountControl) ToIntValue(attribute);
        }
    }
}

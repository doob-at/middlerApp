using System;
using doob.Reflectensions.Common;
using doob.Reflectensions.ExtensionMethods;

namespace MiddlerApp.Ldap
{
    public class AttributeValueConverter
    {

        public object TryConvert(AttributeInfo attributeInfo, object value)
        {
            switch (attributeInfo.LDAPSyntax)
            {
                case "Integer":
                    {
                        return ConvertTo_Integer(value);
                    }
                case "LargeInteger":
                    {
                        return ConvertTo_LargeInteger(value);
                    }
                case "Boolean":
                    {
                        return ConvertToBoolean(value);
                    }
                case "String(Octet)":
                {
                    return ConvertTo_String_Octet(value);
                }
                case "String(Unicode)":
                {
                    return value.ToString();
                }
            }

            return value;
        }


        private string ConvertToBoolean(object value)
        {
            return value.Reflect().ToBoolean().ToString().ToUpper();
        }
        

        private string ConvertTo_LargeInteger(object value)
        {
            
            if (value is string str)
            {
                var v = str.ToNullableDateTime();
               
                if (v.HasValue)
                    return v.Value.ToFileTime().ToString();
            }

            if (value is DateTime dt)
            {
                return dt.ToFileTime().ToString();
            }


            return value.ToString();
        }
       



        private string ConvertTo_Integer(object value)
        {
            if (value is UserAccountControl en)
            {
                return ((int)en).ToString();
            }

            if (value.ToString().IsInt())
            {
                return value.ToString();
            }
            
            return null;
        }


        private byte[] ConvertTo_String_Octet(object value)
        {

            if (value is byte[] _bytes)
            {
                return _bytes;
            }

            if (value is string str)
            {
                return Convert.FromBase64String(value.ToString());
            }

            return null;
        }
    }
}

using System.Collections.Generic;
using System.Linq;

namespace MiddlerApp.Ldap
{
    public class SchemaHelper
    {
        private static List<AttributeType> attributeTypes = new List<AttributeType>()
        {
            new AttributeType("Boolean","2.5.5.8","1"),
            new AttributeType("Enumeration","2.5.5.9","10"),
            new AttributeType("Integer","2.5.5.9","2"),
            new AttributeType("LargeInteger","2.5.5.16","65"),
            new AttributeType("Object(Access-Point)","2.5.5.14","127","0x2B 0x0C 0x02 0x87 0x73 0x1C 0x00 0x85 0x3E"),
            new AttributeType("Object(DN-String)","2.5.5.14","127","0x2A 0x86 0x48 0x86 0xF7 0x14 0x01 0x01 0x01 0x0C"),
            new AttributeType("Object(OR-Name)","2.5.5.7","127","0x56 0x06 0x01 0x02 0x05 0x0B 0x1D"),
            new AttributeType("Object(DN-Binary)","2.5.5.7","127","0x2A 0x86 0x48 0x86 0xF7 0x14 0x01 0x01 0x01 0x0B"),
            new AttributeType("Object(DS-DN)","2.5.5.1","127","0x2B 0x0C 0x02 0x87 0x73 0x1C 0x00 0x85 0x4A"),
            new AttributeType("Object(Presentation-Address)","2.5.5.13","127","0x2B 0x0C 0x02 0x87 0x73 0x1C 0x00 0x85 0x5C"),
            new AttributeType("Object(Replica-Link)","2.5.5.10","127","0x2A 0x86 0x48 0x86 0xF7 0x14 0x01 0x01 0x01 0x06"),
            new AttributeType("String(Case)","2.5.5.3","27"),
            new AttributeType("String(IA5)","2.5.5.5","22"),
            new AttributeType("String(NT-Sec-Desc)","2.5.5.15","66"),
            new AttributeType("String(Numeric)","2.5.5.6","18","-"),
            new AttributeType("String(Object-Identifier)","2.5.5.2","6"),
            new AttributeType("String(Octet)","2.5.5.10","4"),
            new AttributeType("String(Printable)","2.5.5.5","19"),
            new AttributeType("String(Sid)","2.5.5.17","4"),
            new AttributeType("String(Teletex)","2.5.5.4","20"),
            new AttributeType("String(Unicode)","2.5.5.12","64"),
            new AttributeType("String(UTC-Time)","2.5.5.11","23"),
            new AttributeType("String(Generalized-Time)","2.5.5.11","24"),

        };


        public static AttributeType GetAttributeType(string attributeSyntax, string oMSyntax, string omObjectClass)
        {
            return attributeTypes.FirstOrDefault(a =>
                a.AttributeSyntax == attributeSyntax && a.OMSyntax == oMSyntax && a.OMObjectClass == omObjectClass);
        }
    }
}

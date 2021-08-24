using System.Collections.Generic;
using doob.Reflectensions;

namespace MiddlerApp.Ldap
{
    public class LdapObject: ExpandableObject
    {
        public string DistinguishedName { get; set; }

        public string[] ObjectClass { get; set; }


        public LdapObject(Dictionary<string, object> properties): base(properties)
        {
            
        }


      
    }
}

using System;
using System.DirectoryServices.Protocols;
using MiddlerApp.Ldap.Helpers;

namespace MiddlerApp.Ldap
{
    public class ConcreteLdapAttribute
    {

        public Func<DirectoryAttribute, object> FromLdap { get; private set; }

        //public Action<LdapAttribute, object> ToLdap { get; private set; }
        
        public ConcreteLdapAttribute MapFromLdap(Func<DirectoryAttribute, object> fromLdap)
        {
            FromLdap = fromLdap;
            return this;
        }

        //public ConcreteLdapAttribute MapToLdap(Action<LdapAttribute, object> toLdap)
        //{
        //    ToLdap = toLdap;
        //    return this;
        //}

        public object GetFromLdap(DirectoryAttribute ldapAttribute)
        {
            if (FromLdap != null)
            {
                return FromLdap(ldapAttribute);
            }

            return TypeMapper.ToStringValue(ldapAttribute);
        }

        
    }
}

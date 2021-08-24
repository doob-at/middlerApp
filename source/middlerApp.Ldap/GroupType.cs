namespace MiddlerApp.Ldap
{
    public enum GroupType : long
    {
        BuiltIn = 1,
        Global = 2,
        DomainLocal = 4,
        Universal = 8,
        Security = -2147483648,
        

        DomainLocalSecurity = DomainLocal | Security,
        GlobalSecurity = Global | Security,
        UniversalSecurity = Universal | Security
    }
}

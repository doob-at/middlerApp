namespace MiddlerApp.Ldap
{
    public class AttributeType
    {
        public string LDAPSyntaxName { get; set; }
        public string AttributeSyntax { get; set; }
        public string OMSyntax { get; set; }
        public string OMObjectClass { get; set; }

        public bool IsForwardLink { get; }

        public AttributeType(string ldapSyntaxName, string attributeSyntax, string oMSyntax, string omObjectClass = null)
        {
            LDAPSyntaxName = ldapSyntaxName;
            AttributeSyntax = attributeSyntax;
            OMSyntax = oMSyntax;
            OMObjectClass = omObjectClass;
            IsForwardLink = attributeSyntax == "2.5.5.1" || attributeSyntax == "2.5.5.7" ||
                            attributeSyntax == "2.5.5.14";
        }
    }
}

namespace MiddlerApp.Ldap
{
    public class AttributeInfo
    {
        public string Name { get; set; }
        public string LDAPSyntax { get; set; }
        public string Description { get; set; }
        public bool IsSingleValued { get; set; }
        public bool IsSystemOnly { get; set; }
        public int? LinkID { get; set; }
        public int? LinkedID
        {
            get
            {
                if (!LinkID.HasValue)
                    return null;

                if (IsForwardLink)
                {
                    return LinkID.Value + 1;
                }
                else
                {
                    return LinkID.Value - 1;
                }   


            }
        }

        public bool IsForwardLink => LinkID.HasValue && LinkID.Value > 0 && (LinkID.Value % 2 == 0);
        public bool IsBackwardLink => LinkID.HasValue && LinkID.Value > 0 && (LinkID.Value % 2 != 0);

        
    }
}

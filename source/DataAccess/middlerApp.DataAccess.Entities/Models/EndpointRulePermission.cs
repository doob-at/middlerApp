using System;

namespace middlerApp.DataAccess.Entities.Models
{
    public class EndpointRulePermission
    {
        public Guid Id { get; set; }
        public decimal Order { get; set; }
        public string PrincipalName { get; set; }
        public string Type { get; set; }
        public string AccessMode { get; set; }
        public string Client { get; set; }
        public string SourceAddress { get; set; }
    }
}

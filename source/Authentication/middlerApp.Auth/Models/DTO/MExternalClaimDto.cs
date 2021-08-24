using System;

namespace middlerApp.Auth.Models.DTO
{
    public class MExternalClaimDto
    {
        public Guid Id { get; set; }

        public string Type { get; set; }

        public string Value { get; set; }

        public string Issuer { get; set; }
        
    }
}

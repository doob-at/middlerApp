using System;

namespace middlerApp.Auth.Models.DTO
{
    public class AuthenticationProviderListDto
    {
        public Guid Id { get; set; }

        public string Type { get; set; }
        public bool Enabled { get; set; }
        public string Name { get; set; }
        public string DisplayName { get; set; }
        public string Description { get; set; }
    }
}

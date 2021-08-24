using System;

namespace middlerApp.Auth.Entities
{
    public class AuthenticationProvider
    {
        public Guid Id { get; set; }

        public string Type { get; set; }

        public bool Enabled { get; set; }
        public string Name { get; set; }
        public string DisplayName { get; set; }
        public string Description { get; set; }

        public string? Parameters { get; set; }

    }
    
}

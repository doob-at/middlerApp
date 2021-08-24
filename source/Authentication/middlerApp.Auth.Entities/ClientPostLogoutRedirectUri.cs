using System;

namespace middlerApp.Auth.Entities
{
    public class ClientPostLogoutRedirectUri
    {
        public Guid Id { get; set; }
        public string PostLogoutRedirectUri { get; set; }

        public Guid ClientId { get; set; }
        public Client Client { get; set; }
    }
}

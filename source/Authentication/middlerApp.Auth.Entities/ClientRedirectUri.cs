using System;

namespace middlerApp.Auth.Entities
{
    public class ClientRedirectUri
    {
        public Guid Id { get; set; }
        public string RedirectUri { get; set; }

        public Guid ClientId { get; set; }
        public Client Client { get; set; }
    }
}

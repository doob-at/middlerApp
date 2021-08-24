using System.Collections.Generic;

namespace middlerApp.Auth
{
    public class IdpConfiguration
    {
        public List<string> AdminUIRedirectUris { get; set; } = new List<string>();
        public List<string> AdminUIPostLogoutUris { get; set; } = new List<string>();
    }
}

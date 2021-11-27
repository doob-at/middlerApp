using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace middlerApp.Auth.Models.DTO
{
    public class MClientDto
    {
        public Guid Id { get; set; }
        public string ClientId { get; set; }
        public string DisplayName { get; set; }

        public string Description { get; set; }

        public bool RequirePkce { get; set; }

        public List<string> RedirectUris { get; set; }
        public List<string> PostLogoutRedirectUris { get; set; }
    }
}

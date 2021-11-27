using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;

namespace middlerApp.Auth.Entities
{
    public class MRole: IdentityRole<Guid>
    {

        public string DisplayName { get; set; }
        public string Description { get; set; }

        public bool BuiltIn { get; set; }

        public ICollection<MUser> Users { get; set; } = new List<MUser>();
    }
}

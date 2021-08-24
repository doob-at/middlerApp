using System;
using System.Collections.Generic;

namespace middlerApp.Auth.Models.DTO
{
    public class MUserListDto
    {

        public Guid Id { get; set; }

        public string UserName { get; set; }

        public string Email { get; set; }
        public bool EmailConfirmed { get; set; }

        public string FirstName { get; set; }
        public string LastName { get; set; }

        public string PhoneNumber { get; set; }
        public bool PhoneNumberConfirmed { get; set; }

        public bool HasPassword { get; set; }

        public bool Active { get; set; }

        public ICollection<string> Logins { get; set; } = new List<string>();

    }
}

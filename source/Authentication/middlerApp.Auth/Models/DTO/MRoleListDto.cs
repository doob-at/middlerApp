using System;

namespace middlerApp.Auth.Models.DTO
{
    public class MRoleListDto
    {
        public Guid Id { get; set; }

        public string Name { get; set; }
        public string DisplayName { get; set; }
        public string Description { get; set; }

    }
}

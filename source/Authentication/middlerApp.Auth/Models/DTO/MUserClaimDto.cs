using System;
using System.ComponentModel.DataAnnotations;

namespace middlerApp.Auth.Models.DTO
{
    public class MUserClaimDto
    {
        public Guid? Id { get; set; }

        [MaxLength(250)]
        [Required]
        public string Type { get; set; }

        [MaxLength(250)]
        [Required]
        public string Value { get; set; }


        public string ConcurrencyStamp { get; set; }

    }
}

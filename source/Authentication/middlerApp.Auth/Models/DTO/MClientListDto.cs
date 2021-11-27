using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace middlerApp.Auth.Models.DTO
{
    public class MClientListDto
    {
        public Guid Id { get; set; }
        public string ClientId { get; set; }
        public string DisplayName { get; set; }

    }
}

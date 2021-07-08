using System.Collections.Generic;

namespace middlerApp.Api.Models
{
    public class LoggedInUser
    {
        public string DN { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string UserName { get; set; }
        public string DisplayName { get; set; }
        public List<string> Groups { get; set; } = new List<string>();

        public bool IsAdmin { get; set; }
        public bool IsMemberOf(string dn)
        {
            return Groups.Contains(dn);
        }
    }
}

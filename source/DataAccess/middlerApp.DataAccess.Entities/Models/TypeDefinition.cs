using System;

namespace middlerApp.DataAccess.Entities.Models
{
    public class TypeDefinition
    {
        public Guid? Id { get; set; }
        public string Module { get; set; }
        public string Content { get; set; }
    }
}

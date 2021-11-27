using System.Collections.Generic;
using JintTsDefinition.Definitions;

namespace JintTsDefinition
{
    public class NamespaceDefinition : IDefinition
    {
        public string Name { get; set; }
        public List<NamespaceDefinition> Namespaces { get; set; } = new List<NamespaceDefinition>();

        public List<TypeDefinition> Types { get; set; } = new List<TypeDefinition>();
        
    }
}
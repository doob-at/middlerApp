using JintTsDefinition.Definitions;

namespace JintTsDefinition
{
    public class EnumValueDefinition: IDefinition
    {
        public string Name { get; set; }

        public object Value { get; set; }
    }
}

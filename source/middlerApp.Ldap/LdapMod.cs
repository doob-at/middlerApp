using System.Collections.Generic;
using System.Linq;
using doob.Reflectensions.ExtensionMethods;

namespace MiddlerApp.Ldap
{
    public class LdapMod
    {

        public AttributeInfo AttributeInfo { get; }

        public object Value { get; internal set; }

        public bool ValueIsArray { get; }


        
        public LdapMod(AttributeInfo attributeInfo, object value)
        {
            AttributeInfo = attributeInfo;
            Value = value;
            ValueIsArray = !(value is byte[]) && value.GetType().IsEnumerableType();

        }

        public IEnumerable<T> ValueAsEnumerableOf<T>()
        {
            return ValueIsArray ? Value.Reflect().To<List<object>>().Select(v => v.Reflect().To<T>()) : new List<T>{Value.Reflect().To<T>()};
        }

        
        //public DirectoryAttributeModification BuildAttributeModification(DirectoryAttributeOperation operation)
        //{
        //    var mod = new DirectoryAttributeModification();
        //    mod.Name = AttributeInfo.Name;
        //    mod.Operation = operation;


        //}


        //private void AddModificationValue(DirectoryAttributeModification modification)
        //{
        //    switch (AttributeInfo.LDAPSyntax)
        //    {
        //        case "Boolean":
        //        {
        //            modification.Add(Value.ToString());
        //            break;
        //        }
        //        case "Enumeration":
        //        {

        //            break;
        //        }
                
        //    }
        //}

        
    }


   
}

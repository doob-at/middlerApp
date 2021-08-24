using System;
using System.Collections.Generic;

namespace MiddlerApp.Ldap
{
    public class AttributeInfoCollector
    {

        private Dictionary<string, List<AttributeInfo>> AttributeInfoPerClass = new Dictionary<string, List<AttributeInfo>>(StringComparer.OrdinalIgnoreCase);
        
        public AttributeInfoCollector()
        {
            
        }


        public void SetAttributeInfos(string objectClass, List<AttributeInfo> attributeInfos)
        {
            AttributeInfoPerClass[objectClass] = attributeInfos;
        }

        public List<AttributeInfo> Get(string objectClass)
        {
            return AttributeInfoPerClass.TryGetValue(objectClass, out var attributes) ? attributes : null;
        }
    }
}

using System;
using System.Collections;
using System.Collections.Generic;
using System.DirectoryServices.Protocols;
using System.Linq;
using doob.Reflectensions.Common.Helper;
using doob.Reflectensions.ExtensionMethods;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using TinyCsvParser;
using TinyCsvParser.Mapping;

namespace MiddlerApp.Ldap
{
    public class LdapObjectChangeBuilder
    {

        private Ldap _ldap;
        public string DistinguishedName { get; private set; }
        public Dictionary<string, AttributeInfo> AllowedAttributes { get; }
        
        public List<LdapMod> Mods = new List<LdapMod>();

        public List<LdapMod> BackwardLinkMods = new List<LdapMod>();

        public Dictionary<string, object> SpecialMods = new Dictionary<string, object>();

        public LdapObjectChangeBuilder(Ldap ldap, string distinguishedName, IEnumerable<AttributeInfo> allowedAttributes)
        {
            _ldap = ldap;
            DistinguishedName = distinguishedName;
            AllowedAttributes = allowedAttributes.ToDictionary(a => a.Name.ToLower(), a => a, StringComparer.OrdinalIgnoreCase);
            
            //_attributeNamesNormalized = allowedAttributes?.ToDictionary(at => at.Name.ToLower(), at => at.Name);
        }

        public LdapObjectChangeBuilder AddModification(string attributeName, object value)
        {
            if (AllowedAttributes.TryGetValue(attributeName, out var attr))
            {
                var mod = new LdapMod(attr, value);
                Mods.Add(mod);
            }
            else
            {
                SpecialMods.Add(attributeName, value);
            }


            return this;
        }

        public LdapObjectChangeBuilder AddModifications(Dictionary<string, object> modifications)
        {
            foreach (var kvp in modifications)
            {
                AddModification(kvp.Key, kvp.Value);
            }

            return this;
        }
        public LdapObjectChangeBuilder AddModifications(JObject modifications)
        {
            
            return AddModifications(Converter.Json.ToBasicDotNetDictionary(modifications));
        }

       


        public LdapChangeResult SendChangedModifications()
        {
            var ldapObject = _ldap.GetObjectByDn(DistinguishedName, Mods.Select(m => m.AttributeInfo.Name).ToArray());
            if (ldapObject == null)
            {
                throw new Exception("Ldap Object not found");
            }


            var result = new LdapChangeResult();
            var changes = new List<DirectoryAttributeModification>();

            //foreach (var ldapMod in Mods)
            //{
                
            //    if (ldapMod.Value.TryAs<string>(out var str))
            //    {
            //        ldapMod.Value = _ldap.CalculateJsonLdapAttributes(str).ToNull();

            //    }
            //    else if (ldapMod.ValueIsArray)
            //    {
            //        ldapMod.Value = ldapMod.ValueAsEnumerableOf<object>()
            //            .Select(e => e is string ? _ldap.CalculateJsonLdapAttributes(e.ToString()).ToNull() : e)
            //            .Where(s => s != null)
            //            .ToList();
                    
            //    }

            //}

            foreach (var mod in Mods.Where(m => !m.AttributeInfo.IsBackwardLink))
            {

                var oVal = ldapObject.ContainsKey(mod.AttributeInfo.Name)
                    ? ldapObject[mod.AttributeInfo.Name]
                    : null;
                var normalizedMod = NormalizeLdapMod(mod, oVal);

                if (ldapObject.ContainsKey(mod.AttributeInfo.Name))
                {
                    var nJson = JsonConvert.SerializeObject(normalizedMod.Value);
                    var oJson = JsonConvert.SerializeObject(oVal);
                    if (nJson != oJson)
                    {

                        

                        if (normalizedMod.Value == null)
                        {
                            changes.Add(CreateMod(normalizedMod.AttributeInfo, normalizedMod.Value, DirectoryAttributeOperation.Delete));
                        }
                        else
                        {
                            changes.Add(CreateMod(normalizedMod.AttributeInfo, normalizedMod.Value, DirectoryAttributeOperation.Replace));
                        }
                        
                    }
                }
                else
                {
                    if (normalizedMod.AttributeInfo.Name.Equals("ou", StringComparison.OrdinalIgnoreCase))
                    {
                        SendMoveRequest(normalizedMod.ValueAsEnumerableOf<string>().FirstOrDefault());
                    }

                    if (normalizedMod.Value != null)
                    {
                        changes.Add(CreateMod(normalizedMod.AttributeInfo, normalizedMod.Value, DirectoryAttributeOperation.Add));
                    }
                    
                }
            }

            foreach (var mod in Mods.Where(m => m.AttributeInfo.IsBackwardLink))
            {
                if (ldapObject.ContainsKey(mod.AttributeInfo.Name))
                {

                    var oVal = ldapObject[mod.AttributeInfo.Name];
                    var nJson = JsonConvert.SerializeObject(mod.Value);
                    var oJson = JsonConvert.SerializeObject(oVal);
                    if (nJson != oJson)
                    {
                        BackwardLinkMods.Add(mod);
                    }
                }
                else
                {
                    BackwardLinkMods.Add(mod);
                }

            }


            
            result.DirectoryResponse = SendChanges(changes);
            result.DistinguishedName = DistinguishedName;
            return result;
        }


        private LdapMod NormalizeLdapMod(LdapMod ldapMod, object oldValue)
        {

            switch (ldapMod.AttributeInfo.Name)
            {
                case "userAccountControl":
                {
                    
                    if (ldapMod.Value is string str)
                    {
                        if (str.StartsWith("-"))
                        {
                            var current = (UserAccountControl)oldValue;
                            str = str.Substring(1).Trim();
                            var uac = Enum<UserAccountControl>.Find(str);
                            ldapMod.Value = current &= ~uac;
                        } else if (str.StartsWith("+"))
                        {
                            var current = (UserAccountControl)oldValue;
                            str = str.Substring(1).Trim();
                            var uac = Enum<UserAccountControl>.Find(str);
                            ldapMod.Value = current |= uac;
                        }
                        else
                        {
                            ldapMod.Value = Enum<UserAccountControl>.Find(str.Trim());
                        }
                        
                    }

                    return ldapMod;
                }
                default:
                {
                    ldapMod.Value = new AttributeValueConverter().TryConvert(ldapMod.AttributeInfo, ldapMod.Value);
                    return ldapMod;
                }
            }
            
        }

        private DirectoryResponse SendChanges(List<DirectoryAttributeModification> changes)
        {

            var modifyRequest = new ModifyRequest();
            modifyRequest.DistinguishedName = DistinguishedName;


            foreach (var directoryAttributeModification in changes)
            {
                modifyRequest.Modifications.Add(directoryAttributeModification);
            }

            DirectoryResponse result = null;

            if (modifyRequest.Modifications.Count > 0)
            {
                result = _ldap.Connect().SendRequest(modifyRequest);
            }


            
            if (BackwardLinkMods?.Any() == true)
            {

                foreach (var forwardLinkMod in BackwardLinkMods)
                {

                    var backwardLinkAttribute =
                        _ldap.GetAttributeInfoByLinkID(forwardLinkMod.AttributeInfo.LinkedID.Value);
                    

                    foreach (var s in forwardLinkMod.ValueAsEnumerableOf<string>())
                    {
                        _ldap.SetBacklinkAttribute(s, DistinguishedName, backwardLinkAttribute.Name);
                    }
                    
                }
            }
            

            return result;
        }

        private void SendMoveRequest(string newOuDn, string newName = null)
        {

            string oldName = SplitDnParts(DistinguishedName).First();
            var _newName = SplitDnParts(DistinguishedName).First();
            if (!String.IsNullOrWhiteSpace(newName))
            {
                if (!newName.Contains("="))
                {
                    var pre = _newName.Split("=".ToCharArray())[0];
                    _newName = $"{pre}=\"{newName}\"";
                }
                else
                {
                    _newName = newName;
                }
                
            }

            if (DistinguishedName == _newName)
            {
                return;
            }

            var mr = new ModifyDNRequest();
            mr.DeleteOldRdn = true;
            mr.DistinguishedName = DistinguishedName;
            mr.NewName = _newName;
            mr.NewParentDistinguishedName = newOuDn;

            var result = _ldap.Connect().SendRequest(mr);
            if (result.ResultCode == ResultCode.Success)
            {
                DistinguishedName = $"{_newName},{newOuDn}";
            }
            
        }

        private List<string> SplitDnParts(string distinguishedName)
        {
            CsvParserOptions csvParserOptions = new CsvParserOptions(false, ',');
            CsvReaderOptions csvReaderOptions = new CsvReaderOptions(new[] { Environment.NewLine });
            CsvStringArrayMapping csvMapper = new CsvStringArrayMapping();
            CsvParser<string[]> csvParser = new CsvParser<string[]>(csvParserOptions, csvMapper);

            var values = csvParser
                .ReadFromString(csvReaderOptions, distinguishedName).FirstOrDefault()?.Result;

            return values.ToList();
        }

        private DirectoryAttributeModification CreateMod(AttributeInfo attribute, object value, DirectoryAttributeOperation operation)
        {
            var attributeMod = new DirectoryAttributeModification();
            attributeMod.Name = attribute.Name;
            attributeMod.Operation = operation;

            if (value == null)
            {
                return attributeMod;
            }
            value = new AttributeValueConverter().TryConvert(attribute, value); //NormalizeAttributeValue(attributeName, value);

            var isEnumerable = !(value is byte[]) && value.GetType().IsEnumerableType();

            if (isEnumerable)
            {
                var en = value as IEnumerable;
                foreach (var o in en)
                {
                    if (o is string strValue1)
                    {
                        attributeMod.Add(strValue1);
                    }
                    else if (o is byte[] strByte1)
                    {
                        attributeMod.Add(strByte1);
                    }
                }
            }
            else
            {
                if (value is string strValue)
                {
                    attributeMod.Add(strValue);
                }
                else if (value is byte[] strByte)
                {
                    attributeMod.Add(strByte);
                }
            }




            return attributeMod;
        }

        private object NormalizeAttributeValue(string attributeName, object value)
        {


            if (AllowedAttributes != null)
            {
                var attr = AllowedAttributes[attributeName];

                switch (attr.LDAPSyntax)
                {
                    case "String(Octet)":
                        {
                            if (value is string str)
                            {
                                return Convert.FromBase64String(value.ToString());
                            }
                            else
                            {
                                return value;
                            }

                        }
                    default:
                        {
                            return value.ToString();
                        }
                }
            }

            return null;
        }


    }

    public class LdapChangeResult
    {
        public string DistinguishedName { get; set; }
        public List<ChangedAttributeInformation> Modifications { get; set; } = new List<ChangedAttributeInformation>();
        public string ErrorMessage => DirectoryResponse?.ErrorMessage;

        public ResultCode ResultCode => DirectoryResponse?.ResultCode ?? ResultCode.Success;

        internal DirectoryResponse DirectoryResponse { get; set; }


    }
}

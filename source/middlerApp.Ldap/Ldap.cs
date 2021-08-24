using System;
using System.Collections;
using System.Collections.Generic;
using System.DirectoryServices.Protocols;
using System.Linq;
using System.Net;
using MiddlerApp.Ldap.ExtensionMethods;
using MiddlerApp.Ldap.Helpers;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Scriban;
using Scriban.Runtime;
using Scriban.Syntax;
using SearchScope = System.DirectoryServices.Protocols.SearchScope;

namespace MiddlerApp.Ldap
{
    public class Ldap
    {
        public LdapOptions Options = new LdapOptions();

        public LdapServer LastSuccessfullLdapServer { get; private set; }

        public Ldap(params LdapServer[] server)
        {
            Options.LdapServers.AddRange(server);
        }

        public Ldap(IEnumerable<LdapServer> server)
        {
            Options.LdapServers.AddRange(server);
        }

        public Ldap(params Action<LdapServerBuilder>[] server)
        {
            foreach (var ldapServerBuilder in server.Select(s => s.InvokeAction()))
            {
                Options.LdapServers.Add(ldapServerBuilder);
            }

        }


        public Ldap AddLdapServer(LdapServer server)
        {
            Options.LdapServers.Add(server);
            return this;
        }

        public Ldap AddLdapServer(Action<LdapServerBuilder> server)
        {
            return AddLdapServer(server.InvokeAction());
        }

        public Ldap SetBaseDN(string bindBaseDN)
        {
            Options.BindBaseDN = bindBaseDN;
            return this;
        }

        public Ldap SetMaxPageSize(int pageSize)
        {
            Options.SearchPageSize = pageSize;
            return this;
        }

        public Ldap UseCredentials(string username, string password, AuthType? authType = null)
        {
            if (String.IsNullOrEmpty(username))
            {
                return this;
            }

            var cred = new NetworkCredential(username, password);
            return UseCredentials(cred, authType);
        }

        public Ldap UseCredentials(NetworkCredential credential, AuthType? authType = null)
        {
            Options.Credential = credential;
            Options.AuthType = authType;
            return this;

        }


        private LdapConnection _ldapConnection;
        private AttributeInfoCollector _attributeInfoCollector = new AttributeInfoCollector();


        internal LdapConnection Connect()
        {

            List<Exception> Errors = new List<Exception>();

            if (_ldapConnection == null)
            {
                if (LastSuccessfullLdapServer != null)
                {
                    try
                    {
                        _ldapConnection = LastSuccessfullLdapServer.Connect(Options);
                        return _ldapConnection;
                    }
                    catch (Exception e)
                    {
                        Errors.Add(e);
                    }
                }

                foreach (var ldapServer in Options.LdapServers.Where(s => s.Enabled))
                {
                    try
                    {
                        _ldapConnection = ldapServer.Connect(Options);
                        LastSuccessfullLdapServer = ldapServer;
                        break;
                    }
                    catch (Exception e)
                    {
                        Errors.Add(e);
                    }

                }
            }

            if (Errors.Any())
            {
                throw new Exception(String.Join(Environment.NewLine, Errors.Select(e => e.Message).ToArray()));
            }

            return _ldapConnection;
        }



        public Ldap UseObjectClassAttributeInfos(string objectClass, List<AttributeInfo> attributeInfos)
        {
            _attributeInfoCollector.SetAttributeInfos(objectClass, attributeInfos);
            return this;
        }

        public LdapObject GetObjectBySid(string sid, params string[] attributes)
        {
            
            return Search($"(objectsid={sid})", SearchScope.Subtree, attributes).FirstOrDefault();
        }

        public LdapObject GetObjectByDn(string distinguishedName, params string[] attributes)
        {
            return Search($"(distinguishedName={distinguishedName})", SearchScope.Subtree, attributes).FirstOrDefault();
        }

        public IEnumerable<LdapObject> Search(string searchFilter, params string[] attributes)
        {
            return Search(searchFilter, SearchScope.Subtree, attributes);
        }

        public IEnumerable<LdapObject> Search(string searchFilter, SearchScope scope, params string[] attributes)
        {
            var baseDN = !String.IsNullOrEmpty(Options.BindBaseDN) ? Options.BindBaseDN : GetDefaultNamingContext();
            return Search(baseDN, searchFilter, scope, attributes);
        }

        public IEnumerable<LdapObject> Search(string distinguishedName, string searchFilter, SearchScope searchScope, params string[] attributes)
        {
            var con = Connect();
            //var baseDN = !String.IsNullOrEmpty(Options.BindBaseDN) ? Options.BindBaseDN : GetDefaultNamingContext();

            if (attributes.Any())
            {
                var attrList = new List<string>()
                {
                    "distinguishedName",
                    "objectClass"
                };
                attrList.AddRange(attributes.Where(att => att != "distinguishedName"));
                attributes = attrList.ToArray();
            }

            List<SearchResponse> result = new List<SearchResponse>();
            SearchResponse response = null;
            int maxResultsToRequest = 1000;
            if (Options.SearchPageSize.HasValue && Options.SearchPageSize > 0)
            {
                maxResultsToRequest = Options.SearchPageSize.Value;
            }

            PageResultRequestControl pageRequestControl = new PageResultRequestControl(maxResultsToRequest);

            // used to retrieve the cookie to send for the subsequent request
            PageResultResponseControl pageResponseControl;
            SearchRequest searchRequest = new SearchRequest(distinguishedName, searchFilter, searchScope, attributes);
            searchRequest.Controls.Add(pageRequestControl);

            while (true)
            {
                response = (SearchResponse)con.SendRequest(searchRequest);
                SearchResultEntryCollection entries = response.Entries;
                for (int i = 0; i < entries.Count; i++)//Iterate through the results
                {
                    var dict = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);
                    SearchResultEntry entry = entries[i];
                    IDictionaryEnumerator attribEnum = entry.Attributes.GetEnumerator();
                    while (attribEnum.MoveNext())//Iterate through the result attributes
                    {
                        //Attributes have one or more values so we iterate through all the values 
                        //for each attribute
                        DirectoryAttribute subAttrib = (DirectoryAttribute)attribEnum.Value;

                        var val = TypeMapper.GetAttributeValue(subAttrib);
                        dict.Add(subAttrib.Name, val);
                    }

                    yield return new LdapObject(dict);
                }
                result.Add(response);
                pageResponseControl = (PageResultResponseControl)response.Controls[0];
                if (pageResponseControl.Cookie.Length == 0)
                    break;
                pageRequestControl.Cookie = pageResponseControl.Cookie;
            }


        }

        public string GetDefaultNamingContext()
        {
            var rootDse = GetRootDSE();
            return rootDse["defaultNamingContext"].ToString();
        }

        private Dictionary<string, object> _rootDse;
        public Dictionary<string, object> GetRootDSE()
        {
            if (_rootDse != null)
            {
                return _rootDse;
            }

            var con = Connect();

            SearchResponse dirRes = (SearchResponse)con.SendRequest(new SearchRequest(null, "(defaultNamingContext=*)",
                SearchScope.Base));


            var dict = new Dictionary<string, object>();
            SearchResultEntry entry = dirRes.Entries[0];
            IDictionaryEnumerator attribEnum = entry.Attributes.GetEnumerator();
            while (attribEnum.MoveNext())//Iterate through the result attributes
            {
                //Attributes have one or more values so we iterate through all the values 
                //for each attribute
                DirectoryAttribute subAttrib = (DirectoryAttribute)attribEnum.Value;

                var val = TypeMapper.GetAttributeValue(subAttrib);
                dict.Add(subAttrib.Name, val);
            }

            _rootDse = dict;
            return dict;
        }

        public Dictionary<string, AttributeInfo> GetObjectClassAttributes(string name)
        {
            var skipclasses = new List<string>();
            return GetObjectClassAttributes(name, ref skipclasses);
        }

        public Dictionary<string, AttributeInfo> GetObjectClassAttributes(params string[] names)
        {
            return GetObjectClassAttributes(names.ToList());
        }
        public Dictionary<string, AttributeInfo> GetObjectClassAttributes(IEnumerable<string> names)
        {
            var skipclasses = new List<string>();
            Dictionary<string, AttributeInfo> result = new Dictionary<string, AttributeInfo>();
            foreach (var name in names.Reverse())
            {
                var attributes = GetObjectClassAttributes(name, ref skipclasses);
                foreach (var kvp in attributes)
                {
                    result[kvp.Key] = kvp.Value;
                }
            }

            return result;
        }
        internal Dictionary<string, AttributeInfo> GetObjectClassAttributes(string name, ref List<string> skipClass)
        {
            Dictionary<string, AttributeInfo> result = new Dictionary<string, AttributeInfo>();

            if (skipClass == null)
            {
                skipClass = new List<string>();
            }

            if (skipClass.Contains(name))
            {
                return result;
            }
            skipClass.Add(name);


            var attributes = new List<string>
            {
                "subClassOf",
                "systemMayContain",
                "mayContain",
                "ldapDisplayName",
                "auxiliaryClass",
                "systemAuxiliaryClass"
            };
            //attributes = new List<string>();
            var rootDse = GetRootDSE();

            var schemaInfo = Search(rootDse["schemaNamingContext"].ToString(), $"(ldapDisplayName={name})", SearchScope.Subtree, attributes.ToArray()).FirstOrDefault();

            if (schemaInfo == null)
                return result;

            var parent = schemaInfo.GetStringValue("subClassOf");
            if (parent != name && !skipClass.Contains(parent))
            {
                foreach (var kv in GetObjectClassAttributes(parent, ref skipClass))
                {
                    if (!result.ContainsKey(kv.Key))
                    {
                        result.Add(kv.Key, kv.Value);
                    }
                    else
                    {
                        result[kv.Key] = kv.Value;
                    }
                }
            }


            var auxiliaryClass = schemaInfo.GetStringArrayValue("auxiliaryClass");
            if (auxiliaryClass != null)
            {
                foreach (var s in auxiliaryClass)
                {
                    if (s != name && !skipClass.Contains(s))
                    {
                        foreach (var kv in GetObjectClassAttributes(s, ref skipClass))
                        {
                            if (!result.ContainsKey(kv.Key))
                            {
                                result.Add(kv.Key, kv.Value);
                            }
                            else
                            {
                                result[kv.Key] = kv.Value;
                            }
                        }
                    }
                }
            }

            var systemAuxiliaryClass = schemaInfo.GetStringArrayValue("systemAuxiliaryClass");
            if (systemAuxiliaryClass != null)
            {
                foreach (var s in systemAuxiliaryClass)
                {
                    if (s != name && !skipClass.Contains(s))
                    {
                        foreach (var kv in GetObjectClassAttributes(s, ref skipClass))
                        {
                            if (!result.ContainsKey(kv.Key))
                            {
                                result.Add(kv.Key, kv.Value);
                            }
                            else
                            {
                                result[kv.Key] = kv.Value;
                            }
                        }
                    }
                }
            }



            var systemMayContain = schemaInfo.GetStringArrayValue("systemMayContain");
            var mayContain = schemaInfo.GetStringArrayValue("mayContain");

            if (systemMayContain != null)
            {
                foreach (var s in systemMayContain)
                {
                    var attribute = GetAttributeInfo(s);
                    if (!result.ContainsKey(s))
                    {
                        result.Add(s, attribute);
                    }
                    else
                    {
                        result[s] = attribute;
                    }
                }
            }

            if (mayContain != null)
            {
                foreach (var s in mayContain)
                {
                    var attribute = GetAttributeInfo(s);
                    if (!result.ContainsKey(s))
                    {
                        result.Add(s, attribute);
                    }
                    else
                    {
                        result[s] = attribute;
                    }
                }
            }

            return result;
        }

        private AttributeInfo GetAttributeInfo(string ldapDisplayName)
        {
            var attributes = new List<string>
            {
                "ldapDisplayName",
                "attributeSyntax",
                "oMSyntax",
                "oMObjectClass",
                "adminDescription",
                "isSingleValued",
                "systemOnly",
                "linkID"
            };

            var rootDse = GetRootDSE();
            var attr = Search(rootDse["schemaNamingContext"].ToString(), $"(ldapDisplayName={ldapDisplayName})",
                SearchScope.Subtree, attributes.ToArray())?.FirstOrDefault();

            string attributeSyntax = attr.GetStringValue("attributeSyntax");
            string oMSyntax = attr.GetStringValue("oMSyntax");
            string oMObjectClass = attr.GetStringValue("oMObjectClass");
            var linkId = attr.GetIntegerValue("linkID");

            var att = SchemaHelper.GetAttributeType(attributeSyntax, oMSyntax, oMObjectClass);

            return new AttributeInfo()
            {
                Name = ldapDisplayName,
                LDAPSyntax = att?.LDAPSyntaxName ?? "Unknown",
                Description = attr.GetStringValue("adminDescription"),
                IsSingleValued = attr.GetBoolenValue("isSingleValued"),
                IsSystemOnly = attr.GetBoolenValue("systemOnly"),
                LinkID = linkId
            };
        }



        public AttributeInfo GetAttributeInfoByLinkID(int linkID)
        {
            var attributes = new List<string>
            {
                "ldapDisplayName",
                "attributeSyntax",
                "oMSyntax",
                "oMObjectClass",
                "adminDescription",
                "isSingleValued",
                "systemOnly",
                "linkID"
            };

            var rootDse = GetRootDSE();
            var attr = Search(rootDse["schemaNamingContext"].ToString(), $"(linkID={linkID})",
                SearchScope.Subtree, attributes.ToArray())?.FirstOrDefault();

            string attributeSyntax = attr.GetStringValue("attributeSyntax");
            string oMSyntax = attr.GetStringValue("oMSyntax");
            string oMObjectClass = attr.GetStringValue("oMObjectClass");
            var linkId = attr.GetIntegerValue("linkID");
            var ldapDisplayName = attr.GetStringValue("ldapDisplayName");



            var att = SchemaHelper.GetAttributeType(attributeSyntax, oMSyntax, oMObjectClass);


            return new AttributeInfo()
            {
                Name = ldapDisplayName,
                LDAPSyntax = att?.LDAPSyntaxName ?? "Unknown",
                Description = attr.GetStringValue("adminDescription"),
                IsSingleValued = attr.GetBoolenValue("isSingleValued"),
                IsSystemOnly = attr.GetBoolenValue("systemOnly"),
                LinkID = linkID
            };
        }


        public bool Exists(string searchFilter, SearchScope scope = SearchScope.Subtree)
        {
            var found = Search(searchFilter, scope, "distinguishedName");
            return found.Any();
        }

        


        public LdapChangeResult ChangeObjectAttributes(string distinguishedName, string jsonAttributes)
        {
            return ChangeObjectAttributes(distinguishedName, Converter.Json.ToJObject(jsonAttributes));
        }
        public LdapChangeResult ChangeObjectAttributesForClass(string distinguishedName, string jsonAttributes, string objectClass)
        {
            return ChangeObjectAttributesForClass(distinguishedName, Converter.Json.ToJObject(jsonAttributes), objectClass);
        }
        public LdapChangeResult ChangeObjectAttributes(string distinguishedName, string jsonAttributes, string jsonAllowedAttributes)
        {
            return ChangeObjectAttributes(distinguishedName, Converter.Json.ToJObject(jsonAttributes), Converter.Json.ToObject<List<AttributeInfo>>(jsonAllowedAttributes));
        }
        public LdapChangeResult ChangeObjectAttributes(string distinguishedName, string jsonAttributes, IEnumerable<AttributeInfo> allowedAttributes)
        {
            return ChangeObjectAttributes(distinguishedName, Converter.Json.ToJObject(jsonAttributes), allowedAttributes);
        }


        public LdapChangeResult ChangeObjectAttributes(string distinguishedName, Dictionary<string, object> attributes)
        {
            return ChangeObjectAttributes(distinguishedName, Converter.Json.ToJObject(attributes));
        }
        public LdapChangeResult ChangeObjectAttributesForClass(string distinguishedName, Dictionary<string, object> attributes, string objectClass)
        {
            return ChangeObjectAttributesForClass(distinguishedName, Converter.Json.ToJObject(attributes), objectClass);
        }

        public LdapChangeResult ChangeObjectAttributes(string distinguishedName, Dictionary<string, object> attributes, string jsonAllowedAttributes)
        {
            return ChangeObjectAttributes(distinguishedName, Converter.Json.ToJObject(attributes), Converter.Json.ToObject<List<AttributeInfo>>(jsonAllowedAttributes));
        }

        public LdapChangeResult ChangeObjectAttributes(string distinguishedName, Dictionary<string, object> attributes, IEnumerable<AttributeInfo> allowedAttributes)
        {
            return ChangeObjectAttributes(distinguishedName, Converter.Json.ToJObject(attributes), allowedAttributes);
        }

        public LdapChangeResult ChangeObjectAttributes(string distinguishedName, JObject attributes)
        {
            var ldapObj = GetObjectByDn(distinguishedName, "objectClass");
            var allowedAttributes = GetObjectClassAttributes(ldapObj.ObjectClass);
            return ChangeObjectAttributes(distinguishedName, attributes, allowedAttributes.Values);
        }
        public LdapChangeResult ChangeObjectAttributesForClass(string distinguishedName, JObject attributes, string objectClass)
        {
            var allowedAttributes = GetObjectClassAttributes(objectClass).Values.ToList();
            return ChangeObjectAttributes(distinguishedName, attributes, allowedAttributes);
        }
        public LdapChangeResult ChangeObjectAttributes(string distinguishedName, JObject attributes, string jsonAllowedAttributes)
        {
            return ChangeObjectAttributes(distinguishedName, attributes, Converter.Json.ToObject<List<AttributeInfo>>(jsonAllowedAttributes));
        }
        public LdapChangeResult ChangeObjectAttributes(string distinguishedName, JObject attributes, IEnumerable<AttributeInfo> allowedAttributes)
        {

            var attributeNames = attributes.Properties().Select(p => p.Name).ToArray();
            var obj = GetObjectByDn(distinguishedName, attributeNames);
            if (obj == null)
            {
                throw new Exception("Ldap Object not found");
            }

            var changer = new LdapObjectChangeBuilder(this, distinguishedName, allowedAttributes).AddModifications(attributes);

            var result = changer.SendChangedModifications();

            var oldJson = JObject.Parse(JsonConvert.SerializeObject(obj));
            var nobj = GetObjectByDn(result.DistinguishedName, attributeNames);
            var newJson = JObject.Parse(JsonConvert.SerializeObject(nobj));


            foreach (var jProperty in newJson.Properties())
            {
                var n = jProperty.Name;
                if (oldJson.ContainsKey(n))
                {
                    if (newJson[n]?.ToString() != oldJson[n]?.ToString())
                    {
                        result.Modifications.Add(new ChangedAttributeInformation(n, ChangedAttributeType.Changed, nobj.GetValueOrDefault(n), obj.GetValueOrDefault(n)));
                    }
                }
                else
                {
                    result.Modifications.Add(new ChangedAttributeInformation(n, ChangedAttributeType.Added, nobj[n], null));
                }
            }

            foreach (var jProperty in oldJson.Properties())
            {
                var n = jProperty.Name;
                if (!newJson.ContainsKey(n))
                {
                     result.Modifications.Add(new ChangedAttributeInformation(n, ChangedAttributeType.Removed, null, obj[n]));
                }
            }

            return result;
        }



        public LdapChangeResult ChangeObjectAttribute(string distinguishedName, string attributeName, object value)
        {
            var ldapObj = GetObjectByDn(distinguishedName, "objectClass");
            var allowedAttributes = GetObjectClassAttributes(ldapObj.ObjectClass).Values;
            return ChangeObjectAttribute(distinguishedName, attributeName, value, allowedAttributes);
        }

        public LdapChangeResult ChangeObjectAttributeForClass(string distinguishedName, string attributeName, object value, string objectClass)
        {
            var allowedAttributes = GetObjectClassAttributes(objectClass).Values.ToList();
            return ChangeObjectAttribute(distinguishedName, attributeName, value, allowedAttributes);
        }

        public LdapChangeResult ChangeObjectAttribute(string distinguishedName, string attributeName, object value, IEnumerable<AttributeInfo> allowedAttributes)
        {

            var attributeNames = new string[]{attributeName};
            var obj = GetObjectByDn(distinguishedName, attributeNames);
            if (obj == null)
            {
                throw new Exception("Ldap Object not found");
            }

            

            var changer = new LdapObjectChangeBuilder(this, distinguishedName, allowedAttributes)
                .AddModification(attributeName, value);


            var result = changer.SendChangedModifications();

            var oldJson = JObject.Parse(JsonConvert.SerializeObject(obj));
            var nobj = GetObjectByDn(result.DistinguishedName, attributeNames);
            var newJson = JObject.Parse(JsonConvert.SerializeObject(nobj));


            foreach (var jProperty in newJson.Properties())
            {
                var n = jProperty.Name;
                if (oldJson.ContainsKey(n))
                {
                    if (newJson[n]?.ToString() != oldJson[n]?.ToString())
                    {
                        result.Modifications.Add(new ChangedAttributeInformation(n, ChangedAttributeType.Changed, nobj.GetValueOrDefault(n), obj.GetValueOrDefault(n)));
                    }
                }
                else
                {
                    result.Modifications.Add(new ChangedAttributeInformation(n, ChangedAttributeType.Added, nobj[n], null));
                }
            }

            foreach (var jProperty in oldJson.Properties())
            {
                var n = jProperty.Name;
                if (!newJson.ContainsKey(n))
                {
                    result.Modifications.Add(new ChangedAttributeInformation(n, ChangedAttributeType.Removed, null, obj[n]));
                }
            }

            return result;
        }




        internal DirectoryResponse SetBacklinkAttribute(string linkedDN, string backLinkDN, string attributeName)
        {

            var linked = GetObjectByDn(linkedDN, attributeName);

            IEnumerable<string> values = new List<string>();
            if (linked.TryGetValue(attributeName, out var _values))
            {
                values = _values as IEnumerable<string>;
            }

            if (!values.Contains(backLinkDN))
            {
                var modifyMemebrOfRequest = new ModifyRequest();
                modifyMemebrOfRequest.DistinguishedName = linkedDN;
                var memberMod = new DirectoryAttributeModification();
                memberMod.Name = attributeName;
                memberMod.Operation = DirectoryAttributeOperation.Add;
                memberMod.Add(backLinkDN);
                modifyMemebrOfRequest.Modifications.Add(memberMod);
                return Connect().SendRequest(modifyMemebrOfRequest);
            }

            return null;
        }

        public string CalculateJsonLdapAttributes(string json, int maxRecurse = 10)
        {
            return CalculateJsonLdapAttributes(json, ref maxRecurse);
        }
        private string CalculateJsonLdapAttributes(string json, ref int count)
        {

            if (count < 1)
            {
                return json;
            }

            count--;

            var data = JsonConvert.DeserializeObject<Dictionary<string, object>>(json);

            var dataObj = new ScriptObject(StringComparer.OrdinalIgnoreCase);
            dataObj.Import(data, renamer: (member) =>
            {
                return member.Name;
            });

            var ldapFunctionsObject = new ScriptObject();
            ldapFunctionsObject.AddClassInstance(new TemplateFunctions(this));

            var dataObject = new ScriptObject();
            dataObject.Add("_", dataObj);
            
            var context = new TemplateContext
            {
                MemberRenamer = member => member.Name
            };
            context.PushGlobal(ldapFunctionsObject);
            context.PushGlobal(dataObject);

            var t = Template.Parse(json);
            var rendered = t.Render(context);

            if (Template.Parse(rendered).Page.Body.Statements.OfType<ScriptExpressionStatement>().Any())
            {
                //var tdat = JsonConvert.DeserializeObject<Dictionary<string, object>>(rendered);
                rendered = CalculateJsonLdapAttributes(rendered, ref count);
            }

            return rendered;

           
        }
    }

    public class LdapOptions
    {

        public string BindBaseDN { get; set; }

        public NetworkCredential Credential { get; set; }

        public AuthType? AuthType { get; set; }

        public List<LdapServer> LdapServers { get; set; } = new List<LdapServer>();

        public int? SearchPageSize { get; set; }

    }

}

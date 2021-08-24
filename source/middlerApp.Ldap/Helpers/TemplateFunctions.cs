using System;
using System.Globalization;
using System.Linq;
using System.Text;

namespace MiddlerApp.Ldap.Helpers
{
    public class TemplateFunctions
    {
        private readonly Ldap _ldap;

        public TemplateFunctions(Ldap ldap)
        {
            _ldap = ldap;
        }

        public string QueryAttr(string filter, string attribute)
        {
            var value = _ldap.Search(filter, attribute).FirstOrDefault()?[attribute];
            if (value == null)
                return null;

            var json = Converter.Json.ToJson(value);
            
            return json.Trim('"');
        }

        public string QueryDN(string filter)
        {
            return QueryAttr(filter, "distinguishedName"); 
        }

        public string ReplaceSonderzeichen(string input)
        {
            return input.Replace("ß", "ss").Replace("ö", "oe").Replace("Ö", "Oe").Replace("ä", "ae").Replace("Ä", "Ae").Replace("ü", "üe").Replace("Ü", "Ue");
        }

        public string RemoveDiacritics(string input)
        {
            string str = input.Normalize(NormalizationForm.FormD);
            StringBuilder stringBuilder = new StringBuilder();
            for (int index = 0; index < str.Length; ++index)
            {
                char ch = str[index];
                if (CharUnicodeInfo.GetUnicodeCategory(ch) != UnicodeCategory.NonSpacingMark)
                    stringBuilder.Append(ch);
            }
            return stringBuilder.ToString();
        }

        public string Normalize(string input)
        {
            if (String.IsNullOrWhiteSpace(input))
            {
                return input;
            }

            input = ReplaceSonderzeichen(input);
            
            string str = input.Normalize(NormalizationForm.FormD);
            StringBuilder stringBuilder = new StringBuilder();
            for (int index = 0; index < str.Length; ++index)
            {
                char ch = str[index];
                if (CharUnicodeInfo.GetUnicodeCategory(ch) != UnicodeCategory.NonSpacingMark)
                    stringBuilder.Append(ch);
            }
            return stringBuilder.ToString();

        }

        
    }
}

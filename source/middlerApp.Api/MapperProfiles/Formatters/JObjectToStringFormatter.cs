using AutoMapper;
using doob.middler;
using Newtonsoft.Json.Linq;

namespace middlerApp.Api.MapperProfiles.Formatters
{
    public class JObjectToStringFormatter : IValueConverter<JObject, string>
    {

        public string Convert(JObject sourceMember, ResolutionContext context)
        {
            return Converter.Json.ToJson(sourceMember);
        }
    }
}
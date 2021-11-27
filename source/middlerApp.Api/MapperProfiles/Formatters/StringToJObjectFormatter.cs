using AutoMapper;
using doob.Reflectensions;
using Newtonsoft.Json.Linq;

namespace middlerApp.Api.MapperProfiles.Formatters
{
    public class StringToJObjectFormatter : IValueConverter<string, JObject>
    {
        public JObject Convert(string sourceMember, ResolutionContext context)
        {
            return Json.Converter.ToJObject(sourceMember);
        }
    }
}
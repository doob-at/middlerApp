using System;
using doob.Reflectensions;
using doob.Reflectensions.JsonConverters;
using Newtonsoft.Json.Converters;

namespace MiddlerApp.Ldap
{
    public static class Converter
    {

        private static readonly Lazy<Json> lazyJson = new Lazy<Json>(() => new Json()
            .RegisterJsonConverter<StringEnumConverter>()
            .RegisterJsonConverter<DefaultDictionaryConverter>()
        );

        public static Json Json => lazyJson.Value;


        public static T CopyTo<T>(object @object)
        {
            var json = Json.ToJson(@object);
            return Json.ToObject<T>(json);
        }

    }

}

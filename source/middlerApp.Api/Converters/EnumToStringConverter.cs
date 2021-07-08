using System;
using doob.Reflectensions.Common;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace middlerApp.Api.Converters
{
    public class EnumToStringConverter: StringEnumConverter
    {
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            var en = (Enum) value;
            writer.WriteValue(en.GetName());
        }

    }
}

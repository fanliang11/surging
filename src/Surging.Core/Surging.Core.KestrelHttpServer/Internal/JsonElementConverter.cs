using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Surging.Core.KestrelHttpServer.Internal
{
    public class JsonElementConverter : System.Text.Json.Serialization.JsonConverter<object>
    {
        private const string DateTimeFormat = "yyyy-MM-dd HH:mm:ss";

        public override object Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType == JsonTokenType.String && DateTime.TryParseExact(reader.GetString(), DateTimeFormat, CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime dateTime))
            { 
                return dateTime;
            }
            if(reader.TokenType==JsonTokenType.String)
            {
                return reader.GetString();
            }

            if (reader.TokenType == JsonTokenType.Number)
            {
                var num = reader.GetDecimal();
                return num;
            }
            return JsonElement.ParseValue(ref reader);
        }

        public override void Write(Utf8JsonWriter writer, object value, JsonSerializerOptions options)
        {
            var jsonEl = (JsonElement)value;
            writer.WriteStringValue(jsonEl.ValueKind.ToString(DateTimeFormat));
        }
    }
}


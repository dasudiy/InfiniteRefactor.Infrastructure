using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using InfiniteRefactor.Infrastructure.Extensions;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using JsonException = System.Text.Json.JsonException;
using JsonSerializer = System.Text.Json.JsonSerializer;

namespace InfiniteRefactor.Infrastructure.Serializer
{
    public class SystemJsonSerializer : StreamSerializer
    {
        public static readonly SystemJsonSerializer Instance = new SystemJsonSerializer();
        public override string Name => "json2";

        public override string ContentType => "application/json";

        public JsonSerializerOptions Options { get; set; } = new JsonSerializerOptions
        {
            NumberHandling = JsonNumberHandling.AllowReadingFromString,
            ReferenceHandler = ReferenceHandler.IgnoreCycles
        };

        public SystemJsonSerializer()
        {
            this.Options.Converters.Add(new JTokenConverter());
            this.Options.Converters.Add(new JObjectConverter());
            this.Options.Converters.Add(new LongLikeToStringConverter<long>());
            this.Options.Converters.Add(new LongLikeToStringConverter<ulong>());
            this.Options.Converters.Add(new LongLikeToStringConverter<long?>());
            this.Options.Converters.Add(new LongLikeToStringConverter<ulong?>());
        }

        public override object Deserialize(Stream stream, Type type)
        {
            return JsonSerializer.Deserialize(stream, type, Options);
        }

        public override void Serialize(object o, Stream stream, Dictionary<string, object> options = null)
        {
            if (options != null)
            {
                JsonSerializerOptions op = new JsonSerializerOptions(Options).ApplyDictionary(options);
                JsonSerializer.Serialize(stream, o, op);
            }
            else
            {
                JsonSerializer.Serialize(stream, o, Options);
            }
        }

        public override Task SerializeAsync(object o, Stream stream, Dictionary<string, object> options = null)
        {
            if (options != null)
            {
                JsonSerializerOptions op = new JsonSerializerOptions(Options).ApplyDictionary(options);
                return JsonSerializer.SerializeAsync(stream, o, op);
            }
            else
            {
                return JsonSerializer.SerializeAsync(stream, o, Options);
            }
        }

        public override async Task<object> DeserializeAsync(Stream stream, Type type)
        {
            return await JsonSerializer.DeserializeAsync(stream, type, Options);
        }

        public override bool SupportType(string typeName)
        {
            return true;
        }


        public class JTokenConverter : System.Text.Json.Serialization.JsonConverter<JToken>
        {
            public override JToken Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
            {
                // Logic to parse JSON using System.Text.Json and convert to JToken
                using JsonDocument document = JsonDocument.ParseValue(ref reader);
                return JToken.Parse(document.RootElement.GetRawText());
            }

            public override void Write(Utf8JsonWriter writer, JToken value, JsonSerializerOptions options)
            {
                // Logic to write JToken content to System.Text.Json writer
                writer.WriteRawValue(value.ToString(Formatting.Indented));
            }
        }

        public class JObjectConverter : System.Text.Json.Serialization.JsonConverter<JObject>
        {
            public override JObject Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
            {
                // Logic to parse JSON using System.Text.Json and convert to JToken
                using JsonDocument document = JsonDocument.ParseValue(ref reader);
                return JObject.Parse(document.RootElement.GetRawText());
            }

            public override void Write(Utf8JsonWriter writer, JObject value, JsonSerializerOptions options)
            {
                // Logic to write JToken content to System.Text.Json writer
                writer.WriteRawValue(value.ToString(Formatting.Indented));
            }
        }

        public class LongLikeToStringConverter<T> : System.Text.Json.Serialization.JsonConverter<T>
        {
            private static readonly bool IsNullable = Nullable.GetUnderlyingType(typeof(T)) != null;
            private static readonly Type UnderlyingType = Nullable.GetUnderlyingType(typeof(T)) ?? typeof(T);

            public override T Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
            {
                // Handle explicit null in JSON
                if (reader.TokenType == JsonTokenType.Null)
                {
                    if (IsNullable)
                        return default!;
                    throw new JsonException($"Null value not allowed for non-nullable {UnderlyingType.Name}.");
                }

                string str;
                if (reader.TokenType == JsonTokenType.String)
                {
                    str = reader.GetString()!;
                }
                else if (reader.TokenType == JsonTokenType.Number)
                {
                    // Numbers are safe to get directly
                    if (UnderlyingType == typeof(long))
                        return (T)(object)reader.GetInt64();
                    if (UnderlyingType == typeof(ulong))
                        return (T)(object)reader.GetUInt64();

                    throw new JsonException($"Unsupported type: {UnderlyingType}");
                }
                else
                {
                    throw new JsonException($"Unexpected token parsing {UnderlyingType.Name}.");
                }

                // Try parse based on target type
                if (UnderlyingType == typeof(long))
                {
                    if (long.TryParse(str, out var l))
                        return (T)(object)l;
                }
                else if (UnderlyingType == typeof(ulong))
                {
                    if (ulong.TryParse(str, out var ul))
                        return (T)(object)ul;
                }

                // Failed parsing
                if (IsNullable)
                    return default!; // null
                throw new JsonException($"Invalid {UnderlyingType.Name} value: {str}");
            }

            public override void Write(Utf8JsonWriter writer, T value, JsonSerializerOptions options)
            {
                if (value == null)
                {
                    writer.WriteNullValue();
                    return;
                }

                writer.WriteStringValue(value.ToString());
            }
        }
    }
}
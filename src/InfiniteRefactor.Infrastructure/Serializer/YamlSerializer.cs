using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using YamlDotNet.Serialization;

namespace AirMaster.Infrastructure.Serializer
{
    public class YamlSerializer : ISerializer
    {
        private YamlDotNet.Serialization.Serializer serializer;
        private Deserializer deserializer;

        public string Name => "yaml";

        public string ContentType => "text/vnd.yaml";

        public Dictionary<string, object> Config { get; set; }

        public Encoding Encoding { get; set; }


        public YamlSerializer()
        {
            serializer = new YamlDotNet.Serialization.Serializer();
            deserializer = new Deserializer();
        }

        public object Deserialize(Stream stream, Type type)
        {
            using (var reader = new StreamReader(stream, Encoding))
            {
                return deserializer.Deserialize(reader, type);
            }
        }

        public object Deserialize(string i, Type targetType)
        {
            return deserializer.Deserialize(i, targetType);
        }

        public void Serialize(object o, Stream stream, Dictionary<string, object> options = null)
        {
            using (var writer = new StreamWriter(stream, Encoding))
            {
                serializer.Serialize(writer, o);
            }
        }

        public string Serialize(object o, Dictionary<string, object> options = null)
        {
            return serializer.Serialize(o);
        }

        public bool SupportType(string typeName)
        {
            return typeName == Name;
        }

        public Task SerializeAsync(object o, Stream stream, Dictionary<string, object> options = null)
        {
            using (var writer = new StreamWriter(stream, Encoding))
            {
                serializer.Serialize(writer, o);
                return Task.CompletedTask;
            }
        }

        public Task<object> DeserializeAsync(Stream stream, Type type)
        {
            using (var reader = new StreamReader(stream, Encoding))
            {
                return Task.FromResult(deserializer.Deserialize(reader, type));
            }
        }
    }
}

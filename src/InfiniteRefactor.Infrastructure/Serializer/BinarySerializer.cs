using System;
using System.Collections.Generic;
using System.Runtime.Serialization.Formatters.Binary;

namespace InfiniteRefactor.Infrastructure.Serializer
{
    [Obsolete(".NET6已废弃BinaryFormatter，请使用其它序列化方式")]
    public class BinarySerializer : StreamSerializer
    {
        public static readonly BinarySerializer Instance = new BinarySerializer();

        public override string Name
        {
            get { return "bin"; }
        }

        public override bool SupportType(string typeName)
        {
            return typeName == "bin";
        }

        public override string ContentType
        {
            get { return "application/octet-stream"; }
        }

        public override void Serialize(object o, System.IO.Stream stream, Dictionary<string, object> options = null)
        {
            var ser = new BinaryFormatter();
            ser.Serialize(stream, o);
        }

        public override object Deserialize(System.IO.Stream stream, Type type)
        {
            throw new NotSupportedException("BinaryFormatter deserialization is disabled due to security risks. Use a safe serializer such as JSON or XML instead.");
        }
    }
}

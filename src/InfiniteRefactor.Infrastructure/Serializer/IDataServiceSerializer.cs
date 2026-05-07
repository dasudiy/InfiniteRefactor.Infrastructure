using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace InfiniteRefactor.Infrastructure.Serializer
{
    public interface ISerializer
    {
        string Name { get; }
        bool SupportType(string typeName);
        string ContentType { get; }
        Dictionary<string, object> Config { get; }
        Encoding Encoding { get; set; }

        #region ForStream
        void Serialize(object o, Stream stream, Dictionary<string, object> options = null);
        object Deserialize(Stream stream, Type type);

        Task SerializeAsync(object o, Stream stream, Dictionary<string, object> options = null);

        Task<object> DeserializeAsync(Stream stream, Type type);
        #endregion

        #region ForText
        string Serialize(object o, Dictionary<string, object> options = null);
        object Deserialize(string i, Type targetType);
        #endregion
    }

    public abstract class TextSerializer : ISerializer
    {
        public abstract string Name { get; }

        public abstract bool SupportType(string typeName);

        public abstract string ContentType { get; }

        public Encoding Encoding { get; set; }

        public virtual void Serialize(object o, Stream stream, Dictionary<string, object> options = null)
        {
            var result = Serialize(o, options);
            var bytes = (this.Encoding ?? Encoding.UTF8).GetBytes(result);
            stream.Write(bytes, 0, bytes.Length);
        }

        public virtual object Deserialize(Stream stream, Type type)
        {
            using (var reader = new StreamReader(stream, this.Encoding ?? Encoding.UTF8, false, 1024, true))
            {
                return Deserialize(reader.ReadToEnd(), type);
            }
        }

        public abstract string Serialize(object o, Dictionary<string, object> options = null);

        public abstract object Deserialize(string i, Type targetType);

        public virtual async Task SerializeAsync(object o, Stream stream, Dictionary<string, object> options = null)
        {
            var result = Serialize(o, options);
            var bytes = (this.Encoding ?? Encoding.UTF8).GetBytes(result);
            await stream.WriteAsync(bytes, 0, bytes.Length);
        }

        public virtual async Task<object> DeserializeAsync(Stream stream, Type type)
        {
            using (var reader = new StreamReader(stream, this.Encoding ?? Encoding.UTF8, false, 1024, true))
            {
                return Deserialize(await reader.ReadToEndAsync(), type);
            }
        }

        public Dictionary<string, object> Config { get; set; }
    }

    public abstract class StreamSerializer : ISerializer
    {
        public abstract string Name { get; }

        public abstract bool SupportType(string typeName);

        public abstract string ContentType { get; }

        public Encoding Encoding { get; set; }

        public abstract void Serialize(object o, Stream stream, Dictionary<string, object> options = null);

        public abstract object Deserialize(Stream stream, Type type);

        public virtual string Serialize(object o, Dictionary<string, object> options = null)
        {
            using (var ms = new MemoryStream())
            {
                Serialize(o, ms, options);
                ms.Position = 0;
                using (var reader = new StreamReader(ms, this.Encoding ?? Encoding.UTF8))
                {
                    return reader.ReadToEnd();
                }
            }
        }

        public virtual object Deserialize(string i, Type targetType)
        {
            using (var ms = new MemoryStream((this.Encoding ?? Encoding.UTF8).GetBytes(i)))
            {
                ms.Position = 0;
                return Deserialize(ms, targetType);
            }
        }

        public virtual byte[] SerializeToBytes(object obj, Dictionary<string, object> options = null)
        {
            using (var ms = new MemoryStream())
            {
                Serialize(obj, ms, options);
                return ms.ToArray();
            }
        }

        public virtual object DeserializeFromBytes(byte[] binary, Type type)
        {
            using (var ms = new MemoryStream(binary))
            {
                return Deserialize(ms, type);
            }
        }

        public virtual async Task SerializeAsync(object o, Stream stream, Dictionary<string, object> options = null)
        {
            await Task.CompletedTask;
            Serialize(o, stream, options);
        }

        public virtual async Task<object> DeserializeAsync(Stream stream, Type type)
        {
            await Task.CompletedTask;
            return Deserialize(stream, type);
        }

        public Dictionary<string, object> Config { get; set; }
    }

    public class SerializerConfigAttribute : Attribute
    {
        public string For { get; set; }
        public string Config { get; set; }

        public Dictionary<string, object> GetConfig()
        {
            throw new NotImplementedException();
        }
    }
}

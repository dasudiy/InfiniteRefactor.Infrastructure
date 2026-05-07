/*
 * 由SharpDevelop创建。
 * 用户： Asuka
 * 日期: 2013/4/29
 * 时间: 11:05
 *

 */

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace InfiniteRefactor.Infrastructure.Serializer
{
    /// <summary>
    /// a shortcut for SerializerFactory
    /// </summary>
    public static class SF
    {
        public static class JSON
        {
            public static string Serialize(object obj) => SerializerFactory.Serialize("json", obj);
            public static void Serialize(object obj, Stream outputStream) => SerializerFactory.Serialize("json", obj, outputStream);
            public static T Deserialize<T>(Stream stream) => SerializerFactory.Deserialize<T>("json", stream);
            public static T Deserialize<T>(string text) => SerializerFactory.Deserialize<T>("json", text);
        }
        public static class XML
        {
            public static string Serialize(object obj) => SerializerFactory.Serialize("xml", obj);
            public static void Serialize(object obj, Stream outputStream) => SerializerFactory.Serialize("xml", obj, outputStream);
            public static T Deserialize<T>(Stream stream) => SerializerFactory.Deserialize<T>("xml", stream);
            public static T Deserialize<T>(string text) => SerializerFactory.Deserialize<T>("xml", text);
        }
    }

    /// <summary>
    /// Description of SerializerFactory.
    /// </summary>
    public static class SerializerFactory
    {
        private static Dictionary<string, ISerializer> _serializerCache = new Dictionary<string, ISerializer>();

        public static void LoadSerializerFromAssembly(Assembly assem)
        {
            var q = assem.GetTypes().Where(t => t.IsClass && !t.IsAbstract && typeof(ISerializer).IsAssignableFrom(t) && t.GetConstructor(Type.EmptyTypes) != null);
            foreach (var item in q)
            {
                try
                {
                    var instance = Activator.CreateInstance(item) as ISerializer;
                    _serializerCache.Add(instance.Name, instance);
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine(ex);
                }
            }
        }

        static SerializerFactory()
        {
            //LoadSerializerFromAssembly(typeof(SerializerFactory).Assembly);
            Add(NewtonJsonSerializerAdapter.Instance);
            Add(new XmlSerializer());
            Add(NewtonJsonToXmlSerializer.Instance);
            Add(new SimpleSerializer());
            Add(new YamlSerializer());
            Add(SystemJsonSerializer.Instance);
        }

        public static void Add(ISerializer ser)
        {
            _serializerCache.Add(ser.Name, ser);
        }

        public static ISerializer Create(string type)
        {
            ISerializer ser;
            if (string.IsNullOrWhiteSpace(type) || !_serializerCache.TryGetValue(type, out ser))
            {
                return GetDefault();
            }

            return ser;
        }

        public static ISerializer GetDefault()
        {
            if (_serializerCache.TryGetValue("json", out ISerializer ser))
            {
                return ser;
            }
            else
            {
                return _serializerCache.Values.FirstOrDefault();
            }
        }

        public static T LoadFromFile<T>(string type, string file)
        {
            if (!File.Exists(file))
            {
                return default(T);
            }

            using (var fs = new FileStream(file, FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                return Create(type).Deserialize<T>(fs);
            }
        }

        public static void SaveToFile(string type, string file, object obj, Dictionary<string, object> options = null)
        {
            var dir = Path.GetDirectoryName(file);
            if (dir != "" && !Directory.Exists(dir)) { Directory.CreateDirectory(dir); }
            using (var fs = new FileStream(file, FileMode.OpenOrCreate, FileAccess.Write, FileShare.None))
            {
                fs.Position = 0;
                Create(type).Serialize(obj, fs, options);
                fs.SetLength(fs.Position);
            }
        }

        public static T Deserialize<T>(this ISerializer instance, string text)
        {
            return (T)instance.Deserialize(text, typeof(T));
        }

        public static T Deserialize<T>(this ISerializer instance, Stream stream)
        {
            return (T)instance.Deserialize(stream, typeof(T));
        }

        public static T Deserialize<T>(string type, Stream stream)
        {
            return Create(type).Deserialize<T>(stream);
        }
        public static object Deserialize(string type, string obj, Type objType)
        {
            return Create(type).Deserialize(obj, objType);
        }
        public static object Deserialize(string type, Stream stream, Type objType)
        {
            return Create(type).Deserialize(stream, objType);
        }

        public static T Deserialize<T>(string type, string obj)
        {
            return Create(type).Deserialize<T>(obj);
        }

        public static void Serialize(string type, object obj, Stream stream, Dictionary<string, object> options = null)
        {
            Create(type).Serialize(obj, stream, options);
        }

        public static string Serialize(string type, object obj, Dictionary<string, object> options = null)
        {
            return Create(type).Serialize(obj, options);
        }
    }
}

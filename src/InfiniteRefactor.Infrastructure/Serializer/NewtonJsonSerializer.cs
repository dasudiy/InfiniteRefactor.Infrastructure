using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;

using AirMaster.Infrastructure.Extensions;

namespace AirMaster.Infrastructure.Serializer
{
    public class NewtonJsonSerializerAdapter : TextSerializer
    {
        public static readonly NewtonJsonSerializerAdapter Instance = new NewtonJsonSerializerAdapter();
        private JsonSerializer ser;
        private JsonSerializerSettings settings = new JsonSerializerSettings
        {
            Error = (err, e) => { e.ErrorContext.Handled = true; },
            ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
            NullValueHandling = NullValueHandling.Ignore
            //Formatting = Formatting.Indented
        };
        private NewtonJsonSerializerAdapter()
        {
            JsonConvert.DefaultSettings = () => settings;
            ser = JsonSerializer.Create(settings);
        }

        public void ChangeSettings(Action<JsonSerializerSettings> func)
        {
            func(settings);
            ser = JsonSerializer.Create(settings);
        }

        public void AddConverter(JsonConverter converter) => ser.Converters.Add(converter);

        //private class MyConvertor : JsonConverter
        //{
        //    public override bool CanConvert(Type objectType)
        //    {
        //        if (objectType.Name == "System.Data.EntityKey") { return true; }
        //        if (!objectType.IsGenericType) { return false; }
        //        return objectType.Name == "EntityWrapperWithRelationships`1" || objectType.Name == "EntityCollection`1";
        //    }

        //    public override object ReadJson(JsonReader reader, Type objectType, object existingValue, Newtonsoft.Json.JsonSerializer serializer)
        //    {
        //        return null;
        //    }

        //    public override void WriteJson(JsonWriter writer, object value, Newtonsoft.Json.JsonSerializer serializer)
        //    {                
        //        writer.WriteNull();
        //    }
        //}

        //public class InterfaceContractResolver : DefaultContractResolver, IContractResolver
        //{
        //    public InterfaceContractResolver() : this(false) { }
        //    public InterfaceContractResolver(bool shareCache) : base(shareCache) { }

        //    protected override JsonProperty CreateProperty(MemberInfo member, MemberSerialization memberSerialization)
        //    {
        //        var property = base.CreateProperty(member, memberSerialization);
        //        var interfaces = member.DeclaringType.GetInterfaces();
        //        foreach (var @interface in interfaces)
        //        {
        //            foreach (var interfaceProperty in @interface.GetProperties())
        //            {
        //                // This is weak: among other things, an implementation 
        //                // may be deliberately hiding an interface member
        //                if (interfaceProperty.Name == member.Name && interfaceProperty.MemberType == member.MemberType)
        //                {
        //                    if (interfaceProperty.GetCustomAttributes(typeof(JsonPropertyAttribute), true).Any())
        //                    {
        //                        property.Ignored = false;
        //                        return property;
        //                    }
        //                }
        //            }
        //        }
        //        return property;
        //    }
        //}        

        public override string Name
        {
            get { return "json"; }
        }

        public override bool SupportType(string typeName)
        {
            return typeName == Name;
        }

        public override string ContentType
        {
            get { return "application/json"; }
        }

        public override string Serialize(object o, Dictionary<string, object> options = null)
        {
            using (var sw = new StringWriter())
            {
                if (options != null && options["indent"].To<bool>(false))
                {
                    using (var writer = new JsonTextWriter(sw))
                    {
                        writer.Formatting = Formatting.Indented;
                        ser.Serialize(writer, o);
                    }
                }
                else
                {
                    ser.Serialize(sw, o);
                }
                return sw.ToString();
            }
        }

        public override object Deserialize(string i, Type targetType)
        {
            if (i == null) { return null; }
            using (var sr = new StringReader(i))
            {
                return ser.Deserialize(sr, targetType);
            }
        }
    }
}

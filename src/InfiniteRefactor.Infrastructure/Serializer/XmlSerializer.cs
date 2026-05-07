using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml;
using System.Xml.Serialization;
using InfiniteRefactor.Infrastructure.Extensions;

namespace InfiniteRefactor.Infrastructure.Serializer
{
    public class XmlSerializer : StreamSerializer
    {
        private XmlSerializerNamespaces ns = new XmlSerializerNamespaces();

        public XmlSerializer()
        {
            ns.Add(string.Empty, string.Empty);
        }

        public override string Name
        {
            get { return "xml"; }
        }

        public override bool SupportType(string typeName)
        {
            return typeName == Name;
        }

        public override string ContentType
        {
            get { return "text/xml"; }
        }

        public override object Deserialize(Stream stream, Type type)
        {
            var ser = new System.Xml.Serialization.XmlSerializer(type);
            return ser.Deserialize(stream);
        }

        public override void Serialize(object o, Stream stream, Dictionary<string, object> options = null)
        {
            var ser = new System.Xml.Serialization.XmlSerializer(o.GetType());
            if (options != null && options["indent"].To<bool>() == false)
            {
                using (var writer = new StreamWriter(stream, Encoding.UTF8, 1024, true))
                {
                    using (var xmlWriter = XmlWriter.Create(writer, new XmlWriterSettings { Indent = false, OmitXmlDeclaration = true, Encoding = Encoding.UTF8 }))
                    {
                        ser.Serialize(xmlWriter, o, ns);
                    }
                }
            }
            else
            {
                using (var writer = new StreamWriter(stream, Encoding.UTF8, 1024, true))
                {
                    using (var xmlWriter = XmlWriter.Create(writer, new XmlWriterSettings { Encoding = Encoding.UTF8 })) //携程接口需要
                    {
                        ser.Serialize(xmlWriter, o, ns);
                    }
                }
            }
        }
    }

    //public class XmlSerializer : StreamSerializer
    //{

    //    public override string Name
    //    {
    //        get { return "xml"; }
    //    }

    //    public override bool SupportType(string typeName)
    //    {
    //        return typeName == Name;
    //    }

    //    public override string ContentType
    //    {
    //        get { return "text/xml"; }
    //    }



    //    public override void Serialize(object o, Stream stream)
    //    {
    //        var setting = new SharpSerializerXmlSettings
    //        {
    //            IncludeAssemblyVersionInTypeName = false,
    //            IncludeCultureInTypeName = false,
    //            IncludePublicKeyTokenInTypeName = false
    //        };
    //        var serializer = new SharpSerializer(setting);
    //        serializer.Serialize(o, stream);
    //    }

    //    public override object Deserialize(Stream stream, Type type)
    //    {
    //        var serializer = new SharpSerializer();
    //        return serializer.Deserialize(stream);
    //    }
    //}
}

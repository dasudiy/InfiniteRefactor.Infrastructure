using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml;

namespace AirMaster.Infrastructure.Serializer
{
    public class NewtonJsonToXmlSerializer : TextSerializer
    {
        public static readonly NewtonJsonToXmlSerializer Instance = new NewtonJsonToXmlSerializer();

        public override string Name
        {
            get { return "xml2"; }
        }

        public override bool SupportType(string typeName)
        {
            return typeName == Name;
        }

        public override string ContentType
        {
            get { return "text/xml"; }
        }

        public override string Serialize(object o, Dictionary<string, object> options = null)
        {
            var json = NewtonJsonSerializerAdapter.Instance.Serialize(o, options);
            var doc = Newtonsoft.Json.JsonConvert.DeserializeXmlNode(json, o is IEnumerable ? "root" : o.GetType().Name);
            using (var ms = new MemoryStream())
            {
                doc.Save(ms);
                return Encoding.UTF8.GetString(ms.ToArray());
            }
        }

        public override object Deserialize(string xml, Type targetType)
        {
            var doc = new XmlDocument();
            doc.LoadXml(xml);

            if (string.IsNullOrWhiteSpace(doc.DocumentElement.InnerXml))
            {
                return null;
            }
            else
            {
                return NewtonJsonSerializerAdapter.Instance.Deserialize(Newtonsoft.Json.JsonConvert.SerializeXmlNode(doc.DocumentElement, Newtonsoft.Json.Formatting.None, true), targetType);
            }
        }
    }
}

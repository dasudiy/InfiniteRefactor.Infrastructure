using System;
using System.Collections.Generic;

using AirMaster.Infrastructure.Extensions;

namespace AirMaster.Infrastructure.Serializer
{
    public class SimpleSerializer : TextSerializer
    {
        public override string Name
        {
            get { return "txt"; }
        }

        public override bool SupportType(string typeName)
        {
            return typeName == Name;
        }

        public override string ContentType
        {
            get { return "text/plain"; }
        }

        public override string Serialize(object o, Dictionary<string, object> options = null)
        {
            if (o == null) { return null; }
            return o.ToString();
        }

        public override object Deserialize(string i, Type targetType)
        {
            return i.To(targetType);
        }
    }
}

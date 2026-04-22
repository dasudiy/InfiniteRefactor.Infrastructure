//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Runtime.Serialization.Formatters.Soap;
//using System.Text;
//using System.Threading.Tasks;

//namespace AirMaster.Infrastructure.Serializer
//{
//    public class SoapSerializer : StreamSerializer
//    {
//        public static readonly BinarySerializer Instance = new BinarySerializer();

//        public override string Name
//        {
//            get { return "soap"; }
//        }

//        public override bool SupportType(string typeName)
//        {
//            return typeName == "soap";
//        }

//        public override string ContentType
//        {
//            get { return "text/xml"; }
//        }

//        public override void Serialize(object o, System.IO.Stream stream, Dictionary<string, object> options = null)
//        {
//            var ser = new SoapFormatter();
//            ser.Serialize(stream, o);
//        }

//        public override object Deserialize(System.IO.Stream stream, Type type)
//        {
//            var ser = new SoapFormatter();
//            return ser.Deserialize(stream);
//        }
//    }
//}

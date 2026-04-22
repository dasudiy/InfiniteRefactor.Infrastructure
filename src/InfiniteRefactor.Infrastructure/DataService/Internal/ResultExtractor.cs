using AirMaster.Infrastructure.Serializer;
using System;

namespace AirMaster.Infrastructure.DataService.Internal
{
    public class ResultExtractor
    {
        public virtual DataServiceResult Extract(string response)
        {
            return SerializerFactory.Create("json").Deserialize<DataServiceResult>(response);
        }

        public virtual object Extract(string response, Type type, string formatter = "json")
        {
            if (type == typeof(void))
            {
                return SerializerFactory.Create(formatter).Deserialize(response, typeof(DataServiceResult));
            }
            else
            {
                return SerializerFactory.Create(formatter).Deserialize(response, typeof(DataServiceResult<>).MakeGenericType(type));
            }
        }

        public virtual object RawOutputExtract(string response, Type type, string formatter = "json")
        {
            if (type == typeof(void))
            {
                return null;
            }
            else
            {
                return SerializerFactory.Create(formatter).Deserialize(response, type);
            }
        }
    }
}

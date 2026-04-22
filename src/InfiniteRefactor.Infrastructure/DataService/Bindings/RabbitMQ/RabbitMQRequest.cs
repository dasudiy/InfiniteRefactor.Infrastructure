using AirMaster.Infrastructure.DataService.Abstractions;
using AirMaster.Infrastructure.DataService.Bindings.Stream.Infrastructure;
using System;
using System.Net;

namespace AirMaster.Infrastructure.DataService.Bindings.RabbitMQ
{
    class RabbitMQRequest : DataServiceRequest
    {
        private RequestInfo requestInfo;

        public RabbitMQRequest(RabbitMQContext streamContext, RequestInfo requestInfo) : base(streamContext)
        {
            this.requestInfo = requestInfo;
        }

        public override object this[string key]
        {
            get
            {
                string value;
                if (requestInfo.arguments.TryGetValue(key, out value))
                {
                    return value;
                }
                else
                {
                    return null;
                }
            }
        }

        public override object ReadParameter(string name, Type targetType)
        {
            if (requestInfo.arguments.ContainsKey(name))
            {
                return Context.Serializer.Deserialize(this[name].ToString(), targetType);
            }
            return null;
        }

        public override object RawRequestObject
        {
            get
            {
                return requestInfo;
            }
        }

        internal IPEndPoint InternalRemote { get; set; }
        public override IPEndPoint Remote => InternalRemote;

        public override Uri Url
        {
            get
            {
                return new Uri(this.Context.RouteInfo.ToString());
            }
        }

    }
}

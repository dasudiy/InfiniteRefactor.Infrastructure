using AirMaster.Infrastructure.DataService.Abstractions;
using AirMaster.Infrastructure.DataService.Bindings.Stream.Infrastructure;
using AirMaster.Infrastructure.Session;
using System;

namespace AirMaster.Infrastructure.DataService.Bindings.RabbitMQ
{
    class RabbitMQContext : DataServiceContext
    {
        private RequestInfo requestInfo;
        //private RabbitMQServer rabbitMQServer;
        private RabbitMQRequest request;
        private RabbitMQResponse response;
        private RabbitMQServerClient client;

        public RabbitMQContext(RequestInfo requestInfo, RabbitMQServerClient rabbitMQConnection)
            : base(requestInfo)
        {
            this.requestInfo = requestInfo;
            //this.rabbitMQServer = rabbitMQServer;
            this.request = new RabbitMQRequest(this, requestInfo);
            this.response = new RabbitMQResponse(this);
            this.client = rabbitMQConnection;
        }

        public override DataServiceRequest Request => request;

        public override DataServiceResponse Response => response;

        public override ApplicationSession Session => throw new NotImplementedException();

        public override DataServiceClientBase Client => client;
    }
}

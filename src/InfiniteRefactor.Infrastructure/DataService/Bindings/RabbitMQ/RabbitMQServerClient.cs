using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using InfiniteRefactor.Infrastructure.DataService.Abstractions;
using InfiniteRefactor.Infrastructure.DataService.Metadata;

namespace InfiniteRefactor.Infrastructure.DataService.Bindings.RabbitMQ
{
    internal class RabbitMQServerClient : DataServiceClientBase
    {
        private readonly RabbitMQConnection connection;
        private readonly string clientId;

        internal RabbitMQServerClient(RabbitMQConnection connection, string clientId) : base(null)
        {
            this.connection = connection;
            this.clientId = clientId;
            this.ResultExtractor = this.connection.ResultExtractor;
        }

        public override string Invoke(RouteInfo routeInfo, Dictionary<string, object> paramsDict)
        {
            return InvokeAsync(routeInfo, paramsDict).Result;
        }

        public override Task<string> InvokeAsync(RouteInfo routeInfo, Dictionary<string, object> paramsDict)
        {
            return connection.InternalSendAsync(routeInfo, paramsDict, clientId);
        }

        public override byte[] InvokeRaw(RouteInfo routeInfo, Dictionary<string, object> paramsDict)
        {
            throw new NotImplementedException();
        }

        public override void InvokeAndForget(RouteInfo routeInfo, Dictionary<string, object> paramsDict)
        {
            connection.InternalSendIgnoreResponse(routeInfo, paramsDict, clientId);
        }
    }
}

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Web;
using InfiniteRefactor.Infrastructure.DataService.Abstractions;
using InfiniteRefactor.Infrastructure.DataService.Metadata;
using InfiniteRefactor.Infrastructure.Utilities.Snowflake;

namespace InfiniteRefactor.Infrastructure.DataService.Bindings.RabbitMQ
{
    class RabbitMQClient : DataServiceClientBase, IDisposable
    {
        private RabbitMQConnection connection;
        private static IdWorker idgen = new IdWorker(0, 0);

        public RabbitMQClient(string url, DataServiceHost host = null, TimeSpan? timeout = null) : base(url, null, false, host, timeout)
        {
            var uri = new Uri(url);
            var qs = HttpUtility.ParseQueryString(uri.Query);
            if (string.IsNullOrWhiteSpace(qs["name"])) { throw new ArgumentException("必须指定name"); }

            var produce = qs["name"];
            var consume = produce + "-REPLY-" + idgen.NextId();
            var newUri = url.Replace(uri.Query, $"?produce={produce}&consume={consume}");
            this.connection = new RabbitMQConnection(newUri, timeout, host);
            this.connection.Start();
            this.ResultExtractor = this.connection.ResultExtractor;
        }

        public void Dispose()
        {
            this.connection.Dispose();
        }

        public override string Invoke(RouteInfo routeInfo, Dictionary<string, object> paramsDict)
        {
            return connection.Invoke(routeInfo, paramsDict);
        }

        public override Task<string> InvokeAsync(RouteInfo routeInfo, Dictionary<string, object> paramsDict)
        {
            return connection.InvokeAsync(routeInfo, paramsDict);
        }

        public override byte[] InvokeRaw(RouteInfo routeInfo, Dictionary<string, object> paramsDict)
        {
            return connection.InvokeRaw(routeInfo, paramsDict);
        }

        public override void InvokeAndForget(RouteInfo routeInfo, Dictionary<string, object> paramsDict)
        {
            connection.InternalSendIgnoreResponse(routeInfo, paramsDict, null);
        }
    }
}

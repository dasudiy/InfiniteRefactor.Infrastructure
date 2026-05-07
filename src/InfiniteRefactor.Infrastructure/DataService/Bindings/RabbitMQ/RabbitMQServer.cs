using System;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Web;
using InfiniteRefactor.Infrastructure.DataService.Abstractions;
using InfiniteRefactor.Infrastructure.DataService.Common.Processor;

namespace InfiniteRefactor.Infrastructure.DataService.Bindings.RabbitMQ
{
    public class RabbitMQServer : DataServiceServerBase
    {

        public string QueueName { get; }

        private RabbitMQConnection connection;

        public RabbitMQServer(string prefix, X509Certificate2 certificate = null, bool validateCertificate = true, DataServiceHost host = null) : base(prefix, certificate, validateCertificate, host)
        {
            var uri = new Uri(prefix);

            var qs = HttpUtility.ParseQueryString(uri.Query);
            if (string.IsNullOrWhiteSpace(qs["name"])) { throw new ArgumentException("必须指定name"); }

            var consume = qs["name"];
            var produce = consume + "-REPLY";
            var newUri = prefix.Replace(uri.Query, $"?produce={produce}&consume={consume}");
            this.connection = new RabbitMQConnection(newUri, null, host);

            if (!this.DataServiceHost.PostProcessors.Any(p => p is ResultWrapperAttribute))
            {
                //throw new ArgumentException("DataServiceHost必须添加ResultWrapper");
                this.DataServiceHost.PostProcessors.Add(new ResultWrapperAttribute() { Priority = -1, DirectOutputParameterToResult = false });
            }
        }

        public override void Start()
        {
            this.connection.Start();
        }

        public override void Stop()
        {
            connection.Dispose();
        }

        public void SetPrefetchSize(ushort qty)
        {
            //if (connection.IsConnected) { throw new Exception("Set this value before start server"); }
            connection.PrefetchCount = qty;
        }

        public void SetConcurrentLimit(int qty)
        {
            connection.SetConcurrentLimit(qty);
        }
    }
}

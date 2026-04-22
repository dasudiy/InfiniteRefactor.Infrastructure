using AirMaster.Infrastructure.DataService.Bindings.AspNetCore;
using AirMaster.Infrastructure.DataService.Bindings.Http;
using AirMaster.Infrastructure.DataService.Bindings.RabbitMQ;
using AirMaster.Infrastructure.DataService.Bindings.Stream.NP;
using AirMaster.Infrastructure.DataService.Bindings.Stream.Tcp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.WebSockets;
using System.Security.Cryptography.X509Certificates;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace AirMaster.Infrastructure.DataService.Abstractions
{
    public class DataServiceServer : DataServiceServerBase, IDisposable
    {
        private List<DataServiceServerBase> servers = new List<DataServiceServerBase>();

        internal DataServiceServer(string[] prefixes, X509Certificate2 certificate = null, bool validateCertificate = true, DataServiceHost host = null)
            : base(string.Join(",", prefixes), certificate, validateCertificate, host)
        {
            foreach (var prefix in prefixes)
            {
                servers.Add(CreateServer(prefix, certificate, validateCertificate, host));
            }
        }

        public static DataServiceServerBase Create(string prefix, X509Certificate2 certificate = null, bool validateCertificate = true, DataServiceHost host = null)
        {
            return CreateServer(prefix, certificate, validateCertificate, host);
        }

        public static DataServiceServerBase Create(string[] prefixes, X509Certificate2 certificate = null, bool validateCertificate = true, DataServiceHost host = null)
        {
            return new DataServiceServer(prefixes, certificate, validateCertificate, host);
        }

        internal static DataServiceServerBase CreateServer(string prefix, X509Certificate2 certificate, bool validateCertificate, DataServiceHost host)
        {
            var match = Regex.Match(prefix, @"(\w+)://([^:/]+)(?::(\d+))?(.*)");

            switch (match.Groups[1].Value)
            {
                case "http":
                case "https":
                    return new DataServiceHttpServer(prefix, certificate, validateCertificate, host);
                case "np":
                case "nps":
                    return new NamedPipeServer(prefix, certificate, validateCertificate, host);
                case "tcp":
                case "tcps":
                    return new DataServiceTcpServer(prefix, certificate, validateCertificate, host);
                case "amqp":
                    return new RabbitMQServer(prefix, certificate, validateCertificate, host);
                case "ws":
                case "wss":
                    return new WebSocketServer(prefix, certificate, validateCertificate, host);
                default:
                    throw new NotImplementedException();
            }
        }

        public override void Dispose()
        {
            Parallel.ForEach(servers, s =>
            {
                try
                {
                    s.Dispose();
                }
                catch
                {
                }
            });
        }

        public override void Start()
        {
            Parallel.ForEach(servers, s =>
            {
                s.Start();
            });
        }

        public override void Stop()
        {
            Parallel.ForEach(servers, s =>
            {
                s.Stop();
            });
        }
    }
}

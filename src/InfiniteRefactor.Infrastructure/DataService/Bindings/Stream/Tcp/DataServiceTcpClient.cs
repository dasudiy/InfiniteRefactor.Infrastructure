using System;
using System.Net.Sockets;
using System.Security.Cryptography.X509Certificates;
using InfiniteRefactor.Infrastructure.DataService.Bindings.Stream.Infrastructure;
using InfiniteRefactor.Infrastructure.Net;

namespace InfiniteRefactor.Infrastructure.DataService.Bindings.Stream.Tcp
{
    public class DataServiceTcpClient : StreamClient
    {
        private TcpClient client;
        private string host;
        private int port;

        public DataServiceTcpClient(string url, X509Certificate2 certificate = null, bool validateCertificate = true, DataServiceHost host = null, TimeSpan? timeout = null, string proxy = null)
            : base(url, certificate, validateCertificate, host, timeout, proxy)
        {
            var uri = new Uri(url);
            if (uri.Scheme == "tcps") { UseSSL = true; }
            HostName = uri.Host;

            this.host = uri.Host;
            this.port = uri.Port;
        }

        protected override IConnectionController OpenConnection()
        {
            if (Proxy != null)
            {
                client = ProxiedTcpClient.CreateTcpClient(Proxy, host, port);
            }
            else
            {
                client = new TcpClient();
                client.Connect(host, port);
            }

            return new TcpConnectionController(client);
        }
    }
}

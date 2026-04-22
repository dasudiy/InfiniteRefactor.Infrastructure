using AirMaster.Infrastructure.DataService.Bindings.Stream.Infrastructure;
using System;
using System.IO.Pipes;
using System.Security.Cryptography.X509Certificates;

namespace AirMaster.Infrastructure.DataService.Bindings.Stream.NP
{
    public class NamedPipeClient : StreamClient
    {
        private string pipeName;
        private string hostAddr;

        public NamedPipeClient(string url, X509Certificate2 certificate = null, bool validateCertificate = true, DataServiceHost host = null, TimeSpan? timeout = null)
            : base(url, certificate, validateCertificate, host, timeout)
        {
            var uri = new Uri(url);
            if (uri.Scheme == "nps") { UseSSL = true; }

            this.pipeName = uri.AbsolutePath.TrimStart('/');
            this.hostAddr = uri.Host;
        }

        protected override IConnectionController OpenConnection()
        {
            var np = new NamedPipeClientStream(hostAddr, pipeName, PipeDirection.InOut, PipeOptions.Asynchronous);
            np.Connect(1000);
            if (!np.IsConnected)
            {
                throw new Exception("连接失败！");
            }
            return new NamedPipeController(np);
        }
    }

}

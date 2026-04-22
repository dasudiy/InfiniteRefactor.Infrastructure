using AirMaster.Infrastructure.DataService.Bindings.Stream.Infrastructure;
using AirMaster.Infrastructure.Extensions;

using NLog;
using System;
using System.Net;
using System.Net.Sockets;
using System.Security.Cryptography.X509Certificates;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace AirMaster.Infrastructure.DataService.Bindings.Stream.Tcp
{
    public class DataServiceTcpServer : StreamServer, IDisposable
    {
        private TcpListener _listener;
        private volatile bool _exiting = false;
        private Task _workingThread;
        private Logger log = LogManager.GetCurrentClassLogger();

        public DataServiceTcpServer(string prefix, X509Certificate2 certificate = null, bool validateCertificate = true, DataServiceHost host = null)
            : base(prefix, certificate, validateCertificate, host)
        {
            var match = Regex.Match(prefix, @"(\w+)://([^:/]+)(?::(\d+))?(.*)");
            var ip = match.Groups[2].Value == "*" ? IPAddress.Any : IPAddress.Parse(match.Groups[2].Value);

            _listener = new TcpListener(new IPEndPoint(ip, match.Groups[3].Value.To<int>()));
        }

        public override void Start()
        {
            _exiting = false;
            _listener.Start();
            _workingThread = Task.Run(() => work());
            log.Trace("DataService server在{0}开始监听...", _listener.LocalEndpoint.ToString());
        }

        private async Task work()
        {
            while (!_exiting)
            {
                try
                {
                    var client = await _listener.AcceptTcpClientAsync().ConfigureAwait(false);
                    try
                    {
                        OnNewConnection(client);
                    }
                    catch
                    {
                        client.Dispose();
                    }
                }
                catch
                {
                }
            }
        }

        private void OnNewConnection(TcpClient client)
        {
            base.NewConnection(new TcpConnectionController(client));
        }

        public override void Stop()
        {
            base.Dispose();
            if (_listener != null)
            {
                _listener.Stop();
            }
            _exiting = true;
            if (_workingThread != null)
            {
                try
                {
                    _workingThread.Wait();
                }
                catch
                {
                }
            }
        }

        public override void Dispose()
        {
            Stop();
        }
    }
}

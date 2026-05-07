using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;
using InfiniteRefactor.Infrastructure.DataService.Abstractions;
using InfiniteRefactor.Infrastructure.DataService.Common.Processor;
using InfiniteRefactor.Infrastructure.DataService.Internal;
using InfiniteRefactor.Infrastructure.DataService.Metadata;

namespace InfiniteRefactor.Infrastructure.DataService.Bindings.Stream.Infrastructure
{
    public abstract class StreamClient : DataServiceClientBase, IDisposable
    {
        public string HostName { get; set; }

        private StreamConnection connection;
        private X509Certificate2 rootCert;
        //private X509Certificate2 cert;

        public StreamClient(string url, X509Certificate2 certificate = null, bool validateCertificate = true, DataServiceHost host = null, TimeSpan? timeout = null, string proxy = null)
            : base(url, certificate, validateCertificate, host, timeout, proxy)
        {
            if (!this.DataServiceHost.PostProcessors.Any(p => p is ResultWrapperAttribute))
            {
                this.DataServiceHost.PostProcessors.Add(new ResultWrapperAttribute() { Priority = -1, DirectOutputParameterToResult = false });
                //throw new ArgumentException("DataServiceHost必须添加ResultWrapper");
            }
            this.ResultExtractor = new ResultExtractor();
            if (certificate != null)
            {
                rootCert = CertificateValidator.ValidateAndGetRootCA(certificate);
            }            
        }

        public void Open()
        {
            var connCtrl = OpenConnection();
            connCtrl.ConnectionClosed += ConnCtrl_ConnectionClosed;
            X509Certificate cert = null;
            if (UseSSL)
            {
                var ssl = new SslStream(connCtrl.Stream, false, new RemoteCertificateValidationCallback(CertValidate), new LocalCertificateSelectionCallback(CertSelect));
                ssl.AuthenticateAsClientAsync("aaa").Wait();
                connCtrl.Stream = ssl;
                cert = ssl.RemoteCertificate;
            }

            connection = new StreamConnection(DataServiceHost, connCtrl, cert, Timeout, Proxy);
        }

        private void ConnCtrl_ConnectionClosed(object sender, EventArgs e)
        {
            connection.Dispose();
        }

        private bool CertValidate(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors)
        {
            return CertificateValidator.Validate(certificate as X509Certificate2, rootCert, chain);
        }

        private X509Certificate CertSelect(object sender, string targetHost, X509CertificateCollection localCertificates, X509Certificate remoteCertificate, string[] acceptableIssuers)
        {
            return LocalCertificate;
        }

        protected abstract IConnectionController OpenConnection();

        public override string Invoke(RouteInfo routeInfo, Dictionary<string, object> paramsDict)
        {
            if (connection == null || !connection.ConnectionController.IsConnected)
            {
                lock (this)
                {
                    if (connection == null || !connection.ConnectionController.IsConnected)
                    {
                        this.Open();
                    }
                }
            }
            if (!connection.ConnectionController.IsConnected)
            {
                throw new Exception("连接失败！");
            }
            return connection.Invoke(routeInfo, paramsDict);
        }

        public override Task<string> InvokeAsync(RouteInfo routeInfo, Dictionary<string, object> paramsDict)
        {
            if (connection == null || !connection.ConnectionController.IsConnected)
            {
                lock (this)
                {
                    if (connection == null || !connection.ConnectionController.IsConnected)
                    {
                        this.Open();
                    }
                }
            }
            if (!connection.ConnectionController.IsConnected)
            {
                throw new Exception("连接失败！");
            }
            return connection.InvokeAsync(routeInfo, paramsDict);
        }

        public override byte[] InvokeRaw(RouteInfo routeInfo, Dictionary<string, object> paramsDict)
        {
            return connection.InvokeRaw(routeInfo, paramsDict);
        }

        public override void InvokeAndForget(RouteInfo routeInfo, Dictionary<string, object> paramsDict)
        {
            InvokeAsync(routeInfo, paramsDict);
        }

        public void Close()
        {
            Dispose();
        }

        public void Dispose()
        {
            if (connection != null)
            {
                connection.Dispose();
            }
        }
    }
}

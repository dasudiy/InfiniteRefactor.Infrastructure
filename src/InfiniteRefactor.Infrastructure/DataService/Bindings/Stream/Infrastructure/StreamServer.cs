using AirMaster.Infrastructure.DataService.Abstractions;
using AirMaster.Infrastructure.DataService.Common.Processor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;

namespace AirMaster.Infrastructure.DataService.Bindings.Stream.Infrastructure
{
    public abstract class StreamServer : DataServiceServerBase, IDisposable
    {
        private Dictionary<IConnectionController, StreamConnection> activeConnections = new Dictionary<IConnectionController, StreamConnection>();
        private X509Certificate2 rootCert;

        public StreamServer(string prefix, X509Certificate2 certificate = null, bool validateCertificate = true, DataServiceHost host = null)
            : base(prefix, certificate, validateCertificate, host)
        {
            if (certificate != null)
            {
                rootCert = CertificateValidator.ValidateAndGetRootCA(certificate, null, null);
            }

            if (!this.DataServiceHost.PostProcessors.Any(p => p is ResultWrapperAttribute))
            {
                //throw new ArgumentException("DataServiceHost必须添加ResultWrapper");
                this.DataServiceHost.PostProcessors.Add(new ResultWrapperAttribute() { Priority = -1, DirectOutputParameterToResult = false });
            }
        }

        protected void NewConnection(IConnectionController connCtrl)
        {
            try
            {
                X509Certificate cert = null;
                if (UseSSL)
                {
                    var ssl = new SslStream(connCtrl.Stream, false, new RemoteCertificateValidationCallback(CertValidate));
                    ssl.AuthenticateAsServer(Certificate, this.ValidateClientCertificate, System.Security.Authentication.SslProtocols.Tls12, true);
                    connCtrl.Stream = ssl;
                    cert = ssl.RemoteCertificate;
                }
                connCtrl.ConnectionClosed += ConnCtrl_ConnectionClosed;
                lock (activeConnections)
                {
                    activeConnections[connCtrl] = new StreamConnection(this.DataServiceHost, connCtrl, cert, TimeSpan.FromMinutes(1), null);
                    this.DataServiceHost.NewConnection(activeConnections[connCtrl]);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex.Message);
                connCtrl.Close();
            }
        }

        private void ConnCtrl_ConnectionClosed(object sender, EventArgs e)
        {
            lock (activeConnections)
            {
                StreamConnection conn;
                if (activeConnections.TryGetValue(sender as IConnectionController, out conn))
                {
                    activeConnections.Remove(sender as IConnectionController);
                    this.DataServiceHost.NewConnection(conn);
                }
            }
        }

        private bool CertValidate(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors)
        {
            if (ValidateClientCertificate)
            {
                return CertificateValidator.Validate(certificate as X509Certificate2, rootCert, chain);
            }
            else
            {
                return true;
            }
        }

        public override void Dispose()
        {
            lock (activeConnections)
            {
                foreach (var item in activeConnections.Values.ToList())
                {
                    item.Dispose();
                }
            }
        }
    }
}

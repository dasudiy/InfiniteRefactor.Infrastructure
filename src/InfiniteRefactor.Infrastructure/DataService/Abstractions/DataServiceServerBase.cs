using AirMaster.Infrastructure.Utilities;
using System;
using System.Collections.Generic;
using System.Security.Cryptography.X509Certificates;

namespace AirMaster.Infrastructure.DataService.Abstractions
{
    public abstract class DataServiceServerBase : IDisposable
    {
        public DataServiceHost DataServiceHost { get; private set; }
        public string Prefix { get; private set; }
        public X509Certificate2 Certificate { get; private set; }
        public bool UseSSL { get; private set; }
        public bool ValidateClientCertificate { get; private set; }
        public abstract void Start();
        public abstract void Stop();


        // moved to dataservice host
        //public event EventHandler<SimpleArgument<DataServiceClientBase>> ClientDisconnected;
        //public event EventHandler<SimpleArgument<DataServiceClientBase>> ClientConnected;
        //public abstract IReadOnlyList<DataServiceClientBase> ActiveClients { get; }

        //protected void OnClientConnected(object sender, DataServiceClientBase client)
        //{
        //    ClientConnected?.Invoke(sender, new SimpleArgument<DataServiceClientBase>(client));
        //}

        //protected void OnClientDisconnected(object sender, DataServiceClientBase client)
        //{
        //    ClientDisconnected?.Invoke(sender, new SimpleArgument<DataServiceClientBase>(client));
        //}

        public DataServiceServerBase(string prefix, X509Certificate2 certificate = null, bool validateCertificate = true, DataServiceHost host = null)
        {
            this.DataServiceHost = host ?? DataServiceHost.Instance;
            this.Certificate = certificate;
            if (certificate != null) { UseSSL = true; }
            this.ValidateClientCertificate = validateCertificate;
            this.Prefix = prefix;
        }

        public virtual void Dispose()
        {
            this.Stop();
        }
    }
}

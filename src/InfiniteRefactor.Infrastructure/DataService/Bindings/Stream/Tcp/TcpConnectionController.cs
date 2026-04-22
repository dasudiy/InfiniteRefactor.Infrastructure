using AirMaster.Infrastructure.DataService.Bindings.Stream.Infrastructure;
using System;
using System.Net;
using System.Net.Sockets;

namespace AirMaster.Infrastructure.DataService.Bindings.Stream.Tcp
{
    internal class TcpConnectionController : IConnectionController
    {
        private TcpClient client;

        public TcpConnectionController(TcpClient client)
        {
            this.client = client;
            this.Stream = client.GetStream();
            this.Remote = client.Client.RemoteEndPoint;
        }

        public bool IsConnected
        {
            get
            {
                return client.Connected;
            }
        }

        public System.IO.Stream Stream { get; set; }

        public EndPoint Remote { get; private set; }

        public event EventHandler ConnectionClosed;

        public void Close()
        {
            Stream.Dispose();
            client.Dispose();
            ConnectionClosed?.Invoke(this, EventArgs.Empty);
        }
    }
}
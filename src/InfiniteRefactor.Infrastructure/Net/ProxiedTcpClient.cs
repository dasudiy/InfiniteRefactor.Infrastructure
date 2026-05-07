using System;
using System.Net.Sockets;
using System.Text;

namespace InfiniteRefactor.Infrastructure.Net
{
    public static class ProxiedTcpClient
    {
        public static TcpClient CreateTcpClient(string proxy, string address, int port)
        {
            var proxyUri = new Uri(proxy);
            var socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

            socket.Connect(proxyUri.Host, proxyUri.Port);

            var connectMessage = Encoding.UTF8.GetBytes($"CONNECT {address}:{port} HTTP/1.1{Environment.NewLine}{Environment.NewLine}");
            socket.Send(connectMessage);

            byte[] receiveBuffer = new byte[1024];
            var received = socket.Receive(receiveBuffer);

            var response = ASCIIEncoding.ASCII.GetString(receiveBuffer, 0, received);

            if (!response.Contains(" 200 "))
            {
                throw new Exception($"Error connecting to proxy server {address}:{port}. Response: {response}");
            }

            return new TcpClient
            {
                Client = socket
            };
        }
    }
}

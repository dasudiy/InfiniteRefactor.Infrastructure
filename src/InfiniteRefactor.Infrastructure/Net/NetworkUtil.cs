using System;
using System.Net.Sockets;

namespace AirMaster.Infrastructure.Net
{
    public class NetworkUtil
    {
        public static bool IsTcpPortOpen(string address, int port, int waitSeconds = 5000)
        {
            var isSuccess = false;
            var wait = new System.Threading.ManualResetEventSlim();
            var socket = new Socket(SocketType.Stream, ProtocolType.Tcp);
            socket.BeginConnect(address, port, new AsyncCallback(ar =>
            {
                try
                {
                    socket.EndConnect(ar);
                    isSuccess = true;
                }
                catch
                {

                }
                finally
                {
                    wait.Set();
                }
            }), null);
            wait.Wait(waitSeconds);
            socket.Shutdown(SocketShutdown.Both);
            socket.Close();
            return isSuccess;
        }

        //public static bool IsTcpPortOpen2(string address, int port, int waitSeconds = 5000)
        //{
        //    var isSuccess = false;
        //    var wait = new System.Threading.ManualResetEventSlim();
        //    var socket = new Socket(SocketType.Raw, ProtocolType.IP);

        //    socket.BeginSendTo(,,,SocketFlags.ip

        //}
    }
}

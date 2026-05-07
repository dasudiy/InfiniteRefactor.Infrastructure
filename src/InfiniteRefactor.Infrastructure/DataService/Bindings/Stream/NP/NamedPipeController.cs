using System;
using System.IO.Pipes;
using System.Net;
using InfiniteRefactor.Infrastructure.DataService.Bindings.Stream.Infrastructure;

namespace InfiniteRefactor.Infrastructure.DataService.Bindings.Stream.NP
{
    public class NamedPipeController : IConnectionController
    {
        private PipeStream rawStream;

        public NamedPipeController(PipeStream stream)
        {
            rawStream = stream;
            Stream = stream;
        }

        public bool IsConnected
        {
            get
            {
                return rawStream.IsConnected;
            }
        }

        public System.IO.Stream Stream { get; set; }

        public EndPoint Remote => null;

        public event EventHandler ConnectionClosed;

        public void Close()
        {
            Stream.Close();
            Stream.Dispose();
            ConnectionClosed?.Invoke(this, EventArgs.Empty);
        }
    }
}

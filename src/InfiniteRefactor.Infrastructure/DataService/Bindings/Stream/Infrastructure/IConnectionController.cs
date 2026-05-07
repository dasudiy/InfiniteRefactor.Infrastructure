using System;
using System.Net;

namespace InfiniteRefactor.Infrastructure.DataService.Bindings.Stream.Infrastructure
{
    public interface IConnectionController
    {
        EndPoint Remote { get; }
        event EventHandler ConnectionClosed;
        System.IO.Stream Stream { get; set; }
        bool IsConnected { get; }
        void Close();
    }
}

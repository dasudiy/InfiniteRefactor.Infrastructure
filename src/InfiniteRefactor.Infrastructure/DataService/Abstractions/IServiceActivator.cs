using System;

namespace InfiniteRefactor.Infrastructure.DataService.Abstractions
{
    public interface IServiceActivator
    {
        object GetInstance(Type type);
        bool RequireDispose { get; }
    }
}

using System;

namespace AirMaster.Infrastructure.DataService.Abstractions
{
    public interface IServiceActivator
    {
        object GetInstance(Type type);
        bool RequireDispose { get; }
    }
}

using System;
using System.Reflection;
using InfiniteRefactor.Infrastructure.DataService.Abstractions;

namespace InfiniteRefactor.Infrastructure.DataService.Common.Activator
{
    public class SingletonServiceInstanceRetriver : IServiceActivator
    {
        public object GetInstance(Type type)
        {
            var filed = type.GetTypeInfo().GetField("Instance", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static);
            if (filed != null)
            {
                return filed.GetValue(null);
            }
            return null;
        }

        public bool RequireDispose
        {
            get { return false; }
        }
    }
}

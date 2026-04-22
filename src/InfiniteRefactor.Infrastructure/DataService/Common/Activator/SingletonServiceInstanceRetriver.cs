using AirMaster.Infrastructure.DataService.Abstractions;
using System;
using System.Reflection;

namespace AirMaster.Infrastructure.DataService.Common.Activator
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

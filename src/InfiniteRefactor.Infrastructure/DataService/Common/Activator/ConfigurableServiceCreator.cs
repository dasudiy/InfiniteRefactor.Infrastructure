using AirMaster.Infrastructure.DataService.Abstractions;
using AirMaster.Infrastructure.DataService.Annotations;
using AirMaster.Infrastructure.Extensions;

using System;
using System.Collections.Generic;

namespace AirMaster.Infrastructure.DataService.Common.Activator
{
    public class ConfigurableServiceCreator : IServiceActivator
    {
        public static Dictionary<string, string> Mapper { get; private set; }

        static ConfigurableServiceCreator()
        {
            Mapper = new Dictionary<string, string>();
        }

        public object GetInstance(Type type)
        {
            try
            {
                var attr = type.GetCustomAttribute<DataServiceAttribute>();
                string typeName;
                if (Mapper.TryGetValue(attr.Name, out typeName))
                {
                    var serviceType = Type.GetType(typeName, false);
                    if (serviceType != null)
                    {
                        return System.Activator.CreateInstance(serviceType);
                    }
                }
            }
            catch
            {
            }

            return null;
        }

        public bool RequireDispose
        {
            get { return true; }
        }
    }
}

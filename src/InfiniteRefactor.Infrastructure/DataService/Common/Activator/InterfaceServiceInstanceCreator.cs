using System;
using InfiniteRefactor.Infrastructure.DataService.Abstractions;

namespace InfiniteRefactor.Infrastructure.DataService.Common.Activator
{
    public class InterfaceServiceInstanceCreator : IServiceActivator
    {
        public Type ContractType { get; set; }
        public Type ServiceType { get; set; }
        public bool Singleton { get; set; }

        public InterfaceServiceInstanceCreator(Type contractType, Type serviceType)
        {
            if (!contractType.IsAssignableFrom(serviceType)) { throw new ArgumentException("serviceType必须是contractType的子类或实现！"); }
            ContractType = contractType;
            ServiceType = serviceType;
        }

        public InterfaceServiceInstanceCreator(Type contractType, object serviceInstance)
        {
            var serviceType = serviceInstance.GetType();
            if (!contractType.IsAssignableFrom(serviceType)) { throw new ArgumentException("serviceType必须是contractType的子类或实现！"); }
            ContractType = contractType;
            ServiceType = serviceType;
            instance = serviceInstance;
        }

        private object instance = null;

        public object GetInstance(Type type)
        {

            if (type == ContractType)
            {
                if (Singleton)
                {
                    if (instance == null)
                    {
                        lock (this)
                        {
                            if (instance == null)
                            {
                                instance = System.Activator.CreateInstance(ServiceType);
                            }
                        }
                    }
                    return instance;
                }
                else
                {
                    return System.Activator.CreateInstance(ServiceType);
                }
            }
            else
            {
                throw new Exception(string.Format("无法创建{0}的新实例", type.FullName));
            }
        }

        public bool RequireDispose
        {
            get { return !Singleton; }
        }
    }
}

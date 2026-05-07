using System;
using InfiniteRefactor.Infrastructure.DataService.Abstractions;

namespace InfiniteRefactor.Infrastructure.DataService.Common.Activator
{
    public class DefaultServiceCreator : IServiceActivator
    {
        public static readonly DefaultServiceCreator Instance = new DefaultServiceCreator();
        private DefaultServiceCreator() { }

        public object GetInstance(Type type)
        {
            //return FastActivator.CreateInstance(type);
            return System.Activator.CreateInstance(type);
        }

        public bool RequireDispose
        {
            get { return true; }
        }
    }
}

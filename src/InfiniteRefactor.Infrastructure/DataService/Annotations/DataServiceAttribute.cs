using System;

namespace InfiniteRefactor.Infrastructure.DataService.Annotations
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Interface, AllowMultiple = false)]
    public class DataServiceAttribute : System.Attribute
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public Type InstanceRetriver { get; set; }
        public Type InstanceType { get; set; }
        public SessionRequireType SessionRequireType { get; set; }

        public bool UseAsyncHandle { get; set; }

        public DataServiceAttribute()
        {
            SessionRequireType = SessionRequireType.ReadOnly;
        }
    }

    public enum SessionRequireType
    {
        FullAccess = 2,
        ReadOnly = 1,
        None = 0
    }
}

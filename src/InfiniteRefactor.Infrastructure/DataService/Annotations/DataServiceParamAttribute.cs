using System;

namespace AirMaster.Infrastructure.DataService.Annotations
{
    [AttributeUsage(AttributeTargets.Parameter, AllowMultiple = false)]
    public class DataServiceParamAttribute : Attribute
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public Type ValueReader { get; set; }
        public object[] Parameters { get; set; }
    }
}

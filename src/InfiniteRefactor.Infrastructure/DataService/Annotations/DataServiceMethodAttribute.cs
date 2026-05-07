using System;

namespace InfiniteRefactor.Infrastructure.DataService.Annotations
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
    public class DataServiceMethodAttribute : Attribute
    {
        public string Name { get; set; }
        public string Summary { get; set; }
        public string Description { get; set; }
        public bool Async { get; set; }
        public Type ParameterValueReader { get; set; }
        public object[] ValueReaderParameters { get; set; }
    }
}

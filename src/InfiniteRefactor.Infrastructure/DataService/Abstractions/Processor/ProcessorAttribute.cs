using System;

namespace AirMaster.Infrastructure.DataService.Abstractions.Processor
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method | AttributeTargets.Interface, AllowMultiple = true)]
    public abstract class ProcessorAttribute : Attribute
    {
        public int Priority { get; set; }
    }
}

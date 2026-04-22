using System;
using System.Threading.Tasks;

namespace AirMaster.Infrastructure.DataService.Abstractions.Processor
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method | AttributeTargets.Interface, AllowMultiple = true)]
    public abstract class PreProcessorAttribute : ProcessorAttribute, IPreProcessor
    {
        public int Order { get; set; }
        public abstract ProcessResult Process(DataServiceRequest request);

        public virtual Task<ProcessResult> ProcessAsync(DataServiceRequest request)
        {
            return Task.FromResult(Process(request));
        }
    }
}

using System;
using System.Threading.Tasks;

namespace InfiniteRefactor.Infrastructure.DataService.Abstractions.Processor
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true)]
    public abstract class PostProcessorAttribute : ProcessorAttribute, IPostProcessor
    {
        public abstract ProcessResult Process(DataServiceResponse response);
        public virtual Task<ProcessResult> ProcessAsync(DataServiceResponse response)
        {
            return Task.FromResult(Process(response));
        }
    }
}

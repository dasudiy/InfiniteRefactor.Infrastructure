using System.Threading.Tasks;

namespace InfiniteRefactor.Infrastructure.DataService.Abstractions.Processor
{
    public interface IPostProcessor : IHavePriority
    {
        Task<ProcessResult> ProcessAsync(DataServiceResponse response);
        ProcessResult Process(DataServiceResponse response);
    }
}

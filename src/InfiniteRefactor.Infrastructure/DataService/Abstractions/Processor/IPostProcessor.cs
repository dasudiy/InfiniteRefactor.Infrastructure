using System.Threading.Tasks;

namespace AirMaster.Infrastructure.DataService.Abstractions.Processor
{
    public interface IPostProcessor : IHavePriority
    {
        Task<ProcessResult> ProcessAsync(DataServiceResponse response);
        ProcessResult Process(DataServiceResponse response);
    }
}

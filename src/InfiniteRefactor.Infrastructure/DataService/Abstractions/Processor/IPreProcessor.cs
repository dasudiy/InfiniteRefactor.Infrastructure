using System.Threading.Tasks;

namespace AirMaster.Infrastructure.DataService.Abstractions.Processor
{
    public interface IPreProcessor : IHavePriority
    {
        Task<ProcessResult> ProcessAsync(DataServiceRequest request);
        ProcessResult Process(DataServiceRequest request);
    }
}

using System.Threading.Tasks;

namespace InfiniteRefactor.Infrastructure.Examples.ClientPublishedService;

public class WorkerService : IWorkerService
{
    public Task<WorkResult> ProcessAsync(WorkItem item) =>
        Task.FromResult(new WorkResult
        {
            ProcessedBy = "client-worker",
            Output = $"processed:{item.Payload}"
        });
}

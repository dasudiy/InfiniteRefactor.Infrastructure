using System;
using System.Threading.Tasks;
using InfiniteRefactor.Infrastructure.DataService.Abstractions;

namespace InfiniteRefactor.Infrastructure.Examples.ClientPublishedService;

public class GatewayService : IGatewayService
{
    public async Task<WorkResult> DelegateWorkAsync(WorkItem item)
    {
        var client = DataServiceContext.Current.Client
            ?? throw new InvalidOperationException(
                "No connected client on this request. Call gateway methods over the WebSocket connection.");

        var worker = client.Create<IWorkerService>();
        return await worker.ProcessAsync(item);
    }
}

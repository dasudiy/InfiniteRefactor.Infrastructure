using System.Threading.Tasks;
using InfiniteRefactor.Infrastructure.DataService.Annotations;

namespace InfiniteRefactor.Infrastructure.Examples.ClientPublishedService;

public class WorkItem
{
    public string Payload { get; set; }
}

public class WorkResult
{
    public string ProcessedBy { get; set; }
    public string Output { get; set; }
}

/// <summary>
/// Published on the client; the server invokes it through DataServiceContext.Current.Client.
/// </summary>
[DataService(Name = "worker")]
public interface IWorkerService
{
    [DataServiceMethod]
    Task<WorkResult> ProcessAsync(WorkItem item);
}

/// <summary>
/// Hosted on the server; delegates work to the connected client's worker service.
/// </summary>
[DataService(Name = "gateway")]
public interface IGatewayService
{
    [DataServiceMethod]
    Task<WorkResult> DelegateWorkAsync(WorkItem item);
}

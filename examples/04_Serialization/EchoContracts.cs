using System.Threading.Tasks;
using InfiniteRefactor.Infrastructure.DataService.Annotations;

namespace InfiniteRefactor.Infrastructure.Examples.Serialization;

public class EchoRequest
{
    public string Message { get; set; }
}

[DataService(Name = "EchoService")]
public interface IEchoService
{
    [DataServiceMethod]
    Task<EchoRequest> EchoAsync(EchoRequest request);
}

public class EchoService : IEchoService
{
    public Task<EchoRequest> EchoAsync(EchoRequest request) => Task.FromResult(request);
}

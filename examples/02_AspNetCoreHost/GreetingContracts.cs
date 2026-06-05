using System.Threading.Tasks;
using InfiniteRefactor.Infrastructure.DataService.Annotations;

namespace InfiniteRefactor.Infrastructure.Examples.AspNetCoreHost;

public class GreetingRequest
{
    public string Name { get; set; }
}

public class GreetingResponse
{
    public string Text { get; set; }
}

[DataService(Name = "greeting")]
public interface IGreetingService
{
    [DataServiceMethod(Name = "hello")]
    Task<GreetingResponse> SayHelloAsync(GreetingRequest request);
}

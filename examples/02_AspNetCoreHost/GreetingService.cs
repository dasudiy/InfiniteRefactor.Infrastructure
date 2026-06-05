using System.Threading.Tasks;

namespace InfiniteRefactor.Infrastructure.Examples.AspNetCoreHost;

public class GreetingService : IGreetingService
{
    public Task<GreetingResponse> SayHelloAsync(GreetingRequest request) =>
        Task.FromResult(new GreetingResponse { Text = $"Hello, {request.Name}!" });
}

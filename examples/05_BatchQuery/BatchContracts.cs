using System.Threading.Tasks;
using InfiniteRefactor.Infrastructure.DataService.Annotations;

namespace InfiniteRefactor.Infrastructure.Examples.BatchQuery;

public class MultiplyRequest
{
    public int A { get; set; }
    public int B { get; set; }
}

public class MultiplyResponse
{
    public int Product { get; set; }
}

[DataService(Name = "MultiplyService")]
public interface IMultiplyService
{
    [DataServiceMethod]
    Task<MultiplyResponse> MultiplyAsync(MultiplyRequest request);
}

public class MultiplyService : IMultiplyService
{
    public Task<MultiplyResponse> MultiplyAsync(MultiplyRequest request) =>
        Task.FromResult(new MultiplyResponse { Product = request.A * request.B });
}

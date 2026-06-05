using System.Threading.Tasks;

namespace InfiniteRefactor.Infrastructure.Examples.BasicHttpHostAndClient;

public class CalculatorService : ICalculatorService
{
    public Task<CalculateResponse> AddAsync(CalculateRequest request) =>
        Task.FromResult(new CalculateResponse
        {
            Result = request.A + request.B,
            Message = "ok"
        });

    public Task<CalculateResponse> SubtractAsync(CalculateRequest request) =>
        Task.FromResult(new CalculateResponse
        {
            Result = request.A - request.B,
            Message = "ok"
        });
}

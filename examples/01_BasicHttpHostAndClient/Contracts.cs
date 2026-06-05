using System.Threading.Tasks;
using InfiniteRefactor.Infrastructure.DataService.Annotations;

namespace InfiniteRefactor.Infrastructure.Examples.BasicHttpHostAndClient;

public class CalculateRequest
{
    public int A { get; set; }
    public int B { get; set; }
}

public class CalculateResponse
{
    public int Result { get; set; }
    public string Message { get; set; }
}

[DataService(Name = "CalculatorService")]
public interface ICalculatorService
{
    [DataServiceMethod]
    Task<CalculateResponse> AddAsync(CalculateRequest request);

    [DataServiceMethod]
    Task<CalculateResponse> SubtractAsync(CalculateRequest request);
}

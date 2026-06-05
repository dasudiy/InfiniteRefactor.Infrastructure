using System.Linq;
using System.Threading.Tasks;
using InfiniteRefactor.Infrastructure.DataService;
using InfiniteRefactor.Infrastructure.DataService.Abstractions;
using InfiniteRefactor.Infrastructure.DataService.Bindings.Http;
using InfiniteRefactor.Infrastructure.DataService.Common.Processor;

namespace InfiniteRefactor.Infrastructure.Examples.BasicHttpHostAndClient;

internal static class Program
{
    private const string BaseUrl = "http://127.0.0.1:15001";

    private static async Task Main()
    {
        var host = DataServiceHost.Instance;
        EnsureResultWrapper(host);
        host.AddService(typeof(ICalculatorService), typeof(CalculatorService));

        using var server = DataServiceServer.Create(BaseUrl + "/", host: host);
        server.Start();
        await Task.Delay(1500);

        var client = new DataServiceHttpClient(BaseUrl + "/");
        var calculator = client.Create<ICalculatorService>();

        var add = await calculator.AddAsync(new CalculateRequest { A = 10, B = 20 });
        var sub = await calculator.SubtractAsync(new CalculateRequest { A = 30, B = 12 });

        Console.WriteLine($"Add: {add.Result}");
        Console.WriteLine($"Subtract: {sub.Result}");

        server.Stop();
        server.Dispose();
    }

    internal static void EnsureResultWrapper(DataServiceHost host)
    {
        if (!host.PostProcessors.Any(p => p is ResultWrapperAttribute))
        {
            host.PostProcessors.Add(new ResultWrapperAttribute { DirectOutputParameterToResult = false });
        }
    }
}

using System.Linq;
using System.Threading.Tasks;
using InfiniteRefactor.Infrastructure.DataService;
using InfiniteRefactor.Infrastructure.DataService.Abstractions;
using InfiniteRefactor.Infrastructure.DataService.Bindings.Http;
using InfiniteRefactor.Infrastructure.DataService.Common.Processor;

namespace InfiniteRefactor.Infrastructure.Examples.BatchQuery;

internal static class Program
{
    private const string BaseUrl = "http://127.0.0.1:15005";

    private static async Task Main()
    {
        var host = DataServiceHost.Instance;
        EnsureResultWrapper(host);
        host.AddService(typeof(IMultiplyService), typeof(MultiplyService));

        using var server = DataServiceServer.Create(BaseUrl + "/", host: host);
        server.Start();
        await Task.Delay(800);

        var client = new DataServiceHttpClient(BaseUrl + "/");
        var proxy = client.Create<IMultiplyService>();

        var inputs = new[] { (2, 3), (4, 5), (6, 7) };
        var results = await Task.WhenAll(inputs.Select(t =>
            proxy.MultiplyAsync(new MultiplyRequest { A = t.Item1, B = t.Item2 })));

        Console.WriteLine(string.Join(", ", results.Select(r => r.Product)));
        Console.WriteLine("Note: DataServiceClientBase.BatchInvoke exists; server-side BatchQueryRequest is not implemented yet.");

        server.Stop();
        server.Dispose();
    }

    private static void EnsureResultWrapper(DataServiceHost host)
    {
        if (!host.PostProcessors.Any(p => p is ResultWrapperAttribute))
        {
            host.PostProcessors.Add(new ResultWrapperAttribute { DirectOutputParameterToResult = false });
        }
    }
}

using System.Linq;
using System.Threading.Tasks;
using InfiniteRefactor.Infrastructure.DataService;
using InfiniteRefactor.Infrastructure.DataService.Abstractions;
using InfiniteRefactor.Infrastructure.DataService.Bindings.AspNetCore;
using InfiniteRefactor.Infrastructure.DataService.Common.Processor;
using Microsoft.AspNetCore.Builder;

namespace InfiniteRefactor.Infrastructure.Examples.ClientPublishedService;

internal static class Program
{
    private const string BaseUrl = "http://127.0.0.1:15007";
    private const string WebSocketUrl = "ws://127.0.0.1:15007/ws";

    private static async Task Main()
    {
        var serverHost = DataServiceHost.Instance;
        EnsureResultWrapper(serverHost);
        serverHost.AddService(typeof(IGatewayService), typeof(GatewayService));

        var clientHost = new DataServiceHost();
        EnsureResultWrapper(clientHost);
        clientHost.AddService(typeof(IWorkerService), typeof(WorkerService));

        var builder = WebApplication.CreateBuilder();
        builder.WebHost.UseUrls(BaseUrl + "/");
        var app = builder.Build();

        app.UseWebSockets();
        app.UseRouting();
        app.UseDataService(new DataServiceOption
        {
            RequestPath = "/api",
            WebSocketPath = "/ws",
            UseRestApi = false,
            InitFn = _ => { }
        }, serverHost);

        var runTask = app.RunAsync();
        await Task.Delay(1000);

        var clientConnection = DataServiceClientBase.CreateClient(WebSocketUrl, host: clientHost);
        try
        {
            await WaitForConnectionAsync(clientConnection);

            var gateway = clientConnection.Create<IGatewayService>();
            var result = await gateway.DelegateWorkAsync(new WorkItem { Payload = "reverse-rpc" });

            Console.WriteLine($"ProcessedBy: {result.ProcessedBy}");
            Console.WriteLine($"Output: {result.Output}");
        }
        finally
        {
            (clientConnection as IDisposable)?.Dispose();
        }
        await app.StopAsync();
        await runTask;
    }

    private static async Task WaitForConnectionAsync(DataServiceClientBase connection)
    {
        for (var i = 0; i < 50 && !connection.IsConnected; i++)
        {
            await Task.Delay(100);
        }

        if (!connection.IsConnected)
        {
            throw new InvalidOperationException("WebSocket client failed to connect.");
        }
    }

    private static void EnsureResultWrapper(DataServiceHost host)
    {
        if (!host.PostProcessors.Any(p => p is ResultWrapperAttribute))
        {
            host.PostProcessors.Add(new ResultWrapperAttribute { DirectOutputParameterToResult = false });
        }
    }
}

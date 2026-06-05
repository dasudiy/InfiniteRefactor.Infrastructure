using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using InfiniteRefactor.Infrastructure.DataService;
using InfiniteRefactor.Infrastructure.DataService.Bindings.AspNetCore;
using InfiniteRefactor.Infrastructure.DataService.Bindings.Http;
using InfiniteRefactor.Infrastructure.DataService.Common.Processor;
using Microsoft.AspNetCore.Builder;

namespace InfiniteRefactor.Infrastructure.Examples.AspNetCoreHost;

internal static class Program
{
    private const string BaseUrl = "http://127.0.0.1:15002";

    private static async Task Main()
    {
        var builder = WebApplication.CreateBuilder();
        builder.WebHost.UseUrls(BaseUrl + "/");
        var app = builder.Build();

        app.UseRouting();
        app.UseDataService(new DataServiceOption
        {
            RequestPath = "/api",
            UseRestApi = false,
            InitFn = host =>
            {
                EnsureResultWrapper(host);
                host.AddService(typeof(IGreetingService), typeof(GreetingService));
            }
        });

        var runTask = app.RunAsync();
        await Task.Delay(1000);

        var client = new DataServiceHttpClient(BaseUrl + "/api/");
        var greeting = client.Create<IGreetingService>();
        var response = await greeting.SayHelloAsync(new GreetingRequest { Name = "DataService" });

        Console.WriteLine(response.Text);
        await app.StopAsync();
        await runTask;
    }

    private static void EnsureResultWrapper(DataServiceHost host)
    {
        if (!host.PostProcessors.Any(p => p is ResultWrapperAttribute))
        {
            host.PostProcessors.Add(new ResultWrapperAttribute { DirectOutputParameterToResult = false });
        }
    }
}

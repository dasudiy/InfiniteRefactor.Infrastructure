using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Threading.Tasks;
using InfiniteRefactor.Infrastructure.DataService;
using InfiniteRefactor.Infrastructure.DataService.Bindings.AspNetCore;
using InfiniteRefactor.Infrastructure.DataService.Common.Processor;
using Microsoft.AspNetCore.Builder;

namespace InfiniteRefactor.Infrastructure.Examples.MetadataAndSwagger;

internal static class Program
{
    private const string BaseUrl = "http://127.0.0.1:15006";

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
            EnableMetadataService = true,
            InitFn = host =>
            {
                host.Title = "Infrastructure Examples";
                host.Version = "1.0";
                EnsureResultWrapper(host);
                host.AddService(typeof(ICatalogService), typeof(CatalogService));
            }
        });

        var runTask = app.RunAsync();
        await Task.Delay(1000);

        using var http = new HttpClient();
        var swaggerJson = await http.GetStringAsync(BaseUrl + "/api/Metadata/Swagger");
        Console.WriteLine(swaggerJson.Contains("catalog") ? "Swagger document contains catalog service." : "Swagger missing catalog.");

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

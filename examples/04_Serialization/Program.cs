using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using InfiniteRefactor.Infrastructure.DataService;
using InfiniteRefactor.Infrastructure.DataService.Abstractions;
using InfiniteRefactor.Infrastructure.DataService.Bindings.Http;
using InfiniteRefactor.Infrastructure.DataService.Common.Processor;
using InfiniteRefactor.Infrastructure.Serializer;
using SerializerFactory = InfiniteRefactor.Infrastructure.Serializer.SerializerFactory;

namespace InfiniteRefactor.Infrastructure.Examples.Serialization;

internal static class Program
{
    private const string BaseUrl = "http://127.0.0.1:15004";

    private static async Task Main()
    {
        var host = DataServiceHost.Instance;
        var yaml = new YamlSerializer();
        host.DefaultSerializer = yaml;
        EnsureResultWrapper(host);
        host.AddService(typeof(IEchoService), typeof(EchoService));

        using var server = DataServiceServer.Create(BaseUrl + "/", host: host);
        server.Start();
        await Task.Delay(800);

        host.DefaultSerializer = SerializerFactory.GetDefault();
        var client = new DataServiceHttpClient(BaseUrl + "/");
        var echo = client.Create<IEchoService>();
        var payload = new EchoRequest { Message = "serializer-configured" };
        var result = await echo.EchoAsync(payload);

        Console.WriteLine($"Host default serializer was: {yaml.Name}");
        Console.WriteLine($"Echo: {result.Message}");

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

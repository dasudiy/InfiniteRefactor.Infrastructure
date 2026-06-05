using System;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using InfiniteRefactor.Infrastructure.DataService;
using InfiniteRefactor.Infrastructure.DataService.Abstractions;
using InfiniteRefactor.Infrastructure.DataService.Bindings.Http;
using InfiniteRefactor.Infrastructure.DataService.Common.Processor;

namespace InfiniteRefactor.Infrastructure.Examples.ProcessorsAndExceptions;

internal static class Program
{
    private const string BaseUrl = "http://127.0.0.1:15003";

    private static async Task Main()
    {
        var host = DataServiceHost.Instance;
        EnsureResultWrapper(host);
        host.PostProcessors.Add(new ResponseLogPostProcessor());
        host.AddService(typeof(ISecureService), typeof(SecureServiceImpl));

        using var server = DataServiceServer.Create(BaseUrl + "/", host: host);
        server.Start();
        await Task.Delay(800);

        var client = new DataServiceHttpClient(BaseUrl + "/");
        var proxy = client.Create<ISecureService>();

        client.DefaultHeaders["Token"] = "wrong";
        try
        {
            await proxy.GetSecretAsync();
            throw new InvalidOperationException("Expected unauthorized call to fail.");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Unauthorized blocked: {ex.Message}");
        }

        client.DefaultHeaders["Token"] = "secret";
        var secret = await proxy.GetSecretAsync();
        Console.WriteLine($"Authorized: {secret}");

        try
        {
            await proxy.ThrowBusinessErrorAsync();
            throw new InvalidOperationException("Expected business error.");
        }
        catch (DataServiceException ex)
        {
            Console.WriteLine($"DataServiceException: {ex.ErrorMessage} ({(int)ex.StatusCode})");
        }

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

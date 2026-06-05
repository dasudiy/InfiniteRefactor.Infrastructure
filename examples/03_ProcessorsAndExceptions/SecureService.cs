using System;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using InfiniteRefactor.Infrastructure.DataService;
using InfiniteRefactor.Infrastructure.DataService.Abstractions;
using InfiniteRefactor.Infrastructure.DataService.Abstractions.Processor;
using InfiniteRefactor.Infrastructure.DataService.Annotations;
using Microsoft.AspNetCore.Http;

namespace InfiniteRefactor.Infrastructure.Examples.ProcessorsAndExceptions;

public class TokenAuthAttribute : PreProcessorAttribute
{
    public override ProcessResult Process(DataServiceRequest request)
    {
        if (request.RawRequestObject is HttpRequest httpRequest
            && httpRequest.Headers.TryGetValue("Token", out var token)
            && token == "secret")
        {
            return ProcessResult.Default;
        }

        return new ProcessResult
        {
            CancelProcess = true,
            Message = "Unauthorized",
            SourceName = nameof(TokenAuthAttribute)
        };
    }
}

[DataService(Name = "SecureService")]
[TokenAuth]
public interface ISecureService
{
    [DataServiceMethod]
    Task<string> GetSecretAsync();

    [DataServiceMethod]
    Task<string> ThrowBusinessErrorAsync();
}

public class SecureServiceImpl : ISecureService
{
    public Task<string> GetSecretAsync() => Task.FromResult("top-secret");

    public Task<string> ThrowBusinessErrorAsync() =>
        throw new DataServiceException("Item not found") { StatusCode = HttpStatusCode.NotFound };
}

public class ResponseLogPostProcessor : IPostProcessor, IHavePriority
{
    public int Priority { get; set; } = 1;

    public ProcessResult Process(DataServiceResponse response)
    {
        var status = response.Exception == null ? "Success" : "Error";
        Console.WriteLine($"[PostProcessor] {status}");
        return ProcessResult.Default;
    }

    public Task<ProcessResult> ProcessAsync(DataServiceResponse response) =>
        Task.FromResult(Process(response));
}

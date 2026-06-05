# DataService Examples

Runnable sample programs for the **InfiniteRefactor.Infrastructure** DataService framework. Each folder is a standalone .NET 8 project with its own `README.md`.

Patterns follow production usage (contract + implementation, typed DTOs) similar to ASP.NET backends that call `UseDataService` — without dynamic `JToken` / `JObject` payloads.

Register services with `host.AddService(typeof(TContract), typeof(TImplementation))` when the `[DataService]` attribute is on the contract interface. Use `FindDataservicesFromAssembly` only when the attribute is on concrete service classes (as in the AirV backend).

## Projects

| Project | Topic |
|---------|--------|
| [01_BasicHttpHostAndClient](01_BasicHttpHostAndClient/) | Self-hosted HTTP server, dynamic client proxy |
| [02_AspNetCoreHost](02_AspNetCoreHost/) | ASP.NET Core `UseDataService` middleware |
| [03_ProcessorsAndExceptions](03_ProcessorsAndExceptions/) | Pre/post processors, `DataServiceException` |
| [04_Serialization](04_Serialization/) | Host-level serializer (YAML) |
| [05_BatchQuery](05_BatchQuery/) | Parallel RPC calls; documents `BatchInvoke` API limits |
| [06_MetadataAndSwagger](06_MetadataAndSwagger/) | Swagger metadata endpoint |
| [07_ClientPublishedService](07_ClientPublishedService/) | Client publishes services; server calls back over WebSocket |

## Build and run all

```bash
dotnet build examples/InfiniteRefactor.Infrastructure.Examples.sln
dotnet run --project examples/01_BasicHttpHostAndClient/01_BasicHttpHostAndClient.csproj
```

Run any example with `dotnet run --project examples/<folder>/<project>.csproj`.

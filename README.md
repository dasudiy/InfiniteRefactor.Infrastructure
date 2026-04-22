# InfiniteRefactor.Infrastructure

A .NET 8 infrastructure library providing a comprehensive set of utilities, extensions, and a lightweight DataService framework for building scalable backend services.

## Features

- **DataService Framework** — A protocol-agnostic RPC framework with bindings for HTTP, TCP, Named Pipes, RabbitMQ, WebSocket, and ASP.NET Core middleware.
- **Extensions** — Fluent extensions for `IEnumerable`, `Task`, `Delegate`, `Reflection`, and more.
- **Net** — HTTP client utilities with cookie management, proxy support, and custom handlers.
- **Security** — RSA/PEM cryptography and BouncyCastle-backed cipher utilities.
- **Serializer** — Unified serializer interface with implementations for JSON (Newtonsoft, System.Text.Json), XML, YAML, Binary, SOAP, and Marshal.
- **Session** — Pluggable session management for ASP.NET and standalone applications.
- **Reflection** — Emit-based fast activator, method helpers, and dynamic wrapper utilities.
- **Linq** — LinqKit integration with dynamic LINQ and predicate builder.
- **Utilities** — Snowflake ID generator, event system, gzip compression, and argument parser.
- **Config** — Unified configuration loading from JSON, environment variables, YAML, and command-line args.

## Installation

```bash
dotnet add package InfiniteRefactor.Infrastructure
```

> NuGet package coming soon. For now, reference the project directly.

## Quick Start

### DataService (HTTP)

```csharp
// Define a service
[DataService]
public class MyService
{
    [DataServiceMethod]
    public string Hello(string name) => $"Hello, {name}!";
}

// Host it
var host = new DataServiceHost();
host.AddService<MyService>();
host.UseHttpBinding("http://localhost:5000/");
await host.StartAsync();
```

### HTTP Client

```csharp
var http = new Http();
var result = await http.GetAsync<MyResponse>("https://api.example.com/data");
```

### Snowflake ID

```csharp
var worker = new IdWorker(workerId: 1, datacenterId: 1);
long id = worker.NextId();
```

## Requirements

- .NET 8.0+

## License

MIT

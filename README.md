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

## Examples

Runnable DataService samples live under [`examples/`](examples/). Open `examples/InfiniteRefactor.Infrastructure.Examples.sln` or run an individual project, for example:

```bash
dotnet run --project examples/01_BasicHttpHostAndClient/01_BasicHttpHostAndClient.csproj
```

See [examples/README.md](examples/README.md) for the full list.

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

// Create and start the server
var server = DataServiceServer.Create("http://0.0.0.0:5000/", host: host);
server.Start();
```

### HTTP Client

```csharp
var http = new Http();
var response = await http.GetAsync("https://api.example.com/data");
var content = await response.Content.ReadAsStringAsync();
```

### Snowflake ID

```csharp
var worker = new IdWorker(workerId: 1, datacenterId: 1);
long id = worker.NextId();
```

## Requirements

- .NET 8.0+

## Third-Party Credits

- **[Twitter Snowflake](https://github.com/twitter-archive/snowflake)** — Distributed ID generation algorithm by Twitter, Inc. Licensed under Apache 2.0.
- **[LinqKit](https://github.com/scottksmith95/LINQKit)** — LINQ expression utilities by [Joseph Albahari](http://www.albahari.com) and [Tomas Petricek](http://tomasp.net). Licensed under MIT.
- **[Microsoft Dynamic LINQ](https://learn.microsoft.com/en-us/dotnet/framework/data/adonet/ef/language-reference/linq-to-entities)** — Dynamic query support from Microsoft. Licensed under Ms-PL.
- **[ObjectDumper](https://github.com/microsoft/LinqSamples)** — Object graph dump utility from Microsoft LINQ samples. Licensed under Ms-PL.

## License

Mozilla Public License 2.0

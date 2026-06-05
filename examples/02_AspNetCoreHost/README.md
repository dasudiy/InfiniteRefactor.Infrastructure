# ASP.NET Core Host

Shows how to host DataService inside ASP.NET Core using `UseDataService`, the same integration pattern used in production apps:

- `RequestPath = "/api"` maps RPC endpoints under `/api/{service}/{action}`
- `InitFn` registers the contract and implementation with `AddService`
- `UseRestApi = false` keeps the classic wrapped JSON response shape

The sample starts Kestrel, calls the service through `DataServiceHttpClient`, and prints the greeting.

## Run

```bash
dotnet run --project examples/02_AspNetCoreHost/02_AspNetCoreHost.csproj
```

Expected output: `Hello, DataService!`

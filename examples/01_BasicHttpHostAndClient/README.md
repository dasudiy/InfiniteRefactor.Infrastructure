# Basic HTTP Host and Client

Standalone console program that demonstrates the core DataService workflow:

1. Mark a contract with `[DataService]` / `[DataServiceMethod]`
2. Implement the contract and register with `AddService(typeof(TContract), typeof(TImplementation))` (avoids registering a bare interface from assembly scanning)
3. Host with `DataServiceServer.Create("http://...")`
4. Call remotely through `DataServiceHttpClient` and `Create<T>()`

This mirrors production usage (interface + implementation in the same assembly) without dynamic JSON types.

## Run

```bash
dotnet run --project examples/01_BasicHttpHostAndClient/01_BasicHttpHostAndClient.csproj
```

Expected output includes `Add: 30` and `Subtract: 18`.

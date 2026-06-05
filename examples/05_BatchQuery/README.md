# Batch and Parallel Calls

`DataServiceClientBase.BatchInvoke<T>()` is the API for sending multiple argument sets in one HTTP request (`DSBatch` / `DSArgumentN`).

The server-side `BatchQueryRequest` parser in this repository currently throws `NotImplementedException`, so this sample demonstrates the same outcome with **parallel typed RPC calls** via `Task.WhenAll`.

When batch parsing is implemented, replace the parallel calls with:

```csharp
client.BatchInvoke<MultiplyResponse>("MultiplyService", "MultiplyAsync", paramsDicts);
```

## Run

```bash
dotnet run --project examples/05_BatchQuery/05_BatchQuery.csproj
```

Expected output: `6, 20, 42`

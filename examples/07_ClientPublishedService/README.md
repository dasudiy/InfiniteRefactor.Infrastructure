# Client-Published Service (Server Callback)

Demonstrates **reverse RPC**: the client hosts a DataService, connects over WebSocket, and the server calls back into that client through the active connection.

This matches the AirV pattern (`AirVApi` client host + `DataServiceContext.Current.Client.Create<T>()` in gateway services).

## Roles

| Side | Host | Service |
|------|------|---------|
| **Client** | Separate `DataServiceHost` on `WebSocketConnection` | `IWorkerService` — runs on the client machine/process |
| **Server** | `DataServiceHost` behind `UseDataService` | `IGatewayService` — receives work and delegates to `Current.Client` |

## Flow

1. Server listens for HTTP (`/api`) and WebSocket (`/ws`).
2. Client opens `WebSocketConnection` to `/ws` and registers `IWorkerService` on its own host.
3. Client calls `gateway.DelegateWorkAsync(...)` over the socket.
4. Server `GatewayService` resolves `DataServiceContext.Current.Client` (the WebSocket peer) and invokes `IWorkerService` on the client.
5. Client worker runs locally and returns the result to the server, which completes the gateway call.

## Run

```bash
dotnet run --project examples/07_ClientPublishedService/07_ClientPublishedService.csproj
```

Expected output:

```
ProcessedBy: client-worker
Output: processed:reverse-rpc
```

## Notes

- Callbacks require a **WebSocket** (or similar duplex) connection; plain HTTP-only contexts do not expose `DataServiceContext.Current.Client`.
- Use a **dedicated** `DataServiceHost` on the client for published services (do not reuse the server singleton).
- Typed DTOs only; no `JToken` / `JObject`.

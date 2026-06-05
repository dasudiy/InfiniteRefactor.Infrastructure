# Processors and DataServiceException

Demonstrates:

- **Pre-processors** via `PreProcessorAttribute` (`TokenAuthAttribute` on the service contract)
- **Global post-processors** registered on `DataServiceHost.PostProcessors`
- **`DataServiceException`** for structured business errors surfaced to the client

The program first calls without a valid token (blocked), then with `Token: secret`, then triggers a `DataServiceException` with HTTP 404.

## Run

```bash
dotnet run --project examples/03_ProcessorsAndExceptions/03_ProcessorsAndExceptions.csproj
```

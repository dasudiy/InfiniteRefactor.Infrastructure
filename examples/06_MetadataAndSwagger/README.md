# Metadata and Swagger

Enables built-in API documentation via `EnableMetadataService = true` on `DataServiceOption`.

The sample fetches `/api/Metadata/Swagger` and verifies the generated document includes the `catalog` service.

Set `DataServiceHost.Title` and `Version` to populate Swagger `info`.

## Run

```bash
dotnet run --project examples/06_MetadataAndSwagger/06_MetadataAndSwagger.csproj
```

Expected output: `Swagger document contains catalog service.`

## Other protocols (TCP, RabbitMQ, named pipes)

These bindings are available via `DataServiceServer.Create(...)` with prefixes such as `tcp://`, `amqp://`, and `np://`. They require external infrastructure (broker, listeners) and are not exercised in automated examples here. Use example **01** for a self-contained HTTP setup.

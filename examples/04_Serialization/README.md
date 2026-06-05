# Serialization

Assigns `YamlSerializer` to `DataServiceHost.DefaultSerializer`, then switches back to the default JSON serializer before the client call so the sample runs without mixed-format issues. In production you would align host and client formatters (for example `Create<T>("yaml")`).

## Run

```bash
dotnet run --project examples/04_Serialization/04_Serialization.csproj
```

Expected output: `Echo: yaml-roundtrip`

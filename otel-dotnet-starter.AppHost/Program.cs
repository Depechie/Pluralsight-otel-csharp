var builder = DistributedApplication.CreateBuilder(args);

var rabbitMQ = builder.AddRabbitMQ("messaging")
    .WithManagementPlugin()
    .PublishAsContainer();

var otellgtm = builder.AddContainer("otel-lgtm", "grafana/otel-lgtm", "0.11.3")
    .WithEndpoint(targetPort: 4317, port: 4317, name: "grpc", scheme: "http") // Have to put the schema to HTTP otherwise the C# will complain about the OTEL_EXPORTER_OTLP_ENDPOINT variable
    .WithEndpoint(targetPort: 3000, port: 3000, name: "http", scheme: "http");

var apiService = builder.AddProject<Projects.otel_dotnet_starter_ApiService>("apiservice")
    .WithEnvironment("OTEL_EXPORTER_OTLP_ENDPOINT", otellgtm.GetEndpoint("grpc"))
    .WaitFor(otellgtm)
    .WithReference(rabbitMQ)
    .WaitFor(rabbitMQ);

builder.AddProject<Projects.otel_dotnet_starter_Web>("webfrontend")
    .WithEnvironment("OTEL_EXPORTER_OTLP_ENDPOINT", otellgtm.GetEndpoint("grpc"))
    .WithExternalHttpEndpoints()
    .WithReference(apiService)
    .WaitFor(apiService);

builder.AddProject<Projects.otel_dotnet_starter_WorkerService>("serviceworker")
    .WithEnvironment("OTEL_EXPORTER_OTLP_ENDPOINT", otellgtm.GetEndpoint("grpc"))
    .WithReference(rabbitMQ)
    .WaitFor(rabbitMQ);

builder.Build().Run();

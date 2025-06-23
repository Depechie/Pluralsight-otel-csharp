using otel_dotnet_starter.WorkerService;

var builder = Host.CreateApplicationBuilder(args);

builder.AddServiceDefaults();
builder.AddRabbitMQClient("messaging");

builder.Services.AddHostedService<Worker>();

var host = builder.Build();
host.Run();

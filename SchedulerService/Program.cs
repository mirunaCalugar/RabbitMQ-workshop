using SchedulerService.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddGrpc();

var app = builder.Build();

// pornește consumerul RabbitMQ asincron
var consumer = new RabbitMqConsumer();
await consumer.StartAsync();

app.MapGrpcService<TaskSchedulerService>();
app.MapGet("/", () => "SchedulerService is running...");

await app.RunAsync();

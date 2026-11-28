using Confluent.Kafka;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using VetClinic.Generator;

var builder = Host.CreateApplicationBuilder(args);

var bootstrapServers = builder.Configuration.GetConnectionString("kafka")
    ?? throw new InvalidOperationException("Не задан адрес подключения к Kafka");

builder.Services.AddSingleton(new ProducerConfig
{
    BootstrapServers = bootstrapServers,
    EnableIdempotence = true,
    MessageTimeoutMs = 5000
});
builder.Services.AddSingleton<AppointmentGenerator>();
builder.Services.AddHostedService<GeneratorWorker>();

await builder.Build().RunAsync();

using Confluent.Kafka;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using VetClinic.Generator;

var builder = Host.CreateApplicationBuilder(new HostApplicationBuilderSettings
{
    Args = args,
    ContentRootPath = AppContext.BaseDirectory
});

var bootstrapServers = builder.Configuration.GetConnectionString("kafka")
    ?? throw new InvalidOperationException("Не задан адрес подключения к Kafka");

builder.Services.AddSingleton(new ProducerConfig
{
    BootstrapServers = bootstrapServers,
    AllowAutoCreateTopics = false,
    EnableIdempotence = true,
    MessageTimeoutMs = 5000
});
builder.Services.AddOptions<GenerationOptions>()
    .Bind(builder.Configuration.GetRequiredSection("Generation"))
    .Validate(options => options.IntervalSeconds > 0 && options.IntervalSeconds <= (uint.MaxValue - 1) / 1000,
        "Интервал генерации должен быть положительным и допустимым для таймера")
    .Validate(options => options.BatchSize > 0, "Размер пачки должен быть положительным")
    .ValidateOnStart();
builder.Services.AddSingleton<AppointmentGenerator>();
builder.Services.AddHostedService<GeneratorWorker>();

await builder.Build().RunAsync();

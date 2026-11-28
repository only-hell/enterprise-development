using System.Text.Json;
using Confluent.Kafka;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using VetClinic.Contracts.Messages;

namespace VetClinic.Generator;

/// <summary>
/// Публикует новые записи на приём и повторяет отправку при сбое Kafka
/// </summary>
/// <param name="generator">Генератор записей</param>
/// <param name="config">Настройки продюсера Kafka</param>
/// <param name="options">Интервал генерации и размер пачки</param>
/// <param name="logger">Журнал генератора</param>
internal sealed class GeneratorWorker(
    AppointmentGenerator generator,
    ProducerConfig config,
    IOptions<GenerationOptions> options,
    ILogger<GeneratorWorker> logger) : BackgroundService
{
    /// <inheritdoc/>
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var producer = new ProducerBuilder<string, string>(config).Build();
        var generation = options.Value;
        var interval = TimeSpan.FromSeconds(generation.IntervalSeconds);

        while (!stoppingToken.IsCancellationRequested)
        {
            for (var index = 0; index < generation.BatchSize; index++)
            {
                stoppingToken.ThrowIfCancellationRequested();
                var contract = generator.Generate();
                var message = new Message<string, string>
                {
                    Key = contract.ExternalId.ToString(),
                    Value = JsonSerializer.Serialize(contract, JsonSerializerOptions.Web)
                };

                await PublishAsync(producer, message, stoppingToken);
                logger.LogInformation("Отправлена запись на приём {ExternalId}", contract.ExternalId);
            }

            await Task.Delay(interval, stoppingToken);
        }
    }

    /// <summary>
    /// Отправить то же сообщение после ошибки доставки
    /// </summary>
    /// <param name="producer">Продюсер Kafka</param>
    /// <param name="message">Сообщение с записью</param>
    /// <param name="cancellationToken">Токен отмены</param>
    private async Task PublishAsync(
        IProducer<string, string> producer,
        Message<string, string> message,
        CancellationToken cancellationToken)
    {
        var delaySeconds = 1;
        while (true)
        {
            try
            {
                await producer.ProduceAsync(AppointmentContract.Topic, message, cancellationToken);
                return;
            }
            catch (KafkaException exception) when (!exception.Error.IsFatal)
            {
                logger.LogWarning(exception,
                    "Kafka не подтвердила запись {ExternalId}. Повтор через {DelaySeconds} с",
                    message.Key, delaySeconds);
                await Task.Delay(TimeSpan.FromSeconds(delaySeconds), cancellationToken);
                delaySeconds = Math.Min(delaySeconds * 2, 30);
            }
        }
    }
}

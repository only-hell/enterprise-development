using System.Text.Json;
using Confluent.Kafka;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using VetClinic.Contracts.Messages;

namespace VetClinic.Generator;

/// <summary>
/// Публикует новые записи на приём и повторяет отправку при сбое Kafka
/// </summary>
/// <param name="generator">Генератор записей</param>
/// <param name="config">Настройки продюсера Kafka</param>
/// <param name="logger">Журнал генератора</param>
internal sealed class GeneratorWorker(
    AppointmentGenerator generator,
    ProducerConfig config,
    ILogger<GeneratorWorker> logger) : BackgroundService
{
    private static readonly TimeSpan GenerationInterval = TimeSpan.FromSeconds(10);

    /// <inheritdoc/>
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var producer = new ProducerBuilder<string, string>(config).Build();

        while (!stoppingToken.IsCancellationRequested)
        {
            var contract = generator.Generate();
            var message = new Message<string, string>
            {
                Key = contract.ExternalId.ToString(),
                Value = JsonSerializer.Serialize(contract, JsonSerializerOptions.Web)
            };

            await PublishAsync(producer, message, stoppingToken);
            logger.LogInformation("Отправлена запись на приём {ExternalId}", contract.ExternalId);
            await Task.Delay(GenerationInterval, stoppingToken);
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

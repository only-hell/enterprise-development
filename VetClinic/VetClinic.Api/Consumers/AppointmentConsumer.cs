using System.ComponentModel.DataAnnotations;
using System.Text;
using System.Text.Json;
using Confluent.Kafka;
using Confluent.Kafka.Admin;
using VetClinic.Application.Services;
using VetClinic.Contracts.Messages;

namespace VetClinic.Api.Consumers;

/// <summary>
/// Читает записи из Kafka и подтверждает обработку после сохранения или отклонения контракта
/// </summary>
/// <param name="config">Настройки консьюмера</param>
/// <param name="scopeFactory">Создаёт отдельный контекст базы для каждого сообщения</param>
/// <param name="logger">Журнал консьюмера</param>
internal sealed class AppointmentConsumer(
    ConsumerConfig config,
    IServiceScopeFactory scopeFactory,
    ILogger<AppointmentConsumer> logger) : BackgroundService
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web)
    {
        RespectNullableAnnotations = true,
        RespectRequiredConstructorParameters = true
    };

    /// <inheritdoc/>
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var rejectedMessages = new ProducerBuilder<string, string>(new ProducerConfig
        {
            BootstrapServers = config.BootstrapServers,
            AllowAutoCreateTopics = false,
            EnableIdempotence = true,
            MessageTimeoutMs = 5000
        }).Build();

        var delaySeconds = 1;
        while (!stoppingToken.IsCancellationRequested)
        {
            string? messageKey = null;
            try
            {
                await EnsureTopicsAsync(stoppingToken);
                using var consumer = new ConsumerBuilder<string, string>(config)
                    .SetErrorHandler((_, error) =>
                    {
                        if (error.Code == ErrorCode.Local_AllBrokersDown)
                        {
                            logger.LogWarning("Kafka недоступна. Консьюмер повторяет подключение");
                        }
                    })
                    .Build();
                try
                {
                    consumer.Subscribe(AppointmentContract.Topic);
                    while (!stoppingToken.IsCancellationRequested)
                    {
                        var record = consumer.Consume(stoppingToken);
                        messageKey = record.Message.Key;
                        try
                        {
                            await ImportAsync(record.Message, stoppingToken);
                        }
                        catch (Exception exception) when (exception is JsonException or ValidationException)
                        {
                            await SendToDeadLetterAsync(rejectedMessages, record, stoppingToken);
                            logger.LogWarning("Запись {Key} перенесена в {Topic}: {Reason}",
                                messageKey, AppointmentContract.DeadLetterTopic, exception.Message);
                        }

                        consumer.Commit(record);
                        messageKey = null;
                        delaySeconds = 1;
                    }
                }
                finally
                {
                    consumer.Close();
                }
            }
            catch (Exception exception) when (!stoppingToken.IsCancellationRequested)
            {
                logger.LogWarning(exception, "Импорт из Kafka прерван. Ключ записи {Key}. Повтор через {DelaySeconds} с",
                    messageKey, delaySeconds);
                await Task.Delay(TimeSpan.FromSeconds(delaySeconds), stoppingToken);
                delaySeconds = Math.Min(delaySeconds * 2, 30);
            }
        }
    }

    /// <summary>
    /// Создать топики записей и отклонённых сообщений перед подпиской
    /// </summary>
    /// <param name="cancellationToken">Токен отмены</param>
    private async Task EnsureTopicsAsync(CancellationToken cancellationToken)
    {
        using var admin = new AdminClientBuilder(new AdminClientConfig
        {
            BootstrapServers = config.BootstrapServers
        }).Build();
        try
        {
            await admin.CreateTopicsAsync(
                [
                    new TopicSpecification { Name = AppointmentContract.Topic, NumPartitions = 1, ReplicationFactor = 1 },
                    new TopicSpecification { Name = AppointmentContract.DeadLetterTopic, NumPartitions = 1, ReplicationFactor = 1 }
                ],
                new CreateTopicsOptions { RequestTimeout = TimeSpan.FromSeconds(5) }).WaitAsync(cancellationToken);
        }
        catch (CreateTopicsException exception) when (exception.Results.All(
            result => result.Error.Code is ErrorCode.NoError or ErrorCode.TopicAlreadyExists))
        {
        }
    }

    /// <summary>
    /// Сохранить исходное сообщение и его позицию в топике отклонённых сообщений
    /// </summary>
    /// <param name="producer">Продюсер Kafka</param>
    /// <param name="record">Исходная запись</param>
    /// <param name="cancellationToken">Токен отмены</param>
    private static Task SendToDeadLetterAsync(
        IProducer<string, string> producer, ConsumeResult<string, string> record, CancellationToken cancellationToken) =>
        producer.ProduceAsync(AppointmentContract.DeadLetterTopic, new Message<string, string>
        {
            Key = record.Message.Key,
            Value = record.Message.Value,
            Headers = new Headers { { "source-offset", Encoding.UTF8.GetBytes(record.TopicPartitionOffset.ToString()) } }
        }, cancellationToken);

    /// <summary>
    /// Передать сообщение в сервис импорта и дождаться завершения транзакции
    /// </summary>
    /// <param name="message">Сообщение Kafka</param>
    /// <param name="cancellationToken">Токен отмены</param>
    private async Task ImportAsync(Message<string, string> message, CancellationToken cancellationToken)
    {
        var contract = JsonSerializer.Deserialize<AppointmentContract>(message.Value ?? string.Empty, JsonOptions)
            ?? throw new JsonException("Сообщение не содержит записи на приём");

        await using var scope = scopeFactory.CreateAsyncScope();
        var importer = scope.ServiceProvider.GetRequiredService<AppointmentImportService>();
        if (await importer.ImportAsync(contract, cancellationToken))
        {
            logger.LogInformation("Сохранена запись на приём {ExternalId}", contract.ExternalId);
        }
        else
        {
            logger.LogInformation("Запись на приём {ExternalId} уже сохранена", contract.ExternalId);
        }
    }
}

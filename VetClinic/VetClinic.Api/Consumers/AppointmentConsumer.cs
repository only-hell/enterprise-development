using System.ComponentModel.DataAnnotations;
using System.Text.Json;
using Confluent.Kafka;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using VetClinic.Application.Exceptions;
using VetClinic.Application.Services;
using VetClinic.Contracts.Messages;

namespace VetClinic.Api.Consumers;

/// <summary>
/// Читает записи на приём из Kafka и подтверждает обработку после сохранения
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
        var delaySeconds = 1;
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
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
                        await ImportAsync(record.Message, stoppingToken);
                        consumer.Commit(record);
                        delaySeconds = 1;
                    }
                }
                finally
                {
                    consumer.Close();
                }
            }
            catch (Exception exception) when (!stoppingToken.IsCancellationRequested &&
                exception is KafkaException or DbUpdateException or SqlException or InvalidOperationException)
            {
                logger.LogWarning(exception, "Импорт из Kafka прерван. Повтор через {DelaySeconds} с", delaySeconds);
                await Task.Delay(TimeSpan.FromSeconds(delaySeconds), stoppingToken);
                delaySeconds = Math.Min(delaySeconds * 2, 30);
            }
        }
    }

    /// <summary>
    /// Передать сообщение в сервис импорта или отклонить некорректные данные
    /// </summary>
    /// <param name="message">Сообщение Kafka</param>
    /// <param name="cancellationToken">Токен отмены</param>
    private async Task ImportAsync(Message<string, string> message, CancellationToken cancellationToken)
    {
        try
        {
            var contract = JsonSerializer.Deserialize<AppointmentContract>(message.Value ?? string.Empty, JsonOptions);
            if (contract is null)
            {
                throw new ValidationException("Сообщение не содержит записи на приём");
            }

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
        catch (Exception exception) when (exception is JsonException or ValidationException or KeyNotFoundException
            or InvalidPetBreedException)
        {
            logger.LogWarning("Запись на приём {Key} отклонена: {Reason}", message.Key, exception.Message);
        }
        catch (DbUpdateException exception) when (exception.InnerException is SqlException { Number: 2601 or 2627 })
        {
            logger.LogWarning("Запись на приём {Key} отклонена: нарушено ограничение уникальности", message.Key);
        }
    }
}

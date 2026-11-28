namespace VetClinic.Infrastructure.SqlServer;

/// <summary>
/// Отметка о сохранённом контракте генератора
/// </summary>
/// <param name="ExternalId">Идентификатор контракта</param>
/// <param name="ProcessedAt">Время обработки</param>
internal sealed record ProcessedContract(Guid ExternalId, DateTimeOffset ProcessedAt);

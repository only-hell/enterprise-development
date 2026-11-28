namespace VetClinic.Domain.Repositories;

/// <summary>
/// Отслеживает записи на приём, уже принятые от генератора
/// </summary>
public interface IProcessedContractRepository
{
    /// <summary>
    /// Проверить, был ли сохранён контракт
    /// </summary>
    /// <param name="externalId">Идентификатор контракта</param>
    /// <param name="cancellationToken">Токен отмены</param>
    Task<bool> ExistsAsync(Guid externalId, CancellationToken cancellationToken);

    /// <summary>
    /// Отметить контракт как обработанный в текущей транзакции
    /// </summary>
    /// <param name="externalId">Идентификатор контракта</param>
    /// <param name="cancellationToken">Токен отмены</param>
    Task AddAsync(Guid externalId, CancellationToken cancellationToken);
}

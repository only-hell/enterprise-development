using Microsoft.EntityFrameworkCore;
using VetClinic.Domain.Repositories;

namespace VetClinic.Infrastructure.SqlServer.Repositories;

/// <summary>
/// Репозиторий отметок об обработке контрактов в SQL Server
/// </summary>
/// <param name="context">Контекст базы данных</param>
/// <param name="timeProvider">Источник текущего времени</param>
public sealed class ProcessedContractRepository(VetClinicDbContext context, TimeProvider timeProvider)
    : IProcessedContractRepository
{
    /// <inheritdoc/>
    public Task<bool> ExistsAsync(Guid externalId, CancellationToken cancellationToken) =>
        context.Set<ProcessedContract>().AnyAsync(contract => contract.ExternalId == externalId, cancellationToken);

    /// <inheritdoc/>
    public async Task AddAsync(Guid externalId, CancellationToken cancellationToken)
    {
        context.Add(new ProcessedContract(externalId, timeProvider.GetUtcNow()));
        await context.SaveChangesAsync(cancellationToken);
    }
}

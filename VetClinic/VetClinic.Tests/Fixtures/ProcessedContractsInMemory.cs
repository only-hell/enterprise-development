using VetClinic.Domain.Repositories;

namespace VetClinic.Tests.Fixtures;

/// <summary>
/// Хранит отметки об обработке в памяти на время теста
/// </summary>
internal sealed class ProcessedContractsInMemory : IProcessedContractRepository
{
    private readonly HashSet<Guid> _ids = [];

    /// <inheritdoc/>
    public Task<bool> ExistsAsync(Guid externalId, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return Task.FromResult(_ids.Contains(externalId));
    }

    /// <inheritdoc/>
    public Task AddAsync(Guid externalId, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        _ids.Add(externalId);
        return Task.CompletedTask;
    }
}

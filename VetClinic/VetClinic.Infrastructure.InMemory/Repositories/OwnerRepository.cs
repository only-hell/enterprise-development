using VetClinic.Domain.Models;
using VetClinic.Domain.Repositories;
using VetClinic.Domain.Seeds;

namespace VetClinic.Infrastructure.InMemory.Repositories;

/// <summary>
/// Репозиторий владельцев в памяти
/// </summary>
/// <param name="seed">Начальные данные</param>
public class OwnerRepository(VetClinicSeed seed) : InMemoryRepository<Owner>(seed.Owners), IOwnerRepository
{
    /// <inheritdoc/>
    protected override int GetId(Owner entity) => entity.Id;

    /// <inheritdoc/>
    protected override void SetId(Owner entity, int id) => entity.Id = id;
}

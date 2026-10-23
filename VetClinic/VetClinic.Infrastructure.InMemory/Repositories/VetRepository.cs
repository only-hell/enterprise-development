using VetClinic.Domain.Models;
using VetClinic.Domain.Repositories;
using VetClinic.Domain.Seeds;

namespace VetClinic.Infrastructure.InMemory.Repositories;

/// <summary>
/// Репозиторий ветеринаров в памяти
/// </summary>
/// <param name="seed">Начальные данные</param>
public class VetRepository(VetClinicSeed seed) : InMemoryRepository<Vet>(seed.Vets), IVetRepository
{
    /// <inheritdoc/>
    protected override int GetId(Vet entity) => entity.Id;

    /// <inheritdoc/>
    protected override void SetId(Vet entity, int id) => entity.Id = id;
}

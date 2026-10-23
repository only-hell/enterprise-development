using VetClinic.Domain.Models;
using VetClinic.Domain.Repositories;
using VetClinic.Domain.Seeds;

namespace VetClinic.Infrastructure.InMemory.Repositories;

/// <summary>
/// Репозиторий питомцев в памяти
/// </summary>
/// <param name="seed">Начальные данные</param>
public class PetRepository(VetClinicSeed seed) : InMemoryRepository<Pet>(seed.Pets), IPetRepository
{
    /// <inheritdoc/>
    protected override int GetId(Pet entity) => entity.Id;

    /// <inheritdoc/>
    protected override void SetId(Pet entity, int id) => entity.Id = id;
}

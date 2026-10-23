using VetClinic.Application.Mapping;
using VetClinic.Contracts.Dto.Breeds;
using VetClinic.Contracts.Services;
using VetClinic.Domain.Repositories;

namespace VetClinic.Application.Services;

/// <summary>
/// Сервис пород, при удалении породы у питомцев порода сбрасывается
/// </summary>
/// <param name="breeds">Репозиторий пород</param>
/// <param name="pets">Репозиторий питомцев</param>
public class BreedService(
    IBreedRepository breeds,
    IPetRepository pets) : IBreedService
{
    /// <inheritdoc/>
    public async Task<IReadOnlyList<BreedDto>> GetAllAsync()
    {
        var all = await breeds.GetAllAsync();
        return [.. all.Select(b => b.ToDto())];
    }

    /// <inheritdoc/>
    public async Task<BreedDto?> GetByIdAsync(int id)
    {
        var breed = await breeds.GetByIdAsync(id);
        return breed?.ToDto();
    }

    /// <inheritdoc/>
    public async Task<BreedDto> CreateAsync(BreedEditDto dto)
    {
        var created = await breeds.CreateAsync(dto.ToEntity());
        return created.ToDto();
    }

    /// <inheritdoc/>
    public async Task<BreedDto?> UpdateAsync(int id, BreedEditDto dto)
    {
        var breed = await breeds.GetByIdAsync(id);
        if (breed is null)
        {
            return null;
        }

        breed.Apply(dto);
        await breeds.UpdateAsync(breed);
        return breed.ToDto();
    }

    /// <inheritdoc/>
    public async Task DeleteAsync(int id)
    {
        var allPets = await pets.GetAllAsync();
        foreach (var pet in allPets.Where(p => p.Breed?.Id == id))
        {
            pet.Breed = null;
            await pets.UpdateAsync(pet);
        }
        await breeds.DeleteAsync(id);
    }
}

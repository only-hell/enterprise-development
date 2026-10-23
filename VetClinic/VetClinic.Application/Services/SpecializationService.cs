using VetClinic.Application.Mapping;
using VetClinic.Contracts.Dto.Specializations;
using VetClinic.Contracts.Services;
using VetClinic.Domain.Repositories;

namespace VetClinic.Application.Services;

/// <summary>
/// Сервис специализаций врачей, при удалении специализации у врачей она сбрасывается
/// </summary>
/// <param name="specializations">Репозиторий специализаций</param>
/// <param name="vets">Репозиторий ветеринаров</param>
public class SpecializationService(
    ISpecializationRepository specializations,
    IVetRepository vets) : ISpecializationService
{
    /// <inheritdoc/>
    public async Task<IReadOnlyList<SpecializationDto>> GetAllAsync()
    {
        var all = await specializations.GetAllAsync();
        return [.. all.Select(s => s.ToDto())];
    }

    /// <inheritdoc/>
    public async Task<SpecializationDto?> GetByIdAsync(int id)
    {
        var specialization = await specializations.GetByIdAsync(id);
        return specialization?.ToDto();
    }

    /// <inheritdoc/>
    public async Task<SpecializationDto> CreateAsync(SpecializationEditDto dto)
    {
        var created = await specializations.CreateAsync(dto.ToEntity());
        return created.ToDto();
    }

    /// <inheritdoc/>
    public async Task<SpecializationDto?> UpdateAsync(int id, SpecializationEditDto dto)
    {
        var specialization = await specializations.GetByIdAsync(id);
        if (specialization is null)
        {
            return null;
        }

        specialization.Apply(dto);
        await specializations.UpdateAsync(specialization);
        return specialization.ToDto();
    }

    /// <inheritdoc/>
    public async Task DeleteAsync(int id)
    {
        var allVets = await vets.GetAllAsync();
        foreach (var vet in allVets.Where(v => v.Specialization?.Id == id))
        {
            vet.Specialization = null;
            await vets.UpdateAsync(vet);
        }
        await specializations.DeleteAsync(id);
    }
}

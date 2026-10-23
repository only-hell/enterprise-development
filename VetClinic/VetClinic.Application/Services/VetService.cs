using VetClinic.Application.Mapping;
using VetClinic.Contracts.Dto.Vets;
using VetClinic.Contracts.Services;
using VetClinic.Domain.Models;
using VetClinic.Domain.Repositories;

namespace VetClinic.Application.Services;

/// <summary>
/// Сервис ветеринаров, при удалении врача удаляются его записи на прием
/// </summary>
/// <param name="vets">Репозиторий ветеринаров</param>
/// <param name="specializations">Репозиторий специализаций</param>
/// <param name="appointments">Репозиторий записей на прием</param>
public class VetService(
    IVetRepository vets,
    ISpecializationRepository specializations,
    IAppointmentRepository appointments) : IVetService
{
    /// <inheritdoc/>
    public async Task<IReadOnlyList<VetDto>> GetAllAsync()
    {
        var all = await vets.GetAllAsync();
        return [.. all.Select(v => v.ToDto())];
    }

    /// <inheritdoc/>
    public async Task<VetDto?> GetByIdAsync(int id)
    {
        var vet = await vets.GetByIdAsync(id);
        return vet?.ToDto();
    }

    /// <inheritdoc/>
    public async Task<VetDto> CreateAsync(VetEditDto dto)
    {
        var vet = dto.ToEntity();
        vet.Specialization = await GetSpecializationAsync(dto.SpecializationId);

        var created = await vets.CreateAsync(vet);
        return created.ToDto();
    }

    /// <inheritdoc/>
    public async Task<VetDto?> UpdateAsync(int id, VetEditDto dto)
    {
        var vet = await vets.GetByIdAsync(id);
        if (vet is null)
        {
            return null;
        }

        var specialization = await GetSpecializationAsync(dto.SpecializationId);
        vet.Apply(dto);
        vet.Specialization = specialization;
        await vets.UpdateAsync(vet);
        return vet.ToDto();
    }

    /// <inheritdoc/>
    public async Task DeleteAsync(int id)
    {
        var allAppointments = await appointments.GetAllAsync();
        foreach (var appointment in allAppointments.Where(a => a.Vet?.Id == id))
        {
            await appointments.DeleteAsync(appointment.Id);
        }
        await vets.DeleteAsync(id);
    }

    /// <summary>
    /// Найти специализацию, если она указана, или сообщить, что её нет
    /// </summary>
    /// <param name="specializationId">Идентификатор специализации или null</param>
    /// <exception cref="KeyNotFoundException">Специализация указана, но не найдена</exception>
    private async Task<Specialization?> GetSpecializationAsync(int? specializationId)
    {
        if (specializationId is null)
        {
            return null;
        }
        return await specializations.GetByIdAsync(specializationId.Value)
            ?? throw new KeyNotFoundException($"Специализация с идентификатором {specializationId} не найдена");
    }
}

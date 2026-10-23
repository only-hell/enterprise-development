using VetClinic.Application.Mapping;
using VetClinic.Contracts.Dto.Owners;
using VetClinic.Contracts.Services;
using VetClinic.Domain.Repositories;

namespace VetClinic.Application.Services;

/// <summary>
/// Сервис владельцев, при удалении владельца удаляются его питомцы и их записи на прием
/// </summary>
/// <param name="owners">Репозиторий владельцев</param>
/// <param name="pets">Репозиторий питомцев</param>
/// <param name="appointments">Репозиторий записей на прием</param>
public class OwnerService(
    IOwnerRepository owners,
    IPetRepository pets,
    IAppointmentRepository appointments) : IOwnerService
{
    /// <inheritdoc/>
    public async Task<IReadOnlyList<OwnerDto>> GetAllAsync()
    {
        var all = await owners.GetAllAsync();
        return [.. all.Select(o => o.ToDto())];
    }

    /// <inheritdoc/>
    public async Task<OwnerDto?> GetByIdAsync(int id)
    {
        var owner = await owners.GetByIdAsync(id);
        return owner?.ToDto();
    }

    /// <inheritdoc/>
    public async Task<OwnerDto> CreateAsync(OwnerEditDto dto)
    {
        var created = await owners.CreateAsync(dto.ToEntity());
        return created.ToDto();
    }

    /// <inheritdoc/>
    public async Task<OwnerDto?> UpdateAsync(int id, OwnerEditDto dto)
    {
        var owner = await owners.GetByIdAsync(id);
        if (owner is null)
        {
            return null;
        }

        owner.Apply(dto);
        await owners.UpdateAsync(owner);
        return owner.ToDto();
    }

    /// <inheritdoc/>
    public async Task DeleteAsync(int id)
    {
        var owner = await owners.GetByIdAsync(id);
        if (owner is null)
        {
            return;
        }

        var petIds = owner.Pets.Select(p => p.Id).ToHashSet();
        var allAppointments = await appointments.GetAllAsync();
        foreach (var appointment in allAppointments.Where(a => a.Pet is not null && petIds.Contains(a.Pet.Id)))
        {
            await appointments.DeleteAsync(appointment.Id);
        }
        foreach (var petId in petIds)
        {
            await pets.DeleteAsync(petId);
        }
        await owners.DeleteAsync(id);
    }
}

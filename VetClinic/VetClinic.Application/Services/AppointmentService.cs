using VetClinic.Application.Mapping;
using VetClinic.Contracts.Dto.Appointments;
using VetClinic.Contracts.Services;
using VetClinic.Domain.Models;
using VetClinic.Domain.Repositories;

namespace VetClinic.Application.Services;

/// <summary>
/// Сервис записей на прием
/// </summary>
/// <param name="appointments">Репозиторий записей на прием</param>
/// <param name="pets">Репозиторий питомцев</param>
/// <param name="vets">Репозиторий ветеринаров</param>
public class AppointmentService(
    IAppointmentRepository appointments,
    IPetRepository pets,
    IVetRepository vets) : IAppointmentService
{
    /// <inheritdoc/>
    public async Task<IReadOnlyList<AppointmentDto>> GetAllAsync()
    {
        var all = await appointments.GetAllAsync();
        return [.. all.Select(a => a.ToDto())];
    }

    /// <inheritdoc/>
    public async Task<AppointmentDto?> GetByIdAsync(int id)
    {
        var appointment = await appointments.GetByIdAsync(id);
        return appointment?.ToDto();
    }

    /// <inheritdoc/>
    public async Task<AppointmentDto> CreateAsync(AppointmentEditDto dto)
    {
        var appointment = dto.ToEntity();
        appointment.Pet = await GetPetAsync(dto.PetId);
        appointment.Vet = await GetVetAsync(dto.VetId);

        var created = await appointments.CreateAsync(appointment);
        return created.ToDto();
    }

    /// <inheritdoc/>
    public async Task<AppointmentDto?> UpdateAsync(int id, AppointmentEditDto dto)
    {
        var appointment = await appointments.GetByIdAsync(id);
        if (appointment is null)
        {
            return null;
        }

        var pet = await GetPetAsync(dto.PetId);
        var vet = await GetVetAsync(dto.VetId);
        appointment.Apply(dto);
        appointment.Pet = pet;
        appointment.Vet = vet;
        await appointments.UpdateAsync(appointment);
        return appointment.ToDto();
    }

    /// <inheritdoc/>
    public async Task DeleteAsync(int id) => await appointments.DeleteAsync(id);

    /// <summary>
    /// Найти питомца или сообщить, что его нет
    /// </summary>
    /// <param name="petId">Идентификатор питомца</param>
    /// <exception cref="KeyNotFoundException">Питомец не найден</exception>
    private async Task<Pet> GetPetAsync(int petId) =>
        await pets.GetByIdAsync(petId)
        ?? throw new KeyNotFoundException($"Питомец с идентификатором {petId} не найден");

    /// <summary>
    /// Найти ветеринара или сообщить, что его нет
    /// </summary>
    /// <param name="vetId">Идентификатор ветеринара</param>
    /// <exception cref="KeyNotFoundException">Ветеринар не найден</exception>
    private async Task<Vet> GetVetAsync(int vetId) =>
        await vets.GetByIdAsync(vetId)
        ?? throw new KeyNotFoundException($"Ветеринар с идентификатором {vetId} не найден");
}

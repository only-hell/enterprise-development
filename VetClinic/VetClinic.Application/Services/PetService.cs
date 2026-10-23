using VetClinic.Application.Mapping;
using VetClinic.Contracts.Dto.Pets;
using VetClinic.Contracts.Services;
using VetClinic.Domain.Models;
using VetClinic.Domain.Repositories;

namespace VetClinic.Application.Services;

/// <summary>
/// Сервис питомцев, поддерживает список питомцев у владельца в актуальном состоянии
/// </summary>
/// <param name="pets">Репозиторий питомцев</param>
/// <param name="owners">Репозиторий владельцев</param>
/// <param name="breeds">Репозиторий пород</param>
/// <param name="appointments">Репозиторий записей на прием</param>
public class PetService(
    IPetRepository pets,
    IOwnerRepository owners,
    IBreedRepository breeds,
    IAppointmentRepository appointments) : IPetService
{
    /// <inheritdoc/>
    public async Task<IReadOnlyList<PetDto>> GetAllAsync()
    {
        var all = await pets.GetAllAsync();
        return [.. all.Select(p => p.ToDto())];
    }

    /// <inheritdoc/>
    public async Task<PetDto?> GetByIdAsync(int id)
    {
        var pet = await pets.GetByIdAsync(id);
        return pet?.ToDto();
    }

    /// <inheritdoc/>
    public async Task<PetDto> CreateAsync(PetEditDto dto)
    {
        var owner = await GetOwnerAsync(dto.OwnerId);
        var breed = await GetBreedAsync(dto.BreedId);

        var pet = dto.ToEntity();
        pet.Owner = owner;
        pet.Breed = breed;

        var created = await pets.CreateAsync(pet);
        owner.Pets.Add(created);
        await owners.UpdateAsync(owner);
        return created.ToDto();
    }

    /// <inheritdoc/>
    public async Task<PetDto?> UpdateAsync(int id, PetEditDto dto)
    {
        var pet = await pets.GetByIdAsync(id);
        if (pet is null)
        {
            return null;
        }

        var newOwner = await GetOwnerAsync(dto.OwnerId);
        var breed = await GetBreedAsync(dto.BreedId);

        var oldOwner = pet.Owner;
        if (oldOwner is not null && oldOwner.Id != newOwner.Id)
        {
            oldOwner.Pets.Remove(pet);
            await owners.UpdateAsync(oldOwner);
        }
        if (!newOwner.Pets.Contains(pet))
        {
            newOwner.Pets.Add(pet);
            await owners.UpdateAsync(newOwner);
        }

        pet.Apply(dto);
        pet.Owner = newOwner;
        pet.Breed = breed;
        await pets.UpdateAsync(pet);
        return pet.ToDto();
    }

    /// <inheritdoc/>
    public async Task DeleteAsync(int id)
    {
        var pet = await pets.GetByIdAsync(id);
        if (pet is null)
        {
            return;
        }

        var allAppointments = await appointments.GetAllAsync();
        foreach (var appointment in allAppointments.Where(a => a.Pet?.Id == id))
        {
            await appointments.DeleteAsync(appointment.Id);
        }
        if (pet.Owner is not null)
        {
            pet.Owner.Pets.Remove(pet);
            await owners.UpdateAsync(pet.Owner);
        }
        await pets.DeleteAsync(id);
    }

    /// <summary>
    /// Найти владельца или сообщить, что его нет
    /// </summary>
    /// <param name="ownerId">Идентификатор владельца</param>
    /// <exception cref="KeyNotFoundException">Владелец не найден</exception>
    private async Task<Owner> GetOwnerAsync(int ownerId) =>
        await owners.GetByIdAsync(ownerId)
        ?? throw new KeyNotFoundException($"Владелец с идентификатором {ownerId} не найден");

    /// <summary>
    /// Найти породу, если она указана, или сообщить, что её нет
    /// </summary>
    /// <param name="breedId">Идентификатор породы или null</param>
    /// <exception cref="KeyNotFoundException">Порода указана, но не найдена</exception>
    private async Task<Breed?> GetBreedAsync(int? breedId)
    {
        if (breedId is null)
        {
            return null;
        }
        return await breeds.GetByIdAsync(breedId.Value)
            ?? throw new KeyNotFoundException($"Порода с идентификатором {breedId} не найдена");
    }
}

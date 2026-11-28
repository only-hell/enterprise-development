using System.ComponentModel.DataAnnotations;
using VetClinic.Contracts.Dto.Appointments;
using VetClinic.Contracts.Dto.Owners;
using VetClinic.Contracts.Dto.Pets;
using VetClinic.Contracts.Dto.Vets;
using VetClinic.Contracts.Messages;
using VetClinic.Contracts.Services;
using VetClinic.Domain.Repositories;

namespace VetClinic.Application.Services;

/// <summary>
/// Сохраняет запись на приём из контракта генератора
/// </summary>
/// <param name="owners">Сервис владельцев</param>
/// <param name="pets">Сервис питомцев</param>
/// <param name="vets">Сервис врачей</param>
/// <param name="appointments">Сервис записей на приём</param>
/// <param name="processedContracts">Отметки об обработке контрактов</param>
/// <param name="transaction">Общая транзакция записи и отметки</param>
public sealed class AppointmentImportService(
    IOwnerService owners,
    IPetService pets,
    IVetService vets,
    IAppointmentService appointments,
    IProcessedContractRepository processedContracts,
    IClinicTransaction transaction)
{
    /// <summary>
    /// Проверить и сохранить контракт со связанными сущностями
    /// </summary>
    /// <param name="contract">Контракт записи на приём</param>
    /// <param name="cancellationToken">Токен отмены</param>
    /// <returns>True, если контракт сохранён, false при повторной доставке</returns>
    public Task<bool> ImportAsync(AppointmentContract contract, CancellationToken cancellationToken) =>
        transaction.ExecuteSerializableAsync(async token =>
        {
            if (contract.ExternalId == Guid.Empty)
            {
                throw new ValidationException("Идентификатор контракта не задан");
            }

            if (await processedContracts.ExistsAsync(contract.ExternalId, token))
            {
                return false;
            }

            if (contract.Pet.BirthDate == default || contract.Pet.BirthDate.Date > DateTime.Today ||
                contract.Pet.BirthDate > contract.DateTime)
            {
                throw new ValidationException("Дата рождения питомца должна предшествовать приёму и не быть в будущем");
            }

            var owner = await owners.CreateAsync(Validate(new OwnerEditDto
            {
                FullName = contract.Owner.FullName,
                Address = contract.Owner.Address,
                Phone = contract.Owner.Phone
            }), token);

            var pet = await pets.CreateAsync(Validate(new PetEditDto
            {
                Nickname = contract.Pet.Nickname,
                Species = contract.Pet.Species,
                BreedId = contract.Pet.BreedId,
                BirthDate = contract.Pet.BirthDate,
                Weight = contract.Pet.Weight,
                OwnerId = owner.Id
            }), token);

            var vet = await vets.CreateAsync(Validate(new VetEditDto
            {
                PassportNumber = contract.Vet.PassportNumber,
                FullName = contract.Vet.FullName,
                BirthYear = contract.Vet.BirthYear,
                SpecializationId = contract.Vet.SpecializationId,
                WorkExperienceYears = contract.Vet.WorkExperienceYears
            }), token);

            await appointments.CreateAsync(Validate(new AppointmentEditDto
            {
                DateTime = contract.DateTime,
                RoomNumber = contract.RoomNumber,
                IsRepeat = contract.IsRepeat,
                PetId = pet.Id,
                VetId = vet.Id
            }), token);

            await processedContracts.AddAsync(contract.ExternalId, token);
            return true;
        }, cancellationToken);

    /// <summary>
    /// Проверить данные по тем же правилам, что использует API
    /// </summary>
    /// <typeparam name="T">Тип данных для сервиса</typeparam>
    /// <param name="dto">Данные для сохранения</param>
    private static T Validate<T>(T dto) where T : class
    {
        Validator.ValidateObject(dto, new ValidationContext(dto), validateAllProperties: true);
        return dto;
    }
}

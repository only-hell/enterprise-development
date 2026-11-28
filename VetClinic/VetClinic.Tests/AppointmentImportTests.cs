using System.ComponentModel.DataAnnotations;
using VetClinic.Application.Services;
using VetClinic.Contracts.Dto.Owners;
using VetClinic.Contracts.Dto.Pets;
using VetClinic.Contracts.Dto.Vets;
using VetClinic.Contracts.Messages;
using VetClinic.Infrastructure.InMemory;
using VetClinic.Shared;
using VetClinic.Tests.Fixtures;

namespace VetClinic.Tests;

/// <summary>
/// Проверки преобразования и валидации записей на приём из генератора
/// </summary>
public sealed class AppointmentImportTests
{
    private static readonly AppointmentContract Contract = new(
        Guid.NewGuid(),
        new OwnerContract("Иванов Павел Иванович", "ул. Лесная, 12", "+79000000012"),
        new PetContract("Марта", AnimalSpecies.Dog, 1, new DateTime(2022, 5, 1), 25.4),
        new VetContract("1000 123456", "Петров Иван Петрович", 1985, 1, 15),
        new DateTime(2026, 11, 30, 10, 0, 0),
        "205",
        true);

    private readonly CrudServices _clinic = new CrudServicesFixture().Create();

    /// <summary>
    /// Поля и связи контракта переносятся в сохранённые сущности
    /// </summary>
    [Fact]
    public async Task ImportAsync_ValidContract_SavesRelatedEntities()
    {
        var cancellationToken = TestContext.Current.CancellationToken;

        Assert.True(await CreateImporter().ImportAsync(Contract, cancellationToken));

        var appointment = Assert.Single(await _clinic.Appointments.GetAllAsync(cancellationToken),
            appointment => appointment.DateTime == Contract.DateTime);
        var pet = Assert.IsType<PetDto>(await _clinic.Pets.GetByIdAsync(appointment.PetId, cancellationToken));
        var vet = Assert.IsType<VetDto>(await _clinic.Vets.GetByIdAsync(appointment.VetId, cancellationToken));
        var owner = Assert.IsType<OwnerDto>(await _clinic.Owners.GetByIdAsync(pet.OwnerId, cancellationToken));

        Assert.Equal((Contract.RoomNumber, Contract.IsRepeat), (appointment.RoomNumber, appointment.IsRepeat));
        Assert.Equal(Contract.Pet, new PetContract(pet.Nickname, pet.Species, pet.BreedId, pet.BirthDate, pet.Weight));
        Assert.Equal(Contract.Owner, new OwnerContract(owner.FullName, owner.Address, owner.Phone));
        Assert.Equal(Contract.Vet, new VetContract(
            vet.PassportNumber, vet.FullName, vet.BirthYear, vet.SpecializationId, vet.WorkExperienceYears));
    }

    /// <summary>
    /// Некорректные поля сообщения проверяются перед созданием приёма
    /// </summary>
    /// <param name="field">Некорректное поле</param>
    [Theory]
    [InlineData("FullName")]
    [InlineData("Weight")]
    [InlineData("BirthYear")]
    public async Task ImportAsync_InvalidField_RejectsContract(string field)
    {
        var invalid = field switch
        {
            "FullName" => Contract with { Owner = Contract.Owner with { FullName = " " } },
            "Weight" => Contract with { Pet = Contract.Pet with { Weight = 0 } },
            "BirthYear" => Contract with { Vet = Contract.Vet with { BirthYear = DateTime.Today.Year + 1 } },
            _ => throw new ArgumentOutOfRangeException(nameof(field))
        };

        var exception = await Assert.ThrowsAsync<ValidationException>(() =>
            CreateImporter().ImportAsync(invalid, TestContext.Current.CancellationToken));
        Assert.Contains(field, exception.ValidationResult.MemberNames);
    }

    /// <summary>
    /// Порода из контракта должна существовать в справочнике
    /// </summary>
    [Fact]
    public async Task ImportAsync_UnknownBreed_RejectsContract()
    {
        var invalid = Contract with { Pet = Contract.Pet with { BreedId = int.MaxValue } };

        await Assert.ThrowsAsync<KeyNotFoundException>(() =>
            CreateImporter().ImportAsync(invalid, TestContext.Current.CancellationToken));
    }

    /// <summary>
    /// Собрать сервис импорта поверх репозиториев в памяти
    /// </summary>
    private AppointmentImportService CreateImporter() =>
        new(_clinic.Owners, _clinic.Pets, _clinic.Vets, _clinic.Appointments,
            new ProcessedContractsInMemory(), new InMemoryClinicTransaction());
}

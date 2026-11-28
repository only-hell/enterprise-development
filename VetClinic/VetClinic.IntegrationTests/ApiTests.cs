using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Confluent.Kafka;
using VetClinic.Contracts.Dto.Appointments;
using VetClinic.Contracts.Dto.Owners;
using VetClinic.Contracts.Dto.Vets;
using VetClinic.Contracts.Messages;
using VetClinic.Shared;

namespace VetClinic.IntegrationTests;

/// <summary>
/// Проверки API на настоящей базе SQL Server, поднятой через Aspire
/// </summary>
public class ApiTests(ClinicApplicationFixture fixture) : IClassFixture<ClinicApplicationFixture>
{
    private readonly ClinicApplicationFixture _fixture = fixture;

    /// <summary>
    /// Миграции применились и начальные данные на месте
    /// </summary>
    [Fact]
    public async Task GetOwners_AfterStart_ReturnsSeedOwners()
    {
        var owners = await GetAsync<List<OwnerResponse>>("api/owners");

        Assert.True(owners.Count >= 10);
    }

    /// <summary>
    /// Без токена изменяющий запрос отклоняется
    /// </summary>
    [Fact]
    public async Task CreatePet_WithoutToken_ReturnsUnauthorized()
    {
        using var client = _fixture.CreateAnonymousClient();
        using var response = await client.PostAsJsonAsync("api/pets", Pet(species: "Dog", breedId: 4),
            TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    /// <summary>
    /// Порода другого вида не принимается: «Персидская» относится к кошкам
    /// </summary>
    [Fact]
    public async Task CreatePet_WithBreedOfAnotherSpecies_ReturnsBadRequest()
    {
        using var response = await _fixture.Api.PostAsJsonAsync("api/pets", Pet(species: "Dog", breedId: 4),
            TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    /// <summary>
    /// Вид породы, к которой привязаны питомцы, менять нельзя
    /// </summary>
    [Fact]
    public async Task UpdateBreed_SpeciesWithPetsAttached_ReturnsConflict()
    {
        using var response = await _fixture.Api.PutAsJsonAsync("api/breeds/1",
            new { name = "Лабрадор", species = "Cat" }, TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
    }

    /// <summary>
    /// Повтор номера паспорта врача отклоняется базой
    /// </summary>
    [Fact]
    public async Task CreateVet_WithDuplicatePassport_ReturnsConflict()
    {
        var vet = new
        {
            passportNumber = "4501 000001",
            fullName = "Тестов Тест Тестович",
            birthYear = 1990,
            workExperienceYears = 5
        };

        using var response = await _fixture.Api.PostAsJsonAsync("api/vets", vet, TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
    }

    /// <summary>
    /// Удаление владельца удаляет его питомцев на стороне базы
    /// </summary>
    [Fact]
    public async Task DeleteOwner_RemovesHisPets()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        var owner = await PostAsync<OwnerResponse>("api/owners", new
        {
            fullName = "Временный Владелец Тестович",
            address = "ул. Проверочная, 1",
            phone = "+79001234567"
        });
        var pet = await PostAsync<PetResponse>("api/pets", Pet(species: "Dog", breedId: 1, ownerId: owner.Id));

        using var deleteResponse = await _fixture.Api.DeleteAsync($"api/owners/{owner.Id}", cancellationToken);
        Assert.Equal(HttpStatusCode.NoContent, deleteResponse.StatusCode);

        using var petCheck = await _fixture.Api.GetAsync($"api/pets/{pet.Id}", cancellationToken);
        Assert.Equal(HttpStatusCode.NotFound, petCheck.StatusCode);
    }

    /// <summary>
    /// Одновременное создание питомца и смена вида его породы не оставляют питомца с чужим видом
    /// </summary>
    [Fact]
    public async Task CreatePetAndChangeBreed_InParallel_KeepDataConsistent()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        var breed = await PostAsync<BreedResponse>("api/breeds", new { name = "Параллельная", species = "Dog" });

        var responses = await Task.WhenAll(
            _fixture.Api.PostAsJsonAsync("api/pets", Pet(species: "Dog", breedId: breed.Id), cancellationToken),
            _fixture.Api.PutAsJsonAsync($"api/breeds/{breed.Id}", new { name = "Параллельная", species = "Cat" }, cancellationToken));
        using var createPet = responses[0];
        using var changeBreed = responses[1];

        var allowed = (createPet.StatusCode == HttpStatusCode.Created && changeBreed.StatusCode == HttpStatusCode.Conflict)
            || (changeBreed.StatusCode == HttpStatusCode.OK && createPet.StatusCode == HttpStatusCode.BadRequest);
        Assert.True(allowed,
            $"неожиданные ответы: создание питомца {(int)createPet.StatusCode}, смена вида породы {(int)changeBreed.StatusCode}");

        var storedBreed = (await GetAsync<List<BreedResponse>>("api/breeds")).Single(item => item.Id == breed.Id);
        var pets = await GetAsync<List<PetResponse>>("api/pets");
        Assert.DoesNotContain(pets, pet => pet.BreedId == breed.Id && pet.Species != storedBreed.Species);
    }

    /// <summary>
    /// Неудачная запись повторно читается после устранения конфликта в SQL Server
    /// </summary>
    [Fact]
    public async Task ConsumeAsync_AfterDatabaseWriteFails_RetriesWithoutDuplicates()
    {
        using var timeout = CancellationTokenSource.CreateLinkedTokenSource(TestContext.Current.CancellationToken);
        timeout.CancelAfter(TimeSpan.FromMinutes(2));
        var cancellationToken = timeout.Token;
        var kafkaAddress = await _fixture.GetKafkaAddressAsync(cancellationToken);
        using var producer = new ProducerBuilder<string, string>(new ProducerConfig
        {
            BootstrapServers = kafkaAddress,
            MessageTimeoutMs = 30000
        }).Build();
        var contract = new AppointmentContract(
            Guid.NewGuid(),
            new OwnerContract("Тестовый Владелец Кафки", "ул. Проверочная, 2", "+79000000002"),
            new PetContract("Пальма", AnimalSpecies.Dog, 1, new DateTime(2022, 10, 10), 27),
            new VetContract("1000 234567", "Тестовый Ветеринар Кафки", 1985, 1, 12),
            new DateTime(2026, 11, 30, 13, 0, 0),
            "777",
            false);
        var pending = contract with
        {
            ExternalId = Guid.NewGuid(),
            Owner = contract.Owner with { FullName = "Владелец Повторно Обработанного Контракта" },
            Vet = contract.Vet with { PassportNumber = "1000 345678" },
            RoomNumber = "778"
        };
        var blockingVet = await PostAsync<VetDto>("api/vets", pending.Vet);
        var next = contract with
        {
            ExternalId = Guid.NewGuid(),
            Owner = contract.Owner with { FullName = "Владелец Следующего Питомца" },
            Vet = contract.Vet with { PassportNumber = "1000 456789" },
            RoomNumber = "779"
        };
        var partition = new TopicPartition(AppointmentContract.Topic, new Partition(0));

        var importFailure = _fixture.WaitForImportFailureAsync(pending.ExternalId, cancellationToken);
        await PublishAsync(contract);
        await PublishAsync(contract);
        var pendingRecord = await PublishAsync(pending);
        await PublishAsync(next);

        await importFailure;
        using var offsetReader = new ConsumerBuilder<Ignore, Ignore>(new ConsumerConfig
        {
            BootstrapServers = kafkaAddress,
            GroupId = "vetclinic-api",
            EnableAutoCommit = false
        }).Build();
        var committed = Assert.Single(offsetReader.Committed([partition], TimeSpan.FromSeconds(10)));
        Assert.Equal(pendingRecord.Offset, committed.Offset);

        var beforeRetry = await GetAsync<List<AppointmentDto>>("api/appointments");
        Assert.DoesNotContain(beforeRetry, item => item.RoomNumber == pending.RoomNumber || item.RoomNumber == next.RoomNumber);
        var ownersBeforeRetry = await GetAsync<List<OwnerDto>>("api/owners");
        Assert.DoesNotContain(ownersBeforeRetry, owner => owner.FullName == pending.Owner.FullName);

        using var response = await _fixture.Api.DeleteAsync($"api/vets/{blockingVet.Id}", cancellationToken);
        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        var appointments = await WaitForAppointmentsAsync(item => item.RoomNumber == next.RoomNumber, cancellationToken);

        var saved = Assert.Single(appointments, item => item.RoomNumber == contract.RoomNumber);
        Assert.Equal(contract.DateTime, saved.DateTime);
        Assert.Equal(contract.IsRepeat, saved.IsRepeat);
        Assert.Single(appointments, item => item.RoomNumber == pending.RoomNumber);
        var owners = await GetAsync<List<OwnerDto>>("api/owners");
        Assert.Single(owners, owner => owner.FullName == contract.Owner.FullName);
        Assert.Single(owners, owner => owner.FullName == pending.Owner.FullName);

        Task<DeliveryResult<string, string>> PublishAsync(AppointmentContract item) =>
            producer.ProduceAsync(partition, new Message<string, string>
            {
                Key = item.ExternalId.ToString(),
                Value = JsonSerializer.Serialize(item, JsonSerializerOptions.Web)
            }, cancellationToken);
    }

    /// <summary>
    /// Генератор запускается через Aspire и его запись появляется в API
    /// </summary>
    [Fact]
    public async Task GetAppointments_AfterGeneratorStarts_ReturnsGeneratedAppointment()
    {
        using var timeout = CancellationTokenSource.CreateLinkedTokenSource(TestContext.Current.CancellationToken);
        timeout.CancelAfter(TimeSpan.FromMinutes(2));
        var cancellationToken = timeout.Token;
        var today = DateTime.Today;

        var appointments = await WaitForAppointmentsAsync(IsGenerated, cancellationToken);
        var generated = appointments.First(IsGenerated);
        var vet = await GetAsync<VetDto>($"api/vets/{generated.VetId}");
        Assert.Equal(3, vet.FullName.Split(' ', StringSplitOptions.RemoveEmptyEntries).Length);

        bool IsGenerated(AppointmentDto item) =>
            item.DateTime.Date > today && item.RoomNumber is "101" or "102" or "205";
    }

    private async Task<List<AppointmentDto>> WaitForAppointmentsAsync(
        Func<AppointmentDto, bool> condition, CancellationToken cancellationToken)
    {
        while (true)
        {
            var appointments = await _fixture.Api.GetFromJsonAsync<List<AppointmentDto>>("api/appointments", cancellationToken)
                ?? throw new InvalidOperationException("API не вернул список записей на приём");
            if (appointments.Any(condition))
            {
                return appointments;
            }

            await Task.Delay(TimeSpan.FromMilliseconds(500), cancellationToken);
        }
    }

    private static object Pet(string species, int breedId, int ownerId = 1) => new
    {
        nickname = "Проверка",
        species,
        breedId,
        birthDate = "2020-01-01",
        weight = 10,
        ownerId
    };

    private async Task<T> GetAsync<T>(string url) =>
        (await _fixture.Api.GetFromJsonAsync<T>(url, TestContext.Current.CancellationToken))!;

    private async Task<T> PostAsync<T>(string url, object body)
    {
        using var response = await _fixture.Api.PostAsJsonAsync(url, body, TestContext.Current.CancellationToken);
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<T>(TestContext.Current.CancellationToken))!;
    }

    /// <summary>
    /// Владелец из ответа API
    /// </summary>
    /// <param name="Id">Идентификатор</param>
    private sealed record OwnerResponse(int Id);

    /// <summary>
    /// Питомец из ответа API
    /// </summary>
    /// <param name="Id">Идентификатор</param>
    /// <param name="BreedId">Идентификатор породы</param>
    /// <param name="Species">Вид питомца</param>
    private sealed record PetResponse(int Id, int? BreedId, string Species);

    /// <summary>
    /// Порода из ответа API
    /// </summary>
    /// <param name="Id">Идентификатор</param>
    /// <param name="Species">Вид животного</param>
    private sealed record BreedResponse(int Id, string Species);
}

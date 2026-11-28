using System.Globalization;
using Bogus;
using Bogus.DataSets;
using VetClinic.Contracts.Messages;
using VetClinic.Shared;

namespace VetClinic.Generator;

/// <summary>
/// Генерирует записи на приём на основе начальных справочников клиники
/// </summary>
internal sealed class AppointmentGenerator
{
    private static readonly (AnimalSpecies Species, int BreedId, int SpecializationId,
        string[] Nicknames, double MinWeight, double MaxWeight)[] Animals =
    [
        (AnimalSpecies.Dog, 1, 1, ["Рекс", "Пальма", "Бим"], 24, 36),
        (AnimalSpecies.Cat, 4, 2, ["Тиша", "Муся", "Барсик"], 3, 7),
        (AnimalSpecies.Bird, 6, 3, ["Кеша", "Рио", "Чика"], 0.03, 0.05),
        (AnimalSpecies.Rabbit, 8, 4, ["Пушок", "Кнопка", "Ушастик"], 2.5, 4.5),
        (AnimalSpecies.Hamster, 10, 8, ["Хома", "Рыжик", "Пончик"], 0.1, 0.2)
    ];

    private readonly Faker<AppointmentContract> _faker = new Faker<AppointmentContract>("ru")
        .UseSeed(6411)
        .CustomInstantiator(faker =>
        {
            var today = DateTime.Today;
            var animal = faker.PickRandom(Animals);
            var owner = new OwnerContract(
                GenerateFullName(faker),
                $"{faker.Address.StreetAddress()}, г. {faker.Address.City()}",
                faker.Phone.PhoneNumber("+79#########"));
            var pet = new PetContract(
                faker.PickRandom(animal.Nicknames),
                animal.Species,
                animal.BreedId,
                faker.Date.Between(today.AddYears(-2), today.AddMonths(-2)).Date,
                Math.Round(faker.Random.Double(animal.MinWeight, animal.MaxWeight), 2));
            var age = faker.Random.Int(25, 65);
            var passport = Random.Shared.NextInt64(10_000_000_000).ToString("D10", CultureInfo.InvariantCulture);
            var vet = new VetContract(
                passport.Insert(4, " "),
                GenerateFullName(faker),
                today.Year - age,
                animal.SpecializationId,
                faker.Random.Int(1, age - 24));

            return new AppointmentContract(
                Guid.NewGuid(),
                owner,
                pet,
                vet,
                today.AddDays(faker.Random.Int(1, 14)).AddHours(faker.Random.Int(9, 17)),
                faker.PickRandom("101", "102", "205"),
                faker.Random.Bool(0.25f));
        });

    /// <summary>
    /// Сгенерировать следующую запись на приём
    /// </summary>
    public AppointmentContract Generate() => _faker.Generate();

    /// <summary>
    /// Сгенерировать фамилию, имя и отчество
    /// </summary>
    /// <param name="faker">Генератор на русском языке</param>
    private static string GenerateFullName(Faker faker)
    {
        var gender = faker.Random.Enum<Name.Gender>();
        var patronymic = gender == Name.Gender.Male
            ? faker.PickRandom("Александрович", "Иванович", "Сергеевич")
            : faker.PickRandom("Александровна", "Ивановна", "Сергеевна");
        return $"{faker.Name.LastName(gender)} {faker.Name.FirstName(gender)} {patronymic}";
    }
}

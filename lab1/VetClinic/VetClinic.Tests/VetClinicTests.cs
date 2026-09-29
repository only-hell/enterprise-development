using VetClinic.Domain.Models;
using Xunit;

namespace VetClinic.Tests;

/// <summary>
/// Unit тесты для предметной области «Ветеринарная клиника»
/// </summary>
public class VetClinicTests(VetClinicTestData data) : IClassFixture<VetClinicTestData>
{
    // Тест 1. Вывести информацию о всех ветеринарах,
    //         специализирующихся на выбранном виде животных

    [Theory]
    [InlineData(AnimalSpecies.Dog, 3)]
    [InlineData(AnimalSpecies.Cat, 2)]
    [InlineData(AnimalSpecies.Bird, 1)]
    [InlineData(AnimalSpecies.Rabbit, 1)]
    [InlineData(AnimalSpecies.Hamster, 1)]
    public void GetVetsBySpeciesReturnsCorrectCount(AnimalSpecies species, int expectedCount)
    {
        var result = data.Vets
            .Where(v => v.Specialization?.FocusSpecies == species)
            .ToList();

        Assert.Equal(expectedCount, result.Count);
    }

    [Fact]
    public void GetVetsBySpeciesDogReturnsSmirnovKorolevaAndSorokin()
    {
        var expected = new HashSet<string>
        {
            "Смирнов Антон Валерьевич",
            "Королева Светлана Алексеевна",
            "Сорокин Владимир Федорович"
        };

        var result = data.Vets
            .Where(v => v.Specialization?.FocusSpecies == AnimalSpecies.Dog)
            .Select(v => v.FullName)
            .ToHashSet();

        Assert.Equal(expected, result);
    }

    // Тест 2. Вывести информацию о всех питомцах, записанных на прием
    //         к указанному врачу, упорядочить по кличке

    [Fact]
    public void GetPetsByVetSmirnovReturnsPetsOrderedByNickname()
    {
        var vetSmirnov = data.Vets.First(v => v.FullName == "Смирнов Антон Валерьевич");
        var expected = new[] { "Буян", "Жучка", "Рекс", "Тоби" };

        var result = data.Appointments
            .Where(a => a.Vet?.Id == vetSmirnov.Id)
            .Select(a => a.Pet)
            .DistinctBy(p => p!.Id)
            .OrderBy(p => p!.Nickname)
            .Select(p => p!.Nickname)
            .ToList();

        Assert.Equal(expected, result);
    }

    [Fact]
    public void GetPetsByVetKorolevaReturnsDistinctPetsOrderedByNickname()
    {
        var vetKoroleva = data.Vets.First(v => v.FullName == "Королева Светлана Алексеевна");
        var expected = new[] { "Буян", "Лаки" };

        var result = data.Appointments
            .Where(a => a.Vet?.Id == vetKoroleva.Id)
            .Select(a => a.Pet)
            .DistinctBy(p => p!.Id)
            .OrderBy(p => p!.Nickname)
            .Select(p => p!.Nickname)
            .ToList();

        Assert.Equal(expected, result);
    }

    // Тест 3. Вывести информацию о количестве повторных приемов
    //         выбранной породы животного

    [Theory]
    [InlineData(1, 3)]  // Лабрадор: Рекс апр (id=10), Лаки апр (id=11), Лаки июнь (id=3)
    [InlineData(3, 2)]  // Хаски: Буян июнь (id=2), Буян май (id=9)
    public void GetRepeatAppointmentCountByBreedReturnsCorrectCount(int breedId, int expectedCount)
    {
        var count = data.Appointments
            .Count(a => a.IsRepeat && a.Pet?.Breed?.Id == breedId);

        Assert.Equal(expectedCount, count);
    }

    [Fact]
    public void GetRepeatAppointmentCountByBreedPersianCatReturnsZero()
    {
        var persian = data.Breeds.First(b => b.Name == "Персидская");

        var count = data.Appointments
            .Count(a => a.IsRepeat && a.Pet?.Breed?.Id == persian.Id);

        Assert.Equal(0, count);
    }

    // Тест 4. Вывести информацию о владельцах, имеющих более одного питосца,
    //         упорядочить по ФИО

    [Fact]
    public void GetOwnersWithMultiplePetsReturnsThreeOwnersSortedByName()
    {
        var expected = new[] { "Иванов Иван Иванович", "Петров Петр Петрович", "Сидорова Анна Викторовна" };

        var result = data.Owners
            .Where(o => o.Pets.Count > 1)
            .OrderBy(o => o.FullName)
            .Select(o => o.FullName)
            .ToList();

        Assert.Equal(expected, result);
    }

    // Тест 5. Вывести информацию о приемах за текущий месяц,
    //         проходящих в выбранном кабинете

    [Theory]
    [InlineData("101", 2024, 6, 4)]
    [InlineData("205", 2024, 6, 2)]
    [InlineData("303", 2024, 6, 2)]
    [InlineData("404", 2024, 6, 2)]
    public void GetAppointmentsByRoomAndMonthReturnsCorrectCount(
        string room, int year, int month, int expectedCount)
    {
        var result = data.Appointments
            .Where(a => a.RoomNumber == room
                     && a.DateTime.Year == year
                     && a.DateTime.Month == month)
            .ToList();

        Assert.Equal(expectedCount, result.Count);
    }

    [Fact]
    public void GetAppointmentsByRoomAndMonthRoom101JuneReturnsCorrectIds()
    {
        var expected = new[] { 1, 2, 3, 13 };

        var result = data.Appointments
            .Where(a => a.RoomNumber == "101"
                     && a.DateTime.Year == 2024
                     && a.DateTime.Month == 6)
            .Select(a => a.Id)
            .OrderBy(id => id)
            .ToList();

        Assert.Equal(expected, result);
    }
}
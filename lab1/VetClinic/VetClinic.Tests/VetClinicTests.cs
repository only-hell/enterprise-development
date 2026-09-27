using VetClinic.Domain.Models;
using Xunit;

namespace VetClinic.Tests;

/// <summary>Unit тесты для предметной области Ветеринарная клиника</summary>
public class VetClinicTests
{
    private readonly VetClinicTestData _data = new();

    // Тест 1. Вывести информацию о всех ветеринарах, специализирующихся на выбранном виде животных
     
    [Theory]
    [InlineData(AnimalSpecies.Dog, 3)]   // SpecDogs, SpecDogs2, SpecDogs3
    [InlineData(AnimalSpecies.Cat, 2)]   // SpecCats, SpecCats2
    [InlineData(AnimalSpecies.Bird, 1)]   // SpecBirds
    [InlineData(AnimalSpecies.Rabbit, 1)]   // SpecRabbits
    [InlineData(AnimalSpecies.Hamster, 1)]   // SpecHamsters
    public void GetVetsBySpeciesReturnsCorrectVets(AnimalSpecies species, int expectedCount)
    {
        // Arrange
        var vets = new List<Vet>
        {
            _data.VetSmirnov, _data.VetKuznetsov, _data.VetPopova,
            _data.VetNovikov, _data.VetLebedev,   _data.VetKoroleva,
            _data.VetMikhailov, _data.VetGorbunov, _data.VetTitova,
            _data.VetSorokin
        };

        // Act
        var result = vets
            .Where(v => v.Specialization.FocusSpecies == species)
            .OrderBy(v => v.FullName)
            .ToList();

        // Assert
        Assert.Equal(expectedCount, result.Count);
        Assert.All(result, v => Assert.Equal(species, v.Specialization.FocusSpecies));
    }

    [Fact]
    public void GetVetsBySpeciesDogReturnsSmirnovKorolevaAndSorokin()
    {
        var vets = new List<Vet>
        {
            _data.VetSmirnov, _data.VetKuznetsov, _data.VetPopova,
            _data.VetNovikov, _data.VetLebedev,   _data.VetKoroleva,
            _data.VetMikhailov, _data.VetGorbunov, _data.VetTitova,
            _data.VetSorokin
        };

        var result = vets
            .Where(v => v.Specialization.FocusSpecies == AnimalSpecies.Dog)
            .Select(v => v.FullName)
            .ToHashSet();

        Assert.Contains("Смирнов Антон Валерьевич", result);
        Assert.Contains("Королева Светлана Алексеевна", result);
        Assert.Contains("Сорокин Владимир Федорович", result);
    }

     
    // Тест 2. Вывести информацию о всех питомцах, записанных на прием к указанному врачу, упорядочить по кличкам
     

    [Fact]
    public void GetPetsByVetSmirnovReturnsPetsOrderedByNickname()
    {
        // Act уникальные питомцы Смирнова, отсортированные по кличке
        var result = _data.Appointments
            .Where(a => a.Vet.Id == _data.VetSmirnov.Id)
            .Select(a => a.Pet)
            .DistinctBy(p => p.Id)
            .OrderBy(p => p.Nickname)
            .ToList();

        // Assert у Смирновап итомцы: Буян, Жучка, Рекс, Тоби
        Assert.Equal(4, result.Count);
        Assert.Equal("Буян", result[0].Nickname);
        Assert.Equal("Жучка", result[1].Nickname);
        Assert.Equal("Рекс", result[2].Nickname);
        Assert.Equal("Тоби", result[3].Nickname);
    }

    [Fact]
    public void GetPetsByVetResultIsSortedByNickname()
    {
        // проверяем набор сортеровки для конкретного врача
        var result = _data.Appointments
            .Where(a => a.Vet.Id == _data.VetKoroleva.Id)
            .Select(a => a.Pet)
            .DistinctBy(p => p.Id)
            .OrderBy(p => p.Nickname)
            .Select(p => p.Nickname)
            .ToList();

        var sorted = result.OrderBy(n => n).ToList();
        Assert.Equal(sorted, result);
    }

     
    // Тест 3. Вывести информацию о количестве повторных приемов выбранной породы животного
     

    [Theory]
    [InlineData(1 /* Лабрадор */, 3)]  // Рекс апр (id=10), Лаки апр (id=11), Лаки июнь (id=3)
    [InlineData(3 /* Хаски */, 2)]  // Буян июнь (id=2) и Буян май (id=9)
    public void GetRepeatAppointmentCountByBreedReturnsCorrectCount(int breedId, int expectedCount)
    {
        // Act
        var count = _data.Appointments
            .Where(a => a.IsRepeat && a.Pet.Breed.Id == breedId)
            .Count();

        // Assert
        Assert.Equal(expectedCount, count);
    }

    [Fact]
    public void GetRepeatAppointmentCountByBreedPersianCat_ReturnsZero()
    {
        // Персидская кошка Барс не имеет повторных приемов в датасиде
        var count = _data.Appointments
            .Where(a => a.IsRepeat && a.Pet.Breed.Id == _data.BreedPersian.Id)
            .Count();

        Assert.Equal(0, count);
    }

     
    // Тест 4. Вывести информацию о владельцах, имеющих более одного питомца упорядочить по ФИО
     

    [Fact]
    public void GetOwnersWithMultiplePetsReturnsThreeOwnersSortedByName()
    {
        // Arrange
        var owners = new List<Owner>
        {
            _data.OwnerIvanov,  _data.OwnerPetrov,   _data.OwnerSidorova,
            _data.OwnerKorolev, _data.OwnerNikitina, _data.OwnerFedorov,
            _data.OwnerMorozova,_data.OwnerZaitsev,  _data.OwnerSokolova,
            _data.OwnerVolkov
        };

        // Act
        var result = owners
            .Where(o => o.Pets.Count > 1)
            .OrderBy(o => o.FullName)
            .ToList();

        // Assert — Иванов (2), Петров (2), Сидорова (2)
        Assert.Equal(3, result.Count);
        Assert.Equal("Иванов Иван Иванович", result[0].FullName);
        Assert.Equal("Петров Петр Петрович", result[1].FullName);
        Assert.Equal("Сидорова Анна Викторовна", result[2].FullName);
    }

    [Fact]
    public void GetOwnersWithMultiplePetsResultIsSortedByFullName()
    {
        var owners = new List<Owner>
        {
            _data.OwnerIvanov,  _data.OwnerPetrov,   _data.OwnerSidorova,
            _data.OwnerKorolev, _data.OwnerNikitina, _data.OwnerFedorov,
            _data.OwnerMorozova,_data.OwnerZaitsev,  _data.OwnerSokolova,
            _data.OwnerVolkov
        };

        var result = owners
            .Where(o => o.Pets.Count > 1)
            .OrderBy(o => o.FullName)
            .Select(o => o.FullName)
            .ToList();

        var sorted = result.OrderBy(n => n).ToList();
        Assert.Equal(sorted, result);
    }

     
    // Тест 5. Вывести информацию о приемах за текущий месяц, проходящих в выбранном кабинете
     

    [Theory]
    [InlineData("101", 2024, 6, 4)]  // id=1,2,3,13
    [InlineData("205", 2024, 6, 2)]  // id=4,5
    [InlineData("303", 2024, 6, 2)]  // id=6,7
    [InlineData("404", 2024, 6, 2)]  // id=14,15
    public void GetAppointmentsByRoomAndMonthReturnsCorrectCount(
        string room, int year, int month, int expectedCount)
    {
        // Act
        var result = _data.Appointments
            .Where(a => a.RoomNumber == room
                     && a.DateTime.Year == year
                     && a.DateTime.Month == month)
            .ToList();

        // Assert
        Assert.Equal(expectedCount, result.Count);
        Assert.All(result, a => Assert.Equal(room, a.RoomNumber));
    }

    [Fact]
    public void GetAppointmentsByRoomAndMonthPreviousMonthExcluded()
    {
        // Кабинет 101, май 2024: id=8 и id=9 — они не должны попасть в июнь
        var juneRoom101 = _data.Appointments
            .Where(a => a.RoomNumber == "101"
                     && a.DateTime.Year == 2024
                     && a.DateTime.Month == 6)
            .ToList();

        Assert.All(juneRoom101, a =>
        {
            Assert.Equal(2024, a.DateTime.Year);
            Assert.Equal(6, a.DateTime.Month);
        });

        Assert.DoesNotContain(juneRoom101, a => a.DateTime.Month == 5);
    }
}
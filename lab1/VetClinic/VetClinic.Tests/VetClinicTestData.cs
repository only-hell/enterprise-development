using VetClinic.Domain.Models;

namespace VetClinic.Tests;

public class VetClinicTestData
{
    public readonly Specialization SpecDogs = new() { Id = 1, Name = "Хирург (собаки)", FocusSpecies = AnimalSpecies.Dog };
    public readonly Specialization SpecCats = new() { Id = 2, Name = "Терапевт (кошки)", FocusSpecies = AnimalSpecies.Cat };
    public readonly Specialization SpecBirds = new() { Id = 3, Name = "Орнитолог", FocusSpecies = AnimalSpecies.Bird };
    public readonly Specialization SpecRabbits = new() { Id = 4, Name = "Экзотика (кролики)", FocusSpecies = AnimalSpecies.Rabbit };
    public readonly Specialization SpecGeneral = new() { Id = 5, Name = "Терапевт (общий)", FocusSpecies = null };
    public readonly Specialization SpecDogs2 = new() { Id = 6, Name = "Кардиолог (собаки)", FocusSpecies = AnimalSpecies.Dog };
    public readonly Specialization SpecCats2 = new() { Id = 7, Name = "Дерматолог (кошки)", FocusSpecies = AnimalSpecies.Cat };
    public readonly Specialization SpecHamsters = new() { Id = 8, Name = "Экзотика (хомяки)", FocusSpecies = AnimalSpecies.Hamster };
    public readonly Specialization SpecGeneral2 = new() { Id = 9, Name = "Хирург (общий)", FocusSpecies = null };
    public readonly Specialization SpecDogs3 = new() { Id = 10, Name = "Невролог (собаки)", FocusSpecies = AnimalSpecies.Dog };

    public readonly Breed BreedLabrador = new() { Id = 1, Name = "Лабрадор", Species = AnimalSpecies.Dog };
    public readonly Breed BreedPoodle = new() { Id = 2, Name = "Пудель", Species = AnimalSpecies.Dog };
    public readonly Breed BreedHusky = new() { Id = 3, Name = "Хаски", Species = AnimalSpecies.Dog };
    public readonly Breed BreedPersian = new() { Id = 4, Name = "Персидская", Species = AnimalSpecies.Cat };
    public readonly Breed BreedBritish = new() { Id = 5, Name = "Британская", Species = AnimalSpecies.Cat };
    public readonly Breed BreedParrot = new() { Id = 6, Name = "Волнистый попугай", Species = AnimalSpecies.Bird };
    public readonly Breed BreedCanary = new() { Id = 7, Name = "Канарейка", Species = AnimalSpecies.Bird };
    public readonly Breed BreedRex = new() { Id = 8, Name = "Рекс", Species = AnimalSpecies.Rabbit };
    public readonly Breed BreedDwarf = new() { Id = 9, Name = "Карликовый", Species = AnimalSpecies.Rabbit };
    public readonly Breed BreedSyrian = new() { Id = 10, Name = "Сирийский", Species = AnimalSpecies.Hamster };
    public readonly Breed BreedDachshund = new() { Id = 11, Name = "Такса", Species = AnimalSpecies.Dog };

    public readonly Owner OwnerIvanov = new() { Id = 1, FullName = "Иванов Иван Иванович", Address = "ул. Ленина, 1", Phone = "+79001110001" };
    public readonly Owner OwnerPetrov = new() { Id = 2, FullName = "Петров Петр Петрович", Address = "пр. Мира, 5", Phone = "+79001110002" };
    public readonly Owner OwnerSidorova = new() { Id = 3, FullName = "Сидорова Анна Викторовна", Address = "ул. Садовая, 12", Phone = "+79001110003" };
    public readonly Owner OwnerKorolev = new() { Id = 4, FullName = "Королев Дмитрий Сергеевич", Address = "ул. Заречная, 3", Phone = "+79001110004" };
    public readonly Owner OwnerNikitina = new() { Id = 5, FullName = "Никитина Ольга Юрьевна", Address = "пер. Тихий, 7", Phone = "+79001110005" };
    public readonly Owner OwnerFedorov = new() { Id = 6, FullName = "Федоров Алексей Николаевич", Address = "ул. Победы, 44", Phone = "+79001110006" };
    public readonly Owner OwnerMorozova = new() { Id = 7, FullName = "Морозова Елена Павловна", Address = "ул. Центральная, 2", Phone = "+79001110007" };
    public readonly Owner OwnerZaitsev = new() { Id = 8, FullName = "Зайцев Михаил Олегович", Address = "ул. Северная, 9", Phone = "+79001110008" };
    public readonly Owner OwnerSokolova = new() { Id = 9, FullName = "Соколова Марина Ивановна", Address = "ул. Восточная, 18", Phone = "+79001110009" };
    public readonly Owner OwnerVolkov = new() { Id = 10, FullName = "Волков Сергей Андреевич", Address = "ул. Лесная, 31", Phone = "+79001110010" };

    public readonly Vet VetSmirnov;
    public readonly Vet VetKuznetsov;
    public readonly Vet VetPopova;
    public readonly Vet VetNovikov;
    public readonly Vet VetLebedev;
    public readonly Vet VetKoroleva;
    public readonly Vet VetMikhailov;
    public readonly Vet VetGorbunov;
    public readonly Vet VetTitova;
    public readonly Vet VetSorokin;

    public readonly Pet PetBars;
    public readonly Pet PetMurka;
    public readonly Pet PetRex;
    public readonly Pet PetBuyan;
    public readonly Pet PetSnowball;
    public readonly Pet PetKesha;
    public readonly Pet PetBelka;
    public readonly Pet PetPushok;
    public readonly Pet PetToby;
    public readonly Pet PetLucky;
    public readonly Pet PetZhuchka;

    public readonly List<Appointment> Appointments;

    public VetClinicTestData()
    {
        VetSmirnov = new Vet { Id = 1, PassportNumber = "4501 000001", FullName = "Смирнов Антон Валерьевич", BirthYear = 1980, Specialization = SpecDogs, WorkExperienceYears = 15 };
        VetKuznetsov = new Vet { Id = 2, PassportNumber = "4501 000002", FullName = "Кузнецов Игорь Петрович", BirthYear = 1975, Specialization = SpecCats, WorkExperienceYears = 22 };
        VetPopova = new Vet { Id = 3, PassportNumber = "4501 000003", FullName = "Попова Наталья Сергеевна", BirthYear = 1988, Specialization = SpecBirds, WorkExperienceYears = 8 };
        VetNovikov = new Vet { Id = 4, PassportNumber = "4501 000004", FullName = "Новиков Роман Михайлович", BirthYear = 1983, Specialization = SpecRabbits, WorkExperienceYears = 12 };
        VetLebedev = new Vet { Id = 5, PassportNumber = "4501 000005", FullName = "Лебедев Павел Денисович", BirthYear = 1990, Specialization = SpecGeneral, WorkExperienceYears = 6 };
        VetKoroleva = new Vet { Id = 6, PassportNumber = "4501 000006", FullName = "Королева Светлана Алексеевна", BirthYear = 1978, Specialization = SpecDogs2, WorkExperienceYears = 18 };
        VetMikhailov = new Vet { Id = 7, PassportNumber = "4501 000007", FullName = "Михайлов Денис Юрьевич", BirthYear = 1985, Specialization = SpecCats2, WorkExperienceYears = 10 };
        VetGorbunov = new Vet { Id = 8, PassportNumber = "4501 000008", FullName = "Горбунов Кирилл Олегович", BirthYear = 1992, Specialization = SpecHamsters, WorkExperienceYears = 4 };
        VetTitova = new Vet { Id = 9, PassportNumber = "4501 000009", FullName = "Титова Ирина Владимировна", BirthYear = 1981, Specialization = SpecGeneral2, WorkExperienceYears = 14 };
        VetSorokin = new Vet { Id = 10, PassportNumber = "4501 000010", FullName = "Сорокин Владимир Федорович", BirthYear = 1970, Specialization = SpecDogs3, WorkExperienceYears = 28 };

        PetBars = new Pet { Id = 1, Nickname = "Барс", Species = AnimalSpecies.Cat, Breed = BreedPersian, BirthDate = new DateTime(2019, 3, 10), Weight = 4.5, Owner = OwnerIvanov };
        PetMurka = new Pet { Id = 2, Nickname = "Мурка", Species = AnimalSpecies.Cat, Breed = BreedBritish, BirthDate = new DateTime(2020, 7, 22), Weight = 3.8, Owner = OwnerPetrov };
        PetRex = new Pet { Id = 3, Nickname = "Рекс", Species = AnimalSpecies.Dog, Breed = BreedLabrador, BirthDate = new DateTime(2018, 5, 15), Weight = 28.0, Owner = OwnerSidorova };
        PetBuyan = new Pet { Id = 4, Nickname = "Буян", Species = AnimalSpecies.Dog, Breed = BreedHusky, BirthDate = new DateTime(2017, 9, 3), Weight = 23.5, Owner = OwnerKorolev };
        PetSnowball = new Pet { Id = 5, Nickname = "Снежок", Species = AnimalSpecies.Rabbit, Breed = BreedRex, BirthDate = new DateTime(2021, 1, 18), Weight = 1.8, Owner = OwnerNikitina };
        PetKesha = new Pet { Id = 6, Nickname = "Кеша", Species = AnimalSpecies.Bird, Breed = BreedParrot, BirthDate = new DateTime(2020, 11, 5), Weight = 0.04, Owner = OwnerFedorov };
        PetBelka = new Pet { Id = 7, Nickname = "Белка", Species = AnimalSpecies.Hamster, Breed = BreedSyrian, BirthDate = new DateTime(2022, 6, 1), Weight = 0.15, Owner = OwnerMorozova };
        PetPushok = new Pet { Id = 8, Nickname = "Пушок", Species = AnimalSpecies.Rabbit, Breed = BreedDwarf, BirthDate = new DateTime(2021, 4, 14), Weight = 1.2, Owner = OwnerZaitsev };
        PetToby = new Pet { Id = 9, Nickname = "Тоби", Species = AnimalSpecies.Dog, Breed = BreedPoodle, BirthDate = new DateTime(2019, 8, 28), Weight = 7.0, Owner = OwnerIvanov };
        PetLucky = new Pet { Id = 10, Nickname = "Лаки", Species = AnimalSpecies.Dog, Breed = BreedLabrador, BirthDate = new DateTime(2020, 2, 11), Weight = 25.5, Owner = OwnerSidorova };
        PetZhuchka = new Pet { Id = 11, Nickname = "Жучка", Species = AnimalSpecies.Dog, Breed = BreedDachshund, BirthDate = new DateTime(2018, 12, 30), Weight = 9.5, Owner = OwnerPetrov };

        OwnerIvanov.Pets.AddRange(new[] { PetBars, PetToby });
        OwnerPetrov.Pets.AddRange(new[] { PetMurka, PetZhuchka });
        OwnerSidorova.Pets.AddRange(new[] { PetRex, PetLucky });
        OwnerKorolev.Pets.Add(PetBuyan);
        OwnerNikitina.Pets.Add(PetSnowball);
        OwnerFedorov.Pets.Add(PetKesha);
        OwnerMorozova.Pets.Add(PetBelka);
        OwnerZaitsev.Pets.Add(PetPushok);

        Appointments = new List<Appointment>
        {
            new() { Id = 1,  DateTime = new DateTime(2024, 6, 3,  10, 0,  0), RoomNumber = "101", IsRepeat = false, Pet = PetRex,      Vet = VetSmirnov   },
            new() { Id = 2,  DateTime = new DateTime(2024, 6, 5,  11, 30, 0), RoomNumber = "101", IsRepeat = true,  Pet = PetBuyan,    Vet = VetSmirnov   },
            new() { Id = 3,  DateTime = new DateTime(2024, 6, 10, 9,  0,  0), RoomNumber = "101", IsRepeat = true,  Pet = PetLucky,    Vet = VetKoroleva  },
            new() { Id = 13, DateTime = new DateTime(2024, 6, 7,  14, 0,  0), RoomNumber = "101", IsRepeat = false, Pet = PetZhuchka,  Vet = VetSmirnov   },
            new() { Id = 4,  DateTime = new DateTime(2024, 6, 12, 14, 0,  0), RoomNumber = "205", IsRepeat = false, Pet = PetBars,     Vet = VetKuznetsov },
            new() { Id = 5,  DateTime = new DateTime(2024, 6, 18, 16, 30, 0), RoomNumber = "205", IsRepeat = true,  Pet = PetMurka,    Vet = VetMikhailov },
            new() { Id = 6,  DateTime = new DateTime(2024, 6, 20, 10, 0,  0), RoomNumber = "303", IsRepeat = false, Pet = PetKesha,    Vet = VetPopova    },
            new() { Id = 7,  DateTime = new DateTime(2024, 6, 25, 13, 0,  0), RoomNumber = "303", IsRepeat = true,  Pet = PetSnowball, Vet = VetNovikov   },
            new() { Id = 14, DateTime = new DateTime(2024, 6, 22, 10, 0,  0), RoomNumber = "404", IsRepeat = true,  Pet = PetBelka,    Vet = VetGorbunov  },
            new() { Id = 15, DateTime = new DateTime(2024, 6, 28, 11, 0,  0), RoomNumber = "404", IsRepeat = false, Pet = PetPushok,   Vet = VetNovikov   },
            new() { Id = 8,  DateTime = new DateTime(2024, 5, 14, 9,  0,  0), RoomNumber = "101", IsRepeat = false, Pet = PetRex,      Vet = VetSmirnov   },
            new() { Id = 9,  DateTime = new DateTime(2024, 5, 20, 11, 0,  0), RoomNumber = "101", IsRepeat = true,  Pet = PetBuyan,    Vet = VetKoroleva  },
            new() { Id = 10, DateTime = new DateTime(2024, 4, 5,  10, 0,  0), RoomNumber = "205", IsRepeat = true,  Pet = PetRex,      Vet = VetSmirnov   },
            new() { Id = 11, DateTime = new DateTime(2024, 4, 7,  15, 0,  0), RoomNumber = "205", IsRepeat = true,  Pet = PetLucky,    Vet = VetKoroleva  },
            new() { Id = 12, DateTime = new DateTime(2024, 3, 1,  9,  30, 0), RoomNumber = "101", IsRepeat = false, Pet = PetToby,     Vet = VetSmirnov   },
        };
    }
}
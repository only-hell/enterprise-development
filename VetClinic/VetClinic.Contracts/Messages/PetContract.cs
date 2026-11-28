using VetClinic.Shared;

namespace VetClinic.Contracts.Messages;

/// <summary>
/// Данные питомца, записанного на приём
/// </summary>
/// <param name="Nickname">Кличка питомца</param>
/// <param name="Species">Биологический вид животного</param>
/// <param name="BreedId">Идентификатор породы в справочнике или null</param>
/// <param name="BirthDate">Дата рождения</param>
/// <param name="Weight">Вес в килограммах</param>
public sealed record PetContract(string Nickname, AnimalSpecies Species, int? BreedId, DateTime BirthDate, double Weight);

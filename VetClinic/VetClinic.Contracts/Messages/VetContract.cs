namespace VetClinic.Contracts.Messages;

/// <summary>
/// Данные врача, к которому записан питомец
/// </summary>
/// <param name="PassportNumber">Номер паспорта врача</param>
/// <param name="FullName">ФИО врача</param>
/// <param name="BirthYear">Год рождения</param>
/// <param name="SpecializationId">Идентификатор специализации в справочнике или null</param>
/// <param name="WorkExperienceYears">Стаж работы в годах</param>
public sealed record VetContract(
    string PassportNumber,
    string FullName,
    int BirthYear,
    int? SpecializationId,
    int WorkExperienceYears);

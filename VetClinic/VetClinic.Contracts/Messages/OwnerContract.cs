namespace VetClinic.Contracts.Messages;

/// <summary>
/// Данные владельца питомца из записи на приём
/// </summary>
/// <param name="FullName">ФИО владельца</param>
/// <param name="Address">Адрес проживания</param>
/// <param name="Phone">Контактный телефон</param>
public sealed record OwnerContract(string FullName, string Address, string Phone);

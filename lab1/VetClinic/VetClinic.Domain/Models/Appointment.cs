namespace VetClinic.Domain.Models;

/// <summary>
/// Запись на приём, используется в качестве контракта между владельцем питомца и врачом
/// </summary>
public class Appointment
{
    /// <summary>
    /// Уникальный идентификатор записи
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Дата и время приёма
    /// </summary>
    public DateTime DateTime { get; set; }

    /// <summary>
    /// Номер кабинета
    /// </summary>
    public required string RoomNumber { get; set; }

    /// <summary>
    /// Признак повторного приёма (true — повторный, false — первичный)
    /// </summary>
    public bool IsRepeat { get; set; }

    /// <summary>
    /// Питомец, записанный на приём
    /// </summary>
    public Pet? Pet { get; set; }

    /// <summary>
    /// Ветеринар, ведущий приём
    /// </summary>
    public Vet? Vet { get; set; }
}
namespace VetClinic.Contracts.Messages;

/// <summary>
/// Запись на приём вместе с данными для создания владельца, питомца и врача
/// </summary>
/// <param name="ExternalId">Идентификатор записи на стороне генератора</param>
/// <param name="Owner">Данные владельца</param>
/// <param name="Pet">Данные питомца владельца</param>
/// <param name="Vet">Данные врача</param>
/// <param name="DateTime">Дата и местное время приёма</param>
/// <param name="RoomNumber">Номер кабинета</param>
/// <param name="IsRepeat">Признак повторного приёма</param>
public sealed record AppointmentContract(
    Guid ExternalId,
    OwnerContract Owner,
    PetContract Pet,
    VetContract Vet,
    DateTime DateTime,
    string RoomNumber,
    bool IsRepeat)
{
    /// <summary>
    /// Топик Kafka для записей на приём
    /// </summary>
    public const string Topic = "vetclinic.appointments";

    /// <summary>
    /// Топик Kafka для сообщений с некорректным содержимым
    /// </summary>
    public const string DeadLetterTopic = "vetclinic.appointments.invalid";
}

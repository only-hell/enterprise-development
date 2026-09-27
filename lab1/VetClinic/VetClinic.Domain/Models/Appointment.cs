namespace VetClinic.Domain.Models;

/// <summary>запись на прием, исп-ся в качестве контракта</summary>
public class Appointment
{
    public int      Id         { get; set; }
    public DateTime DateTime   { get; set; }
    public string   RoomNumber { get; set; } = string.Empty;

    /// <summary>true — повторный прием, false — первичный</summary>
    public bool     IsRepeat   { get; set; }
    public Pet      Pet        { get; set; } = null!;
    public Vet      Vet        { get; set; } = null!;
}

namespace VetClinic.Domain.Models;

/// <summary>вет врач</summary>
public class Vet
{
    public int            Id                  { get; set; }
    public string         PassportNumber      { get; set; } = string.Empty;
    public string         FullName            { get; set; } = string.Empty;
    public int            BirthYear           { get; set; }
    public Specialization Specialization      { get; set; } = null!;

    /// <summary>стаж работы в годах</summary>
    public int            WorkExperienceYears { get; set; }
}

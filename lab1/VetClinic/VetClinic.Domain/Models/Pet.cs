namespace VetClinic.Domain.Models;

/// <summary>питомец</summary>
public class Pet
{
    public int           Id        { get; set; }
    public string        Nickname  { get; set; } = string.Empty;
    public AnimalSpecies Species   { get; set; }
    public Breed         Breed     { get; set; } = null!;
    public DateTime      BirthDate { get; set; }

    /// <summary>вес в килограммах</summary>
    public double        Weight    { get; set; }
    public Owner         Owner     { get; set; } = null!;
}

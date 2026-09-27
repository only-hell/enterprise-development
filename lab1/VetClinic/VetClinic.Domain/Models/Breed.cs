namespace VetClinic.Domain.Models;

/// <summary>порода - справочник привязанный к виду животного</summary>
public class Breed
{
    public int           Id      { get; set; }
    public string        Name    { get; set; } = string.Empty;
    public AnimalSpecies Species { get; set; }
}

namespace VetClinic.Domain.Models;

/// <summary>владелец питомца</summary>
public class Owner
{
    public int         Id       { get; set; }
    public string      FullName { get; set; } = string.Empty;
    public string      Address  { get; set; } = string.Empty;
    public string      Phone    { get; set; } = string.Empty;
    public List<Pet>   Pets     { get; set; } = new();
}

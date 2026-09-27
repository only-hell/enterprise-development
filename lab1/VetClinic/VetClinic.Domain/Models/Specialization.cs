namespace VetClinic.Domain.Models;

/// <summary>специализация врача - справочник
/// FocusSpecies указывает на вид животных на которм сосредоточена специализация
/// null - общая специализация, не привязанная к конкретному виду</summary>
public class Specialization
{
    public int            Id           { get; set; }
    public string         Name         { get; set; } = string.Empty;

    /// <summary>Вид животного, с которым работает данная специализация
    /// null означает что специализация не зависит от вида</summary>
    public AnimalSpecies? FocusSpecies { get; set; }
}

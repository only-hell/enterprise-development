namespace VetClinic.Domain.Models;

/// <summary>
/// Специализация врача — справочник
/// </summary>
public class Specialization
{
    /// <summary>
    /// Уникальный идентификатор специализации
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Название специализации
    /// </summary>
    public required string Name { get; set; }

    /// <summary>
    /// Вид животного, с которым работает данная специализация.
    /// Значение null означает общую специализацию без привязки к конкретному виду
    /// </summary>
    public AnimalSpecies? FocusSpecies { get; set; }
}
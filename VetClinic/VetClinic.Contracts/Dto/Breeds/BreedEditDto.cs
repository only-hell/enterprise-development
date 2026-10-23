using System.ComponentModel.DataAnnotations;

namespace VetClinic.Contracts.Dto.Breeds;

/// <summary>
/// Данные породы для создания и изменения
/// </summary>
public class BreedEditDto
{
    /// <summary>
    /// Название породы
    /// </summary>
    [Required]
    [MaxLength(100)]
    public required string Name { get; init; }

    /// <summary>
    /// Биологический вид животного
    /// </summary>
    [Required]
    [EnumDataType(typeof(AnimalSpeciesDto))]
    public required AnimalSpeciesDto Species { get; init; }
}

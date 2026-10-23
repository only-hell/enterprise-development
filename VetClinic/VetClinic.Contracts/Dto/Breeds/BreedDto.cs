namespace VetClinic.Contracts.Dto.Breeds;

/// <summary>
/// Порода животного
/// </summary>
public class BreedDto
{
    /// <summary>
    /// Идентификатор породы
    /// </summary>
    public required int Id { get; init; }

    /// <summary>
    /// Название породы
    /// </summary>
    public required string Name { get; init; }

    /// <summary>
    /// Биологический вид животного
    /// </summary>
    public required AnimalSpeciesDto Species { get; init; }
}

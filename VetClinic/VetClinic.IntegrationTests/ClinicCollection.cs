namespace VetClinic.IntegrationTests;

/// <summary>
/// Общая коллекция интеграционных тестов: приложение поднимается один раз на все классы
/// </summary>
[CollectionDefinition(Name)]
public sealed class ClinicCollection : ICollectionFixture<ClinicApplicationFixture>
{
    /// <summary>
    /// Имя коллекции
    /// </summary>
    public const string Name = "Clinic";
}

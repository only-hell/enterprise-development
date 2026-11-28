namespace VetClinic.Generator;

/// <summary>
/// Настройки периодической генерации записей на приём
/// </summary>
internal sealed class GenerationOptions
{
    /// <summary>
    /// Пауза между пачками в секундах
    /// </summary>
    public int IntervalSeconds { get; init; }

    /// <summary>
    /// Количество записей в одной пачке
    /// </summary>
    public int BatchSize { get; init; }
}

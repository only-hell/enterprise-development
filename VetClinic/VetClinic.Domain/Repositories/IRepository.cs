namespace VetClinic.Domain.Repositories;

/// <summary>
/// Базовые CRUD-операции над сущностью
/// </summary>
/// <typeparam name="TEntity">Тип сущности</typeparam>
public interface IRepository<TEntity>
    where TEntity : class
{
    /// <summary>
    /// Получить все сущности
    /// </summary>
    public Task<IReadOnlyList<TEntity>> GetAllAsync();

    /// <summary>
    /// Получить сущность по идентификатору
    /// </summary>
    /// <param name="id">Идентификатор сущности</param>
    public Task<TEntity?> GetByIdAsync(int id);

    /// <summary>
    /// Добавить сущность
    /// </summary>
    /// <param name="entity">Новая сущность</param>
    public Task<TEntity> CreateAsync(TEntity entity);

    /// <summary>
    /// Сохранить изменения существующей сущности
    /// </summary>
    /// <param name="entity">Изменённая сущность</param>
    public Task UpdateAsync(TEntity entity);

    /// <summary>
    /// Удалить сущность по идентификатору
    /// </summary>
    /// <param name="id">Идентификатор сущности</param>
    public Task<bool> DeleteAsync(int id);
}

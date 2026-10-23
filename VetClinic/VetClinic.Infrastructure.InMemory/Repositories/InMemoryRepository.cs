using VetClinic.Domain.Repositories;

namespace VetClinic.Infrastructure.InMemory.Repositories;

/// <summary>
/// Базовый репозиторий, хранящий сущности в коллекции в памяти
/// </summary>
/// <typeparam name="TEntity">Тип сущности</typeparam>
/// <param name="items">Коллекция с начальными данными</param>
public abstract class InMemoryRepository<TEntity>(List<TEntity> items) : IRepository<TEntity>
    where TEntity : class
{
    private readonly Lock _lock = new();

    /// <summary>
    /// Получить идентификатор сущности
    /// </summary>
    /// <param name="entity">Сущность</param>
    protected abstract int GetId(TEntity entity);

    /// <summary>
    /// Присвоить сущности идентификатор
    /// </summary>
    /// <param name="entity">Сущность</param>
    /// <param name="id">Новый идентификатор</param>
    protected abstract void SetId(TEntity entity, int id);

    /// <inheritdoc/>
    public Task<IReadOnlyList<TEntity>> GetAllAsync()
    {
        lock (_lock)
        {
            return Task.FromResult<IReadOnlyList<TEntity>>([.. items]);
        }
    }

    /// <inheritdoc/>
    public Task<TEntity?> GetByIdAsync(int id)
    {
        lock (_lock)
        {
            return Task.FromResult(items.Find(item => GetId(item) == id));
        }
    }

    /// <inheritdoc/>
    public Task<TEntity> CreateAsync(TEntity entity)
    {
        lock (_lock)
        {
            var nextId = items.Count == 0 ? 1 : items.Max(GetId) + 1;
            SetId(entity, nextId);
            items.Add(entity);
        }
        return Task.FromResult(entity);
    }

    /// <inheritdoc/>
    public Task UpdateAsync(TEntity entity)
    {
        lock (_lock)
        {
            var id = GetId(entity);
            var index = items.FindIndex(item => GetId(item) == id);
            if (index < 0)
            {
                throw new KeyNotFoundException($"Сущность {typeof(TEntity).Name} с идентификатором {id} не найдена");
            }
            items[index] = entity;
        }
        return Task.CompletedTask;
    }

    /// <inheritdoc/>
    public Task<bool> DeleteAsync(int id)
    {
        lock (_lock)
        {
            var removed = items.RemoveAll(item => GetId(item) == id) > 0;
            return Task.FromResult(removed);
        }
    }
}

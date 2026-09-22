using Microsoft.EntityFrameworkCore;

namespace MiniB2B.DataAccess.Repositories;

public class EfRepository<T> : IRepository<T> where T : class
{
    private readonly DbSet<T> _set;

    public EfRepository(AppDbContext context)
    {
        _set = context.Set<T>();
    }

    public Task<T?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
        => _set.FindAsync([id], cancellationToken).AsTask();

    public IQueryable<T> Query() => _set.AsQueryable();

    public Task AddAsync(T entity, CancellationToken cancellationToken = default)
        => _set.AddAsync(entity, cancellationToken).AsTask();

    public void Remove(T entity) => _set.Remove(entity);
}

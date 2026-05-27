using System.Linq.Expressions;

namespace PRN232.Lab1.Repositories.Interfaces;

public interface IGenericRepository<TEntity, TKey> where TEntity : class
{
    IQueryable<TEntity> Query();
    Task<TEntity?> GetByIdAsync(TKey id, params Expression<Func<TEntity, object>>[] includes);
    Task<TEntity> AddAsync(TEntity entity);
    Task UpdateAsync(TEntity entity);
    Task<bool> DeleteAsync(TKey id);
    Task<bool> ExistsAsync(TKey id);
}

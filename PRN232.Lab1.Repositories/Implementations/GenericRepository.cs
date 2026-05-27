using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using PRN232.Lab1.Repositories.Interfaces;

namespace PRN232.Lab1.Repositories.Implementations;

public abstract class GenericRepository<TEntity, TKey> : IGenericRepository<TEntity, TKey>
    where TEntity : class
{
    protected readonly LmsDbContext Db;
    protected readonly DbSet<TEntity> Set;

    protected GenericRepository(LmsDbContext db)
    {
        Db = db;
        Set = db.Set<TEntity>();
    }

    public IQueryable<TEntity> Query() => Set.AsNoTracking();

    public virtual async Task<TEntity?> GetByIdAsync(TKey id, params Expression<Func<TEntity, object>>[] includes)
    {
        // Use FindAsync first then re-query with includes if needed
        if (includes.Length == 0)
            return await Set.FindAsync(new object[] { id! });

        var entity = await Set.FindAsync(new object[] { id! });
        if (entity == null) return null;
        Db.Entry(entity).State = EntityState.Detached;

        IQueryable<TEntity> q = Set.AsNoTracking();
        foreach (var inc in includes) q = q.Include(inc);
        return await q.FirstOrDefaultAsync(BuildIdPredicate(id));
    }

    public async Task<TEntity> AddAsync(TEntity entity)
    {
        Set.Add(entity);
        await Db.SaveChangesAsync();
        return entity;
    }

    public async Task UpdateAsync(TEntity entity)
    {
        Set.Update(entity);
        await Db.SaveChangesAsync();
    }

    public async Task<bool> DeleteAsync(TKey id)
    {
        var entity = await Set.FindAsync(new object[] { id! });
        if (entity == null) return false;
        Set.Remove(entity);
        await Db.SaveChangesAsync();
        return true;
    }

    public async Task<bool> ExistsAsync(TKey id)
    {
        return await Set.FindAsync(new object[] { id! }) != null;
    }

    private Expression<Func<TEntity, bool>> BuildIdPredicate(TKey id)
    {
        var keyName = Db.Model.FindEntityType(typeof(TEntity))!
            .FindPrimaryKey()!.Properties[0].Name;
        var p = Expression.Parameter(typeof(TEntity), "x");
        var prop = Expression.Property(p, keyName);
        var val = Expression.Constant(id, typeof(TKey));
        var eq = Expression.Equal(prop, val);
        return Expression.Lambda<Func<TEntity, bool>>(eq, p);
    }
}

using System.Linq.Expressions;
using Dfe.SignIn.Core.Interfaces.DataAccess;
using Microsoft.EntityFrameworkCore;

namespace Dfe.SignIn.Gateways.EntityFramework;

/// <summary>
/// Entity Framework Core implementation of <see cref="IRepository{TEntity}"/>.
/// Supports fluent query composition and basic CRUD operations.
/// </summary>
/// <typeparam name="TEntity">The entity type.</typeparam>
public class EntityFrameworkRepository<TEntity> : IRepository<TEntity> where TEntity : class
{
    private readonly DbContext dbContext;
    private readonly DbSet<TEntity> dbSet;
    private IQueryable<TEntity> query;

    /// <summary>
    /// Creates a new repository for the specified DbContext.
    /// </summary>
    /// <param name="dbContext">The EF Core DbContext.</param>
    public EntityFrameworkRepository(DbContext dbContext)
    {
        this.dbContext = dbContext;
        this.dbSet = this.dbContext.Set<TEntity>();
        this.query = this.dbSet;
        this.ResetQuery();
    }

    /// <summary>
    /// Resets the internal query to the base DbSet.
    /// </summary>
    private void ResetQuery()
    {
        this.query = this.dbSet;
        // this.query = this.options.UseTracking
        //  ? this.dbset
        //  : this.dbset.AsNoTracking();
    }

    /// <inheritdoc />
    public IRepository<TEntity> Where(Expression<Func<TEntity, bool>> predicate)
    {
        this.query = this.query.Where(predicate);
        return this;
    }

    /// <inheritdoc />
    public IRepository<TEntity> Include<TProperty>(Expression<Func<TEntity, TProperty>> navigation)
    {
        this.query = this.query.Include(navigation);
        return this;
    }

    /// <inheritdoc />
    public IRepository<TEntity> OrderBy<TProperty>(Expression<Func<TEntity, TProperty>> keySelector)
    {
        this.query = this.query.OrderBy(keySelector);
        return this;
    }

    /// <inheritdoc />
    public IRepository<TEntity> OrderByDescending<TProperty>(Expression<Func<TEntity, TProperty>> keySelector)
    {
        this.query = this.query.OrderByDescending(keySelector);
        return this;
    }

    /// <inheritdoc />
    public IRepository<TEntity> Skip(int count)
    {
        this.query = this.query.Skip(count);
        return this;
    }

    /// <inheritdoc />
    public IRepository<TEntity> Take(int count)
    {
        this.query = this.query.Take(count);
        return this;
    }

    /// <inheritdoc />
    public IQueryable<TEntity> Query() => this.query;

    /// <inheritdoc />
    public Task AddAsync(TEntity entity, CancellationToken cancellationToken = default)
    {
        return this.dbSet.AddAsync(entity, cancellationToken).AsTask();
    }

    /// <inheritdoc />
    public void Update(TEntity entity)
    {
        this.dbContext.Update(entity);
    }

    /// <inheritdoc />
    public void Delete(TEntity entity)
    {
        this.dbContext.Remove(entity);
    }

    /// <inheritdoc />
    public Task<List<TEntity>> ToListAsync(CancellationToken cancellationToken = default)
    {
        var result = this.query.ToListAsync(cancellationToken);
        this.ResetQuery();
        return result;
    }

    /// <inheritdoc />
    public Task<TEntity?> FirstOrDefaultAsync(CancellationToken cancellationToken = default)
    {
        var result = this.query.FirstOrDefaultAsync(cancellationToken);
        this.ResetQuery();
        return result;
    }
}

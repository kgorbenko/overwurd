using System;
using System.Collections.Immutable;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Overwurd.Model.Models;

namespace Overwurd.Model.Repositories
{
    public class OverwurdRepository<T, TContext> : IOverwurdRepository<T>
        where T : class, IEntityWithNumericId
        where TContext : OverwurdDbContext
    {
        private readonly DbContext dbContext;
        private readonly DbSet<T> dbSet;

        public OverwurdRepository(TContext dbContext)
        {
            this.dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
            dbSet          = dbContext.Set<T>();
        }

        public async Task<T> FindByIdAsync(long id, CancellationToken cancellationToken = default)
        {
            return await dbSet.FindAsync(new object[] { id }, cancellationToken);
        }

        public async Task<OverwurdPaginationResult<T>> PaginateByAsync(Expression<Func<T, bool>> predicate, int page, int pageSize, CancellationToken cancellationToken = default)
        {
            var query = dbSet.Where(predicate);
            var results = await query.Select(x => new { Entity = x, TotalCount = query.Count() })
                                     .Skip(pageSize * (page - 1))
                                     .Take(pageSize)
                                     .ToArrayAsync(cancellationToken: cancellationToken);

            return new OverwurdPaginationResult<T>(
                results.Select(x => x.Entity).ToImmutableArray(),
                results.FirstOrDefault()?.TotalCount ?? query.Count());
        }

        public async Task<IImmutableList<T>> FindByAsync(Expression<Func<T, bool>> filter, CancellationToken cancellationToken = default)
        {
            return (await dbSet.Where(filter).ToArrayAsync(cancellationToken)).ToImmutableList();
        }

        public async Task<IImmutableList<T>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            return (await dbSet.ToArrayAsync(cancellationToken)).ToImmutableList();
        }

        public async Task AddAsync(T entity, CancellationToken cancellationToken = default)
        {
            await dbSet.AddAsync(entity, cancellationToken);
            await dbContext.SaveChangesAsync(cancellationToken);
        }

        public async Task AddRangeAsync(IImmutableList<T> entities, CancellationToken cancellationToken = default)
        {
            await dbSet.AddRangeAsync(entities, cancellationToken);
            await dbContext.SaveChangesAsync(cancellationToken);
        }

        public async Task UpdateAsync(T entity, CancellationToken cancellationToken = default)
        {
            dbSet.Update(entity);
            await dbContext.SaveChangesAsync(cancellationToken);
        }

        public async Task RemoveAsync(T entity, CancellationToken cancellationToken = default)
        {
            dbSet.Remove(entity);
            await dbContext.SaveChangesAsync(cancellationToken);
        }

        public async Task RemoveRangeAsync(IImmutableList<T> entities, CancellationToken cancellationToken = default)
        {
            dbSet.RemoveRange(entities);
            await dbContext.SaveChangesAsync(cancellationToken);
        }
    }
}
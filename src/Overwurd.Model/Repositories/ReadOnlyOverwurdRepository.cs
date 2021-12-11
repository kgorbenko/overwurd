using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Overwurd.Model.Models;

namespace Overwurd.Model.Repositories {
    public class ReadOnlyOverwurdRepository<T, TContext> : IReadOnlyOverwurdRepository<T>
        where T : class, IEntityWithNumericId
        where TContext : DbContext {
        private readonly DbContext dbContext;
        private readonly DbSet<T> dbSet;

        public ReadOnlyOverwurdRepository(TContext dbContext) {
            this.dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
            dbSet = dbContext.Set<T>();
        }

        public async Task<T> FindByIdAsync(long id, CancellationToken cancellationToken = default) {
            return (await FindByAsync(x => x.Id == id, cancellationToken))
                .SingleOrDefault();
        }

        public async Task<OverwurdPaginationResult<T>> PaginateAsync(Expression<Func<T, bool>> predicate, int page, int pageSize,
                                                                     CancellationToken cancellationToken = default) {
            var query = dbSet.Where(predicate);
            var results = await query.Select(x => new { Entity = x, TotalCount = query.Count() })
                                     .Skip(pageSize * (page - 1))
                                     .Take(pageSize)
                                     .AsNoTracking()
                                     .ToArrayAsync(cancellationToken: cancellationToken);

            return new OverwurdPaginationResult<T>(
                results.Select(x => x.Entity).ToImmutableArray(),
                results.FirstOrDefault()?.TotalCount ?? query.Count());
        }

        public async Task<IImmutableList<T>> FindByAsync(Expression<Func<T, bool>> predicate, CancellationToken cancellationToken = default) {
            var results = await dbSet.AsNoTracking()
                                     .Where(predicate)
                                     .ToArrayAsync(cancellationToken);
            return results.ToImmutableArray();
        }

        public async Task<IImmutableList<T>> GetAllAsync(CancellationToken cancellationToken = default) {
            var results = await dbSet.AsNoTracking().ToArrayAsync(cancellationToken);
            return results.ToImmutableArray();
        }
    }
}
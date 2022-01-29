using System;
using System.Collections.Immutable;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Overwurd.Model.Models;

namespace Overwurd.Model.Repositories;

public class ReadOnlyRepository<T> : IReadOnlyRepository<T> where T : class, IEntityWithNumericId {
    protected ApplicationDbContext DbContext { get; }
    private readonly DbSet<T> dbSet;

    public ReadOnlyRepository(ApplicationDbContext dbContext) {
        DbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
        dbSet = dbContext.Set<T>();
    }

    public async Task<T> FindByIdAsync(int id, CancellationToken cancellationToken = default) {
        return (await FindByAsync(x => x.Id == id, cancellationToken))
            .SingleOrDefault();
    }

    public async Task<PaginationResult<T>> PaginateAsync(Expression<Func<T, bool>> predicate, int page, int pageSize,
                                                         CancellationToken cancellationToken = default) {
        var query = dbSet.Where(predicate);
        var results = await query.Select(x => new { Entity = x, TotalCount = query.Count() })
                                 .Skip(pageSize * (page - 1))
                                 .Take(pageSize)
                                 .AsNoTracking()
                                 .ToArrayAsync(cancellationToken: cancellationToken);

        return new PaginationResult<T>(
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
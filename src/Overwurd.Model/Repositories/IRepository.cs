using System;
using System.Collections.Immutable;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using Overwurd.Model.Models;

namespace Overwurd.Model.Repositories;

public interface IRepository<T> where T: IEntityWithNumericId
{
    Task<T> FindByIdAsync(int id, CancellationToken cancellationToken = default);

    Task<PaginationResult<T>> PaginateByAsync(Expression<Func<T, bool>> predicate, int page, int pageSize, CancellationToken cancellationToken = default);

    Task<IImmutableList<T>> FindByAsync(Expression<Func<T, bool>> predicate, CancellationToken cancellationToken = default);

    Task<IImmutableList<T>> GetAllAsync(CancellationToken cancellationToken = default);

    Task AddAsync(T entity, CancellationToken cancellationToken = default);

    Task AddRangeAsync(IImmutableList<T> entities, CancellationToken cancellationToken = default);

    Task UpdateAsync(T entity, CancellationToken cancellationToken = default);

    Task RemoveAsync(T entity, CancellationToken cancellationToken = default);

    Task RemoveRangeAsync(IImmutableList<T> entities, CancellationToken cancellationToken = default);

    Task<int> CountByAsync(Expression<Func<T, bool>> filter, CancellationToken cancellationToken = default);
}